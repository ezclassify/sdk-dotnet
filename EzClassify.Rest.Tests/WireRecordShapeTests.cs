using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace EzClassify.Tests;

/// <summary>
/// Holds every public record to the additive model shape: a sealed record whose
/// one public constructor takes exactly the members a caller must supply, which
/// are read-only, while every optional member is a settable property. A later
/// optional member is then a new property that breaks no caller, and nothing in
/// the shape needs a compiler newer than C# 7.3 to build a record.
/// </summary>
public class WireRecordShapeTests
{
    [Fact]
    public void EveryPublicRecordTakesItsRequiredMembersThroughOnePublicConstructor()
    {
        var problems = new List<string>();

        foreach (var record in PublicRecords())
        {
            const BindingFlags declared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var properties = record.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(property => property.Name != "EqualityContract")
                .ToList();

            foreach (var property in properties)
            {
                if (property.SetMethod is { } setter && IsInitOnly(setter))
                    problems.Add($"{record.Name}.{property.Name} has an init accessor.");
                if (property.CustomAttributes.Any(attribute => attribute.AttributeType.Name == "RequiredMemberAttribute"))
                    problems.Add($"{record.Name}.{property.Name} is a required member.");
                if (property.SetMethod is { IsPublic: false })
                    problems.Add($"{record.Name}.{property.Name} has a set accessor that is not public.");
            }

            if (record.IsAbstract)
            {
                // A closed union: only this assembly derives from it. The
                // compiler's protected copy constructor is the one exception.
                if (record.GetConstructors(declared).Any(constructor => (constructor.IsPublic || constructor.IsFamily || constructor.IsFamilyOrAssembly)
                    && !(constructor.GetParameters() is [{ } only] && only.ParameterType == record)))
                    problems.Add($"{record.Name} is abstract but can be derived outside this assembly.");
                continue;
            }

            if (!record.IsSealed) problems.Add($"{record.Name} is not sealed.");

            var constructors = record.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (constructors.Length != 1)
            {
                problems.Add($"{record.Name} has {constructors.Length} public constructors.");
                continue;
            }

            var parameters = constructors[0].GetParameters();
            var readOnly = properties.Where(property => property.SetMethod is null).ToList();
            foreach (var property in readOnly)
            {
                var parameter = parameters.FirstOrDefault(p => string.Equals(p.Name, property.Name, StringComparison.OrdinalIgnoreCase));
                if (parameter is null)
                    problems.Add($"{record.Name}.{property.Name} is read-only but no constructor parameter sets it.");
                else if (parameter.ParameterType != property.PropertyType)
                    problems.Add($"{record.Name}({parameter.Name}) is {parameter.ParameterType.Name}, but {property.Name} is {property.PropertyType.Name}.");
            }
            foreach (var parameter in parameters.Where(p => readOnly.All(property => !string.Equals(p.Name, property.Name, StringComparison.OrdinalIgnoreCase))))
                problems.Add($"{record.Name}({parameter.Name}) sets no read-only member; an optional member belongs in a settable property.");
            foreach (var parameter in parameters.Where(p => p.IsOptional))
                problems.Add($"{record.Name}({parameter.Name}) is optional; adding a defaulted parameter later breaks compiled callers.");
        }

        Assert.True(problems.Count == 0, "Public records outside the additive shape:\n  " + string.Join("\n  ", problems));
    }

    [Fact]
    public void ARecordIsBuiltFromItsRequiredMembersAndLeavesOptionalMembersNull()
    {
        var assertion = new Assertion("level", "CONFIDENTIAL", Confidence.Certain, "user");

        Assert.Equal("level", assertion.CategoryId);
        Assert.Null(assertion.Value);
        Assert.Equal(assertion, new Assertion("level", "CONFIDENTIAL", Confidence.Certain, "user"));

        assertion.Value = "x";
        Assert.Equal("x", assertion.Value);
        Assert.NotEqual(assertion, new Assertion("level", "CONFIDENTIAL", Confidence.Certain, "user"));
    }

    [Fact]
    public void ARecordRefusesNullForARequiredReferenceMember()
    {
        var thrown = Assert.Throws<ArgumentNullException>(() => new Assertion("level", null!, Confidence.Certain, "user"));
        Assert.Equal("conceptId", thrown.ParamName);
    }

    private static IEnumerable<Type> PublicRecords() =>
        typeof(Assertion).Assembly.GetExportedTypes()
            .Where(type => type.GetMethod("<Clone>$") is not null)
            .OrderBy(type => type.Name, StringComparer.Ordinal);

    private static bool IsInitOnly(MethodInfo setter) =>
        setter.ReturnParameter.GetRequiredCustomModifiers().Any(modifier => modifier.Name == "IsExternalInit");
}
