using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebApiToko.Data;
using WebApiToko.Interfaces;
using WebApiToko.Interfaces.Services;
using WebApiToko.Middleware;
using WebApiToko.ModelsEF.Toko;
using WebApiToko.Repositories;
using WebApiToko.Services;
using WebApiToko.Settings;

var builder = WebApplication.CreateBuilder(args);
string envName = builder.Environment.EnvironmentName;

#region Konfigurasi DBContext ke SQL Server/LocalDB
// Konfigurasi DBContext ke SQL Server/LocalDB
builder.Services.AddDbContext<TokoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

#endregion

#region Konfigurasi Idendity
// Tambahkan Identity
builder.Services.AddDbContext<AppIdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.AllowedUserNameCharacters = null;
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 6;
    // ... lainnya
})
.AddEntityFrameworkStores<AppIdentityDbContext>() // ← PASTIKAN INI AppIdentityDbContext
.AddDefaultTokenProviders();

#endregion

#region JWT Conf
//Jwt config
var key = Encoding.ASCII.GetBytes(builder.Configuration["JWT:Secret"]);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        ClockSkew = TimeSpan.Zero
    };

    // Jika Anda ingin tambahkan event (misal: cek revoked token), aktifkan ini:
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async ctx =>
        {
            var jti = ctx.SecurityToken is JwtSecurityToken jwtToken
            ? jwtToken.Id
            : ctx.SecurityToken?.Id ?? ctx.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (string.IsNullOrEmpty(jti))
            {
                ctx.Fail("Invalid token: missing 'jti' claim.");
                return;
            }

            // 2. Cek apakah token sudah direvoke di database
            var userRepository = ctx.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
            var isRevoked = await userRepository.IsTokenRevokedAsync(jti);

            if (isRevoked)
            {
                ctx.Fail("Token has been revoked.");
            }
        }
    };
});
builder.Services.AddAuthorization();
#endregion

#region regist controller
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
#endregion

#region Regist service and repo
// Add services and repo to the container.
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

//Global Exception Handler
builder.Services.AddExceptionHandler<GlobalException>();
#endregion

#region Konfigurasi Swagger
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.\n\nExample: 'Bearer abc123'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
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
            new List<string>()
        }
    });
    c.SwaggerDoc("v1.0", new OpenApiInfo
    {
        Version = "v1.0",
        Title = "ipubers-point - WebApi",
        Description = "This Api will be responsible for overall data ipubers point and authorization.",
    });
    c.SwaggerDoc("v2.0", new OpenApiInfo
    {
        Version = "v2.0",
        Title = "ipubers-point - WebApi",
        Description = "This Api will be responsible for overall data ipubers point and authorization.",
    });
    c.CustomSchemaIds(x => x.FullName);
    c.OperationFilter<SwaggerHeaderFilter>();
    // Resolve conflict for XML comments (optional)
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

});
builder.Services.AddApiVersioning(options =>
{
    // Specify the default API Version as 1.0
    options.DefaultApiVersion = new ApiVersion(1, 0);
    // If the client hasn't specified the API version in the request, use the default API version number
    options.AssumeDefaultVersionWhenUnspecified = true;
    // Advertise the API versions supported for the particular endpoint
    options.ReportApiVersions = true;
    options.ApiVersionReader = new HeaderApiVersionReader("x-api-version");
});

#endregion

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

#region App
var app = builder.Build();

app.MapGet("/healthcheck", () => Results.Ok(new
{
    statusCode = 200,
    statusDesc = "SERVICE IS HEALTHY",
    data = new
    {
        host = Environment.MachineName,
        env = envName,
        version = "API Point" + Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
    }
}));

app.UseExceptionHandler(opt => { });

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    string swaggerJsonBasePath = string.IsNullOrWhiteSpace(c.RoutePrefix) ? "." : "..";
    c.SwaggerEndpoint($"{swaggerJsonBasePath}/swagger/v1.0/swagger.json", "WebApiToko v1.0");
    c.SwaggerEndpoint($"{swaggerJsonBasePath}/swagger/v2.0/swagger.json", "WebApiToko v2.0");
    c.DisplayRequestDuration();
    c.DefaultModelsExpandDepth(-1);
});

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.MapControllers();
//}

app.UseCors();

app.MapControllers();

app.Run();
#endregion