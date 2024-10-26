using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Templates;
using Serilog.Templates.Themes;

namespace Backend;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        //Kestrel configs
        builder.WebHost.UseKestrel();
        builder.WebHost.ConfigureKestrel((context, options) =>
        {
            options.Listen(IPAddress.Any, 80);
        });
        
        //Databases
        
        //Main app db
        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            //options.UseSqlServer(builder.Configuration["ConnectionString"]);
            options.UseInMemoryDatabase("AppВb");
        });
        
         //Cors
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                policy =>
                {
                    policy.WithOrigins("*")
                        .AllowAnyHeader()
                        .AllowAnyMethod();;
                });
        });

        //Logger
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
        
        builder.Services.AddSerilog((services, lc) => lc
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Cors.Infrastructure.CorsService"))
            .WriteTo.Console(new ExpressionTemplate(
                // Include trace and span ids when present.
                "[{@t:HH:mm:ss} {@l:u3}{#if @tr is not null} ({substring(@tr,0,4)}:{substring(@sp,0,4)}){#end}] {@m}\n{@x}",
                theme: TemplateTheme.Code)));
        
        //Authentication with JWT
        builder.Services.AddAuthorization();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                //options.UseSecurityTokenValidators = false;
                //options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true, // укзывает, будет ли валидироваться издатель при валидации токена
                    ValidIssuer = TokenOptions.Issuer, // строка, представляющая издателя
                    ValidateAudience = true, // будет ли валидироваться потребитель токена
                    ValidAudience = TokenOptions.Audience, // установка потребителя токена
                    ValidateLifetime = true, // будет ли валидироваться время существования
                    IssuerSigningKey = TokenOptions.GetSymmetricSecurityKey(), // установка ключа безопасности
                    ValidateIssuerSigningKey = true, // валидация ключа безопасности
                };
                
            });

        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                {
                    new OpenApiSecurityScheme {
                        Reference = new OpenApiReference {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });

        builder.Services.AddControllers();
        
        var app = builder.Build();
        
        app.UseCors();

        //Adds response time middleware
        app.UseTimingMiddleware();
        
        //swagger in dev env
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        
        app.UseSerilogRequestLogging();
        
        app.UseRouting();
        
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapDefaultControllerRoute();
            //.WithOpenApi();

        //redirect index to swagger
        app.MapGet("/", (HttpContext httpContext) =>
        {
            if (app.Environment.IsDevelopment())
            {
                httpContext.Response.Redirect("/swagger/index.html");
            }

            httpContext.Response.WriteAsync(
                $"What are you doing here, you little hacker? :) {httpContext.Connection.RemoteIpAddress}");
        });

        app.Run();
        Log.CloseAndFlush();
    }
}