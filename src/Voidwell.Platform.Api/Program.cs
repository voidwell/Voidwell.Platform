using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Voidwell.Platform.Api;
using Voidwell.Platform.Api.Authentication;
using Voidwell.Platform.Api.Cache;
using Voidwell.Platform.Api.Extensions;
using Voidwell.Platform.Api.Options;
using Voidwell.Platform.Api.Swagger;
using Voidwell.Platform.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationLogging();

var configuration = builder.Configuration;
var services = builder.Services;

services.AddControllers()
    .AddApiJsonOptions();

services.AddApiSwagger(configuration);

services.AddHttpContextAccessor();
services.AddCache(options =>
{
    options.RedisConfiguration = configuration.GetValue<string>("RedisConfiguration");
    options.KeyPrefix = configuration.GetApplicationName();
});
services.AddEntityFrameworkContext(configuration);

var authOptions = configuration.GetSection("Auth").Get<AuthOptions>() ?? throw new InvalidOperationException("Auth configuration section is missing.");

services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddServiceAuthentication(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = authOptions.Authority;
        options.ClientId = authOptions.ClientId;
        options.ClientSecret = authOptions.ClientSecret;
        options.RoleClaimType = authOptions.RoleClaimType;
        options.SupportedTokens = SupportedTokens.Both;
        options.RequireHttpsMetadata = false;
        options.EnableCaching = true;
        options.CacheDuration = TimeSpan.FromMinutes(2);
    });

services.AddAuthorization();

var allowedOrigin = configuration.GetValue<string>("OriginAddress");
services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(new[] { "http://localhost:4200", allowedOrigin }.Where(o => !string.IsNullOrEmpty(o)).ToArray()!)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

services.AddPlatformServices();

var app = builder.Build();

app.InitializeDatabases();

app.UseForwardedHeaders(GetForwardedHeaderOptions());

app.UseApiSwagger();

app.UseRouting();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

static ForwardedHeadersOptions GetForwardedHeaderOptions()
{
    var options = new ForwardedHeadersOptions
    {
        RequireHeaderSymmetry = false,
        ForwardLimit = 15,
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    };

    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();

    return options;
}
