using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// Shared by every service in this solution (User, Parser, Grades, Courses,
    /// Analytics). This is the standard .NET Aspire "ServiceDefaults" project —
    /// every Program.cs across the solution calls
    /// builder.AddServiceDefaults() and app.MapDefaultEndpoints(), which is
    /// what actually wires service discovery, health checks, and telemetry.
    /// Without this project, none of those Program.cs files would compile.
    /// </summary>
    public static class Extensions
    {
        public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder)
            where TBuilder : IHostApplicationBuilder
        {
            builder.ConfigureOpenTelemetry();

            builder.AddDefaultHealthChecks();

            builder.Services.AddServiceDiscovery();

            builder.Services.ConfigureHttpClientDefaults(http =>
            {
                // Every HttpClient created via IHttpClientFactory (i.e. every
                // ICoursesServiceClient / IGradesServiceClient / etc. in this
                // solution) gets standard retry/circuit-breaker resilience and
                // resolves Aspire's logical service names automatically.
                http.AddStandardResilienceHandler();
                http.AddServiceDiscovery();
            });

            return builder;
        }

        public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder)
            where TBuilder : IHostApplicationBuilder
        {
            builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
            });

            builder.Services.AddOpenTelemetry()
                .WithMetrics(metrics =>
                {
                    metrics.AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation();
                })
                .WithTracing(tracing =>
                {
                    tracing.AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation();
                });

            builder.AddOpenTelemetryExporters();

            return builder;
        }

        private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder)
            where TBuilder : IHostApplicationBuilder
        {
            var useOtlpExporter = !string.IsNullOrWhiteSpace(
                builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

            if (useOtlpExporter)
            {
                builder.Services.AddOpenTelemetry().UseOtlpExporter();
            }

            return builder;
        }

        public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder)
            where TBuilder : IHostApplicationBuilder
        {
            builder.Services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" });

            return builder;
        }

        /// <summary>
        /// Maps /health (all checks) and /alive (only "live"-tagged checks) —
        /// used by the Aspire Dashboard and by orchestrators to know a service
        /// is up, matching Chapter 5's description of Aspire's monitoring.
        /// Only exposed in development, same as Swagger in each service.
        /// </summary>
        public static WebApplication MapDefaultEndpoints(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.MapHealthChecks("/health");

                app.MapHealthChecks("/alive", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("live"),
                });
            }

            return app;
        }
    }
}
