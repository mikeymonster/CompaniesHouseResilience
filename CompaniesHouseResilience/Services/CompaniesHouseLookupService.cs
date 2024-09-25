using CompaniesHouseResilience.Extensions;
using CompaniesHouseResilience.Models;
using CompaniesHouseResilience.Services.Interfaces;
using System.Net;
using System.Text.Json;

namespace CompaniesHouseResilience.Services;

public class CompaniesHouseLookupService(HttpClient httpClient) : ICompaniesHouseLookupService
{
    private const string CompaniesHouseEndpoint = "/company";
    private readonly HttpClient _httpClient = httpClient;

    public async Task<Company?> GetCompany(string id)
    {
        var response = await _httpClient.GetAsync($"{CompaniesHouseEndpoint}/{id}");
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            // _logger
            // in real case - save error to notifications
            return null;
        }

        response.EnsureSuccessStatusCode();

        var jsonDocument = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = jsonDocument.RootElement;

        return new Company
        {
            Name = root.GetStringFromJsonElement("company_name"),
            CompaniesHouseNumber = root.GetStringFromJsonElement("company_number"),
            BusinessAddress = root.TryGetProperty("registered_office_address", out var address)
                ? new Address
                {
                    Street = address.GetStringFromJsonElement("address_line_1"),
                    Locality = address.GetStringFromJsonElement("address_line_2"),
                    County = address.GetStringFromJsonElement("locality"),
                    Country = address.GetStringFromJsonElement("country"),
                    Postcode = address.GetStringFromJsonElement("postal_code")
                }
                : new Address()
        };
    }
}
