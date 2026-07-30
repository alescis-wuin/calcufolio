using System.Reflection;

namespace Calcufolio.Application.Tests.Architecture;

public sealed class ApplicationAssemblyTests
{
    [Fact]
    public void ApplicationAssemblyCanBeLoaded()
    {
        Assembly assembly = Assembly.Load(
            new AssemblyName("Calcufolio.Application"));

        Assert.Equal(
            "Calcufolio.Application",
            assembly.GetName().Name);
    }

    [Fact]
    public void ApplicationAssemblyDoesNotReferenceOuterLayers()
    {
        Assembly assembly = Assembly.Load(
            new AssemblyName("Calcufolio.Application"));

        string[] projectReferences = assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .OfType<string>()
            .Where(name => name.StartsWith(
                "Calcufolio.",
                StringComparison.Ordinal))
            .ToArray();

        Assert.DoesNotContain(
            "Calcufolio.Infrastructure",
            projectReferences);

        Assert.DoesNotContain(
            "Calcufolio.Presentation",
            projectReferences);
    }
}
