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
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using Imagination.Model;

namespace Imagination.DataAccess.LWM2M
{
	public class DALServers : IDALLWM2MServers
	{

		private class ServiceFactory
		{
			private Service.ILWM2MServerService[] _Services;
			private int _ServiceIndex;
			private int _MaxServiceCount;
			private string _Host;
			private int _Port;

			public ServiceFactory(string server)
			{
				Uri uri = new Uri(server);
				_Host = uri.DnsSafeHost;
				_Port = uri.Port;
				_MaxServiceCount = 10;
				_Services = new Service.ILWM2MServerService[_MaxServiceCount];
			}

			public Service.ILWM2MServerService CreateChannel()
			{
				int index;
				lock (this)
				{
					index = _ServiceIndex = (_ServiceIndex + 1) % _MaxServiceCount;
				}
				if (_Services[index] == null)
				{
					_Services[index] = new NativeIPCClient(_Host, _Port);
				}
				return _Services[index];
			}


			public void RemoveService(Service.ILWM2MServerService service)
			{
				for (int index = 0; index < _MaxServiceCount; index++)
				{
					if (_Services[index] == service)
					{
						_Services[index] = null;
					}
				}
			}
		}

		private ConcurrentDictionary<string, ServiceFactory> _ChannelFactories;

		public DALServers()
		{
			_ChannelFactories = new ConcurrentDictionary<string, ServiceFactory>(PlatformHelper.DefaultConcurrencyLevel, 10);
		}

		private Service.ILWM2MServerService GetService(string server)
		{
			ServiceFactory serviceFactory;
			if (!_ChannelFactories.TryGetValue(server, out serviceFactory))
			{
				serviceFactory = new ServiceFactory(server);
				_ChannelFactories.TryAdd(server, serviceFactory);
			}
			return serviceFactory.CreateChannel();
		}

        //public List<Client> GetClients()
        //{
        //    List<Client> result = null;
        //    Service.ILWM2MServerService service = GetService("tcp://localhost:14080"); // TODO: replace this with call to DB
        //    lock (service)
        //    {
        //        result = service.GetClients();
        //    }
        //    return result;
        //}

        public void DeleteClient(Client client)
        {
            Service.ILWM2MServerService service = GetService(client.Server);
            lock (service)
            {
                service.DeleteClient(client.ClientID);
            }
        }

        public bool Execute(Client client, Guid objectDefinitionID, string instanceID, Model.Property property)
        {
            bool result = false;
            Service.ILWM2MServerService service = GetService(client.Server);
            lock (service)
            {
                result = service.ExecuteResource(client.ClientID, objectDefinitionID, instanceID, property.PropertyDefinitionID);
            }
            return result;
        }

        public DeviceConnectedStatus GetDeviceConnectedStatus(Client client)
		{
			DeviceConnectedStatus result = null;
			Service.ILWM2MServerService service = GetService(client.Server);
			lock (service)
			{
				result = service.GetDeviceConnectedStatus(client.ClientID);
			}
			return result;
		}

		public Imagination.Model.Object GetObject(Client client, Guid objectDefinitionID, string instanceID)
		{
            Imagination.Model.Object result = null;
			Service.ILWM2MServerService service = GetService(client.Server);
			lock (service)
			{
				result = service.GetObject(client.ClientID, objectDefinitionID, instanceID);
			}
			return result;
		}

		public List<Imagination.Model.Object> GetObjects(Client client, Guid objectDefinitionID)
		{
			List<Imagination.Model.Object> result = null;
			Service.ILWM2MServerService service = GetService(client.Server);
			lock (service)
			{
				result = service.GetObjects(client.ClientID, objectDefinitionID);
			}
			return result;
		}

		public void SaveObject(Client client, Imagination.Model.Object lwm2mObject, Model.TObjectState state)
		{
			Service.ILWM2MServerService service = GetService(client.Server);
			lock (service)
			{
				string instanceID = service.SaveObject(client.ClientID, lwm2mObject, state);
				if (!string.IsNullOrEmpty(instanceID))
				{
					lwm2mObject.InstanceID = instanceID;
				}
			}
		}

		public void SaveObjectProperty(Client client, Guid objectDefinitionID, string instanceID, Property property, Model.TObjectState state)
		{
			Service.ILWM2MServerService service = GetService(client.Server);
			lock (service)
			{
				service.SaveObjectProperty(client.ClientID, objectDefinitionID, instanceID, property, state);
			}
		}

        public void ObserveObjects(Client client, Guid objectDefinitionID)
        {
            Service.ILWM2MServerService service = GetService(client.Server);
            lock (service)
            {
                service.ObserveObjects(client.ClientID, objectDefinitionID);
            }
        }

        public void ObserveObject(Client client, Guid objectDefinitionID, string instanceID)
        {
            Service.ILWM2MServerService service = GetService(client.Server);
            lock (service)
            {
                service.ObserveObject(client.ClientID, objectDefinitionID, instanceID);
            }
        }

        public void ObserveResource(Client client, Guid objectDefinitionID, string instanceID, Guid propertyDefinitionID)
        {
            Service.ILWM2MServerService service = GetService(client.Server);
            lock (service)
            {
                service.ObserveObjectProperty(client.ClientID, objectDefinitionID, instanceID, propertyDefinitionID);
            }
        }

        public void CancelObserveObjects(Client client, Guid objectDefinitionID, bool useReset)
        {
            Service.ILWM2MServerService service = GetService(client.Server);
            lock (service)
            {
                service.CancelObserveObjects(client.ClientID, objectDefinitionID, useReset);
            }
        }

        public void CancelObserveObject(Client client, Guid objectDefinitionID, string instanceID, bool useReset)
        {
            Service.ILWM2MServerService service = GetService(client.Server);
            lock (service)
            {
                service.CancelObserveObject(client.ClientID, objectDefinitionID, instanceID, useReset);
            }
        }

        public void CancelObserveResource(Client client, Guid objectDefinitionID, string instanceID, Guid propertyDefinitionID, bool useReset)
        {
            Service.ILWM2MServerService service = GetService(client.Server);
            lock (service)
            {
                service.CancelObserveObjectProperty(client.ClientID, objectDefinitionID, instanceID, propertyDefinitionID, useReset);
            }
        }

        public bool SetNotificationParameters(Client client, Guid objectDefinitionID, string instanceID, Guid propertyDefinitionID, NotificationParameters notificationParameters)
        {
            bool setNotificationParameters = false;
            Service.ILWM2MServerService service = GetService(client.Server);
            lock (service)
            {
                setNotificationParameters = service.SetNotificationParameters(client.ClientID, objectDefinitionID, instanceID, propertyDefinitionID, notificationParameters);
            }
            return setNotificationParameters;
        }
    }
}
