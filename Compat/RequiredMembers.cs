// Shared compatibility shim, linked into every project rather than copied.
// The net8.0 targets use the BCL types; only netstandard2.0 needs the shim.
#if NETSTANDARD2_0
namespace System.Runtime.CompilerServices
{
    // C# 11 required members need these attributes, which ship in .NET 7+
    // but not in netstandard2.0. The compiler finds them by name.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    internal sealed class RequiredMemberAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    internal sealed class CompilerFeatureRequiredAttribute : Attribute
    {
        public CompilerFeatureRequiredAttribute(string featureName) => FeatureName = featureName;

        public string FeatureName { get; }

        public bool IsOptional { get; init; }
    }
}

namespace System.Diagnostics.CodeAnalysis
{
    // A record's copy constructor carries this attribute, because it sets
    // every required member from the record it copies.
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    internal sealed class SetsRequiredMembersAttribute : Attribute { }
}
#endif
