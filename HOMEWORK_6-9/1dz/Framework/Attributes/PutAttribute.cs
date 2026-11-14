using System;

namespace CustomHttpServer.Framework.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PutAttribute : Attribute
    {
        public string Route { get; }
        public PutAttribute(string route) => Route = route;
    }
}