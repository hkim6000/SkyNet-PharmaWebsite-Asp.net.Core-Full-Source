using System.Globalization;
using System.Text;
using System.Text.Json;
using Pharma.Models;
using SkyNet;

namespace Pharma.codes
{
    public class News : WebPage
    {
        public override async Task OnInitialized()
        {
            HtmlDoc.SetTitle("News & Press Releases | Calvera Therapeutics");
            HtmlDoc.AddMetaElement("viewport", "width=device-width, initial-scale=1");
            HtmlDoc.AddMetaElement("description", "The latest news from Calvera Therapeutics.");

            SiteData site = await LoadSite();
            string cat = Pick(QueryValue("cat"), site.NewsCats.Select(c => c.Key));
            List<NewsItem> list = Articles(site, cat, string.Empty);
            HtmlDoc.HtmlBodyText = HtmlDoc.HtmlBodyText
                .Replace("{plhd_chips}", Chips(site.NewsCats, "cat", cat))
                .Replace("{plhd_cat}", cat)
                .Replace("{plhd_v_year}", string.Empty)
                .Replace("{plhd_count}", CountText(list.Count, "release", "releases"))
                .Replace("{plhd_grid}", NewsGrid(site, list.Take(PageSize).ToList()))
                .Replace("{plhd_more}", MoreBox(list.Count, PageSize));
        }

        private const int PageSize = 9;

        public async Task<ApiResponse> Filter()
        {
            return await Show(0);
        }

        public async Task<ApiResponse> More()
        {
            int shown;
            int.TryParse(GetDataValue("shown"), out shown);
            return await Show(Math.Max(0, shown));
        }

        private async Task<ApiResponse> Show(int shown)
        {
            ApiResponse response = new ApiResponse();
            SiteData site = await LoadSite();
            string cat = Pick(GetDataValue("cat"), site.NewsCats.Select(c => c.Key));
            string year = Pick(GetDataValue("year"), site.News.Select(n => n.Date.Substring(0, 4)).Distinct());
            List<NewsItem> list = Articles(site, cat, year);
            int take = Math.Min(list.Count, shown + PageSize);
            response.SetElementContents("nw-grid", NewsGrid(site, list.Take(take).ToList()));
            response.SetElementContents("nw-count", CountText(list.Count, "release", "releases"));
            response.SetElementContents("nw-more", MoreBox(list.Count, take));
            return response;
        }

        private static List<NewsItem> Articles(SiteData site, string cat, string year)
        {
            return site.News
                .Where(n => (cat == string.Empty || n.Cat == cat) && (year == string.Empty || n.Date.StartsWith(year, StringComparison.Ordinal)))
                .OrderByDescending(n => n.Date)
                .ToList();
        }

        private static string MoreBox(int total, int shown)
        {
            if (total == 0)
            {
                return string.Empty;
            }
            if (shown >= total)
            {
                return "<span class=\"nw-done\">Showing all " + total + " releases</span>";
            }
            return "<button type=\"button\" class=\"nw-btn nw-btn-o\" onclick=\"NewsJs.more()\">Load more (" + (total - shown) + ")</button>";
        }

        public async Task<ApiResponse> Search()
        {
            ApiResponse response = new ApiResponse();
            string q = Clip(GetDataValue("q"));
            SiteData site = await LoadSite();
            List<string> rows = new List<string>();
            int total = 0;
            if (q.Length >= 2)
            {
                foreach (Medicine m in site.Products.Where(m => Has(m.Brand, q) || Has(m.Generic, q) || Has(m.Indication, q)))
                {
                    total++;
                    rows.Add(Sugg("Medicine", AreaOf(site, m.Area).Color, m.Brand + " (" + m.Generic + ")", m.Indication, "Products?area=" + m.Area));
                }
                foreach (PipelineItem p in site.Programs.Where(p => p.Phase != "approved" && (Has(p.Code, q) || Has(p.Name, q) || Has(p.Indication, q))))
                {
                    total++;
                    rows.Add(Sugg("Pipeline", AreaOf(site, p.Area).Color, p.Code + (p.Name != p.Code ? " · " + p.Name : string.Empty), PhaseLabel(site, p.Phase) + " · " + p.Indication, "Pipeline?area=" + p.Area));
                }
                foreach (Trial t in site.Trials.Where(t => Has(t.Id, q) || Has(t.Title, q) || Has(t.Condition, q)))
                {
                    total++;
                    rows.Add(Sugg("Trial", AreaOf(site, t.Area).Color, t.Title, t.Id + " · " + StatusLabel(site, t.Status), "Trials?q=" + t.Id));
                }
                foreach (NewsItem n in site.News.Where(n => Has(n.Title, q)).OrderByDescending(n => n.Date))
                {
                    total++;
                    rows.Add(Sugg("News", "#5B6B75", n.Title, DateText(n.Date), "News?cat=" + n.Cat));
                }
            }

            StringBuilder sb = new StringBuilder();
            if (total == 0)
            {
                sb.Append("<div class=\"nw-sg-none\">No results for &ldquo;" + HtmlEncode(q) + "&rdquo;</div>");
            }
            else
            {
                foreach (string r in rows.Take(7))
                {
                    sb.Append(r);
                }
                sb.Append("<div class=\"nw-sg-foot\">" + total + (total == 1 ? " result" : " results") + " across medicines, pipeline, trials and news</div>");
            }
            response.SetElementContents("nw-sugg", sb.ToString());
            response.ExecuteScript("NewsJs.openSugg();");
            return response;
        }

