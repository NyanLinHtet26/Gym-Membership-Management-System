using FluentValidation;
using GMMS.Api.Extensions;
using GMMS.Api.Health;
using GMMS.Api.Middleware;
using GMMS.Database.AppDbContextModels;
using GMMS.Domain.Features.Auth;
using GMMS.Domain.Features.Member;
using GMMS.Domain.Features.Member.Models;
using GMMS.Domain.Features.MemberShip;
using GMMS.Domain.Features.MemberShipPlan;
using GMMS.Domain.Features.Payment;
using GMMS.Domain.Features.PaymentMethod;
using GMMS.Domain.Features.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Linq;
using System.Text;
using System.Text.Json;

// Health check that uses AppDbContext to verify DB connectivity

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .CreateLogger();

try
{
    Log.Information("Starting Gym Membership Management API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

 // Add services to the container.

 builder.Services.AddControllers();
 // Health checks (basic) — extended with DB connectivity check
 builder.Services.AddHealthChecks()
     .AddCheck<DbHealthCheck>("database");
 // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
 builder.Services.AddEndpointsApiExplorer();
 builder.Services.AddSwaggerGen(options =>
 {
     options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
     {
         Name = "Authorization",
         Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
         Scheme = "Bearer",
         BearerFormat = "JWT",
         In = Microsoft.OpenApi.Models.ParameterLocation.Header,
         Description = "Enter your JWT token"
     });
     options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
     {
         {
             new Microsoft.OpenApi.Models.OpenApiSecurityScheme
             {
                 Reference = new Microsoft.OpenApi.Models.OpenApiReference
                 {
                     Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                     Id = "Bearer"
                 }
             },
             Array.Empty<string>()
         }
     });
 });

 builder.Services.AddDbContext<AppDbContext>(opt =>
 {
     opt.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection"));
 });

 // JWT Authentication
 var jwtKey = builder.Configuration["JwtSettings:Key"];
 var jwtIssuer = builder.Configuration["JwtSettings:Issuer"];
 var jwtAudience = builder.Configuration["JwtSettings:Audience"];

 builder.Services.AddAuthentication(options =>
 {
     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
 })
 .AddJwtBearer(options =>
 {
     options.TokenValidationParameters = new TokenValidationParameters
     {
         ValidateIssuer = true,
         ValidateAudience = true,
         ValidateLifetime = true,
         ValidateIssuerSigningKey = true,
         ValidIssuer = jwtIssuer,
         ValidAudience = jwtAudience,
         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!)),
         ClockSkew = TimeSpan.Zero
     };
     options.Events = new JwtBearerEvents
      {
          OnMessageReceived = context =>
          {
              var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
              if (!string.IsNullOrEmpty(authHeader) &&
                  authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
              {
                  context.Token = authHeader["Bearer ".Length..].Trim();
                  return Task.CompletedTask;
              }

              var cookie = context.Request.Cookies["accessToken"];
              if (!string.IsNullOrEmpty(cookie))
              {
                  context.Token = cookie;
              }
              return Task.CompletedTask;
          }
      };
 });



 builder.Services.AddValidatorsFromAssemblyContaining<CreateMemberRequestValidator>();

     //Service
 builder.Services.AddScoped<MemberService>();
 builder.Services.AddScoped<MemberShipPlanService>();
 builder.Services.AddScoped<MemberShipService>();
 builder.Services.AddScoped<PaymentMethodService>();
 builder.Services.AddScoped<PaymentService>();
 builder.Services.AddScoped<AuthService>();
 builder.Services.AddScoped<UserService>();
 builder.Services.AddScoped<TokenService>();
 builder.Services.AddScoped<CookieService>();


 var app = builder.Build();
  
 //Middleware
 app.UseMiddleware<ExceptionMiddleware>();
 // Serilog request logging
 app.UseSerilogRequestLogging();
 
 // Configure the HTTP request pipeline.
 if (app.Environment.IsDevelopment())
 {
     app.UseSwagger();
     app.UseSwaggerUI();
 }

 app.UseHttpsRedirection();

 app.UseAuthentication();
 app.UseAuthorization();

 // Map health endpoint with JSON response
 app.MapHealthChecks("/health", new HealthCheckOptions
 {
     ResponseWriter = async (context, report) =>
     {
         context.Response.ContentType = "application/json";
         var result = JsonSerializer.Serialize(new
         {
             status = report.Status.ToString(),
             checks = report.Entries.Select(e => new
             {
                 name = e.Key,
                 status = e.Value.Status.ToString(),
                 description = e.Value.Description,
                 exception = e.Value.Exception?.Message,
                 duration = e.Value.Duration.TotalMilliseconds
             })
         });
         await context.Response.WriteAsync(result);
     }
 });

 app.MapControllers();

 app.Run();
 }
 catch (Exception ex)
 {
     Log.Fatal(ex, "Application terminated unexpectedly");
 }
finally
    {
        Log.CloseAndFlush();
    }

