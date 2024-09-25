# Companies House Resilience

Resilience test function for the companies house API.

Call like this:
http://localhost:7288/api/company/{companiesHouseId}
http://localhost:7288/api/company/06499687

See https://anktsrkr.github.io/post/use-httpclientfactory-with-pollyv8-to-implement-resilient-http-requests/#google_vignette
See also https://devblogs.microsoft.com/dotnet/building-resilient-cloud-services-with-dotnet-8/

`local.settings.json` should contain the url of the companies house api access point and other settings:
```
    "ApiConfig__CompaniesHouseDirectBaseUri": "https://localhost:7131/"
    "ApiConfig__RetryPolicyIntitalWaitTime": "3",
    "ApiConfig__RetryPolicyMaxRetries": "3",
    "ApiConfig__RetryPolicyTooManyAttemptsWaitTime": "200",
    "ApiConfig__Timeout": 60
```


Call the API with the following GET url, with the last part being a companies house number:

```
https://localhost:7131/CompaniesHouse/companies/06499687
```

## Polly policies

To use policies with this api:

```
    services.AddHttpClient<ICompaniesHouseLookupService, CompaniesHouseLookupDirectService>((sp, client) =>
    {
        var apiOptions = sp.GetRequiredService<IOptions<ApiOptions>>().Value;

        client.BaseAddress = new Uri(apiOptions.CompaniesHouseDirectBaseUri);
        var apiKey = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiOptions.CompaniesHouseDirectApiKey}:"));
        client.DefaultRequestHeaders.Add("Authorization", $"BASIC {apiKey}");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    })
        .AddPolicyHandler((services, _) => GetRetryPolicy<CompaniesHouseLookupDirectService>(services))
        .AddPolicyHandler((services, _) => GetTimeoutPolicy(services));

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy<T>(IServiceProvider services) => HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .Or<TimeoutRejectedException>()
        .WaitAndRetryAsync(
            3,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(3, retryAttempt)),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                var x = outcome?.Result?.StatusCode;

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

    private static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(IServiceProvider sp)
    {
        var apiOptions = sp.GetRequiredService<IOptions<ApiOptions>>().Value;

        return Policy
            .TimeoutAsync<HttpResponseMessage>(
                timeout: TimeSpan.FromSeconds(apiOptions.Timeout),
                timeoutStrategy: TimeoutStrategy.Optimistic);
    }
```
