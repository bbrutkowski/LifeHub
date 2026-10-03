using LifeHub.Application;
using LifeHub.Application.Interfaces.Repositories;
using LifeHub.Infrastructure;
using LifeHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured");
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
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
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1)
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var principal = context.Principal;
            var userId = principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? principal?.FindFirst("sub")?.Value;
            var sessionId = principal?.FindFirst("sid")?.Value;
            if (!Guid.TryParse(userId, out var parsedUserId) || !Guid.TryParse(sessionId, out var parsedSessionId))
            {
                context.Fail("The token is not associated with a valid session.");
                return;
            }

            await using var scope = context.HttpContext.RequestServices.CreateAsyncScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var sessionRepository = scope.ServiceProvider.GetRequiredService<IRefreshTokenSessionRepository>();
            var user = await userRepository.GetById(parsedUserId, context.HttpContext.RequestAborted);
            var sessionIsActive = await sessionRepository.IsNotRevoked(parsedSessionId, parsedUserId, context.HttpContext.RequestAborted);
            if (user is null || !sessionIsActive)
            {
                context.Fail("The account or session is no longer active.");
            }
        }
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
