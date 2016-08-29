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
using System.Net;
using System.Net.Sockets;
using System.Text;
using Imagination.LWM2M;
using Imagination.Model;
using System.Threading;

namespace Imagination.DataAccess.LWM2M
{
	internal class NativeIPCClient : Service.ILWM2MServerService
	{
		private class ReceiveStateObject
		{
			public Socket Client = null;
			public const int BufferSize = 256;
			public byte[] Buffer = new byte[BufferSize];
			public DateTime StartReceive;
			public MemoryStream Data = new MemoryStream(4096);
			public int RequestLength;
			public bool Error;
		}

		private string _Host;
		private int _Port;

		private Socket _TCPClient;
		private AsyncCallback _ReceiveCallback;
		private bool _Connected;

		public bool _ErrorResponse;
		private byte[] _ResponseBytes;
		private ManualResetEvent _ResponseWaitHandle = new ManualResetEvent(false);

		public NativeIPCClient(string hostname, int port)
		{
			_Host = hostname;
			_Port = port;
			_ReceiveCallback = new AsyncCallback(ReceiveCallback);
		}


		private bool Connect()
		{
			bool result = false;
			try
			{
				Socket tcpClient = null;
				IPAddress[] hostAddresses = Dns.GetHostAddresses(_Host);
				foreach (IPAddress address in hostAddresses)
				{
					if ((address.AddressFamily == AddressFamily.InterNetwork) || (address.AddressFamily == AddressFamily.InterNetworkV6))
					{
						tcpClient = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
						tcpClient.Connect(address, _Port);
						tcpClient.NoDelay = true;
						break;
					}
				}
				ReceiveStateObject stateObject = new ReceiveStateObject();
				stateObject.Client = tcpClient;
				SocketError socketError;
				tcpClient.BeginReceive(stateObject.Buffer, 0, ReceiveStateObject.BufferSize, SocketFlags.None, out socketError, _ReceiveCallback, stateObject);
				_TCPClient = tcpClient;
				result = true;
			}
			catch
			{

			}
			return result;
		}

        public void CancelObserveObject(Guid clientID, Guid objectDefinitionID, string instanceID, bool useReset)
        {
            IPCRequest request = new IPCRequest();
            request.Method = "CancelObserveObject";
            request.AddToPayload(clientID);
            request.AddToPayload(objectDefinitionID);
            request.AddToPayload(instanceID);
            request.AddToPayload(useReset);
            SendRequest(request);
        }

        public void CancelObserveObjectProperty(Guid clientID, Guid objectDefinitionID, string instanceID, Guid propertyMetadataID, bool useReset)
        {
            IPCRequest request = new IPCRequest();
            request.Method = "CancelObserveObjectProperty";
            request.AddToPayload(clientID);
            request.AddToPayload(objectDefinitionID);
            request.AddToPayload(instanceID);
            request.AddToPayload(propertyMetadataID);
            request.AddToPayload(useReset);
            SendRequest(request);
        }

        public void CancelObserveObjects(Guid clientID, Guid objectDefinitionID, bool useReset)
        {
            IPCRequest request = new IPCRequest();
            request.Method = "CancelObserveObjects";
            request.AddToPayload(clientID);
            request.AddToPayload(objectDefinitionID);
            request.AddToPayload(useReset);
            SendRequest(request);
        }

        public void DeleteClient(Guid clientID)
        {
            IPCRequest request = new IPCRequest();
            request.Method = "DeleteClient";
            request.AddToPayload(clientID);
            SendRequest(request);
        }

        public bool ExecuteResource(Guid clientID, Guid objectDefinitionID, string instanceID, Guid propertyMetadataID)
        {
            bool result = false;
            IPCRequest request = new IPCRequest();
            request.Method = "ExecuteResource";
            request.AddToPayload(clientID);
            request.AddToPayload(objectDefinitionID);
            request.AddToPayload(instanceID);
            request.AddToPayload(propertyMetadataID);
            SendRequest(request);
            if (_ResponseBytes.Length > 0)
            {
                result = _ResponseBytes[0] != 0x00;
            }
            return result;
        }

