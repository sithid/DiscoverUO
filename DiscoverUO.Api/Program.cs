using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DiscoverUO.Api.Controllers;

namespace DiscoverUO.Api
{
    public class Program
    {
        private static string secretKey;

        public static void Main(string[] args)
        {
            secretKey = GenerateSecretKey();
            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", secretKey);

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAutoMapper(typeof(Program).Assembly);

            builder.Services.AddDbContext<DiscoverUODatabaseContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DiscoverUOConnection")));

            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                };
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("BasicUser", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(ClaimTypes.Role, "BasicUser") ||
                        context.User.HasClaim(ClaimTypes.Role, "Owner") ||
                        context.User.HasClaim(ClaimTypes.Role, "Admin") ||
                        context.User.HasClaim(ClaimTypes.Role, "Moderator")));

                options.AddPolicy("Privileged", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(ClaimTypes.Role, "Owner") ||
                        context.User.HasClaim(ClaimTypes.Role, "Admin") ||
                        context.User.HasClaim(ClaimTypes.Role, "Moderator")));
                options.AddPolicy("OwnerOrAdmin", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(ClaimTypes.Role, "Owner") ||
                        context.User.HasClaim(ClaimTypes.Role, "Admin")));
                options.AddPolicy("Owner", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(ClaimTypes.Role, "Owner")));
            });

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddControllers();

            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,

                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }

        public static string GenerateSecretKey(int length = 64)
        {
            var bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}
