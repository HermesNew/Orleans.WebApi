using System;

namespace Orleans.WebApi.Abstractions
{

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RouteAttribute : Microsoft.AspNetCore.Mvc.RouteAttribute
    {
        public RouteAttribute(string template) : base(template)
        {

        }
    }
}
