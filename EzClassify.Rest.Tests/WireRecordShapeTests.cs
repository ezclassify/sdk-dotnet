using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace EzClassify.Tests;

/// <summary>
/// Holds every public record to the additive shape: built with an object
/// initializer over init-only properties, never through a constructor
/// parameter list. A new optional member is then a new property, and code
/// compiled against an earlier release keeps building and binding.
/// </summary>
public class WireRecordShapeTests
{
    [Fact]
    public void EveryPublicRecordIsBuiltFromInitOnlyPropertiesAlone()
    {
        var problems = new List<string>();

        foreach (var record in PublicRecords())
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            foreach (var constructor in record.GetConstructors(flags).Where(c => c.IsPublic || c.IsFamily || c.IsFamilyOrAssembly))
            {
                var parameters = constructor.GetParameters();
                var copies = parameters.Length == 1 && parameters[0].ParameterType == record;
                if (parameters.Length > 0 && !copies)
                    problems.Add($"{record.Name} has a constructor that takes ({string.Join(", ", parameters.Select(p => p.Name))}).");
            }

            foreach (var property in record.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (property.Name == "EqualityContract") continue;
                if (property.SetMethod is { IsPublic: true } setter && !IsInitOnly(setter))
                    problems.Add($"{record.Name}.{property.Name} has a public set accessor.");
                if (property.SetMethod is null && !property.GetMethod!.IsAbstract)
                    problems.Add($"{record.Name}.{property.Name} cannot be set in an initializer.");
            }
        }

        Assert.True(problems.Count == 0, "Public records outside the additive shape:\n  " + string.Join("\n  ", problems));
    }

    [Fact]
    public void ARecordIsBuiltWithAnInitializerAndLeavesOptionalMembersNull()
    {
        var assertion = new Assertion
        {
            CategoryId = "level",
            ConceptId = "CONFIDENTIAL",
            Confidence = Confidence.Certain,
            Source = "user",
        };

        Assert.Null(assertion.Value);
        Assert.Equal(assertion, assertion with { });
        Assert.NotEqual(assertion, assertion with { Value = "x" });
    }

    private static IEnumerable<Type> PublicRecords() =>
        typeof(Assertion).Assembly.GetExportedTypes()
            .Where(type => type.GetMethod("<Clone>$") is not null)
            .OrderBy(type => type.Name, StringComparer.Ordinal);

    private static bool IsInitOnly(MethodInfo setter) =>
        setter.ReturnParameter.GetRequiredCustomModifiers().Any(modifier => modifier.Name == "IsExternalInit");
}
