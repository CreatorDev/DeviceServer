using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imagination.Documentation
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RouteDocumentationAttribute: Attribute
    {
        public string Route { get; set; }
        public string DisplayName { get; set; }
        public string Summary { get; set; }
    }
}
