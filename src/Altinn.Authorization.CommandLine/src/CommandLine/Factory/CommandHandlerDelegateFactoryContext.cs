using System.CommandLine;
using System.Linq.Expressions;
using System.Reflection;
using Altinn.Authorization.CommandLine.Results;
using Altinn.Authorization.CommandLine.XmlDoc;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Authorization.CommandLine.Factory;

internal sealed class CommandHandlerDelegateFactoryContext
{
    // Options
    public required IServiceProvider ServiceProvider { get; init; }
    public required IXmlDocProvider XmlDocProvider { get; init; }
    public required CommandResultHandler ResultHandler { get; init; }
    public required IServiceProviderIsService? ServiceProviderIsService { get; init; }
    public List<Option> Options { get; } = new List<Option>();
    public List<Argument> Arguments { get; } = new List<Argument>();

    // Temporary State
    public List<ParameterExpression> ExtraLocals { get; } = new();
    public List<Expression> ResultAssignments { get; } = new();
    public List<Expression> ResultCheckExpressions { get; } = new();
    public List<Expression> ParamAssignments { get; } = new();
    public List<Expression> ParamCheckExpressions { get; } = new();
    public NullabilityInfoContext NullabilityContext { get; } = new();
    public Type[] ArgumentTypes { get; set; } = [];
    public Expression[]? ArgumentExpressions { get; set; }
    public Expression[] BoxedArgs { get; set; } = [];

    public List<ParameterInfo> Parameters { get; set; } = new();
}
