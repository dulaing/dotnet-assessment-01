using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Library.Api.OpenApi
{
    // Overrides global bearer security for explicitly anonymous operations.
    public sealed class AnonymousOperationTransformer : IOpenApiOperationTransformer
    {
        // An empty security list means the operation does not require authentication.
        public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
        {
            if (context.Description.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any())
            {
                operation.Security = [];
            }

            return Task.CompletedTask;
        }
    }
}
