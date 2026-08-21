using Library.Api.Endpoints;
using Library.Application.Services;
using Library.Infrastructure;
using Library.Api.Handlers;

using FluentValidation;
using Library.Application.Validators;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Fluent Validations
builder.Services.AddValidatorsFromAssemblyContaining<CreateBookRequestValidator>();

// builder.Configuration is appsettings already loaded and parsed
builder.Services.AddInfrastructure(builder.Configuration);

// register the global exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// registering the 3 services
builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<MemberService>();
builder.Services.AddScoped<BorrowingService>();

builder.Services.AddOpenApi();

var app =  builder.Build();

// global exception handler
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Library API v1"));
}

app.MapBookEndpoints();
app.MapMemberEndpoints();
app.MapBorrowingEndpoints();

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

app.Run();