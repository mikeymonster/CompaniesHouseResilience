namespace CompaniesHouseResilience.Options;

public static class ApiOptionsExtensions
{
    public static int ConvertToMilliseconds(this ApiOptions apiOptions, int time)
    {
        return apiOptions.TimeUnits switch
        {
            TimeUnit.Seconds => time * 1000,
            TimeUnit.Milliseconds => time,
            _ => throw new NotImplementedException()
        };
    }

    public static TimeSpan GetTimeSpan(this ApiOptions apiOptions, int time)
    {
        return apiOptions.TimeUnits switch
        {
            TimeUnit.Seconds => TimeSpan.FromSeconds(time),
            TimeUnit.Milliseconds => TimeSpan.FromMilliseconds(time),
            _ => throw new NotImplementedException()
        };
    }
}
