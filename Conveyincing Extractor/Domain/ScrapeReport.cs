using System;
using System.Collections.Generic;
using System.Linq;


namespace Conveyincing_Extractor.Domain
{

    public class ScrapeReport
    {
        public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
        public IReadOnlyList<string> LocationsSearched { get; init; } = [];
        public IReadOnlyList<SolicitorResult> Results { get; init; } = [];

        public int TotalSolicitors => Results.Count;
        public double? AverageRating => Results.Where(r => r.Rating.HasValue).Select(r => r.Rating!.Value) is { } rated && rated.Any()
            ? Math.Round(rated.Average(), 2)
            : null;

        public IEnumerable<object> ByLocation =>
            Results.GroupBy(r => r.Location, StringComparer.OrdinalIgnoreCase)
                   .Select(g =>
                   {
                       var ratings = g.Where(x => x.Rating.HasValue).Select(x => x.Rating!.Value).ToList();
                       return new
                       {
                           Location      = g.Key,
                           Count         = g.Count(),
                           AverageRating = ratings.Count > 0 ? Math.Round(ratings.Average(), 2) : (double?)null
                       };
                   })
                   .OrderByDescending(s => s.Count);

        public IReadOnlyList<SolicitorResult> TopRated =>
            Results.Where(r => r.Rating.HasValue)
                   .OrderByDescending(r => r.Rating)
                   .ThenByDescending(r => r.ReviewCount)
                   .Take(10)
                   .ToList();
    }
}
