using Conveyincing_Extractor.Domain;
using Conveyincing_Extractor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Conveyincing_Extractor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScraperController(IEnumerable<IScraper> scrapers, ILogger<ScraperController> logger) : ControllerBase
    {
        private static readonly string[] DefaultLocations =
        [
            "London", "Birmingham", "Leeds", "Manchester",
            "Sheffield", "Bradford", "Liverpool", "Bristol"
        ];

        private readonly IEnumerable<IScraper> _scrapers = scrapers;
        private readonly ILogger<ScraperController> _logger = logger;

        /// <summary>
        /// Returns the default list of locations to search. - FUTURE Currently just a hardcoded list but in future would be pulled from a list from a site
        /// </summary>
        [HttpGet("locations")]
        public IActionResult GetDefaultLocations() => Ok(DefaultLocations);

        /// <summary>
        /// Returns the source identifiers of all registered scrapers. - FUTURE if more than one scraper was to be created to pull more than one site
        /// GET /api/scraper/sources
        /// </summary>
        [HttpGet("sources")]
        public IActionResult GetSources() => Ok(_scrapers.Select(s => s.Source));

        /// <summary>
        /// Runs all registered scrapers (or a filtered subset) for the supplied locations. - FUTURE currently only 1 scraper registered for this 
        /// project but in future if more than one scraper was to be created to pull more than one site this would allow the user to select
        /// which scrapers to run based on which site
        /// POST /api/scraper/scrape
        /// Body: { "locations": ["London"], "sources": ["solicitors.com"] }
        /// Omit "sources" to run every registered scraper.
        /// </summary>
        [HttpPost("scrape")]
        public async Task<IActionResult> Scrape(
            [FromBody] ScrapeRequest request,
            CancellationToken cancellationToken)
        {
            if (request.Locations.Count == 0)
                return BadRequest(new { error = "At least one location is required." });

            var scrapers = request.Sources is { Count: > 0 }
                ? _scrapers.Where(s => request.Sources.Contains(s.Source, StringComparer.OrdinalIgnoreCase))
                : _scrapers;

            _logger.LogInformation("Scrape requested — locations: {Locations}, sources: {Sources}",
                string.Join(", ", request.Locations),
                string.Join(", ", scrapers.Select(s => s.Source)));

            var tasks = scrapers.Select(s => s.ScrapeAsync(request.Locations, cancellationToken));
            var results = (await Task.WhenAll(tasks)).SelectMany(r => r).ToList();

            var report = new ScrapeReport
            {
                LocationsSearched = request.Locations,
                Results = results
            };

            return Ok(report);
        }
    }
}
