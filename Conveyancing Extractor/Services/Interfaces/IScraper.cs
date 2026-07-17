using Conveyancing_Extractor.Domain;

namespace Conveyancing_Extractor.Services.Interfaces
{
    public interface IScraper
    {
        /// <summary>Identifies which site this scraper targets e.g. "solicitors.com"</summary>
        string Source { get; }

        /// <summary>
        /// Scrapes conveyancing solicitor details for the given locations.
        /// </summary>
        Task<IEnumerable<SolicitorResult>> ScrapeAsync(
            IEnumerable<string> locations,
            CancellationToken cancellationToken = default);
    }
}
