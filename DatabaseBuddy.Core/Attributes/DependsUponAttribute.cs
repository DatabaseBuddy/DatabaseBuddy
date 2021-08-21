using System;

namespace DatabaseBuddy.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    public class DependsUponAttribute : Attribute
    {
        public DependsUponAttribute(string propertyName)
        {
            DependencyName = propertyName;
        }

        public string DependencyName { get; private set; }
    }
}
