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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Imagination.Model;

namespace Imagination.LWM2M
{
	public class NativeIPCServer
	{
		private class ReceiveStateObject
		{
			public Socket Client = null;
			public const int BufferSize = 256;
			public byte[] Buffer = new byte[BufferSize];
			public DateTime StartReceive;
			public MemoryStream Data = new MemoryStream(4096);
			public int RequestLength;
		}

		private bool _Terminate;
        private AddressFamily _AddressFamily;
        private int _Port;


		private Socket _Socket;
		private ServerAPI _ServerAPI = new ServerAPI();


		public NativeIPCServer(AddressFamily addressFamily, int port)
		{
            _AddressFamily = addressFamily;
            _Port = port;
		}

		private void BindSocket(Socket socket, EndPoint endPoint)
		{
			bool bound = false;
			DateTime timeOut = DateTime.Now.AddSeconds(3);
			while (!bound && DateTime.Now < timeOut)
			{
				try
				{
					socket.Bind(endPoint);
					bound = true;
				}
				catch (SocketException)
				{
					Thread.Sleep(100);
				}
			}
			if (!bound)
			{
				ApplicationEventLog.WriteEntry("Failed to start listening - could not bind to " + endPoint.ToString(), System.Diagnostics.EventLogEntryType.Error);
			}
			else
			{
				socket.NoDelay = true;
				bool connected = false;
				timeOut = DateTime.Now.AddSeconds(3);
				while (!connected && DateTime.Now < timeOut)
				{
					try
					{
						socket.Listen(100);
						socket.BeginAccept(new AsyncCallback(DoAcceptCallback), socket);
						connected = true;
					}
					catch (SocketException)
					{
						Thread.Sleep(100);
					}
				}
				if (!connected)
					ApplicationEventLog.WriteEntry("Failed to start listening", System.Diagnostics.EventLogEntryType.Error);
			}
		}

