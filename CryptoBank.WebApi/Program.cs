using CryptoBank.WebApi.Authorization.Requirements;
using CryptoBank.WebApi.Database;
using CryptoBank.WebApi.Features.Accounts.Registration;
using CryptoBank.WebApi.Features.Auth.Options;
using CryptoBank.WebApi.Features.Auth.Registration;
using CryptoBank.WebApi.Features.Users.Domain;
using CryptoBank.WebApi.Features.Users.Registration;
using CryptoBank.WebApi.Pipeline;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ApplicationDbContext")));

builder.Services.AddMediatR(cfg => cfg
    .RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
    .AddOpenBehavior(typeof(ValidationBehavior<,>)));

builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtOptions = builder.Configuration.GetSection("Features:Auth").Get<AuthOptions>()!.Jwt;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtOptions.SigningKey)),
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyNames.UserRole, policy => policy.RequireClaim(ClaimTypes.Role, UserRole.UserRole.ToString()));
    options.AddPolicy(PolicyNames.AnalystRole, policy => policy.RequireClaim(ClaimTypes.Role, UserRole.AnalystRole.ToString()));
    options.AddPolicy(PolicyNames.AdministratorRole, policy => policy.RequireClaim(ClaimTypes.Role, UserRole.AdministratorRole.ToString()));
});


builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });


builder.AddUsers();
builder.AddAuth();
builder.AddAccounts();

var app = builder.Build();



app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
