using System.Reflection;
using System.Text;
using JwtAuthentication.Server.Db;
using JwtAuthentication.Server.Options;
using JwtAuthentication.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
////////////////////////////////

//Add Controllers
builder.Services.AddControllers();

// Add JWT Authentication 
builder.Services.AddAuthentication(opt => {
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "https://localhost:5001",
            ValidAudience = "https://localhost:5001",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@345"))
        };
    });

// Add CORS 
builder.Services.AddCors(options =>
{
    options.AddPolicy("EnableCORS", corsPolicyBuilder => 
    { 
        corsPolicyBuilder.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod(); 
    });
});

builder.Services.AddSwaggerGen(x =>
{
    x.SwaggerDoc("v1", new OpenApiInfo
    {
         Title = "AuthenticatedAPI",
         Version = "v1"
     });

    var security = new Dictionary<string, IEnumerable<string>>
    {
        {"Bearer", Array.Empty<string>()}
    };

    x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description =
            "JWT Authorization header using the Bearer scheme. " +
            "\r\n\r\n Enter 'Bearer' [space] and then your token in the text input below." +
            "\r\n\r\nExample: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    x.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme 
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }, new List<string>()
        }
    });
});

// Add DbContext

builder.Services.AddDbContext<UserContext>(opts =>
    opts.UseSqlServer(builder.Configuration["ConnectionString:UserDB"]));

// Add Token Service
builder.Services.AddTransient<ITokenService, TokenService>();

var app = builder.Build();

// Configure Middleware 
/////////////////////////

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

var swaggerOptions = new SwaggerOptions();
configuration.GetSection(nameof(SwaggerOptions)).Bind(swaggerOptions);

if (builder.Environment.IsDevelopment())
{
    app.UseSwagger(option =>
    {
        option.RouteTemplate = (option.RouteTemplate = swaggerOptions.JsonRoute); 
    });

    app.UseSwaggerUI(option =>
    {
        option.SwaggerEndpoint(swaggerOptions.UiEndPoint, swaggerOptions.ApiDescription);
    });
}

app.MapControllers();

app.Run();
