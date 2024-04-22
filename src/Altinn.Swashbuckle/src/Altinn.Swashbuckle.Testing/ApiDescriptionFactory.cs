using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Linq.Expressions;
using System.Reflection;

namespace Altinn.Swashbuckle.Testing;

public static class ApiDescriptionFactory
{
    public static ApiDescription Create(
        ActionDescriptor actionDescriptor,
        MethodInfo methodInfo,
        string groupName = "v1",
        string httpMethod = "POST",
        string relativePath = "resource",
        IEnumerable<ApiParameterDescription>? parameterDescriptions = null,
        IEnumerable<ApiRequestFormat>? supportedFormats = null,
        IEnumerable<ApiResponseType>? supportedResponseTypes = null)
    {
        var apiDescription = new ApiDescription
        {
            ActionDescriptor = actionDescriptor,
            GroupName = groupName,
            HttpMethod = httpMethod,
            RelativePath = relativePath,
        };

        foreach (var parameter in parameterDescriptions ?? Enumerable.Empty<ApiParameterDescription>())
        {
            // If the provided action has a matching parameter - use it to assign ParameterDescriptor & ModelMetadata
            parameter.ParameterDescriptor = actionDescriptor.Parameters
                .OfType<ParameterDescriptor>()
                .FirstOrDefault(p => p.Name == parameter.Name)!;

            if (parameter.ParameterDescriptor is IParameterInfoParameterDescriptor parameterDescriptorWithParameterInfo)
            {
                parameter.ModelMetadata = ModelMetadataFactory.CreateForParameter(parameterDescriptorWithParameterInfo.ParameterInfo);
            }

            apiDescription.ParameterDescriptions.Add(parameter);
        }

        foreach (var requestFormat in supportedFormats ?? Enumerable.Empty<ApiRequestFormat>())
        {
            apiDescription.SupportedRequestFormats.Add(requestFormat);
        }

        foreach (var responseType in supportedResponseTypes ?? Enumerable.Empty<ApiResponseType>())
        {
            // If the provided action has a return value AND the response status is 2XX - use it to assign ModelMetadata
            if (methodInfo.ReturnType is not null && responseType.StatusCode is >= 200 and < 300)
            {
                responseType.ModelMetadata = ModelMetadataFactory.CreateForType(methodInfo.ReturnType);
            }

            apiDescription.SupportedResponseTypes.Add(responseType);
        }

        return apiDescription;
    }

