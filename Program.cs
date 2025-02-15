using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Template.Models;
using Template.Repository;
using Template.Repository.Interfaces; // Add the namespace for repositories

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// ?? Configure Swagger with JWT authentication
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Template API", Version = "v1" });

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

// ?? Use the correct `AppDbContext` instead of `DbContext`
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseLazyLoadingProxies()
        .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// ?? Register UnitOfWork and Generic Repository for dependency injection
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// ?? Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// ?? Configure middleware

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ?? Enable Authentication & Authorization middleware
app.UseAuthentication(); // Required if you plan to use JWT authentication
app.UseAuthorization();

app.UseCors("AllowAll");

app.MapControllers();

app.Run();
