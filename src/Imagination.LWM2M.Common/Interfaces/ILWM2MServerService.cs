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
using System.Text;
using Imagination.Model;

namespace Imagination.Service
{
	public interface ILWM2MServerService
	{
        void CancelObserveObject(Guid clientID, Guid objectDefinitionID, string instanceID, bool useReset);

        void CancelObserveObjectProperty(Guid clientID, Guid objectDefinitionID, string instanceID, Guid propertyDefinitionID, bool useReset);

        void CancelObserveObjects(Guid clientID, Guid objectDefinitionID, bool useReset);

        bool ExecuteResource(Guid clientID, Guid objectDefinitionID, string instanceID, Guid propertyDefinitionID);

        List<Client> GetClients();

        void DeleteClient(Guid clientID);

        DeviceConnectedStatus GetDeviceConnectedStatus(Guid clientID);

        Model.Object GetObject(Guid clientID, Guid objectDefinitionID, string instanceID);

        Property GetObjectProperty(Guid clientID, Guid objectDefinitionID, string instanceID, Guid propertyDefinitionID);
        
		List<Model.Object> GetObjects(Guid clientID, Guid objectDefinitionID);

        void ObserveObject(Guid clientID, Guid objectDefinitionID, string instanceID);

        void ObserveObjectProperty(Guid clientID, Guid objectDefinitionID, string instanceID, Guid propertyDefinitionID);

        void ObserveObjects(Guid clientID, Guid objectDefinitionID);

        string SaveObject(Guid clientID, Model.Object item, TObjectState state);

		void SaveObjectProperty(Guid clientID, Guid objectDefinitionID, string instanceID, Property property, Model.TObjectState state);

        void SetDataFormat(TDataFormat dataFormat);

        bool SetNotificationParameters(Guid clientID, Guid objectDefinitionID, string instanceID, Guid propertyDefinitionID, NotificationParameters notificationParameters);
	}
}
