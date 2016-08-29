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
using System.Linq;
using System.Text;
using Imagination.Model;

namespace Imagination.DataAccess.LWM2M
{
	public interface IDALLWM2MServers
	{
        void CancelObserveObjects(Client client, Guid objectDefinitionID, bool useReset);

        void CancelObserveObject(Client client, Guid objectDefinitionID, string instanceID, bool useReset);

        void CancelObserveResource(Client client, Guid objectDefinitionID, string instanceID, Guid propertyDefinitionID, bool useReset);

        //List<Client> GetClients();

        DeviceConnectedStatus GetDeviceConnectedStatus(Client client);

        void DeleteClient(Client client);

        bool Execute(Client client, Guid objectDefinitionID, string instanceID, Model.Property property);

        Imagination.Model.Object GetObject(Client client, Guid objectDefinitionID, string instanceID);

		List<Imagination.Model.Object> GetObjects(Client client, Guid objectDefinitionID);

        void ObserveObjects(Client client, Guid objectDefinitionID);

        void ObserveObject(Client client, Guid objectDefinitionID, string instanceID);

        void ObserveResource(Client client, Guid objectDefinitionID, string instanceID, Guid propertyDefinitionID);

        void SaveObject(Client client, Imagination.Model.Object lwm2mObject, Model.TObjectState state);

		void SaveObjectProperty(Client client, Guid objectDefinitionID, string instanceID, Property property, Model.TObjectState state);

        bool SetNotificationParameters(Client client, Guid objectDefinitionID, string instanceID, Guid propertyDefinitionID, NotificationParameters notificationParameters);
    }
}
