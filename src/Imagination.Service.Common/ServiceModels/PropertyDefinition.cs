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
using System.Threading.Tasks;

namespace Imagination.ServiceModels
{
    [ContentType("application/vnd.imgtec.propertydefinition")]
    public class PropertyDefinition
    {

        public string PropertyDefinitionID { get; set; }

        public string PropertyID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string DataType { get; set; }

        public int? DataTypeLength { get; set; }

        public string MIMEType { get; set; }

        public string MinValue { get; set; }

        public string MaxValue { get; set; }

        public string Units { get; set; }

        public bool? IsCollection { get; set; }

        public bool? IsMandatory { get; set; }

        public string Access { get; set; }

        public int? SortOrder { get; set; }

        public string SerialisationName { get; set; }

        public string CollectionItemSerialisationName { get; set; }

        public PropertyDefinition()
        {

        }

        public PropertyDefinition(Model.PropertyDefinition property)
        {
            PropertyDefinitionID = StringUtils.GuidEncode(property.PropertyDefinitionID);
            PropertyID = property.PropertyID;
            Name = property.Name;
            Description = property.Description;
            DataType = property.DataType.ToString();
            DataTypeLength = property.DataTypeLength;
            MIMEType = property.MIMEType;
            MinValue = property.MinValue;
            MaxValue = property.MaxValue;
            Units = property.Units;
            IsCollection = property.IsCollection;
            IsMandatory = property.IsMandatory;
            Access = property.Access.ToString();
            SortOrder = property.SortOrder;
            SerialisationName = property.SerialisationName;
            CollectionItemSerialisationName = property.CollectionItemSerialisationName;
        }

        public Model.PropertyDefinition ToModel()
        {
            Model.PropertyDefinition result = new Model.PropertyDefinition();
            if (!string.IsNullOrEmpty(PropertyDefinitionID))
                result.PropertyDefinitionID = StringUtils.GuidDecode(PropertyDefinitionID);
            result.PropertyID = PropertyID;
            result.Name = Name;
            result.Description = Description;
            Model.TPropertyDataType dataType;
            if (Enum.TryParse(DataType, true, out dataType))
                result.DataType = dataType;
            result.DataTypeLength = DataTypeLength;
            result.MIMEType = MIMEType;
            result.MinValue = MinValue;
            result.MaxValue = MaxValue;
            result.Units = Units;
            if (IsCollection.HasValue)
                result.IsCollection = IsCollection.Value;
            if (IsMandatory.HasValue)
                result.IsMandatory = IsMandatory.Value;
            Model.TAccessRight access;
            if (Enum.TryParse(Access, true, out access))
                result.Access = access;
            result.SortOrder = SortOrder;
            result.SerialisationName = SerialisationName;
            result.CollectionItemSerialisationName = CollectionItemSerialisationName;
            return result;
        }

        public void UpdateModel(Model.PropertyDefinition item)
        {
            if (!string.IsNullOrEmpty(PropertyID))
                item.PropertyID = PropertyID;
            if (!string.IsNullOrEmpty(Name))
                item.Name = Name;
            if (!string.IsNullOrEmpty(Description))
                item.Description = Description;
            if (!string.IsNullOrEmpty(DataType))
            {
                Model.TPropertyDataType dataType;
                if (Enum.TryParse(DataType, true, out dataType))
                    item.DataType = dataType;
            }
            if (DataTypeLength.HasValue)
                item.DataTypeLength = DataTypeLength;
            if (!string.IsNullOrEmpty(MIMEType))
                item.MIMEType = MIMEType;
            if (!string.IsNullOrEmpty(MinValue))
                item.MinValue = MinValue;
            if (!string.IsNullOrEmpty(MaxValue))
                item.MaxValue = MaxValue;
            if (!string.IsNullOrEmpty(Units))
                item.Units = Units;
            if (IsCollection.HasValue)
                item.IsCollection = IsCollection.Value;
            if (IsMandatory.HasValue)
                item.IsMandatory = IsMandatory.Value;
            if (!string.IsNullOrEmpty(Access))
            {
                Model.TAccessRight access;
                if (Enum.TryParse(Access, true, out access))
                    item.Access = access;
            }
            if (SortOrder.HasValue)
                item.SortOrder = SortOrder;
            if (!string.IsNullOrEmpty(SerialisationName))
                item.SerialisationName = SerialisationName;
            if (!string.IsNullOrEmpty(CollectionItemSerialisationName))
                item.CollectionItemSerialisationName = CollectionItemSerialisationName;
        }
    }
}
