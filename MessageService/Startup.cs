using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MessageService
{
    /// <summary>
    /// Класс производит конфигурацию приложения.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Конструктор, создающий объект.
        /// </summary>
        /// <param name="configuration">Набор свойств конфигурации приложения</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Набор свойств конфигурации приложения
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Регистрация служб в IServiceCollection.
        /// </summary>
        /// <param name="services">Коллекция IServiceCollection, в которую добавляются службы.</param>
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MessageService", Version = "v1" });
                var filePath = Path.Combine(AppContext.BaseDirectory, "MessageService.xml");
                c.IncludeXmlComments(filePath);
            });

        }

        /// <summary>
        /// Создает конвейера обработки запросов приложения.
        /// </summary>
        /// <param name="app">Механизм для настройки конвейера запросов приложения.</param>
        /// <param name="env">Сведения о среде веб-размещения, в которой выполняется приложение.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MessageService v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
