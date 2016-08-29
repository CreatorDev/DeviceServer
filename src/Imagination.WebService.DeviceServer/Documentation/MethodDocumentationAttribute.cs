using System;
using System.Net;

namespace Imagination.Documentation
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class MethodDocumentationAttribute : Attribute
    {
        public string Summary { get; set; }
        public string[] RequestTypeNames { get; set; }
        public Type[] RequestTypes { get; set; }
        public Type[] ResponseTypes { get; set; }
        public HttpStatusCode[] StatusCodes { get; set; }
        public bool AllowMultipleSecuritySchemes { get; set; }
    }
}
