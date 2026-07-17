using System.ComponentModel.DataAnnotations;

namespace Conveyancing_Extractor.Data
{
    /// <summary>
    /// Persistent record of a solicitor listing scraped from a source site.
    /// (SolicitorId, Source, Location) is the natural unique key. A national
    /// firm appears once per searched location, so Location is part of the key.
    /// If a listing disappears from the site for a location that was included
    /// in the latest scrape run the row is removed. Listings from locations
    /// not in the run are untouched.
    /// </summary>
    public class SolicitorEntity
    {
        public int Id { get; set; }

        /// <summary>
        /// SHA-256 hex digest of normalised(address + source).
        /// Uniquely identifies a physical branch regardless of how the source
        /// site assigns its own IDs.
        /// </summary>
        [MaxLength(64)]
        public string SolicitorId { get; set; } = string.Empty;

        [MaxLength(500)]
        public string ProfileUrl { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Source { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Website { get; set; } = string.Empty;

        public double? Rating { get; set; }

        public int? ReviewCount { get; set; }

        public DateTime FirstSeenAt { get; set; }

        public DateTime LastSeenAt { get; set; }
    }
}
