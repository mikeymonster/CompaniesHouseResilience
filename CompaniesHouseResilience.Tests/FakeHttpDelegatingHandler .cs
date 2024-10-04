namespace CompaniesHouseResilience.Tests;

public class FakeHttpDelegatingHandler : DelegatingHandler
{
    //https://josef.codes/testing-your-polly-policies/
    private readonly Func<int, Task<HttpResponseMessage>> _responseFactory;
    public int Attempts { get; private set; }

    public FakeHttpDelegatingHandler(Func<int, Task<HttpResponseMessage>> responseFactory)
    {
        _responseFactory = responseFactory ?? throw new ArgumentNullException(nameof(responseFactory));
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return await _responseFactory.Invoke(++Attempts);
    }
}
