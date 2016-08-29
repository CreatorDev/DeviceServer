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
using Imagination.Model;
using Imagination.DataAccess;

namespace Imagination.BusinessLogic
{
    internal class ObjectDefinitions
    {
        public ObjectDefinitionLookups GetLookups()
        {
            return DataAccessFactory.ObjectDefinitions.GetLookups();
        }

        public ObjectDefinition GetObjectDefinition(int organisationID, Guid objectDefinitionID)
        {
            ObjectDefinition result = null;
            ObjectDefinitionLookups lookups = GetLookups();
            ObjectDefinition objectDefinition = lookups.GetObjectDefinition(objectDefinitionID);
            if (objectDefinition != null && (!objectDefinition.OrganisationID.HasValue || (objectDefinition.OrganisationID.Value == organisationID)))
                result = objectDefinition;
            return result;
        }

        public List<ObjectDefinition> GetObjectDefinitions(int organisationID)
        {
            List<ObjectDefinition> result = new List<ObjectDefinition>();
            ObjectDefinitionLookups lookups = GetLookups();
            Dictionary<Guid, object> alreadyAdded = new Dictionary<Guid, object>();
            foreach (ObjectDefinition item in lookups.DefaultObjectDefinitions)
            {
                ObjectDefinition objectDefinition = lookups.GetObjectDefinition(organisationID, item.ObjectID);
                result.Add(objectDefinition);
                alreadyAdded.Add(objectDefinition.ObjectDefinitionID, null);
            }
            List<ObjectDefinition> objectDefinitions = lookups.GetObjectDefinitions(organisationID);
            if (objectDefinitions != null)
            {
                foreach (ObjectDefinition item in objectDefinitions)
                {
                    if (!alreadyAdded.ContainsKey(item.ObjectDefinitionID))
                        result.Add(item);
                }
            }
            result.Sort(SortObjectDefinition);
            return result;
        }

        public void SaveObjectDefinition(ObjectDefinition objectDefinition, TObjectState state)
        {
            List<ObjectDefinition> objectDefinitions = new List<ObjectDefinition>();
            objectDefinitions.Add(objectDefinition);
            SaveObjectDefinitions(objectDefinitions, state);
        }

        public void SaveObjectDefinitions(List<ObjectDefinition> objectDefinitions, TObjectState state)
        {
            if (state == TObjectState.Add)
            {
                ObjectDefinitionLookups lookups = GetLookups();
                foreach (ObjectDefinition item in objectDefinitions)
                {
                    int organisationID = 0;
                    if (item.OrganisationID.HasValue)
                        organisationID = item.OrganisationID.Value;
                    ObjectDefinition existingObjectDefinition = lookups.GetObjectDefinition(organisationID, item.ObjectID);
                    if (existingObjectDefinition != null) 
                    {
                        int existingOrganisationID = 0;
                        if (existingObjectDefinition.OrganisationID.HasValue)
                            existingOrganisationID = existingObjectDefinition.OrganisationID.Value;
                        if (organisationID == existingOrganisationID)
                            throw new ConflictException();
                    }
                }
            }
            DataAccessFactory.ObjectDefinitions.SaveObjectDefinitions(objectDefinitions, state);
        }

        private static int SortObjectDefinition(ObjectDefinition x, ObjectDefinition y)
        {
            int result = 0;
            int xValue = 0;
            int yValue = 0;
            if (x != null)
                int.TryParse(x.ObjectID, out xValue);
            if (y != null)
                int.TryParse(y.ObjectID, out yValue);
            result = xValue.CompareTo(yValue);
            return result;
        }
    }
}
