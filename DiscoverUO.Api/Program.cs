using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DiscoverUO.Api.Data;
using DiscoverUO.Api.Data.Repositories;
using DiscoverUO.Api.Data.Repositories.Contracts;

namespace DiscoverUO.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<DiscoverUODatabaseContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DiscoverUOConnection")));

            // Add services to the container.
            builder.Services.AddScoped<IServerDataRepository, ServerDataRepository>();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
