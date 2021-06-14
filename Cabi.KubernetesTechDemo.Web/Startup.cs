using Cabi.KubernetesTechDemo.Web.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Events;

namespace Cabi.KubernetesTechDemo.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecksUI().AddSqliteStorage("Data Source = healthchecksdb");

            services.AddHealthChecks()
                .AddCheck("File Check", () => { return File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "KeepSiteAlive.txt")) ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy(); },
                    new[] {"kubernetes"})
                .AddUrlGroup(new Uri($"https://{Configuration.GetValue<string>("CabiUrls:MySandbox")}/api/weatherforecast"), "Weather API Available", HealthStatus.Degraded);

            services.AddHealthChecksUI(setup =>
            {
                string basePath = "";
                if (!Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT").Equals("Development"))
                {
                    basePath = "http://localhost";
                }

                setup.AddHealthCheckEndpoint("Main", $"{basePath}/health");
                setup.AddHealthCheckEndpoint("Kubernetes", $"{basePath}/health/kubernetes");
            });

            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddSingleton<WeatherForecastService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseSerilogRequestLogging(options => options.GetLevel = ExcludeHealthChecks);

            app.UseRouting();

            app.UseEndpoints(endpoints =>
                {
                    endpoints.MapBlazorHub();
                    endpoints.MapFallbackToPage("/_Host");

                    endpoints.MapHealthChecksUI(); //(setup => setup.UIPath = "/health/ui");

                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    ResultStatusCodes =
                    {
                        [HealthStatus.Healthy] = StatusCodes.Status200OK,
                        [HealthStatus.Degraded] = StatusCodes.Status200OK,
                        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                    },
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                endpoints.MapHealthChecks("/health/kubernetes", new HealthCheckOptions()
                {
                    ResultStatusCodes =
                    {
                        [HealthStatus.Healthy] = StatusCodes.Status200OK,
                        [HealthStatus.Degraded] = StatusCodes.Status200OK,
                        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                    },
                    Predicate = (check) => check.Tags.Contains("kubernetes"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            });

            app.UseHealthChecksUI();
        }

        private bool IsHealthCheckEndpoint(HttpContext ctx)
        {
            var endpoint = ctx.GetEndpoint();
            if (endpoint is object) // same as !(endpoint is null)
            {
                return string.Equals(
                    endpoint.DisplayName,
                    "Health checks",
                    StringComparison.Ordinal)  || string.Equals(
                    endpoint.DisplayName,
                    "HealthChecks UI Api",
                    StringComparison.Ordinal);
            }

            // No endpoint, so not a health check endpoint
            return false;
        }

        private LogEventLevel ExcludeHealthChecks(HttpContext ctx, double _, Exception ex) =>
            ex != null
                ? LogEventLevel.Error
                : ctx.Response.StatusCode > 499
                    ? LogEventLevel.Error
                    : IsHealthCheckEndpoint(ctx) // Not an error, check if it was a health check
                        ? LogEventLevel.Verbose // Was a health check, use Verbose
                        : LogEventLevel.Information;
    }
}
