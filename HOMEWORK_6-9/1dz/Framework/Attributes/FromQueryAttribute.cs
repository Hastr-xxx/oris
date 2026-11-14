using System;

namespace CustomHttpServer.Framework.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromQueryAttribute : Attribute { }
}