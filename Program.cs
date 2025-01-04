
using LotniskoAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Identity;
using LotniskoAPI.Controllers;
using LotniskoAPI.Models;
namespace LotniskoAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            // Add services to the container.
            builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<User, Role>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
            
            //builder.Services.AddScoped<IUserStore<User>, UserStore>();

            var jwtSettings = builder.Configuration.GetSection("JwtSettings");

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            //builder.Services.AddIdentity<User, Role>()
            //.AddUserStore<UserStore>()
            //.AddRoleStore<RoleStore>()
            //.AddDefaultTokenProviders();

            
            builder.Services.AddControllers();


            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]))
                    };
                    options.Validate();
                });
            //builder.Services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
            //    options.AddPolicy("User", policy => policy.RequireRole("User"));
            //});

            //builder.Services.AddAuthentication();
            builder.Services.AddAuthorization();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Lotnisko", Version = "v1" });

    //            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    //            {
    //                Name = "Authorization",
    //                Type = SecuritySchemeType.ApiKey,
    //                Scheme = "Bearer",
    //                BearerFormat = "JWT",
    //                In = ParameterLocation.Header,
    //                Description = "Enter 'Bearer' followed by your token."
    //            });

    //            options.AddSecurityRequirement(new OpenApiSecurityRequirement
    //{
    //    {
    //        new OpenApiSecurityScheme
    //        {
    //            Reference = new OpenApiReference
    //            {
    //                Type = ReferenceType.SecurityScheme,
    //                Id = "Bearer"
    //            }
    //        },
    //        new string[] { }
    //    }
    //});
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHttpsRedirection();


            app.MapControllers();

            app.Run();
        }
    }
}
