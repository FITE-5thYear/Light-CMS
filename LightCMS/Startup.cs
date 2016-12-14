using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http;
using Components.Main;
using LightCMS.Components.SingleArticle;
using LightCMS.Components.CategoryList;

namespace LightCMS
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }
            Configuration = builder.Build();

            var settings = new Settings();
            new ConfigureFromConfigurationOptions<Settings>(Configuration.GetSection("Settings"))
            .Configure(settings);

            //bootstrap components
            BootstrapComponents(settings.MySqlConnectionString);

        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddMvc();

            //add session
            services.AddDistributedMemoryCache();
            services.AddMemoryCache();
            services.AddSession();

            //add functionality to inject IOptions<T>
            services.AddOptions();




            //map Settings section to Settings object
            services.Configure<Settings>(Configuration.GetSection("Settings"));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //enable session before MVC
            app.UseSession();

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseApplicationInsightsRequestTelemetry();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseApplicationInsightsExceptionTelemetry();

            /* Static files */
            app.UseStaticFiles();

            /* Admin Theme */
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                    System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), @"Themes/AdminTheme")),
                RequestPath = new PathString("/admin-theme")
            });

            /* Main Theme */
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                    System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), @"Themes/MainTheme")),
                RequestPath = new PathString("/theme")
            });



            /* Routes */
            app.UseMvc(routes =>
            {

                routes.MapRoute(
                       name: "admin_item",
                       template: "admin/items",
                       defaults: new { controller = "ItemAdmin" }
                       );

                routes.MapRoute(
                name: "admin_category",
                template: "admin/categories",
                defaults: new { controller = "CategoryAdmin" }
                );

                routes.MapRoute(
                name: "admin_menu",
                template: "admin/menus",
                defaults: new { controller = "MenuAdmin" }
                );

                routes.MapRoute(
                name: "admin_menu_item",
                template : "admin/menu-items",
                defaults: new {Controller= "Menu_ItemAdmin" }
                );



                routes.MapRoute(
                    name: "admin-panel",
                    template: "admin",
                    defaults: new { controller = "AdminPanel", action = "Index" }
                );



                routes.MapRoute(
                name: "config",
                template: "config/{action}/{language_id}",
                defaults: new { controller = "Router" });



                routes.MapRoute(
                    name: "default",
                    template: "{*link}",
                    defaults: new { controller = "Router", action = "Index" });
            });
        }

        private void BootstrapComponents(string connectionString)
        {
            MainComponentController.Bootstrap(connectionString);
            SingleArticleComponentController.Bootstrap(connectionString);
            CategoryListComponentController.Bootstrap(connectionString);
        }
    }
}