    public static ApiDescription Create(
        MethodInfo methodInfo,
        string groupName = "v1",
        string httpMethod = "POST",
        string relativePath = "resource",
        IEnumerable<ApiParameterDescription>? parameterDescriptions = null,
        IEnumerable<ApiRequestFormat>? supportedFormats = null,
        IEnumerable<ApiResponseType>? supportedResponseTypes = null)
    {
        var actionDescriptor = CreateActionDescriptorFor(methodInfo);

        return Create(
            actionDescriptor,
            methodInfo,
            groupName,
            httpMethod,
            relativePath,
            parameterDescriptions,
            supportedFormats,
            supportedResponseTypes);

        static ActionDescriptor CreateActionDescriptorFor(MethodInfo methodInfo)
        {
            var httpMethodAttribute = methodInfo.GetCustomAttribute<HttpMethodAttribute>();
            var attributeRouteInfo = httpMethodAttribute is not null
                ? new AttributeRouteInfo { Template = httpMethodAttribute.Template, Name = httpMethodAttribute.Name }
                : null;

            var parameterDescriptor = methodInfo.GetParameters()
                .Select(CreateParameterDescriptor)
                .ToList();

            var routeValues = new Dictionary<string, string?>
            {
                ["controller"] = RemoveControllerSuffix(methodInfo.DeclaringType!.Name),
                ["action"] = methodInfo.Name,
            };

            return new ControllerActionDescriptor
            {
                AttributeRouteInfo = attributeRouteInfo,
                ControllerTypeInfo = methodInfo.DeclaringType!.GetTypeInfo(),
                ControllerName = methodInfo.DeclaringType!.Name,
                MethodInfo = methodInfo,
                Parameters = parameterDescriptor,
                RouteValues = routeValues,
            };
        }

        static ParameterDescriptor CreateParameterDescriptor(ParameterInfo parameterInfo)
        {
            return new ControllerParameterDescriptor
            {
                Name = parameterInfo.Name!,
                ParameterInfo = parameterInfo,
                ParameterType = parameterInfo.ParameterType,
            };
        }

        static string RemoveControllerSuffix(string controllerName)
        {
            if (controllerName.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
            {
                return controllerName[..^"Controller".Length];
            }

            return controllerName;
        }
    }

    public static ControllerApiDescriptionFactory<TController> ForController<TController>()
        => new();

    public readonly struct ControllerApiDescriptionFactory<TController>
    {
        public ApiDescription Create(
            Expression<Func<TController, Task>> action,
            string groupName = "v1",
            string httpMethod = "POST",
            string relativePath = "resource",
            IEnumerable<ApiParameterDescription>? parameterDescriptions = null,
            IEnumerable<ApiRequestFormat>? supportedFormats = null,
            IEnumerable<ApiResponseType>? supportedResponseTypes = null)
        {
            var methodInfo = GetMethodInfo(action);

            return ApiDescriptionFactory.Create(
                methodInfo,
                groupName,
                httpMethod,
                relativePath,
                parameterDescriptions,
                supportedFormats,
                supportedResponseTypes);
        }

        public ApiDescription Create<T1>(
            Expression<Func<TController, T1, Task>> action,
            string groupName = "v1",
            string httpMethod = "POST",
            string relativePath = "resource",
            IEnumerable<ApiParameterDescription>? parameterDescriptions = null,
            IEnumerable<ApiRequestFormat>? supportedFormats = null,
            IEnumerable<ApiResponseType>? supportedResponseTypes = null)
        {
            var methodInfo = GetMethodInfo(action);

            return ApiDescriptionFactory.Create(
                methodInfo,
                groupName,
                httpMethod,
                relativePath,
                parameterDescriptions,
                supportedFormats,
                supportedResponseTypes);
        }

        public ApiDescription Create<T1, T2>(
            Expression<Func<TController, T1, T2, Task>> action,
            string groupName = "v1",
            string httpMethod = "POST",
            string relativePath = "resource",
            IEnumerable<ApiParameterDescription>? parameterDescriptions = null,
            IEnumerable<ApiRequestFormat>? supportedFormats = null,
            IEnumerable<ApiResponseType>? supportedResponseTypes = null)
        {
            var methodInfo = GetMethodInfo(action);

            return ApiDescriptionFactory.Create(
                methodInfo,
                groupName,
                httpMethod,
                relativePath,
                parameterDescriptions,
                supportedFormats,
                supportedResponseTypes);
        }

        public ApiDescription Create<T1, T2, T3>(
            Expression<Func<TController, T1, T2, T3, Task>> action,
            string groupName = "v1",
            string httpMethod = "POST",
            string relativePath = "resource",
            IEnumerable<ApiParameterDescription>? parameterDescriptions = null,
            IEnumerable<ApiRequestFormat>? supportedFormats = null,
            IEnumerable<ApiResponseType>? supportedResponseTypes = null)
        {
            var methodInfo = GetMethodInfo(action);

            return ApiDescriptionFactory.Create(
                methodInfo,
                groupName,
                httpMethod,
                relativePath,
                parameterDescriptions,
                supportedFormats,
                supportedResponseTypes);
        }

        public ApiDescription Create<T1, T2, T3, T4>(
            Expression<Func<TController, T1, T2, T3, T4, Task>> action,
            string groupName = "v1",
            string httpMethod = "POST",
            string relativePath = "resource",
            IEnumerable<ApiParameterDescription>? parameterDescriptions = null,
            IEnumerable<ApiRequestFormat>? supportedFormats = null,
            IEnumerable<ApiResponseType>? supportedResponseTypes = null)
        {
            var methodInfo = GetMethodInfo(action);

            return ApiDescriptionFactory.Create(
                methodInfo,
                groupName,
                httpMethod,
                relativePath,
                parameterDescriptions,
                supportedFormats,
                supportedResponseTypes);
        }

        public ApiDescription Create<T1, T2, T3, T4, T5>(
            Expression<Func<TController, T1, T2, T3, T4, T5, Task>> action,
            string groupName = "v1",
            string httpMethod = "POST",
            string relativePath = "resource",
            IEnumerable<ApiParameterDescription>? parameterDescriptions = null,
            IEnumerable<ApiRequestFormat>? supportedFormats = null,
            IEnumerable<ApiResponseType>? supportedResponseTypes = null)
        {
            var methodInfo = GetMethodInfo(action);

            return ApiDescriptionFactory.Create(
                methodInfo,
                groupName,
                httpMethod,
                relativePath,
                parameterDescriptions,
                supportedFormats,
                supportedResponseTypes);
        }

        public ApiDescription Create<T1, T2, T3, T4, T5, T6>(
            Expression<Func<TController, T1, T2, T3, T4, T5, T6, Task>> action,
            string groupName = "v1",
            string httpMethod = "POST",
            string relativePath = "resource",
            IEnumerable<ApiParameterDescription>? parameterDescriptions = null,
            IEnumerable<ApiRequestFormat>? supportedFormats = null,
            IEnumerable<ApiResponseType>? supportedResponseTypes = null)
        {
            var methodInfo = GetMethodInfo(action);

            return ApiDescriptionFactory.Create(
                methodInfo,
                groupName,
                httpMethod,
                relativePath,
                parameterDescriptions,
                supportedFormats,
                supportedResponseTypes);
        }

        private static MethodInfo GetMethodInfo(Expression expression)
        {
            if (expression is LambdaExpression lambdaExpression)
            {
                expression = lambdaExpression.Body;
            }

            if (expression is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
            {
                expression = unaryExpression.Operand;
            }

            if (expression is MethodCallExpression methodCallExpression)
            {
                return methodCallExpression.Method;
            }

            throw new ArgumentException("Invalid expression", nameof(expression));
        }
    }
}
