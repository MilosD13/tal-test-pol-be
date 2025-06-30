using Dapper;
using PollLibrary.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.Sqlite;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PoolApi.StartupConfig;
using System.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization(opts =>
{
    opts.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
SqlMapper.AddTypeHandler<Guid>(new GuidTypeHandler());


builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration.GetValue<string>("Authentication:Issuer"),
            ValidAudience = builder.Configuration.GetValue<string>("Authentication:Audience"),
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(
                builder.Configuration.GetValue<string>("Authentication:SecretKey")!))
        };
    });

// Add this in order to talk to FE locally
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.AddStandardServices();

string connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=polls.db;";
builder.Services.AddTransient<IDbConnection>(_ => new SqliteConnection(connectionString));
builder.Services.AddScoped<IPollRepository, PollRepository>();

using (var conn = new SqliteConnection(connectionString))
{
    conn.Open();
    conn.Execute(@"
        CREATE TABLE IF NOT EXISTS Poll (
            Id       TEXT    PRIMARY KEY,
            Question TEXT    NOT NULL
        );
        CREATE TABLE IF NOT EXISTS Option (
            Id     INTEGER PRIMARY KEY AUTOINCREMENT,
            PollId TEXT    NOT NULL,
            Text   TEXT    NOT NULL,
            Votes  INTEGER NOT NULL DEFAULT 0,
            FOREIGN KEY(PollId) REFERENCES Poll(Id)
        );
    ");
}
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
        // serve Swagger UI at /swagger  
    app.UseSwaggerUI(c =>
        {
       c.SwaggerEndpoint("/swagger/v1/swagger.json", "Poll API v1");
        c.RoutePrefix = "swagger";
            });
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
