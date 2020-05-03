using LoggerService;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repository;
using Entities;
using Contracts;
using Microsoft.Data.SqlClient;

namespace Pouzeux_MVC.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
        }
    
        public static void ConfigureIISIntegration(this IServiceCollection services)
        {
            services .Configure<IISOptions>(options =>
                    {

                    });
        }

        public static void ConfigureLoggerService(this IServiceCollection services)
        {
            services .AddSingleton<ILoggerManager, LoggerManager>();
        }

        public static void ConfigureMySqlContext(this IServiceCollection services, IConfiguration config)
        {
            string _connection = null;

            // dotnet user-secrets list to see what is set 

            // Microsoft SQL
            var builder = new SqlConnectionStringBuilder(config["MSSQLConnectionString:connectionString"]);
            builder.Password = config["password"];
            _connection = builder.ConnectionString;
            services.AddDbContext<RepositoryContext>(o => { o.UseSqlServer(_connection, sqlServer => sqlServer.MigrationsAssembly("Entities"));});

            // MYSQL
            var MYSQLconnectionString = config["MYSQLConnectionString:connectionString"];
            //services.AddDbContext<RepositoryContext>(o => o.UseMySql(MYSQLconnectionString));
            // services.AddDbContext<RepositoryContext>(o => o.UseMySql(MYSQLconnectionString, sqlServer => sqlServer.MigrationsAssembly("Entities")));

        }


        public static void ConfigureRepositoryWrapper(this IServiceCollection services)
        {
            services .AddScoped<IRepositoryWrapper, RepositoryWrapper>();
        }

    }
}
