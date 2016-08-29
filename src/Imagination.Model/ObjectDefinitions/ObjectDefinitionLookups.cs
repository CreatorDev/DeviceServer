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

namespace Imagination.Model
{
	public class ObjectDefinitionLookups
	{
	
		private Dictionary<Guid,ObjectDefinition> _ObjectDefinitions;
		private List<ObjectDefinition> _ObjectDefinitionList;

        public List<ObjectDefinition> DefaultObjectDefinitions { get { return _DefaultObjectDefinitionList; } }

        public List<ObjectDefinition> ObjectDefinitions { get { return _ObjectDefinitionList; } }

        private List<ObjectDefinition> _DefaultObjectDefinitionList;
        private Dictionary<string, ObjectDefinition> _DefaultObjectDefinitions;
        private Dictionary<int, List<ObjectDefinition>> _ObjectDefinitionsByOrganisation;
        private Dictionary<int, Dictionary<string,ObjectDefinition>> _ObjectDefinitionByOrganisation;


        public ObjectDefinitionLookups()
		{
			_ObjectDefinitions = new Dictionary<Guid, ObjectDefinition>();
			_ObjectDefinitionList = new List<ObjectDefinition>();
            _DefaultObjectDefinitionList = new List<ObjectDefinition>();
            _DefaultObjectDefinitions = new Dictionary<string, ObjectDefinition>();
            _ObjectDefinitionByOrganisation = new Dictionary<int, Dictionary<string, ObjectDefinition>>();
            _ObjectDefinitionsByOrganisation = new Dictionary<int, List<ObjectDefinition>>();

        }

        public void AddObjectDefinition(ObjectDefinition objectDefinition)
		{
			_ObjectDefinitionList.Add(objectDefinition);
			_ObjectDefinitions.Add(objectDefinition.ObjectDefinitionID, objectDefinition);
            if (!string.IsNullOrEmpty(objectDefinition.ObjectID))
            {
                Dictionary<string, ObjectDefinition> objectDefinitions;
                List<ObjectDefinition> objectDefinitionList;
                if (objectDefinition.OrganisationID.HasValue)
                {
                    if (!_ObjectDefinitionByOrganisation.TryGetValue(objectDefinition.OrganisationID.Value, out objectDefinitions))
                    {
                        objectDefinitions = new Dictionary<string, ObjectDefinition>();
                        _ObjectDefinitionByOrganisation.Add(objectDefinition.OrganisationID.Value, objectDefinitions);
                    }
                    if (!_ObjectDefinitionsByOrganisation.TryGetValue(objectDefinition.OrganisationID.Value, out objectDefinitionList))
                    {
                        objectDefinitionList = new List<ObjectDefinition>();
                        _ObjectDefinitionsByOrganisation.Add(objectDefinition.OrganisationID.Value, objectDefinitionList);
                    }
                }
                else
                {
                    objectDefinitions = _DefaultObjectDefinitions;
                    objectDefinitionList = _DefaultObjectDefinitionList;
                }
                objectDefinitionList.Add(objectDefinition);
                objectDefinitions.Add(objectDefinition.ObjectID, objectDefinition);
            }
        }

        public ObjectDefinition GetObjectDefinition(int organisationID, string objectID)
		{
			ObjectDefinition result = null;
            if (!string.IsNullOrEmpty(objectID))
            {
                Dictionary<string, ObjectDefinition> objectDefinitions;
                if (_ObjectDefinitionByOrganisation.TryGetValue(organisationID, out objectDefinitions))
                {
                    objectDefinitions.TryGetValue(objectID, out result);
                }
                if (result == null)
                {
                    _DefaultObjectDefinitions.TryGetValue(objectID, out result);
                }
            }
			return result;
		}

		public ObjectDefinition GetObjectDefinition(Guid objectDefinitionID)
		{
			ObjectDefinition result = null;
			_ObjectDefinitions.TryGetValue(objectDefinitionID, out result);
			return result;
		}

        public List<ObjectDefinition> GetObjectDefinitions(int organisationID)
        {
            List<ObjectDefinition> result;
            _ObjectDefinitionsByOrganisation.TryGetValue(organisationID, out result);
            return result;
        }

        public PropertyDefinition GetPropertyDefinitionFromNameOrID(Guid objectDefinitionID, string propertyNameOrID)
        {
            ObjectDefinition objectDefinition = GetObjectDefinition(objectDefinitionID);
            PropertyDefinition propertyDefinition = null;
            if (objectDefinition != null)
            {
                propertyDefinition = objectDefinition.GetPropertyBySerialisationNameOrID(propertyNameOrID);
            }

            return propertyDefinition;
        }
    }
}
