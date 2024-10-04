using CompaniesHouseResilience.Options;

namespace CompaniesHouseResilience.Tests;

[TestClass]
public class ApiOptionsExtensionsTests
{
    // ApiOptions _options = new ApiOptions

    [TestMethod]
    [DataRow(null, null, 0)]
    [DataRow(1, TimeUnit.Milliseconds, 1)]
    [DataRow(1, TimeUnit.Seconds, 1000)]
    public void GetTimeSpan_ShouldReturnExpectedResult(int input, TimeUnit units, int expectedResultInMilliseconds)
    {
        var options = new ApiOptions
        {
            CompaniesHouseLookupBaseUrl = "localhost",
            TimeUnits = units,
        };

        var result = options.GetTimeSpan(input);

        // Assert
        result.TotalMilliseconds.Should().Be(expectedResultInMilliseconds);
    }

    [TestMethod]
    public void GetTimeSpan_ShouldThrowException_ForUnknownValue()
    {
        var options = new ApiOptions
        {
            CompaniesHouseLookupBaseUrl = "localhost",
            TimeUnits = (TimeUnit)999,
        };

        // Act
        Func<TimeSpan> act = () => options.GetTimeSpan(100);

        // Assert
        act.Should().Throw<NotImplementedException>();
    }
}
