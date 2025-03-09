// Add the namespace for repositories
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Template.DTOs;
using Template.Helpers;
using Template.Models;
using Template.Models.Roles;
using Template.Repository;
using Template.Repository.Interfaces;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();


// TODO: Remove the Enum and  use Roles as a string better

#region Configure Swagger with JWT authentication
// 🔹 Configure Swagger with JWT authentication
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EdufyAPI API", Version = "v1" });

    // Add JWT support
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
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
            new string[] {}
        }
    });
});

#endregion

// 🔹 Add the connection string to the configuration
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseLazyLoadingProxies()
        .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});


#region Identity

// 🔹 Configure Identity with roles
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// 🔹 Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"]
    };
});

#endregion


#region Register services
// 🔹 Register UnitOfWork and Generic Repository for dependency injection

// This ensures that:
//A new instance of UnitOfWork(and consequently DbContext) is created per request.
//Once the request is complete, the instance is disposed of properly. This is important to prevent memory leaks.
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddAutoMapper(typeof(AutoMapperProfile)); // Register AutoMapper
builder.Services.AddLogging(); // This ensures logging is available
#endregion


# region Cores

// 🔹 Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

/*
  // 🔹 Configure CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
});
    * Don't forget to edit the appsettings.json file to include the AllowedOrigins key
 */
#endregion

builder.Services.AddScoped<FileUploadHelper>();

var app = builder.Build();

app.UseStaticFiles();   // It enables requests to the wwwroot folder, making images accessible via URL.

#region Middleware Configuration
// 🔹 Configure middleware

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// 🔹 Enable Authentication & Authorization middleware
app.UseAuthentication(); // Must come before Authorization
app.UseAuthorization();

app.MapControllers();

#endregion

app.Run();