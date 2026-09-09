using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Library.Api.OpenApi
{
    // Adds JWT bearer authentication to the generated OpenAPI document.
    public sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
    {
        // Registers the scheme and makes it available to every documented operation.
        public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            const string schemeName = "Bearer";
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes[schemeName] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter the access token returned by POST /api/auth/login."
            };

            document.Security ??= [];
            document.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(schemeName, document)] = []
            });

            return Task.CompletedTask;
        }
    }
}
