using System;

namespace Orleans.WebApi.Abstractions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class AspNetCoreAttribute : Attribute
    {
    }
}
