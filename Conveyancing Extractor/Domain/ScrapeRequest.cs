namespace Conveyancing_Extractor.Domain
{
    public class ScrapeRequest
    {
        public IReadOnlyList<string> Locations { get; init; } = [];

        /// <summary>
        /// Optional list of scraper sources to run e.g. ["solicitors.com"].
        /// When null or empty all registered scrapers are used.
        /// </summary>
        public IReadOnlyList<string>? Sources { get; init; }
    }
}
