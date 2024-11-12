using Blazored.LocalStorage;
using DiscoverUO.Web.Authentication;
using DiscoverUO.Web.Components;

namespace DiscoverUO.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddBlazoredLocalStorage();

            builder.Services.AddHttpClient("DiscoverUOApiClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7015");
            }).AddHttpMessageHandler<JWTTokenHandler>();

            #region Dependency Injection

            builder.Services.AddScoped<LoginComponent>();
            builder.Services.AddScoped<JWTTokenHandler>();

            #endregion

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
