using CompaniesHouseResilience.Options;
using CompaniesHouseResilience.Services;
using CompaniesHouseResilience.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using Polly;
using Polly.Timeout;
using CompaniesHouseResilience.Resilience;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http.Resilience;
using Polly.Retry;
using System.Net;

namespace CompaniesHouseResilience.Extensions;
public static class ConfigurationExtensions
{
    public static IServiceCollection ConfigureOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ApiOptions>(configuration.GetSection(ApiOptions.SectionName));
        return services;
    }

    public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<ICompaniesHouseLookupService, CompaniesHouseLookupService>((sp, client) =>
        {
            var apiOptions = sp.GetRequiredService<IOptions<ApiOptions>>().Value;

            client.BaseAddress = new Uri(apiOptions.CompaniesHouseLookupBaseUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        })
            // .AddPolicyHandler((services, _) => GetRetryPolicy<CompaniesHouseLookupDirectService>(services))
            // .AddPolicyHandler((services, _) => GetTimeoutPolicy(services))
            //.AddResiliencePipelineHandler(PollyResilienceStrategy.Retry())
            //.AddResilienceHandler(PollyResilienceStrategies.CompaniesHouseResiliencePipelineKey, PollyResilienceStrategies.ConfigureCompaniesHouseResilienceHandler<CompaniesHouseLookupService>())
            .AddCompaniesHouseResilienceHandler();
        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        // services.AddTransient<ICompaniesHouseLookupService, CompaniesHouseLookupService>();

        //var pipeline = new ResiliencePipeline
        //services.AddResiliencePipelineRegistry
        // https://anktsrkr.github.io/post/use-httpclientfactory-with-pollyv8-to-implement-resilient-http-requests/#google_vignette
        // https://devblogs.microsoft.com/dotnet/building-resilient-cloud-services-with-dotnet-8/
        // https://dotnetteach.com/blog/simplifying-resilience-strategies-in-polly

        /*
        services.AddResiliencePipeline(PollyResilienceStrategy.CompaniesHouseResiliencePipelineKey, (builder, context) =>
        {
            var apiOptions = context.ServiceProvider.GetRequiredService<IOptions<ApiOptions>>().Value;

            // Better way?
            var options = context.GetOptions<ApiOptions>();

            builder.AddRetry(new HttpRetryStrategyOptions
            {
                // Delay = TimeSpan.FromSeconds(options.RetryPolicyInitialWaitTime),
                // BackoffType = DelayBackoffType.Exponential,
                // MaxRetryAttempts = options.RetryPolicyMaxRetries,
                // UseJitter = true,
                OnRetry = args =>
                {
                    var logger = context.ServiceProvider.GetRequiredService<ILogger>();

                    logger.LogWarning(
                            "Resilience pipeline startegy will attempt retry {Retry} in {Delay}ms after a transient error or timeout. {ExceptionMessage}",
                            args.AttemptNumber,
                            args.RetryDelay.TotalMilliseconds,
                            args.Outcome.Exception?.Message);

                    if (args.Outcome.Exception is TimeoutException)
                    {
                        logger.LogInformation("Timeout encountered");
                    }

                    return default;
                }
            })
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(apiOptions.Timeout)
            });

            builder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                //ShouldHandle = new PredicateBuilder<HttpResponseMessage>().HandleResult(response => response.StatusCode == HttpStatusCode.TooManyRequests),                
                Delay = TimeSpan.FromSeconds(options.RetryPolicyTooManyAttemptsWaitTime),
                MaxRetryAttempts = options.RetryPolicyMaxRetries,
                UseJitter = true,
                BackoffType = DelayBackoffType.Exponential,
                OnRetry = args =>
                {
                    var logger = context.ServiceProvider.GetRequiredService<ILogger>();
                    logger.LogWarning(
                            "Retry policy will attempt retry {Retry} in {Delay}ms after a 429 error. {ExceptionMessage}",
                            args.AttemptNumber,
                            args.RetryDelay.TotalMilliseconds,
                            args.Outcome.Exception?.Message);

                    return default;
                }
            });
        });

    GET_OUT:
        */
        return services;
    }

    public static IHttpStandardResiliencePipelineBuilder AddCompaniesHouseResilienceHandler(this IHttpClientBuilder builder, IConfigurationSection section)
    {
        //Microsoft.Shared.Diagnostics.Throw.IfNull(builder, "builder");
        //Microsoft.Shared.Diagnostics.Throw.IfNull(section, "section");
        return builder.AddStandardResilienceHandler().Configure(section);
    }

    public static IHttpStandardResiliencePipelineBuilder AddCompaniesHouseResilienceHandler(this IHttpClientBuilder builder, Action<HttpStandardResilienceOptions> configure)
    {
        //Microsoft.Shared.Diagnostics.Throw.IfNull(builder, "builder");
        //Microsoft.Shared.Diagnostics.Throw.IfNull(configure, "configure");
        return builder.AddStandardResilienceHandler().Configure(configure);
    }

    /*
    public static Action<HttpStandardResilienceOptions> ConfigureCompaniesHouseResilienceHandler(this IHttpClientBuilder builder)
    {
        return new Action<HttpStandardResilienceOptions>
    }
    */

    public static Action<ResiliencePipelineBuilder<HttpResponseMessage>, ResilienceHandlerContext> ConfigureCompaniesHouseResilienceHandler<T>() //this IHttpClientBuilder builder)
    {
        return (builder, context) =>
        {
            var apiOptions = context.GetOptions<ApiOptions>();

            builder.AddRetry(new HttpRetryStrategyOptions
            {
                Delay = TimeSpan.FromSeconds(apiOptions.RetryPolicyInitialWaitTime),
                BackoffType = DelayBackoffType.Exponential,
                MaxRetryAttempts = apiOptions.RetryPolicyMaxRetries,
                UseJitter = true,
                OnRetry = args =>
                {
                    var logger = context.ServiceProvider.GetService<ILogger<T>>();
                    logger?.LogWarning(
                            "{Type} retry policy will attempt retry {Retry} in {Delay}ms after a transient error or timeout. {ExceptionMessage}",
                            typeof(T).Name,
                            args.AttemptNumber,
                            args.RetryDelay.TotalMilliseconds,
                            args.Outcome.Exception?.Message);

                    if (args.Outcome.Exception is TimeoutException)
                    {
                        logger?.LogInformation("Timeout encountered");
                    }

                    return default;
                }
            })
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(apiOptions.Timeout)
            });

            builder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .HandleResult(response => response.StatusCode == HttpStatusCode.TooManyRequests),
                Delay = TimeSpan.FromSeconds(apiOptions.RetryPolicyTooManyAttemptsWaitTime),
                MaxRetryAttempts = apiOptions.RetryPolicyMaxRetries,
                UseJitter = true,
                BackoffType = DelayBackoffType.Exponential,
                OnRetry = args =>
                {
                    context.ServiceProvider.GetService<ILogger<T>>()?
                        .LogWarning(
                            "{Type} retry policy will attempt retry {Retry} in {Delay}ms after a 429 error. {ExceptionMessage}",
                            typeof(T).Name,
                            args.AttemptNumber,
                            args.RetryDelay.TotalMilliseconds,
                            args.Outcome.Exception?.Message);

                    return default;
                }
            });
        };
    }

    /*
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy<T>(IServiceProvider services)

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy<T>(IServiceProvider services)
    {
        var apiOptions = services.GetRequiredService<IOptions<ApiOptions>>().Value;

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(
                apiOptions.RetryPolicyMaxRetries,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(apiOptions.RetryPolicyInitialWaitTime, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    var status = outcome?.Result?.StatusCode;

                    // ResilienceStrategy
                    // https://github.com/App-vNext/Polly/issues/854
                    // https://github.com/App-vNext/Polly/blob/f6e09cc99baf3ea892c53580cd90509704a9dfff/samples/Retries/Program.cs#L101-L112
                    // if(new ResilienceStrategyBuilder()
                    // https://stackoverflow.com/questions/67386496/how-can-i-use-polly-to-retry-x-number-of-times-based-on-response-content-and-the
                    // https://www.stevejgordon.co.uk/polly-using-context-to-obtain-retry-count-diagnostics
                    // https://carlpaton.github.io/2021/12/http-retry-polly/
                    services?.GetService<ILogger<T>>()?
                        .LogWarning(
                            "{Type} retry policy will attempt retry {Retry} in {Delay}ms after a transient error or timeout. {ExceptionMessage}",
                            typeof(T).Name,
                            retryAttempt,
                            timespan.TotalMilliseconds,
                            outcome?.Exception?.Message);
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(IServiceProvider sp)
    {
        var apiOptions = sp.GetRequiredService<IOptions<ApiOptions>>().Value;

        return Policy
            .TimeoutAsync<HttpResponseMessage>(
                timeout: TimeSpan.FromSeconds(apiOptions.Timeout),
                timeoutStrategy: TimeoutStrategy.Optimistic);
    }
    */
}
