using Library.Api.Endpoints;
using Library.Application.Services;
using Library.Infrastructure;

using FluentValidation;
using Library.Application.Validators;

var builder = WebApplication.CreateBuilder(args);

// Fluent Validations
builder.Services.AddValidatorsFromAssemblyContaining<CreateBookRequestValidator>();

// builder.Configuration is appsettings already loaded and parsed
builder.Services.AddInfrastructure(builder.Configuration);

// registering the 3 services
builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<MemberService>();
builder.Services.AddScoped<BorrowingService>();

builder.Services.AddOpenApi();

var app =  builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Library API v1"));
}

app.MapBookEndpoints();
app.MapMemberEndpoints();
app.MapBorrowingEndpoints();

app.Run();