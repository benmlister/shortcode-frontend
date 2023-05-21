using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;

namespace ShortCode;
public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        builder.WebHost.UseKestrel().UseKestrel();

        builder.Services.AddSession();
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.SlidingExpiration = true;
                options.AccessDeniedPath = "/Forbidden/";
            });

        builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        builder.Services.AddRazorPages();
        builder.Services.AddControllersWithViews();


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        var cookiePolicyOptions = new CookiePolicyOptions
        {
            MinimumSameSitePolicy = SameSiteMode.Strict,
        };

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseCookiePolicy(cookiePolicyOptions);

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapRazorPages();
        app.MapDefaultControllerRoute();

        app.Run();
    }
    


}
