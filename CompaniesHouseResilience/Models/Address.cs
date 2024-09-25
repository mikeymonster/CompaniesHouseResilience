namespace CompaniesHouseResilience.Models;

public record class Address
{
    public string? AddressSingleLine { get; set; }

    public string? SubBuildingName { get; set; }

    public string? BuildingName { get; set; }

    public string? BuildingNumber { get; set; }

    public string? Street { get; set; }

    public string? Town { get; set; }

    public string? County { get; set; }

    public string? Country { get; set; }

    public string? Postcode { get; set; }

    public string? Locality { get; init; }
}
