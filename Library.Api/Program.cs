using FluentValidation;
using Library.Api.Application.Abstractions;
using Library.Api.Application.Interfaces;
using Library.Api.Application.Services;
using Library.Api.Application.Validation;
using Library.Api.Endpoints;
using Library.Api.Infrastructure.CurrentUser;
using Library.Api.Infrastructure.Data;
using Library.Api.Infrastructure.ExceptionHandling;
using Library.Api.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("LibraryDatabase")
    ?? throw new InvalidOperationException("Connection string 'LibraryDatabase' was not found.");

builder.Services.AddValidatorsFromAssemblyContaining<CreateBookRequestValidator>();
builder.Services.AddSingleton<ICurrentUserService, SystemCurrentUserService>();
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IBorrowingRepository, BorrowingRepository>();
builder.Services.AddScoped<IBorrowingService, BorrowingService>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Library API v1");
        options.RoutePrefix = "swagger";
    });
}
else
{
    app.UseHttpsRedirection();
}

app.MapBookEndpoints();
app.MapMemberEndpoints();
app.MapBorrowingEndpoints();

app.Run();
