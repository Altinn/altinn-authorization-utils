using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.Reflection;

namespace Altinn.Authorization.TestUtils.AspNetCore;

internal class TestControllerApplicationPart(TypeInfo type)
    : ApplicationPart
{
    public override string Name => $"{nameof(TestControllerApplicationPart)}:{type.FullName}";

    public TypeInfo Type => type;
}
