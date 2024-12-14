using Blazored.LocalStorage;
using DiscoverUO.Web.Components;
using MudBlazor.Services;
using MudBlazor;

namespace DiscoverUO.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Dependency Injection & Services

            builder.Services.AddScoped<HttpClient>(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5219") });
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddMudServices();
            builder.Services.AddRazorComponents().AddInteractiveServerComponents();

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
