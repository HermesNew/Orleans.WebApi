using System;

namespace Orleans.WebApi.Abstractions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class MapToWebApiAttribute : Attribute
    {
        public MapToWebApiAttribute(Type igrainType, string? grainClassNamePrefix = default, string? controllerName = default)
        {
            this.IGrainType = igrainType;
            this.GrainClassNamePrefix = grainClassNamePrefix;
            this.ControllerName = controllerName;
        }

        public string? ControllerName { get; }

        public Type IGrainType { get; }

        public string? GrainClassNamePrefix { get; }
    }
}
