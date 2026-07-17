using Conveyancing_Extractor.Domain;

namespace Conveyancing_Extractor.Data
{
    /// <summary>
    /// Persists scrape results and returns them enriched with IsNew.
    /// Only removes records for locations that were included in this scrape run.
    /// </summary>
    public interface ISolicitorRepository
    {
        /// <summary>
        /// Upserts all scraped results and removes any records for the scraped
        /// locations that were not present in this run.
        /// Returns the same results with IsNew set correctly.
        /// </summary>
        Task<IReadOnlyList<SolicitorResult>> SyncAsync(
            IReadOnlyList<SolicitorResult> scraped,
            IReadOnlyList<string> scrapedLocations,
            CancellationToken cancellationToken = default);
    }
}