		private void DoAcceptCallback(IAsyncResult asyncResult)
		{
			Socket listener = (Socket)asyncResult.AsyncState;
			Socket client = null;
			try
			{
                if (!_Terminate)
                    client = listener.EndAccept(asyncResult);
			}
			catch (Exception ex)
			{
				ApplicationEventLog.WriteEntry(ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
			}
			if (!_Terminate)
				listener.BeginAccept(new AsyncCallback(DoAcceptCallback), listener);
			if (client != null)
			{
				ReceiveStateObject stateObject = new ReceiveStateObject();
				stateObject.Client = client;
				SocketError socketError;
				stateObject.StartReceive = DateTime.Now;
				client.BeginReceive(stateObject.Buffer, 0, ReceiveStateObject.BufferSize, SocketFlags.None, out socketError, new AsyncCallback(DoReceiveCallback), stateObject);
			}
		}

		private void DoReceiveCallback(IAsyncResult asyncResult)
		{
			ReceiveStateObject stateObject = asyncResult.AsyncState as ReceiveStateObject;
			try
			{
				int readCount = stateObject.Client.EndReceive(asyncResult);
				bool continueReceive = true;
				if (readCount > 0)
				{
					stateObject.Data.Write(stateObject.Buffer, 0, readCount);
					if ((stateObject.RequestLength == 0) && (stateObject.Data.Length > 4))
					{
						long position = stateObject.Data.Position;
						stateObject.Data.Position = 0;
						stateObject.RequestLength = NetworkByteOrderConverter.ToInt32(stateObject.Data) + 4;
						stateObject.Data.Position = position;
					}
					if (stateObject.RequestLength == stateObject.Data.Position)
					{
						stateObject.Data.Position = 4;
						IPCRequest request = IPCRequest.Deserialise(stateObject.Data);
						ThreadPool.QueueUserWorkItem(new WaitCallback((s) => { ProcessRequest(stateObject.Client, request); }));
						stateObject.Data.Position = 0;
						stateObject.Data.SetLength(0);
						stateObject.RequestLength = 0;
					}
				}
				else
				{
					if (stateObject.Client.ReceiveTimeout <= 0)
						continueReceive = false;
					else
					{
						TimeSpan diff = DateTime.Now.Subtract(stateObject.StartReceive);
						if (diff.TotalMilliseconds < stateObject.Client.ReceiveTimeout)
							continueReceive = false;
					}
				}
				if (continueReceive)
				{
					stateObject.StartReceive = DateTime.Now;
					SocketError socketError;
					stateObject.Client.BeginReceive(stateObject.Buffer, 0, ReceiveStateObject.BufferSize, SocketFlags.None, out socketError, new AsyncCallback(DoReceiveCallback), stateObject);
				}
				else
				{
					stateObject.Client.Close();
				}
			}
#pragma warning disable 0168 
            catch (Exception ex)
#pragma warning restore 0168
            {
				stateObject.Client.Close();

			}
		}

		private void ProcessRequest(Socket client, IPCRequest request)
		{
			byte[] response = new byte[4];
			try
			{

                if (string.Compare(request.Method, "CancelObserveObject", true) == 0)
                {
                    Guid clientID = request.ReadGuid();
                    Guid objectDefinitionID = request.ReadGuid();
                    string instanceID = request.ReadString();
                    bool useReset = request.ReadBoolean();
                    _ServerAPI.CancelObserveObject(clientID, objectDefinitionID, instanceID, useReset);
                }
                else if (string.Compare(request.Method, "CancelObserveObjectProperty", true) == 0)
                {
                    Guid clientID = request.ReadGuid();
                    Guid objectDefinitionID = request.ReadGuid();
                    string instanceID = request.ReadString();
                    Guid propertyDefinitionID = request.ReadGuid();
                    bool useReset = request.ReadBoolean();
                    _ServerAPI.CancelObserveObjectProperty(clientID, objectDefinitionID, instanceID, propertyDefinitionID, useReset);
                }
                else if (string.Compare(request.Method, "CancelObserveObjects", true) == 0)
                {
                    Guid clientID = request.ReadGuid();
                    Guid objectDefinitionID = request.ReadGuid();
                    bool useReset = request.ReadBoolean();
                    _ServerAPI.CancelObserveObjects(clientID, objectDefinitionID, useReset);
                }
                else if (string.Compare(request.Method, "DeleteClient", true) == 0)
                {
                    Guid clientID = request.ReadGuid();
                    _ServerAPI.DeleteClient(clientID);
                }
                else if (string.Compare(request.Method, "ExecuteResource", true) == 0)
                {
                    Guid clientID = request.ReadGuid();
                    Guid objectDefinitionID = request.ReadGuid();
                    string instanceID = request.ReadString();
                    Guid propertyDefinitionID = request.ReadGuid();
                    bool success = _ServerAPI.ExecuteResource(clientID, objectDefinitionID, instanceID, propertyDefinitionID);
                    MemoryStream data = new MemoryStream(4096);
                    data.Position = 4;
                    IPCHelper.Write(data, success);
                    data.Position = 0;
                    NetworkByteOrderConverter.WriteInt32(data, (int)data.Length - 4);
                    response = data.ToArray();
                }   
                if (string.Compare(request.Method, "GetClients", true) == 0)
                {
                    List<Client> clients = _ServerAPI.GetClients();
                    if (clients != null)
                    {
                        MemoryStream data = new MemoryStream(4096);
                        data.Position = 4;
                        IPCHelper.Write(data, clients.Count);
                        foreach (Client item in clients)
                        {
                            item.Serialise(data);                            
                        }
                        data.Position = 0;
                        NetworkByteOrderConverter.WriteInt32(data, (int)data.Length - 4);
                        response = data.ToArray();
                    }
                }
                else if (string.Compare(request.Method, "GetDeviceConnectedStatus", true) == 0)
				{
					Guid clientID = request.ReadGuid();
					DeviceConnectedStatus responseObject = _ServerAPI.GetDeviceConnectedStatus(clientID);
					if (responseObject != null)
					{
						MemoryStream data = new MemoryStream(4096);
						data.Position = 4;
						responseObject.Serialise(data);
						data.Position = 0;
						NetworkByteOrderConverter.WriteInt32(data, (int)data.Length - 4);
						response = data.ToArray();
					}
				}
				else if (string.Compare(request.Method, "GetObject", true) == 0)
				{
					Guid clientID = request.ReadGuid();
					Guid objectDefinitionID = request.ReadGuid();
					string instanceID = request.ReadString();
                    Model.Object responseObject = _ServerAPI.GetObject(clientID, objectDefinitionID, instanceID);
					if (responseObject != null)
					{
						MemoryStream data = new MemoryStream(4096);
						data.Position = 4;
						responseObject.Serialise(data);
						data.Position = 0;
						NetworkByteOrderConverter.WriteInt32(data, (int)data.Length - 4);
						response = data.ToArray();
					}
				}
                else if (string.Compare(request.Method, "GetObjectProperty", true) == 0)
				{
					Guid clientID = request.ReadGuid();
					Guid objectDefinitionID = request.ReadGuid();
					string instanceID = request.ReadString();
                    Guid propertyDefinitionID= request.ReadGuid();
                    Property responseProperty = _ServerAPI.GetObjectProperty(clientID, objectDefinitionID, instanceID, propertyDefinitionID);
                    if (responseProperty != null)
					{
						MemoryStream data = new MemoryStream(4096);
						data.Position = 4;
                        responseProperty.Serialise(data);
						data.Position = 0;
						NetworkByteOrderConverter.WriteInt32(data, (int)data.Length - 4);
						response = data.ToArray();
					}
				}     
				else if (string.Compare(request.Method, "GetObjects", true) == 0)
				{
					Guid clientID = request.ReadGuid();
					Guid objectDefinitionID = request.ReadGuid();
                    List<Model.Object> responseObjects = _ServerAPI.GetObjects(clientID, objectDefinitionID);
					if (responseObjects != null)
					{
						MemoryStream data = new MemoryStream(4096);
						data.Position = 4;
						foreach (Model.Object responseObject in responseObjects)
						{
							responseObject.Serialise(data);
						}
						data.Position = 0;
						NetworkByteOrderConverter.WriteInt32(data, (int)data.Length - 4);
						response = data.ToArray();
					}
				}
                else if (string.Compare(request.Method, "ObserveObject", true) == 0)
                {
                    Guid clientID = request.ReadGuid();
                    Guid objectDefinitionID = request.ReadGuid();
                    string instanceID = request.ReadString();
                    _ServerAPI.ObserveObject(clientID, objectDefinitionID, instanceID);
                }
                else if (string.Compare(request.Method, "ObserveObjectProperty", true) == 0)
                {
                    Guid clientID = request.ReadGuid();
                    Guid objectDefinitionID = request.ReadGuid();
                    string instanceID = request.ReadString();
                    Guid propertyDefinitionID = request.ReadGuid();
                    _ServerAPI.ObserveObjectProperty(clientID, objectDefinitionID, instanceID, propertyDefinitionID);
                }
                else if (string.Compare(request.Method, "ObserveObjects", true) == 0)
                {
                    Guid clientID = request.ReadGuid();
                    Guid objectDefinitionID = request.ReadGuid();
                    _ServerAPI.ObserveObjects(clientID, objectDefinitionID);
                }
                else if (string.Compare(request.Method, "SaveObject", true) == 0)
				{
					Guid clientID = request.ReadGuid();
                    Model.Object item = Model.Object.Deserialise(request.Payload);
					TObjectState state = (TObjectState)request.ReadInt32();
					string responseID = _ServerAPI.SaveObject(clientID, item, state);
					if (responseID != null)
					{
						MemoryStream data = new MemoryStream(4096);
						byte[] buffer = Encoding.UTF8.GetBytes(responseID);
						NetworkByteOrderConverter.WriteInt32(data, (int)(buffer.Length + 4));
						NetworkByteOrderConverter.WriteInt32(data, buffer.Length);
						data.Write(buffer, 0, buffer.Length);
						response = data.ToArray();
					}
				}
				else if (string.Compare(request.Method, "SaveObjectProperty", true) == 0)
				{
					Guid clientID = request.ReadGuid();
					Guid objectDefinitionID = request.ReadGuid();
					string instanceID = request.ReadString();
					Property property = Property.Deserialise(request.Payload);
					TObjectState state = (TObjectState)request.ReadInt32();
					_ServerAPI.SaveObjectProperty(clientID, objectDefinitionID, instanceID, property, state);
				}
                else if (string.Compare(request.Method, "SetDataFormat", true) == 0)
				{
					TDataFormat dataFormat = (TDataFormat)request.ReadInt32();
					_ServerAPI.SetDataFormat(dataFormat);
				}
                else if (string.Compare(request.Method, "SetNotificationParameters", true) == 0)
				{
                    Guid clientID = request.ReadGuid();
                    Guid objectDefinitionID = request.ReadGuid();
                    string instanceID = request.ReadString();
                    Guid propertyDefinitionID = request.ReadGuid();
                    NotificationParameters notificationParameters = NotificationParameters.Deserialise(request.Payload);
                    bool success = _ServerAPI.SetNotificationParameters(clientID, objectDefinitionID, instanceID, propertyDefinitionID, notificationParameters);
                    MemoryStream data = new MemoryStream(4096);
                    data.Position = 4;
                    IPCHelper.Write(data, success);
                    data.Position = 0;
                    NetworkByteOrderConverter.WriteInt32(data, (int)data.Length - 4);
                    response = data.ToArray();
				}
			}
			catch (Exception ex)
			{
				MemoryStream data = new MemoryStream(4096);
				data.Position = 4;
				IPCHelper.Write(data, ex.GetType().AssemblyQualifiedName);
				IPCHelper.Write(data, ex.Message);
				data.Position = 0;
				NetworkByteOrderConverter.WriteInt32(data, -((int)data.Length - 4));
				response = data.ToArray();
			}
			client.Send(response,SocketFlags.None);
		}

		public void Start()
		{
			_Terminate = false;
			bool socketOpened = false;
			IPEndPoint endPoint;
			try
			{               
				_Socket = new Socket(_AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                if (_AddressFamily == AddressFamily.InterNetworkV6)
                {
                    endPoint = new IPEndPoint(IPAddress.IPv6Any, _Port);
                    _Socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, true);
                }
                else
                {
                    endPoint = new IPEndPoint(IPAddress.Any, _Port);
                }

                BindSocket(_Socket, endPoint);
				socketOpened = true;
			}
			catch (SocketException ex)
			{
				ApplicationEventLog.WriteEntry(string.Concat("Socket error - ", ex.SocketErrorCode.ToString(), Environment.NewLine, ex.StackTrace), System.Diagnostics.EventLogEntryType.Warning);
			}
			if (!socketOpened)
			{
				throw new Exception("No socket opened");
			}
		}

		public void Stop()
		{
			_Terminate = true;
			if (_Socket != null)
			{
				//_Socket.Shutdown(SocketShutdown.Both);
				_Socket.Close();
				_Socket.Dispose();
				_Socket = null;
			}
		}

	}
}
