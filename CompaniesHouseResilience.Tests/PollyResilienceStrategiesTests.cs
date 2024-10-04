using CompaniesHouseResilience.Options;
using CompaniesHouseResilience.Resilience;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace CompaniesHouseResilience.Tests;

[TestClass]
public class PollyResilienceStrategiesTests
{
    [TestMethod]
    public async Task ConfigureCompaniesHouseResilienceHandlerTest()
    {
        // Arrange
        const string baseAddress = "http://any.localhost";
        const string httpClientName = "my-httpClient";
        const int maxRetries = 3;

        var services = new ServiceCollection();
        var fakeHttpDelegatingHandler = new FakeHttpDelegatingHandler(
            _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.TooManyRequests)));
        
        // var policy = HttpClientPolicies.DefaultRetryPolicy(sleepDurationProvider: _ => TimeSpan.FromMilliseconds(1));
       
        services.Configure<ApiOptions>(x =>
        {
            x.CompaniesHouseLookupBaseUrl = baseAddress;
            x.RetryPolicyInitialWaitTime = 1;
            x.RetryPolicyMaxRetries = maxRetries;
            x.RetryPolicyTooManyAttemptsWaitTime = 1;
            x.Timeout = 30;
            x.TimeUnits = TimeUnit.Milliseconds;
        });

        services.AddHttpClient(httpClientName, client =>
        {
            client.BaseAddress = new Uri(baseAddress);
        })
            .AddCompaniesHouseResilienceHandlerToHttpClientBuilder()
            .AddHttpMessageHandler(() => fakeHttpDelegatingHandler);

        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var sut = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(httpClientName);
        var request = new HttpRequestMessage(HttpMethod.Get, "/any");

        // Act
        var result = await sut.SendAsync(request);

        // Assert
        //result.StatusCode.Should().Be(HttpStatusCode.GatewayTimeout);
        result.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        fakeHttpDelegatingHandler.Attempts.Should().Be(maxRetries + 1);
    }

    [TestMethod]
    [DataRow(HttpStatusCode.GatewayTimeout)]
    [DataRow(HttpStatusCode.TooManyRequests)]
    [DataRow(HttpStatusCode.InternalServerError)]

    public async Task ConfigureCompaniesHouseResilienceHandlerTest(HttpStatusCode statusCode)
    {
        // Arrange
        const string baseAddress = "http://any.localhost";
        const string httpClientName = "my-httpClient";
        const int maxRetries = 3;

        var services = new ServiceCollection();
        var fakeHttpDelegatingHandler = new FakeHttpDelegatingHandler(
            _ => Task.FromResult(new HttpResponseMessage(statusCode)));

        services.Configure<ApiOptions>(x =>
        {
            x.CompaniesHouseLookupBaseUrl = baseAddress;
            x.RetryPolicyInitialWaitTime = 2;
            x.RetryPolicyMaxRetries = maxRetries;
            x.RetryPolicyTooManyAttemptsWaitTime = 2;
            x.Timeout = 10; //Minimum value 10ms required for this
            x.TimeUnits = TimeUnit.Milliseconds;
        });

        services.AddHttpClient(httpClientName, client =>
        {
            client.BaseAddress = new Uri(baseAddress);
        })
            .AddCompaniesHouseResilienceHandlerToHttpClientBuilder()
            .AddHttpMessageHandler(() => fakeHttpDelegatingHandler);

        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var sut = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(httpClientName);
        var request = new HttpRequestMessage(HttpMethod.Get, "/any");

        // Act
        var result = await sut.SendAsync(request);

        // Assert
        result.StatusCode.Should().Be(statusCode);
        fakeHttpDelegatingHandler.Attempts.Should().Be(maxRetries + 1);
    }
}