        public List<Client> GetClients()
        {
            List<Client> result = null;
            IPCRequest request = new IPCRequest();
            request.Method = "GetClients";
            SendRequest(request);
            if (_ResponseBytes.Length > 0)
            {
                result = new List<Client>();
                MemoryStream stream = new MemoryStream(_ResponseBytes);
                int count = IPCHelper.ReadInt32(stream);
                if (count > 0)
                {
                    for (int index = 0; index < count; index++)
                    {                        
                        result.Add(Client.Deserialise(stream));
                    }
                }
            }
            return result;
        }
        
		public DeviceConnectedStatus GetDeviceConnectedStatus(Guid clientID)
		{
			DeviceConnectedStatus result = null;
			IPCRequest request = new IPCRequest();
			request.Method = "GetDeviceConnectedStatus";
			request.AddToPayload(clientID);
			SendRequest(request);
			if (_ResponseBytes.Length > 0)
				result = DeviceConnectedStatus.Deserialise(new MemoryStream(_ResponseBytes));
			return result; 
		}

		public Imagination.Model.Object GetObject(Guid clientID, Guid objectDefinitionID, string instanceID)
		{
            Imagination.Model.Object result = null;
			IPCRequest request = new IPCRequest();
			request.Method = "GetObject";
			request.AddToPayload(clientID);
			request.AddToPayload(objectDefinitionID);
			request.AddToPayload(instanceID);
			SendRequest(request);
			if (_ResponseBytes.Length > 0)
				result = Imagination.Model.Object.Deserialise(new MemoryStream(_ResponseBytes));
			return result; 
		}


        public Property GetObjectProperty(Guid clientID, Guid objectDefinitionID, string instanceID, Guid propertyMetadataID)
        {
            Property result = null;
            IPCRequest request = new IPCRequest();
            request.Method = "GetObjectProperty";
            request.AddToPayload(clientID);
            request.AddToPayload(objectDefinitionID);
            request.AddToPayload(instanceID);
            request.AddToPayload(propertyMetadataID);
            SendRequest(request);
            if (_ResponseBytes.Length > 0)
                result = Property.Deserialise(new MemoryStream(_ResponseBytes));
            return result;
        }

		private void SendRequest(IPCRequest request)
		{
			if (!_Connected)
			{
				if (!Connect())
				{
					
                    throw new SocketException();
				}
				_Connected = true;
			}
			byte[] data = request.Serialise();
			_ResponseBytes = null;
			_ResponseWaitHandle.Reset();
			_TCPClient.Send(data, SocketFlags.None);
			_ResponseWaitHandle.WaitOne(20000);
			if (_ResponseBytes == null)
				throw new TimeoutException();
		}

		public List<Imagination.Model.Object> GetObjects(Guid clientID, Guid objectDefinitionID)
		{
			List<Imagination.Model.Object> result = new List<Imagination.Model.Object>();
			IPCRequest request = new IPCRequest();
			request.Method = "GetObjects";
			request.AddToPayload(clientID);
			request.AddToPayload(objectDefinitionID);
			SendRequest(request);
			if (_ResponseBytes.Length > 0)
			{
				MemoryStream stream = new MemoryStream(_ResponseBytes);
				while (stream.Position < stream.Length)
					result.Add(Imagination.Model.Object.Deserialise(stream));
			}
			return result;
		}

        public void ObserveObject(Guid clientID, Guid objectDefinitionID, string instanceID)
        {
            IPCRequest request = new IPCRequest();
            request.Method = "ObserveObject";
            request.AddToPayload(clientID);
            request.AddToPayload(objectDefinitionID);
            request.AddToPayload(instanceID);
            SendRequest(request);
        }

        public void ObserveObjectProperty(Guid clientID, Guid objectDefinitionID, string instanceID, Guid propertyMetadataID)
        {
            IPCRequest request = new IPCRequest();
            request.Method = "ObserveObjectProperty";
            request.AddToPayload(clientID);
            request.AddToPayload(objectDefinitionID);
            request.AddToPayload(instanceID);
            request.AddToPayload(propertyMetadataID);
            SendRequest(request);
        }

        public void ObserveObjects(Guid clientID, Guid objectDefinitionID)
        {
            IPCRequest request = new IPCRequest();
            request.Method = "ObserveObjects";
            request.AddToPayload(clientID);
            request.AddToPayload(objectDefinitionID);
            SendRequest(request);
        }

