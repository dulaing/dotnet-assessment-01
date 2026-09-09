using Library.Api.Endpoints;
using Library.Application.Services;
using Library.Infrastructure;
using Library.Api.Handlers;
using Library.Api.OpenApi;
using Library.Infrastructure.Persistence;

using FluentValidation;
using Library.Api.Extensions;
using Library.Application.Validators;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// wiring to Aspire
builder.AddServiceDefaults();

// Fluent Validations
builder.Services.AddValidatorsFromAssemblyContaining<CreateBookRequestValidator>();

// builder.Configuration is appsettings already loaded and parsed
// AddInfrastructure comes from DependencyInjection
builder.Services.AddInfrastructure(builder.Configuration);

// register the global exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// registering the 3 services
builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<MemberService>();
builder.Services.AddScoped<BorrowingService>();

builder.Services.AddOpenApi(options =>
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();

// validates the bearer token on every incoming request
builder.Services.AddJwtAuthentication(builder.Configuration);

var app =  builder.Build();

// global exception handler
app.UseExceptionHandler();

// order matters: work out who the caller is, then decide what they may do
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Library API v1"));
}

app.MapBookEndpoints();
app.MapMemberEndpoints();
app.MapBorrowingEndpoints();

app.MapAuthEndpoints();
app.MapUserEndpoints();

// no checks at all, so this only proves the process is up and answering
app.MapHealthChecks("/alive", new HealthCheckOptions { Predicate = _ => false });

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description
            })
        });
    }
}); 

await app.Services.SeedAdminAsync();

app.Run();
