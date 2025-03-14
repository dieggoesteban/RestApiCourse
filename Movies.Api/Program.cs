using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application;
using Movies.Application.Database;
using System.Text;

namespace Movies.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;
            // Add services to the container.

            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Movies:JwtKey"]!)),
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateAudience = true,
                    ValidateIssuer = true
                };
            });

            builder.Services.AddAuthorization(x =>
            {
                x.AddPolicy(AuthConstants.AdminUserPolicyName, policy => policy.RequireClaim(AuthConstants.AdminUserClaimName, "true"));
            });

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddApplication();
            builder.Services.AddDatabase(configuration["Movies:ConnectionString"]!);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<ValidationMappingMiddleware>();

            app.MapControllers();

            var dbInitializer = app.Services.GetRequiredService<DbInitializer>();
            await dbInitializer.InitializeAsync();

            app.Run();
        }
    }
}