        private static string Sugg(string type, string color, string title, string sub, string href)
        {
            return "<a class=\"nw-sg\" href=\"" + href + "\"><span class=\"nw-sg-t\" style=\"--ac:" + color + "\">" + type + "</span><span><b>" + HtmlEncode(title) + "</b><small>" + HtmlEncode(sub) + "</small></span></a>";
        }

        private async Task<SiteData> LoadSite()
        {
            string file = Path.Combine(DataPath ?? string.Empty, "site.json");
            if (!File.Exists(file))
            {
                file = Path.Combine(Directory.GetCurrentDirectory(), "data", "site.json");
            }
            if (!File.Exists(file))
            {
                return new SiteData();
            }
            string json = await File.ReadAllTextAsync(file);
            JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<SiteData>(json, options) ?? new SiteData();
        }

        private static string Clip(string? value)
        {
            string q = (value ?? string.Empty).Trim();
            return q.Length > 40 ? q.Substring(0, 40) : q;
        }

        private static string Pick(string? value, IEnumerable<string> allowed)
        {
            string v = (value ?? string.Empty).Trim().ToLowerInvariant();
            return allowed.Contains(v) ? v : string.Empty;
        }

        private static bool Has(string text, string q)
        {
            return (text ?? string.Empty).Contains(q, StringComparison.OrdinalIgnoreCase);
        }

        private static string Money(decimal value)
        {
            return value.ToString("0.00", CultureInfo.InvariantCulture);
        }

        private static string DateText(string ymd)
        {
            DateTime d;
            if (DateTime.TryParseExact(ymd, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
            {
                return d.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);
            }
            return ymd;
        }

        private static string CountText(int n, string one, string many)
        {
            return n + " " + (n == 1 ? one : many);
        }

        private static AreaInfo AreaOf(SiteData site, string key)
        {
            return site.Areas.FirstOrDefault(a => a.Key == key) ?? new AreaInfo { Key = key, Label = key, Color = "#5B6B75", Tint = "#EEF3F5" };
        }

        private static string PhaseLabel(SiteData site, string key)
        {
            return site.Phases.Where(p => p.Key == key).Select(p => p.Label).FirstOrDefault() ?? key;
        }

        private static string StatusLabel(SiteData site, string key)
        {
            return site.Statuses.Where(s => s.Key == key).Select(s => s.Label).FirstOrDefault() ?? key;
        }

        private static List<decimal> Range(List<decimal> prices, string range)
        {
            int n = range == "6m" ? 26 : range == "5y" ? prices.Count : 52;
            n = Math.Min(n, prices.Count);
            return prices.Skip(prices.Count - n).ToList();
        }

        private static string Chips(List<KeyLabel> items, string group, string active)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<button type=\"button\" class=\"nw-chip" + (active == string.Empty ? " nw-act" : string.Empty) + "\" onclick=\"NewsJs.chip(this, '" + group + "', '')\">All</button>");
            foreach (KeyLabel k in items)
            {
                sb.Append("<button type=\"button\" class=\"nw-chip" + (k.Key == active ? " nw-act" : string.Empty) + "\" onclick=\"NewsJs.chip(this, '" + group + "', '" + k.Key + "')\">" + HtmlEncode(k.Label) + "</button>");
            }
            return sb.ToString();
        }

