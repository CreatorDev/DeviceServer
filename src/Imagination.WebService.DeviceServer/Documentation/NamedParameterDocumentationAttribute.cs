using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imagination.Documentation
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class NamedParameterDocumentationAttribute : Attribute
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public TNamedParameterType Type { get; set; }
        public string Description { get; set; }

        public NamedParameterDocumentationAttribute(string name, string displayName, TNamedParameterType type, string description)
        {
            Name = name;
            DisplayName = displayName;
            Type = type;
            Description = description;
        }
    }
}
