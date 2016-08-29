/***********************************************************************************************************************
 Copyright (c) 2016, Imagination Technologies Limited and/or its affiliated group companies.
 All rights reserved.

 Redistribution and use in source and binary forms, with or without modification, are permitted provided that the
 following conditions are met:
     1. Redistributions of source code must retain the above copyright notice, this list of conditions and the
        following disclaimer.
     2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the
        following disclaimer in the documentation and/or other materials provided with the distribution.
     3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote
        products derived from this software without specific prior written permission.

 THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
 DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
 SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
 WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE 
 USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
***********************************************************************************************************************/

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Imagination.BusinessLogic;

namespace Imagination.Service
{
	public class NotificationTcpServer
	{
		public class ReceiveStateObject
		{
			public const int BufferSize = 256;
			public byte[] Buffer = new byte[BufferSize];
			public StringBuilder Data = new StringBuilder();
			public DateTime StartReceive;
		}

		private Socket _TcpListener;


		private const string SETUP_NOTIFICATION = "1";
		private const string BROADCAST_PURGE_NOTIFICATION = "4";
		private const string BROADCAST_NOTIFICATION = "5";

		public NotificationTcpServer()
		{
		

		}

		/// <summary>
		/// Start listening for incoming messages
		/// </summary>
		public void StartListening(int port)
		{
			if (_TcpListener == null)
			{
				IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
				_TcpListener = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				
				bool bound = false;
				DateTime timeOut = DateTime.Now.AddSeconds(3);
				while (!bound && DateTime.Now < timeOut)
				{
					try
					{
						_TcpListener.Bind(endPoint);
						bound = true;
					}
					catch (SocketException)
					{
						Thread.Sleep(100);
					}
				}
				if (!bound)
					Trace.WriteLine(TTracePriority.High, "Failed to start listening - could not bind to port: " + port);
				else
				{
					_TcpListener.NoDelay = true;
					bool connected = false;
					timeOut = DateTime.Now.AddSeconds(3);
					while (!connected && DateTime.Now < timeOut)
					{
						try
						{
							_TcpListener.Listen(100);
                            SocketAsyncEventArgs parameters = new SocketAsyncEventArgs();
                            parameters.Completed += AcceptCompleted;
                            if (!_TcpListener.AcceptAsync(parameters))
                                AcceptCompleted(_TcpListener, parameters);
							connected = true;
						}
						catch (SocketException)
						{
							Thread.Sleep(100);
						}
					}
					if (!connected)
						Trace.WriteLine(TTracePriority.High, "Failed to start listening");
				}
			}
		}

        /// <summary>
        /// Stop listening for incoming messages
        /// </summary>
        public void StopListening()
		{
			if (_TcpListener != null)
			{
				//_TcpListener.Shutdown(SocketShutdown.Both);
				_TcpListener.Close();
				_TcpListener = null;
			}
		}

		private void AcceptCompleted(object sender, SocketAsyncEventArgs e)
		{
			Socket listener = (Socket)sender;
            Socket client = null;
            do
            {
                try
                {
                    client = e.AcceptSocket;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(TTracePriority.High, ex.ToString());
                }
                if (client != null)
                {
                    ReceiveStateObject stateObject = new ReceiveStateObject();
                    stateObject.StartReceive = DateTime.Now;

                    SocketAsyncEventArgs parameters = new SocketAsyncEventArgs();
                    parameters.UserToken = stateObject;
                    parameters.SetBuffer(stateObject.Buffer, 0, ReceiveStateObject.BufferSize);
                    parameters.Completed += ReceiveCompleted;
                    if (!client.ReceiveAsync(parameters))
                        ReceiveCompleted(client, parameters);
                }
                e.AcceptSocket = null;
            } while (!listener.AcceptAsync(e));
        }

        private void ReceiveCompleted(object sender, SocketAsyncEventArgs e)
		{
            Socket client = (Socket)sender;
            ReceiveStateObject stateObject = e.UserToken as ReceiveStateObject;
			try
			{
				int readCount = e.BytesTransferred;
				bool continueReceive = true;
				if (readCount > 0)
				{
					string text = Encoding.ASCII.GetString(stateObject.Buffer, 0, readCount);
					stateObject.Data.Append(text);
					if (text.Contains("|"))
					{
						string data = stateObject.Data.ToString();
						int count = 0;
						while (!string.IsNullOrEmpty(data))
						{
							int index = data.IndexOf("|");
							if (index == -1)
							{
								break;
							}
							else
							{
								count += (index + 1);
								string table = data.Substring(0, index);
								index++;
								if (index < data.Length)
									data = data.Substring(index);
								else
									data = null;
								string id = null;
								string[] fields = table.Split(',');
								int fieldLength = fields.Length;
								if (fields[0] == BROADCAST_PURGE_NOTIFICATION)
								{
									bool purge = false;
									if (fieldLength >= 3)
									{
										if (!bool.TryParse(fields[2], out purge))
											purge = false;
										if (fieldLength >= 4)
											id = fields[3];
									}
									if (fieldLength >= 4)
										BusinessLogicFactory.NotificationOrchestrator.SendChangeNotification(fields[1], purge, id);
								}
								else if (fields[0] == BROADCAST_NOTIFICATION)
								{
									if (fieldLength >= 4)
										id = fields[3];
									if (fieldLength >= 4)
										BusinessLogicFactory.NotificationOrchestrator.SendChangeNotification(fields[1], false, id);
								}
								else if (fields[0] == SETUP_NOTIFICATION)
								{
                                    if (fieldLength >= 2)
										BusinessLogicFactory.NotificationOrchestrator.AddNotifcationClient(fields[1], client);
								}
							}
						}
						stateObject.Data.Remove(0, count);
					}
				}
				else
				{
					if (client.ReceiveTimeout <= 0)
						continueReceive = false;
					else
					{
						TimeSpan diff = DateTime.Now.Subtract(stateObject.StartReceive);
						if (diff.TotalMilliseconds < client.ReceiveTimeout)
							continueReceive = false;
					}
				}
				if (continueReceive)
				{
					stateObject.StartReceive = DateTime.Now;
                    if (!client.ReceiveAsync(e))
                        ReceiveCompleted(sender, e);
                }
				else
				{
#if DEBUG
					ApplicationEventLog.WriteEntry("Flow", string.Format("DBNotificationTCPServer::DoReceiveCallback: closing client socket {0}", client), System.Diagnostics.EventLogEntryType.Information);
#endif
                    client.Close();
				}
			}
#pragma warning disable 168
            catch (Exception ex)
#pragma warning restore 168
            {
#if DEBUG
                ApplicationEventLog.WriteEntry("Flow", string.Format("DBNotificationTCPServer::DoReceiveCallback: Exception\n{0}", ex), System.Diagnostics.EventLogEntryType.Error);
#endif
			}
		}

	}
}
