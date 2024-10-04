namespace CompaniesHouseResilience.Options;

public record ApiOptions
{
    public const string SectionName = "ApiConfig";

    public required string CompaniesHouseLookupBaseUrl { get; set; }

    public int RetryPolicyInitialWaitTime { get; set; }

    public int RetryPolicyMaxRetries { get; set; }

    public int RetryPolicyTooManyAttemptsWaitTime { get; set; }

    public int Timeout { get; set; }

    public TimeUnit TimeUnits { get; set; }

    /*
    public static int ConvertTimeToMilliseconds(this ApiOptions apiOptions, int time)
    {
        return apiOptions.TimeUnit switch
        {
            TimeUnit.Seconds => time * 1000,
            TimeUnit.Milliseconds => time,
            _ => throw new NotImplementedException()
        };
    }
    */
}
