using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics.CodeAnalysis;

namespace MovieCategories.Api;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Method)]
public class SwaggerHeaderAttribute : Attribute
{
}

[ExcludeFromCodeCoverage]
public class SwaggerHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var actionAttributes = context.MethodInfo.GetCustomAttributes(true);

        if (actionAttributes.OfType<SwaggerHeaderAttribute>().Any())
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "email",
                In = ParameterLocation.Header,
                Description = "Email header",
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = "string"
                }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "password",
                In = ParameterLocation.Header,
                Description = "Password header",
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = "string"
                }
            });
        }
    }
}