using Conveyancing_Extractor.Domain;
using Microsoft.EntityFrameworkCore;

namespace Conveyancing_Extractor.Data
{
    public class SolicitorRepository(AppDbContext db) : ISolicitorRepository
    {
        public async Task<IReadOnlyList<SolicitorResult>> SyncAsync(
            IReadOnlyList<SolicitorResult> scraped,
            IReadOnlyList<string> scrapedLocations,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            // Load all existing rows for the locations included in this run only.
            // Rows for other locations are never touched.
            var existing = await db.Solicitors
                .Where(s => scrapedLocations.Contains(s.Location))
                .ToListAsync(cancellationToken);


            // SolicitorId is a SHA-256 hash of name + address + source + location.
            // Deduplicate scraped results — the same card can appear twice on a page.
            var deduplicated = scraped.DistinctBy(r => r.SolicitorId).ToList();

            var existingByKey = existing.ToDictionary(e => e.SolicitorId);

            //removal or results removed as it was discovered that the site displays differnet solicitors each time you browse and not a complete list of them

            // Upsert each scraped result
            var enriched = new List<SolicitorResult>(deduplicated.Count);

            foreach (var result in deduplicated)
            {
                bool isNew = !existingByKey.TryGetValue(result.SolicitorId, out var entity);

                if (isNew)
                {
                    entity = new SolicitorEntity
                    {
                        SolicitorId = result.SolicitorId,
                        ProfileUrl  = result.ProfileUrl,
                        Source      = result.Source,
                        FirstSeenAt = now
                    };
                    db.Solicitors.Add(entity);
                }

                // Update all fields on every run so the DB stays current
                entity!.Name        = result.Name;
                entity.Location     = result.Location;
                entity.Address      = result.Address;
                entity.Phone        = result.Phone;
                entity.Website      = result.Website;
                entity.Rating       = result.Rating;
                entity.ReviewCount  = result.ReviewCount;
                entity.LastSeenAt   = now;

                enriched.Add(result with { IsNew = isNew });
            }

            await db.SaveChangesAsync(cancellationToken);

            return enriched;
        }
    }
}
