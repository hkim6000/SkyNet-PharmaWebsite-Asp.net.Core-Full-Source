using System.Globalization;
using System.Text;
using System.Text.Json;
using Pharma.Models;
using SkyNet;

namespace Pharma.codes
{
    public class Trials : WebPage
    {
        public override async Task OnInitialized()
        {
            HtmlDoc.SetTitle("Find a Clinical Trial | Calvera Therapeutics");
            HtmlDoc.AddMetaElement("viewport", "width=device-width, initial-scale=1");
            HtmlDoc.AddMetaElement("description", "Search Calvera clinical trials by condition, phase, status and location.");

            SiteData site = await LoadSite();
            string q = Clip(QueryValue("q"));
            string phase = Pick(QueryValue("phase"), site.Phases.Select(p => p.Key));
            string status = Pick(QueryValue("status"), site.Statuses.Select(s => s.Key));
            string area = Pick(QueryValue("area"), site.Areas.Select(a => a.Key));
            List<Trial> list = Find(site, q, phase, status, area, string.Empty);
            HtmlDoc.HtmlBodyText = HtmlDoc.HtmlBodyText
                .Replace("{plhd_q}", HtmlEncode(q))
                .Replace("{plhd_v_phase}", phase)
                .Replace("{plhd_v_status}", status)
                .Replace("{plhd_v_area}", area)
                .Replace("{plhd_v_region}", string.Empty)
                .Replace("{plhd_count}", CountText(list.Count, "study", "studies"))
                .Replace("{plhd_list}", TrialList(site, list));
        }

        public async Task<ApiResponse> Filter()
        {
            ApiResponse response = new ApiResponse();
            SiteData site = await LoadSite();
            string q = Clip(GetDataValue("q"));
            string phase = Pick(GetDataValue("phase"), site.Phases.Select(p => p.Key));
            string status = Pick(GetDataValue("status"), site.Statuses.Select(s => s.Key));
            string area = Pick(GetDataValue("area"), site.Areas.Select(a => a.Key));
            string region = Pick(GetDataValue("region"), new[] { "us", "intl" });
            List<Trial> list = Find(site, q, phase, status, area, region);
            response.SetElementContents("ct-list", TrialList(site, list));
            response.SetElementContents("ct-count", CountText(list.Count, "study", "studies") + (q == string.Empty ? string.Empty : " for &ldquo;" + HtmlEncode(q) + "&rdquo;"));
            return response;
        }

        public async Task<ApiResponse> Detail()
        {
            ApiResponse response = new ApiResponse();
            SiteData site = await LoadSite();
            string id = (GetDataValue("id") ?? string.Empty).Trim();
            Trial? t = site.Trials.FirstOrDefault(x => x.Id == id);
            if (t == null)
            {
                return response;
            }
            PipelineItem? p = site.Programs.FirstOrDefault(x => x.Code == t.Program);
            StringBuilder sb = new StringBuilder();
            sb.Append("<div class=\"ct-dgrid\"><div><h4>About this study</h4><p>" + HtmlEncode(t.Summary) + "</p>");
            sb.Append("<h4>Who can take part</h4><ul>");
            sb.Append("<li>Ages " + HtmlEncode(t.Ages) + "</li>");
            foreach (string c in t.Criteria)
            {
                sb.Append("<li>" + HtmlEncode(c) + "</li>");
            }
            sb.Append("</ul></div><div><h4>Study facts</h4><dl>");
            sb.Append("<dt>Study ID</dt><dd>" + HtmlEncode(t.Id) + "</dd>");
            sb.Append("<dt>Investigational medicine</dt><dd>" + HtmlEncode(p == null ? t.Program : p.Code + (p.Name != p.Code ? " (" + p.Name + ")" : string.Empty)) + "</dd>");
            sb.Append("<dt>Phase</dt><dd>" + HtmlEncode(PhaseLabel(site, t.Phase)) + "</dd>");
            sb.Append("<dt>Participants</dt><dd>" + t.Enrollment.ToString("N0", CultureInfo.InvariantCulture) + "</dd>");
            sb.Append("<dt>Start</dt><dd>" + HtmlEncode(MonthText(t.Start)) + "</dd>");
            sb.Append("<dt>Estimated completion</dt><dd>" + HtmlEncode(MonthText(t.Completion)) + "</dd></dl>");
            sb.Append("<h4>Locations</h4><ul class=\"ct-locs\">");
            foreach (string l in t.Locations)
            {
                sb.Append("<li>" + HtmlEncode(l) + "</li>");
            }
            sb.Append("</ul></div></div>");
            if (t.Status == "recruiting")
            {
                sb.Append("<div class=\"ct-dcta\"><span>Interested? Ask your doctor, or contact the study team.</span><a class=\"ct-btn ct-btn-sm\" href=\"Contact\">Contact the study team</a></div>");
            }
            response.SetElementContents("ct-d-" + t.Id, sb.ToString());
            response.ExecuteScript("TrialsJs.opened('" + t.Id + "');");
            return response;
        }

