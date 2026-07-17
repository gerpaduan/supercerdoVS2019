using System;

namespace Web.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class SkipAppAntiForgeryValidationAttribute : Attribute
    {
    }
}
