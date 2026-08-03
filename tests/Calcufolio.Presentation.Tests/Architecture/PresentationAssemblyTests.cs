using System.Reflection;
using Calcufolio.Presentation.ViewModels;

namespace Calcufolio.Presentation.Tests.Architecture;

public sealed class PresentationAssemblyTests
{
    [Fact]
    public void PresentationAssemblyCanBeLoaded()
    {
        Assembly assembly = typeof(MainViewModel).Assembly;

        Assert.Equal(
            "Calcufolio.Presentation",
            assembly.GetName().Name);
    }

    [Fact]
    public void PresentationAssemblyDoesNotReferenceInfrastructure()
    {
        Assembly assembly = typeof(MainViewModel).Assembly;

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
    }
}
