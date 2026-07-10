using Library.Api.Endpoints;
using Library.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("LibraryDatabase")
    ?? throw new InvalidOperationException("Connection string 'LibraryDatabase' was not found.");

builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

app.UseHttpsRedirection();

app.MapBookEndpoints();
app.MapMemberEndpoints();
app.MapBorrowingEndpoints();

app.Run();