        private void ReceiveCallback(IAsyncResult asyncResult)
		{
			ReceiveStateObject stateObject = asyncResult.AsyncState as ReceiveStateObject;
			try
			{
				int readCount = stateObject.Client.EndReceive(asyncResult);
				bool continueReceive = true;
				if (readCount > 0)
				{
					stateObject.Data.Write(stateObject.Buffer, 0, readCount);
					if ((stateObject.RequestLength == 0) && (stateObject.Data.Length >= 4))
					{
						long position = stateObject.Data.Position;
						stateObject.Data.Position = 0;
						int requestLength = NetworkByteOrderConverter.ToInt32(stateObject.Data);
						stateObject.RequestLength = Math.Abs(requestLength) + 4;
						stateObject.Error = requestLength < 0;
						stateObject.Data.Position = position;
					}
					if (stateObject.RequestLength == stateObject.Data.Position)
					{
						_ErrorResponse = stateObject.Error;
						_ResponseBytes = new byte[stateObject.RequestLength-4];
						stateObject.Data.Position = 4;
						stateObject.Data.Read(_ResponseBytes,0,_ResponseBytes.Length);						
						stateObject.Data.Position = 0;
						stateObject.Data.SetLength(0);
						stateObject.RequestLength = 0;
						_ResponseWaitHandle.Set();
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
					stateObject.Client.BeginReceive(stateObject.Buffer, 0, ReceiveStateObject.BufferSize, SocketFlags.None, out socketError, _ReceiveCallback, stateObject);
				}
				else
				{
					_Connected = false;
					_ResponseWaitHandle.Set();
					stateObject.Client.Close();
				}
			}
#pragma warning disable 168
            catch (Exception ex)
#pragma warning restore 168
            {
                _Connected = false;
				_ResponseWaitHandle.Set();
				stateObject.Client.Close();
				
			}
        }
        
        public string SaveObject(Guid clientID, Imagination.Model.Object item, Model.TObjectState state)
		{
			string result = null;
			IPCRequest request = new IPCRequest();
			request.Method = "SaveObject";
			request.AddToPayload(clientID);
			item.Serialise(request.Payload);
			request.AddToPayload((int)state);
			SendRequest(request);
			if (_ResponseBytes.Length > 0)
			{
				CheckError();
				result = Encoding.UTF8.GetString(_ResponseBytes, 4, _ResponseBytes.Length - 4);
			}
			return result;
		}

		private void CheckError()
		{
			if (_ErrorResponse)
			{
				using(MemoryStream stream = new MemoryStream(_ResponseBytes))
				{
					string typeName = IPCHelper.ReadString(stream);
					string message = IPCHelper.ReadString(stream);
					Exception exception;
					try
					{
						Type type = Type.GetType(typeName);
						exception = (Exception)Activator.CreateInstance(type, message);
					}
					catch
					{
						exception = new Exception(message);
					}
					throw exception;
				}
			}
		}

		public void SaveObjectProperty(Guid clientID, Guid objectDefinitionID, string instanceID, Property property, Model.TObjectState state)
		{
			IPCRequest request = new IPCRequest();
			request.Method = "SaveObjectProperty";
			request.AddToPayload(clientID);
			request.AddToPayload(objectDefinitionID);
			request.AddToPayload(instanceID);
			property.Serialise(request.Payload);
			request.AddToPayload((int)state);
			SendRequest(request);
		}

        public void SetDataFormat(TDataFormat dataFormat)
        {
            IPCRequest request = new IPCRequest();
            request.Method = "SetDataFormat";
            request.AddToPayload((int)dataFormat);
            SendRequest(request);
        }

        public bool SetNotificationParameters(Guid clientID, Guid objectDefinitionID, string instanceID, Guid propertyMetadataID, NotificationParameters notificationParameters)
        {
            bool result = false;
            IPCRequest request = new IPCRequest();
            request.Method = "SetNotificationParameters";
            request.AddToPayload(clientID);
            request.AddToPayload(objectDefinitionID);
            request.AddToPayload(instanceID);
            request.AddToPayload(propertyMetadataID);
            notificationParameters.Serialise(request.Payload);
            SendRequest(request);
            if (_ResponseBytes.Length > 0)
            {
                result = _ResponseBytes[0] != 0x00;
            }
            return result;
        }

	}
}
