using GptApi.Data;
using GptApi.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 網域
builder.Services.AddCors(options =>
{
    options.AddPolicy("policy1", builder =>
    {
        builder.WithOrigins("*")  
            .AllowAnyHeader()
            .AllowAnyMethod().WithExposedHeaders("*");
    });
});


// jwt 驗證
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = TokenParameter.Issuer,
                ValidAudience = TokenParameter.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TokenParameter.Secret))
            };
        });

// swagger 輸入
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "輸入格式為 Bearer Jwt字串",

        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    //添加安全要求
    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme{
                Reference =new OpenApiReference{
                    Type = ReferenceType.SecurityScheme,
                    Id ="Bearer"
                }
            },new string[]{ }
        }
    });
});
// dbcontext
builder.Services.AddDbContext<GptContext>(optionsBuilder =>
{
    ConfigurationManager config = builder.Configuration;
    DesCrytoHelper.TryDesDecrypt
        (config.GetConnectionString("connStr"), config["Des:k"], config["Des:iv"], out string connStr);
    optionsBuilder.UseSqlServer(connStr);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseCors("policy1");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
