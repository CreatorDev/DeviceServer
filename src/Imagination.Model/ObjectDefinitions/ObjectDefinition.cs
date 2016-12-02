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
	public class ObjectDefinition : ModelBase
	{
		public Guid ObjectDefinitionID { get; set; }
        public int? OrganisationID { get; set; }
        public string ObjectID { get; set; }
        public string Name { get; set; }
		public string MIMEType { get; set; }
		public string Description { get; set; }
		public string SerialisationName { get; set; }
		public bool Singleton { get; set; }
		public List<PropertyDefinition> Properties { get; set; }

		public PropertyDefinition GetProperty(string propertyID)
		{
			PropertyDefinition result = null;
			if (Properties != null)
			{
				foreach (PropertyDefinition item in Properties)
				{
					if (string.Compare(item.PropertyID, propertyID, true) == 0)
					{
						result = item;
						break;
					}
				}
			}
			return result;
		}

		public PropertyDefinition GetProperty(Guid propertyDefinitionID)
		{
			PropertyDefinition result = null;
			if (Properties != null)
			{
				foreach (PropertyDefinition item in Properties)
				{
					if (item.PropertyDefinitionID == propertyDefinitionID)
					{
						result = item;
						break;
					}
				}
			}
			return result;
		}

		public PropertyDefinition GetPropertyBySerialisationName(string serialisationName)
		{
			PropertyDefinition result = null;
			if (Properties != null)
			{
				foreach (PropertyDefinition item in Properties)
				{
					if (string.Compare(item.SerialisationName, serialisationName, true) == 0)
					{
						result = item;
						break;
					}
				}
			}
			return result;
		}

        public PropertyDefinition GetPropertyBySerialisationNameOrID(string nameOrID)
        {
            PropertyDefinition propertyDefinition = GetProperty(nameOrID);

            if (propertyDefinition == null)
                propertyDefinition = GetPropertyBySerialisationName(nameOrID);

            return propertyDefinition;
        }
    }
}