        private static List<Trial> Find(SiteData site, string q, string phase, string status, string area, string region)
        {
            return site.Trials
                .Where(t => q == string.Empty || Has(t.Id, q) || Has(t.Title, q) || Has(t.Condition, q) || Has(t.Program, q) || Has(ProgramName(site, t.Program), q))
                .Where(t => phase == string.Empty || t.Phase == phase)
                .Where(t => status == string.Empty || t.Status == status)
                .Where(t => area == string.Empty || t.Area == area)
                .Where(t => region == string.Empty || t.Regions.Contains(region))
                .OrderBy(t => site.Statuses.FindIndex(s => s.Key == t.Status))
                .ThenByDescending(t => t.Start)
                .ToList();
        }

        private static string ProgramName(SiteData site, string code)
        {
            PipelineItem? p = site.Programs.FirstOrDefault(x => x.Code == code);
            return p == null ? string.Empty : p.Name;
        }

        private static string MonthText(string ym)
        {
            DateTime d;
            if (DateTime.TryParseExact(ym + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
            {
                return d.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
            }
            return ym;
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
                sb.Append("<div class=\"ct-sg-none\">No results for &ldquo;" + HtmlEncode(q) + "&rdquo;</div>");
            }
            else
            {
                foreach (string r in rows.Take(7))
                {
                    sb.Append(r);
                }
                sb.Append("<div class=\"ct-sg-foot\">" + total + (total == 1 ? " result" : " results") + " across medicines, pipeline, trials and news</div>");
            }
            response.SetElementContents("ct-sugg", sb.ToString());
            response.ExecuteScript("TrialsJs.openSugg();");
            return response;
        }

        private static string Sugg(string type, string color, string title, string sub, string href)
        {
            return "<a class=\"ct-sg\" href=\"" + href + "\"><span class=\"ct-sg-t\" style=\"--ac:" + color + "\">" + type + "</span><span><b>" + HtmlEncode(title) + "</b><small>" + HtmlEncode(sub) + "</small></span></a>";
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
            sb.Append("<button type=\"button\" class=\"ct-chip" + (active == string.Empty ? " ct-act" : string.Empty) + "\" onclick=\"TrialsJs.chip(this, '" + group + "', '')\">All</button>");
            foreach (KeyLabel k in items)
            {
                sb.Append("<button type=\"button\" class=\"ct-chip" + (k.Key == active ? " ct-act" : string.Empty) + "\" onclick=\"TrialsJs.chip(this, '" + group + "', '" + k.Key + "')\">" + HtmlEncode(k.Label) + "</button>");
            }
            return sb.ToString();
        }

        private static string ProductGrid(SiteData site, List<Medicine> list)
        {
            if (list.Count == 0)
            {
                return "<div class=\"ct-empty\">No medicines in this area yet.</div>";
            }
            StringBuilder sb = new StringBuilder();
            foreach (Medicine m in list)
            {
                AreaInfo a = AreaOf(site, m.Area);
                sb.Append("<div class=\"ct-pcard\" style=\"--ac:" + a.Color + ";--at:" + a.Tint + "\">");
                sb.Append("<div class=\"ct-pcard-img\"><span class=\"ct-tag\">" + HtmlEncode(a.Label) + "</span><img src=\"" + m.Image + "\" alt=\"" + HtmlEncode(m.Brand) + " pack\" loading=\"lazy\"></div>");
                sb.Append("<div class=\"ct-pcard-b\"><b class=\"ct-brand\">" + HtmlEncode(m.Brand) + "</b><span class=\"ct-gen\">" + HtmlEncode(m.Generic) + "</span>");
                sb.Append("<p>" + HtmlEncode(m.Indication) + "</p>");
                sb.Append("<div class=\"ct-meta\"><span>" + HtmlEncode(m.FormLabel) + "</span><span>Approved " + m.Year + "</span></div>");
                sb.Append("<div class=\"ct-plinks\"><a href=\"#\" onclick=\"return false;\">Prescribing information</a><a href=\"#\" onclick=\"return false;\">Patient information</a></div></div></div>");
            }
            return sb.ToString();
        }

        private static string NewsGrid(SiteData site, List<NewsItem> list)
        {
            if (list.Count == 0)
            {
                return "<div class=\"ct-empty\">No releases match these filters.</div>";
            }
            StringBuilder sb = new StringBuilder();
            foreach (NewsItem n in list)
            {
                string cat = site.NewsCats.Where(c => c.Key == n.Cat).Select(c => c.Label).FirstOrDefault() ?? n.Cat;
                sb.Append("<article class=\"ct-ncard\"><div class=\"ct-nmeta\"><span class=\"ct-ncat ct-c-" + n.Cat + "\">" + HtmlEncode(cat) + "</span><time>" + DateText(n.Date) + "</time></div>");
                sb.Append("<h3><a href=\"#\" onclick=\"return false;\">" + HtmlEncode(n.Title) + "</a></h3><p>" + HtmlEncode(n.Summary) + "</p>");
                sb.Append("<a class=\"ct-nmore\" href=\"#\" onclick=\"return false;\">Read more &rarr;</a></article>");
            }
            return sb.ToString();
        }

        private static string TrialList(SiteData site, List<Trial> list)
        {
            if (list.Count == 0)
            {
                return "<div class=\"ct-empty\">No studies match your search. Try fewer filters.</div>";
            }
            StringBuilder sb = new StringBuilder();
            foreach (Trial t in list)
            {
                AreaInfo a = AreaOf(site, t.Area);
                string locs = string.Join(", ", t.Locations.Take(2)) + (t.Locations.Count > 2 ? " +" + (t.Locations.Count - 2) + " more" : string.Empty);
                sb.Append("<div class=\"ct-trial\" style=\"--ac:" + a.Color + "\"><div class=\"ct-trial-top\">");
                sb.Append("<span class=\"ct-st ct-st-" + t.Status + "\">" + HtmlEncode(StatusLabel(site, t.Status)) + "</span>");
                sb.Append("<span class=\"ct-ph\">" + HtmlEncode(PhaseLabel(site, t.Phase)) + "</span><span class=\"ct-tid\">" + HtmlEncode(t.Id) + "</span></div>");
                sb.Append("<h3>" + HtmlEncode(t.Title) + "</h3><div class=\"ct-cond\"><i></i>" + HtmlEncode(t.Condition) + " &middot; " + HtmlEncode(a.Label) + "</div>");
                sb.Append("<div class=\"ct-facts\"><span>Ages " + HtmlEncode(t.Ages) + "</span><span>" + t.Enrollment.ToString("N0", CultureInfo.InvariantCulture) + " participants</span><span>" + HtmlEncode(locs) + "</span></div>");
                sb.Append("<button type=\"button\" class=\"ct-dbtn\" aria-expanded=\"false\" onclick=\"TrialsJs.detail(this, '" + t.Id + "')\">Study details</button>");
                sb.Append("<div class=\"ct-detail\" id=\"ct-d-" + t.Id + "\"></div></div>");
            }
            return sb.ToString();
        }

        private static string Offices(SiteData site)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Office o in site.Offices)
            {
                sb.Append("<div class=\"ct-office\"><b>" + HtmlEncode(o.Name) + "</b><span>" + HtmlEncode(o.Line1) + "</span><span>" + HtmlEncode(o.Line2) + "</span><a href=\"#\" onclick=\"return false;\">" + HtmlEncode(o.Phone) + "</a></div>");
            }
            return sb.ToString();
        }
    }
}
