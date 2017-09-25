using System;

namespace InSearch
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ObjectSignatureAttribute : Attribute { }
}
