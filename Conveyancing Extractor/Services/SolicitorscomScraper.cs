using Conveyancing_Extractor.Domain;
using Conveyancing_Extractor.Services.Interfaces;
using System.Text.RegularExpressions;

namespace Conveyancing_Extractor.Services
{
    /// <summary>
    /// Scrapes conveyancing solicitor listings from solicitors.com.
    /// Parsing uses targeted regexes against the site's known, stable markup.
    /// No third-party HTML libraries are used.
    /// </summary>
    public class SolicitorscomScraper : IScraper
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SolicitorscomScraper> _logger;

        private const string BaseUrl = "https://www.solicitors.com";

        public string Source => "solicitors.com";

        public SolicitorscomScraper(HttpClient httpClient, ILogger<SolicitorscomScraper> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<SolicitorResult>> ScrapeAsync(
            IEnumerable<string> locations,
            CancellationToken cancellationToken = default)
        {
            var results = new List<SolicitorResult>();

            foreach (var location in locations)
            {
                if (cancellationToken.IsCancellationRequested) break;

                _logger.LogInformation("Scraping location: {Location}", location);

                try
                {
                    var locationResults = await ScrapeLocationAsync(location.Trim(), cancellationToken);
                    results.AddRange(locationResults);
                    _logger.LogInformation("Found {Count} solicitors for {Location}", locationResults.Count, location);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to scrape location: {Location}", location);
                }
            }

            return results;
        }

        // ── Fetch ─────────────────────────────────────────────────────────────

        private async Task<List<SolicitorResult>> ScrapeLocationAsync(string location, CancellationToken ct)
        {
            var html = await FetchPageAsync(location, ct);
            return ParseListings(html, location);
        }

        private async Task<string> FetchPageAsync(string location, CancellationToken ct)
        {
            var slug = location.ToLowerInvariant().Replace(' ', '-');
            var uri = $"{BaseUrl}/conveyancing+{slug}.html";
            _logger.LogInformation("Fetching {Uri}", uri);

            try
            {
                var response = await _httpClient.GetAsync(uri, ct);
                _logger.LogInformation("Response {Status} from {Uri}", (int)response.StatusCode, uri);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Non-success HTTP {Status} for {Uri}", response.StatusCode, uri);
                    return string.Empty;
                }

                var html = await response.Content.ReadAsStringAsync(ct);
                _logger.LogInformation("Received {Bytes} bytes. Has result-section: {HasSection}",
                    html.Length, html.Contains("result-section", StringComparison.OrdinalIgnoreCase));

                return html;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HTTP error fetching {Uri}", uri);
                return string.Empty;
            }
        }

        // ── Parse ─────────────────────────────────────────────────────────────

        // Matches the opening tag of every result card: <div class="result-item"> or
        // <div class="result-item item-small"> etc.
        private static readonly Regex CardOpener =
            new(@"<div[^>]+class=""[^""]*result-item[^""]*""", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private List<SolicitorResult> ParseListings(string html, string location)
        {
            // Narrow scope to the result-section so sidebar content is excluded.
            // We stop at the sidebar div to avoid greedily consuming it.
            var sectionMatch = Regex.Match(html,
                @"<div[^>]+class=""[^""]*result-section[^""]*"">(.+?)<aside",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var section = sectionMatch.Success ? sectionMatch.Value : html;

            // Split on each card-opening tag; CardOpener.Split gives the text
            // between cards, while CardOpener.Matches gives the opening tags.
            var betweenCards = CardOpener.Split(section);
            var openers      = CardOpener.Matches(section);

            var results = new List<SolicitorResult>();

            for (int i = 0; i < openers.Count; i++)
            {
                // Reassemble: the opening tag + everything until the next card boundary
                var card = openers[i].Value + betweenCards[i + 1];

                // Skip banner / ad blocks that share the result-item class
                if (card.Contains("banner-block", StringComparison.OrdinalIgnoreCase))
                    continue;

                var result = ParseCard(card, location);
                if (!string.IsNullOrWhiteSpace(result.Name))
                    results.Add(result);
            }

            return results;
        }

        private SolicitorResult ParseCard(string card, string location)
        {
            // ── Name ──────────────────────────────────────────────────────────
            // <span class="h2">Firm Name<div ...greentick...><span ...rev-results...>
            // The text node we want comes before any child tag, so we stop at the
            // first '<' after the opening span tag.
            var name = HtmlParser.InnerText(
                HtmlParser.Match(card, @"<span[^>]+class=""[^""]*\bh2\b[^""]*""[^>]*>([^<]+)"));

            // ── Profile URL ───────────────────────────────────────────────────
            // Always: <a href="/slug.html" class="link-map">
            var profileHref = HtmlParser.Match(card, @"href=""(/[^""]+\.html)""\s+class=""link-map""");
            var profileUrl = profileHref.StartsWith('/') ? BaseUrl + profileHref : profileHref;

            // ── Address ───────────────────────────────────────────────────────
            // <address>...</address>  always inside the link-map anchor
            var address = HtmlParser.InnerText(
                HtmlParser.Match(card, @"<address>(.+?)</address>"));

            // ── Phone ─────────────────────────────────────────────────────────
            // href="tel:01217458550"
            var phone = HtmlParser.Match(card, @"href=""tel:([^""]+)""");

            // ── Website ───────────────────────────────────────────────────────
            // target="_blank" with an absolute URL, inside the list-item block
            // Always: <a ... href="https://...">...Website</a>
            var website = HtmlParser.Match(card, @"href=""(https?://[^""]+)""[^>]*>[^<]*Website");

            // ── Rating & review count ─────────────────────────────────────────
            // Stars live inside <span class="rev-results">
            var revSpan = HtmlParser.Match(card,
                @"<span[^>]+class=""[^""]*rev-results[^""]*"">(.+?)</span>");

            var fullStars = HtmlParser.Count(revSpan, "star-full");
            var halfStars = HtmlParser.Count(revSpan, "star-half");
            double? rating = (fullStars > 0 || halfStars > 0) ? fullStars + halfStars * 0.5 : null;

            var reviewText = HtmlParser.Match(revSpan, @"\((\d+)\)");
            int? reviewCount = int.TryParse(reviewText, out var rc) ? rc : null;

            // -- Solicitor ID --------------------------------------------------
            // SHA-256 of normalised address + source + location gives a stable, branch-unique, location unique identifier
            
            var solicitorId = ComputeId(name, address, Source, location);

            return new SolicitorResult
            {
                SolicitorId = solicitorId,
                Name        = name,
                Location    = location,
                Source      = Source,
                Address     = address,
                Phone       = phone,
                Website     = website,
                Rating      = rating,
                ReviewCount = reviewCount,
                ProfileUrl  = profileUrl,
                ScrapedAt   = DateTime.UtcNow
            };
        }

        private static string ComputeId(string name, string address, string source, string location)
        {
            var input = $"{name.Trim().ToLowerInvariant()}|{address.Trim().ToLowerInvariant()}|{source.ToLowerInvariant()}|{location.ToLowerInvariant()}";
            var hash  = System.Security.Cryptography.SHA256.HashData(
                            System.Text.Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}