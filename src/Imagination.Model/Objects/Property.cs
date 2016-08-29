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
using System.Runtime.Serialization;

namespace Imagination.Model
{

    public class Property
	{

        public Guid PropertyDefinitionID { get; set; }

        public string PropertyID { get; set; }
		
		public PropertyValue Value { get; set; }
		
		public List<PropertyValue> Values { get; set; }


		public void Serialise(System.IO.Stream stream)
		{
			IPCHelper.Write(stream, PropertyDefinitionID);
			IPCHelper.Write(stream, PropertyID);
			if (Value == null)
				IPCHelper.Write(stream, (byte)0);
			else
			{
				IPCHelper.Write(stream, (byte)1);
				IPCHelper.Write(stream, Value.Value);
			}
			if (Values == null)
				IPCHelper.Write(stream, (int)-1);
			else
			{
				IPCHelper.Write(stream, Values.Count);
				foreach (PropertyValue itemValue in Values)
				{
					IPCHelper.Write(stream, itemValue.PropertyValueID);
					IPCHelper.Write(stream, itemValue.Value);
				}
			}
		}

		public static Property Deserialise(System.IO.Stream stream)
		{
			Property property = new Property();
			property.PropertyDefinitionID = IPCHelper.ReadGuid(stream);
			property.PropertyID = IPCHelper.ReadString(stream);
			byte nullItem = IPCHelper.ReadByte(stream);
            if (nullItem == 1)
            {
                property.Value = new PropertyValue() { Value = IPCHelper.ReadString(stream) };
			}
			int valueCount = IPCHelper.ReadInt32(stream);
			for (int valueIndex = 0; valueIndex < valueCount; valueIndex++)
			{
				PropertyValue value = new PropertyValue();
				value.PropertyValueID = IPCHelper.ReadString(stream);
				value.Value = IPCHelper.ReadString(stream);
				if (property.Values == null)
					property.Values = new List<PropertyValue>();
				property.Values.Add(value);
			}
			return property;
		}

	}
}
