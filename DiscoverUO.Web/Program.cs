using Blazored.LocalStorage;
using DiscoverUO.Lib.DTOs;
using DiscoverUO.Web.Components;
using System.Net.Http.Headers;
using System.Net.Http;

namespace DiscoverUO.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddBlazoredLocalStorage();

            #region Dependency Injection & Services

            builder.Services.AddScoped<HttpClient>();

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            #endregion

            #region Application Confirguration

            var app = builder.Build();


            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();

            #endregion
        }
    }
}
