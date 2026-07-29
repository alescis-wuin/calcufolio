using System.Reflection;

namespace Calcufolio.Domain.Tests.Architecture;

public sealed class DomainAssemblyTests
{
    [Fact]
    public void DomainAssemblyCanBeLoaded()
    {
        Assembly assembly = Assembly.Load(
            new AssemblyName("Calcufolio.Domain"));

        Assert.Equal(
            "Calcufolio.Domain",
            assembly.GetName().Name);
    }

    [Fact]
    public void DomainAssemblyDoesNotReferenceOtherCalcufolioAssemblies()
    {
        Assembly assembly = Assembly.Load(
            new AssemblyName("Calcufolio.Domain"));

        string[] projectReferences = assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .OfType<string>()
            .Where(name => name.StartsWith(
                "Calcufolio.",
                StringComparison.Ordinal))
            .ToArray();

        Assert.Empty(projectReferences);
    }
}
