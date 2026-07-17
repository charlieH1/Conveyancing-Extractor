namespace Conveyincing_Extractor.Domain
{
    public class SolicitorResult
    {
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
    }
}
