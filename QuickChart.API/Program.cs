using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuickChart.API.Domain;
using QuickChart.API.Domain.Dto;
using QuickChart.API.Domain.Entities;
using QuickChart.API.Hub;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IDictionary<string, UserRoomConnection>>(opt =>
    new Dictionary<string, UserRoomConnection>());

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// JWT Auth
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = false,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!)),
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen( 
                        setup => { 
                            var jwtSecurityScheme = new OpenApiSecurityScheme
                            {
                                Scheme = "bearer",
                                BearerFormat = "JWT",
                                Name = "JWT Authentication",
                                In = ParameterLocation.Header,
                                Type = SecuritySchemeType.Http,
                                Description = "Put **_ONLY_** your JWT Bearer token on text-box below!",

                                Reference = new OpenApiReference
                                {
                                    Id = JwtBearerDefaults.AuthenticationScheme,
                                    Type = ReferenceType.SecurityScheme
                                }
                            };

                            setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
                            setup.AddSecurityRequirement(new OpenApiSecurityRequirement
                                        {
                                            {
                                                jwtSecurityScheme, Array.Empty<string>()
                                            }
                                        });
                            setup.SwaggerDoc("v1", new OpenApiInfo
                            {
                                Version = "v1",
                                Title = "Accounting API",
                            });
                        });

builder.Services.AddHealthChecks();
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 1024 * 1024; // 1 MB
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapHealthChecks("/health");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");
app.MapHub<MessageHub>("/chat");

await app.RunAsync();
