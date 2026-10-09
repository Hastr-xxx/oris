using System;

namespace CustomHttpServer.Framework.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HttpControllerAttribute : Attribute
    {
        public string Route { get; }
        public HttpControllerAttribute(string route) => Route = route;
    }
}