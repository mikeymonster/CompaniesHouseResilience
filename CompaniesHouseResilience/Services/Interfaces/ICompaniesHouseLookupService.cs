
using CompaniesHouseResilience.Models;

namespace CompaniesHouseResilience.Services.Interfaces;

public interface ICompaniesHouseLookupService
{
    Task<Company?> GetCompany(string id);
}