        private static string ProductGrid(SiteData site, List<Medicine> list)
        {
            if (list.Count == 0)
            {
                return "<div class=\"nw-empty\">No medicines in this area yet.</div>";
            }
            StringBuilder sb = new StringBuilder();
            foreach (Medicine m in list)
            {
                AreaInfo a = AreaOf(site, m.Area);
                sb.Append("<div class=\"nw-pcard\" style=\"--ac:" + a.Color + ";--at:" + a.Tint + "\">");
                sb.Append("<div class=\"nw-pcard-img\"><span class=\"nw-tag\">" + HtmlEncode(a.Label) + "</span><img src=\"" + m.Image + "\" alt=\"" + HtmlEncode(m.Brand) + " pack\" loading=\"lazy\"></div>");
                sb.Append("<div class=\"nw-pcard-b\"><b class=\"nw-brand\">" + HtmlEncode(m.Brand) + "</b><span class=\"nw-gen\">" + HtmlEncode(m.Generic) + "</span>");
                sb.Append("<p>" + HtmlEncode(m.Indication) + "</p>");
                sb.Append("<div class=\"nw-meta\"><span>" + HtmlEncode(m.FormLabel) + "</span><span>Approved " + m.Year + "</span></div>");
                sb.Append("<div class=\"nw-plinks\"><a href=\"#\" onclick=\"return false;\">Prescribing information</a><a href=\"#\" onclick=\"return false;\">Patient information</a></div></div></div>");
            }
            return sb.ToString();
        }

        private static string NewsGrid(SiteData site, List<NewsItem> list)
        {
            if (list.Count == 0)
            {
                return "<div class=\"nw-empty\">No releases match these filters.</div>";
            }
            StringBuilder sb = new StringBuilder();
            foreach (NewsItem n in list)
            {
                string cat = site.NewsCats.Where(c => c.Key == n.Cat).Select(c => c.Label).FirstOrDefault() ?? n.Cat;
                sb.Append("<article class=\"nw-ncard\"><div class=\"nw-nmeta\"><span class=\"nw-ncat nw-c-" + n.Cat + "\">" + HtmlEncode(cat) + "</span><time>" + DateText(n.Date) + "</time></div>");
                sb.Append("<h3><a href=\"#\" onclick=\"return false;\">" + HtmlEncode(n.Title) + "</a></h3><p>" + HtmlEncode(n.Summary) + "</p>");
                sb.Append("<a class=\"nw-nmore\" href=\"#\" onclick=\"return false;\">Read more &rarr;</a></article>");
            }
            return sb.ToString();
        }

        private static string TrialList(SiteData site, List<Trial> list)
        {
            if (list.Count == 0)
            {
                return "<div class=\"nw-empty\">No studies match your search. Try fewer filters.</div>";
            }
            StringBuilder sb = new StringBuilder();
            foreach (Trial t in list)
            {
                AreaInfo a = AreaOf(site, t.Area);
                string locs = string.Join(", ", t.Locations.Take(2)) + (t.Locations.Count > 2 ? " +" + (t.Locations.Count - 2) + " more" : string.Empty);
                sb.Append("<div class=\"nw-trial\" style=\"--ac:" + a.Color + "\"><div class=\"nw-trial-top\">");
                sb.Append("<span class=\"nw-st nw-st-" + t.Status + "\">" + HtmlEncode(StatusLabel(site, t.Status)) + "</span>");
                sb.Append("<span class=\"nw-ph\">" + HtmlEncode(PhaseLabel(site, t.Phase)) + "</span><span class=\"nw-tid\">" + HtmlEncode(t.Id) + "</span></div>");
                sb.Append("<h3>" + HtmlEncode(t.Title) + "</h3><div class=\"nw-cond\"><i></i>" + HtmlEncode(t.Condition) + " &middot; " + HtmlEncode(a.Label) + "</div>");
                sb.Append("<div class=\"nw-facts\"><span>Ages " + HtmlEncode(t.Ages) + "</span><span>" + t.Enrollment.ToString("N0", CultureInfo.InvariantCulture) + " participants</span><span>" + HtmlEncode(locs) + "</span></div>");
                sb.Append("<button type=\"button\" class=\"nw-dbtn\" aria-expanded=\"false\" onclick=\"NewsJs.detail(this, '" + t.Id + "')\">Study details</button>");
                sb.Append("<div class=\"nw-detail\" id=\"nw-d-" + t.Id + "\"></div></div>");
            }
            return sb.ToString();
        }

        private static string Offices(SiteData site)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Office o in site.Offices)
            {
                sb.Append("<div class=\"nw-office\"><b>" + HtmlEncode(o.Name) + "</b><span>" + HtmlEncode(o.Line1) + "</span><span>" + HtmlEncode(o.Line2) + "</span><a href=\"#\" onclick=\"return false;\">" + HtmlEncode(o.Phone) + "</a></div>");
            }
            return sb.ToString();
        }
    }
}
