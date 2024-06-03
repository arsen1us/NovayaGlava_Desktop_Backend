using System;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer; // JwtBearer authorization/authentication
using Microsoft.IdentityModel.Tokens; // TokenValidationParameters
using Newtonsoft.Json;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authentication;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using NovayaGlava_Desktop_Backend.Identities;
using NovayaGlava_Desktop_Backend.Hubs;
using NovayaGlava_Desktop_Backend.Models;
using NovayaGlava_Desktop_Backend.Services;
using NovayaGlava_Desktop_Backend.Services.RefreshTokenService;
using NovayaGlava_Desktop_Backend.Services.UserService;
using NovayaGlava_Desktop_Backend.Services.JwtTokenService;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(new MongoClient("mongodb://localhost:27017")); // ����������� ���� ������

// �������� SignalR
builder.Services.AddSignalR();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost";
    //options.InstanceName = "local";
});

builder.Services.AddDataProtection();

builder.Services.AddRazorPages();
builder.Services.AddControllers();

builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddTransient<IJwtTokenService, JwtTokenService>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(IdentityData.AdminUserPolicyName, policy =>
    {
        policy.RequireClaim(IdentityData.AdminUserClaimName, "true");
    });
});

var configuration = builder.Configuration;


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        // ��������� �������� ��� ��������� ������
        ValidateIssuer = true,
        // ��������� ����������� ��� ��������� ������
        ValidateAudience = true,
        // ��������� ������� �������������
        ValidateLifetime = true,
        // ��������� ����� ������������
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["JwtSettings:Issuer"],
        ValidAudience = configuration["JwtSettings:Audience"],
        // ��������� � ��������� ����� ������������
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"])),
    };
});

builder.Services.AddSwaggerGen();
//builder.Services.AddAuthentication();
//builder.Services.AddAuthorization();
//builder.Services.AddCors();

builder.Services.AddDistributedMemoryCache(); // ����������� DistributedMemoryCache

builder.Services.AddSession(options => // ��������� ������
{
    options.IdleTimeout = TimeSpan.FromSeconds(10); //����� ����� 10 ��� � ���� �������
    //options.Cookie.HttpOnly = true; // �� ��������� ���������� ���� ["/"]
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".AdventureWorks.Session"; // �������� ���
});

builder.WebHost.UseUrls("https://localhost:7245");

var app = builder.Build();


//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

if (app.Environment.IsDevelopment())
{

}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();

}

app.UseHttpsRedirection();
app.UseCookiePolicy();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapRazorPages();
app.MapControllers();

app.UseDefaultFiles();
app.UseStaticFiles();

// ������� ��� ���� ChatHub
app.MapHub<ChatHub>("/chatHub");

app.MapGet("/Error", (context) =>
{
    return context.Response.WriteAsJsonAsync("Error page ^^, lol)");
});

//app.MapGet("/clientsList", (IHubContext<ChatHub> hubContext, HttpContext context) =>
//{
//    StringBuilder sb = new StringBuilder();
//    return context.Response.WriteAsJsonAsync(hubContext.Clients.ToJson());
//});


app.Run();
