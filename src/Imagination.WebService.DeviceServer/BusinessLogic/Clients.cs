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
using Imagination.DataAccess;

namespace Imagination.BusinessLogic
{
	internal class Clients
	{
        public List<Client> GetClients(int organisationID)
        {
            return DataAccessFactory.Clients.GetClients(organisationID);
        }

        public List<Client> GetConnectedClients(int organisationID)
        {
            return DataAccessFactory.Clients.GetConnectedClients(organisationID);
        }

        public Client GetClient(Guid clientID)
		{
			Client result = null;
			if (clientID != Guid.Empty)
				result = DataAccessFactory.Clients.GetClient(clientID);
			return result;			
		}

        public void DeleteClient(Client client)
        {
            DataAccessFactory.Clients.SaveBlacklistedClient(client, TObjectState.Add);
            //DataAccessFactory.Metrics.DeleteMetrics(client);
            DataAccessFactory.Servers.DeleteClient(client);
        }

        public void Execute(Client client, Imagination.Model.Object lwm2mObject, List<Model.Property> properties)
        {
            foreach (Model.Property item in properties)
            {
                DataAccessFactory.Servers.Execute(client, lwm2mObject.ObjectDefinitionID, lwm2mObject.InstanceID, item);
            }
        }
        
        public Imagination.Model.Object GetObject(Client client, Guid objectDefinitionID, string instanceID)
		{
			return DataAccessFactory.Servers.GetObject(client, objectDefinitionID, instanceID);
		}

		public List<Imagination.Model.Object> GetObjects(Client client, Guid objectDefinitionID)
		{
			return DataAccessFactory.Servers.GetObjects(client, objectDefinitionID);
		}

        public void SaveObject(Client client, Imagination.Model.Object lwm2mObject, Model.TObjectState state)
        {
            DataAccessFactory.Servers.SaveObject(client, lwm2mObject, state);
        }

        public void SaveObjectProperty(Client client, Guid objectDefinitionID, string instanceID, Property property, Model.TObjectState state)
        {
            DataAccessFactory.Servers.SaveObjectProperty(client, objectDefinitionID, instanceID, property, state);
        }
    }
}
