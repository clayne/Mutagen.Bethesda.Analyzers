using System.Reflection;
using AutoFixture.Kernel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Mutagen.Bethesda.Analyzers.Testing.AutoFixture;

public class NullLoggerBuilder : ISpecimenBuilder
{
    public object? Create(object request, ISpecimenContext context)
    {
        if (request is not Type t || !t.GetTypeInfo().IsGenericType || t.GetGenericTypeDefinition() != typeof(ILogger<>))
        {
            return new NoSpecimen();
        }

        var nullType = typeof(NullLogger<>).MakeGenericType(t.GenericTypeArguments);
        return Activator.CreateInstance(nullType);
    }
}
