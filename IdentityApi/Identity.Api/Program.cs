using System.Data;
using FluentValidation;
using FluentValidation.AspNetCore;
using Identity.Api.Dto;
using Identity.Api.Middleware;
using Identity.Application.Interfaces;
using Identity.Application.User;
using Identity.Infrastructure;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

/*
 * No using default ASP.NET Core Identity, in order to not generate stuff using EF.
 */

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();

// Application Services
builder.Services.AddScoped<IDbConnection, SqlConnection>(provider => new SqlConnection(config.GetConnectionString("MoviesDBConnStr")!));
builder.Services.AddScoped<IApplicationUserRepo, ApplicationUserRepo>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();
builder.Services.AddScoped<IApplicationUserService, ApplicationUserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Register the custom exception middleware
app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program{}
