namespace Conveyancing_Extractor.Domain
{
    public record SolicitorResult
    {
        /// <summary>
        /// Unique listing ID from the source site (SiD for solicitors.com).
        /// Each branch of a multi-office firm has its own SolicitorId.
        /// </summary>
        public string SolicitorId { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Location { get; init; } = string.Empty;
        public string Source { get; init; } = string.Empty;
        public string Address { get; init; } = string.Empty;
        public string Phone { get; init; } = string.Empty;
        public string Website { get; init; } = string.Empty;
        public double? Rating { get; init; }
        public int? ReviewCount { get; init; }
        public string ProfileUrl { get; init; } = string.Empty;
        public DateTime ScrapedAt { get; init; } = DateTime.UtcNow;
        public bool IsNew { get; init; }
    }
}
