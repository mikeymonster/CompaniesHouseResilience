using CompaniesHouseResilience.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CompaniesHouseResilience;

public class CompaniesHouseLookupFunction(
    ICompaniesHouseLookupService companiesHouseLookupService,
    ILogger<CompaniesHouseLookupFunction> logger)
{
    private readonly ICompaniesHouseLookupService _companiesHouseLookupService = companiesHouseLookupService;
        private readonly ILogger<CompaniesHouseLookupFunction> _logger = logger;

    [Function("Function1")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "company/{companiesHouseId}")]
        HttpRequest request,
        string companiesHouseId)
    {
        _logger.LogInformation("Companies house lookup called with {CompaniesHouseId}.", companiesHouseId);

        try
        {
            var company = await _companiesHouseLookupService.GetCompany(companiesHouseId);

            return new OkObjectResult(company);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Companies house lookup failed with exception {Message}", ex.Message);
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }
}
