using Blazored.LocalStorage;
using DiscoverUO.Web.Components;

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
