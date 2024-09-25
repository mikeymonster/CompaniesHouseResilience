namespace CompaniesHouseResilience.Models;

public record Company
{
    public required string Name { get; set; }

    public required string CompaniesHouseNumber { get; set; }

    public DateTimeOffset? AccountCreatedOn { get; set; }

    public Address? BusinessAddress { get; set; }
}
