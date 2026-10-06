using System.Globalization;
using System.Text;
using System.Text.Json;
using Pharma.Models;
using SkyNet;

namespace Pharma.codes
{
    public class Home : WebPage
    {
        public override async Task OnInitialized()
        {
            HtmlDoc.SetTitle("Calvera Therapeutics | Science for longer, healthier lives");
            HtmlDoc.AddMetaElement("viewport", "width=device-width, initial-scale=1");
            HtmlDoc.AddMetaElement("description", "Calvera Therapeutics discovers and develops medicines for serious diseases.");

            SiteData site = await LoadSite();
            HtmlDoc.HtmlBodyText = HtmlDoc.HtmlBodyText
                .Replace("{plhd_stats}", Stats(site))
                .Replace("{plhd_phasebars}", PhaseBars(site))
                .Replace("{plhd_invest}", InvestSnapshot(site))
                .Replace("{plhd_products}", ProductGrid(site, site.Products.OrderByDescending(p => p.Year).ThenBy(p => p.Brand).Take(4).ToList()))
                .Replace("{plhd_news}", NewsGrid(site, site.News.OrderByDescending(n => n.Date).Take(3).ToList()));
        }

        private static string Stats(SiteData site)
        {
            int dev = site.Programs.Count(p => p.Phase != "approved");
            int rec = site.Trials.Count(t => t.Status == "recruiting");
            string[,] items =
            {
                { site.Products.Count.ToString(CultureInfo.InvariantCulture), "medicines on the market" },
                { dev.ToString(CultureInfo.InvariantCulture), "programs in development" },
                { rec.ToString(CultureInfo.InvariantCulture), "trials recruiting now" },
                { "40+", "countries we serve" }
            };
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < items.GetLength(0); i++)
            {
                sb.Append("<div class=\"hm-stat\"><b>" + items[i, 0] + "</b><span>" + items[i, 1] + "</span></div>");
            }
            return sb.ToString();
        }

        private static string PhaseBars(SiteData site)
        {
            int max = Math.Max(1, site.Phases.Max(ph => site.Programs.Count(p => p.Phase == ph.Key)));
            StringBuilder sb = new StringBuilder();
            foreach (KeyLabel ph in site.Phases)
            {
                int n = site.Programs.Count(p => p.Phase == ph.Key);
                int w = (int)Math.Round(n * 100.0 / max);
                sb.Append("<a class=\"hm-pbar\" href=\"Pipeline?phase=" + ph.Key + "\"><span class=\"hm-pbar-l\">" + HtmlEncode(ph.Label) + "</span>");
                sb.Append("<span class=\"hm-pbar-t\"><i style=\"width:" + w + "%\"></i></span><b>" + n + "</b></a>");
            }
            return sb.ToString();
        }

        private static string InvestSnapshot(SiteData site)
        {
            List<decimal> pts = Range(site.Prices, "1y");
            decimal last = pts[pts.Count - 1];
            decimal first = pts[0];
            decimal pct = first == 0 ? 0 : (last - first) / first * 100m;
            Quarter q = site.Quarters[site.Quarters.Count - 1];
            StringBuilder sb = new StringBuilder();
            sb.Append("<div class=\"hm-snap\"><div class=\"hm-snap-p\"><b>$" + Money(last) + "</b>");
            sb.Append("<span class=\"" + (pct >= 0 ? "hm-up" : "hm-down") + "\">" + (pct >= 0 ? "+" : "") + pct.ToString("0.0", CultureInfo.InvariantCulture) + "% 1Y</span></div>");
            sb.Append("<div class=\"hm-spark\">" + Spark(pts) + "</div></div>");
            sb.Append("<div class=\"hm-snap-k\"><div><span>" + HtmlEncode(q.Name) + " revenue</span><b>$" + q.Revenue.ToString("0.00", CultureInfo.InvariantCulture) + "B</b></div>");
            sb.Append("<div><span>" + HtmlEncode(q.Name) + " EPS</span><b>$" + Money(q.Eps) + "</b></div>");
            sb.Append("<div><span>Quarterly dividend</span><b>$" + Money(q.Dividend) + "</b></div></div>");
            return sb.ToString();
        }

        private static string Spark(List<decimal> pts)
        {
            decimal min = pts.Min();
            decimal max = pts.Max();
            decimal span = max - min == 0 ? 1 : max - min;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < pts.Count; i++)
            {
                double x = i * 300.0 / (pts.Count - 1);
                double y = 70 - (double)((pts[i] - min) / span) * 62 - 4;
                sb.Append(x.ToString("0.0", CultureInfo.InvariantCulture) + "," + y.ToString("0.0", CultureInfo.InvariantCulture) + " ");
            }
            return "<svg viewBox=\"0 0 300 70\" preserveAspectRatio=\"none\" aria-hidden=\"true\"><polyline points=\"" + sb.ToString().Trim() + "\" fill=\"none\" stroke=\"#7FD8CB\" stroke-width=\"2.5\" vector-effect=\"non-scaling-stroke\"/></svg>";
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
                sb.Append("<div class=\"hm-sg-none\">No results for &ldquo;" + HtmlEncode(q) + "&rdquo;</div>");
            }
            else
            {
                foreach (string r in rows.Take(7))
                {
                    sb.Append(r);
                }
                sb.Append("<div class=\"hm-sg-foot\">" + total + (total == 1 ? " result" : " results") + " across medicines, pipeline, trials and news</div>");
            }
            response.SetElementContents("hm-sugg", sb.ToString());
            response.ExecuteScript("HomeJs.openSugg();");
            return response;
        }

        private static string Sugg(string type, string color, string title, string sub, string href)
        {
            return "<a class=\"hm-sg\" href=\"" + href + "\"><span class=\"hm-sg-t\" style=\"--ac:" + color + "\">" + type + "</span><span><b>" + HtmlEncode(title) + "</b><small>" + HtmlEncode(sub) + "</small></span></a>";
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
            sb.Append("<button type=\"button\" class=\"hm-chip" + (active == string.Empty ? " hm-act" : string.Empty) + "\" onclick=\"HomeJs.chip(this, '" + group + "', '')\">All</button>");
            foreach (KeyLabel k in items)
            {
                sb.Append("<button type=\"button\" class=\"hm-chip" + (k.Key == active ? " hm-act" : string.Empty) + "\" onclick=\"HomeJs.chip(this, '" + group + "', '" + k.Key + "')\">" + HtmlEncode(k.Label) + "</button>");
            }
            return sb.ToString();
        }

        private static string ProductGrid(SiteData site, List<Medicine> list)
        {
            if (list.Count == 0)
            {
                return "<div class=\"hm-empty\">No medicines in this area yet.</div>";
            }
            StringBuilder sb = new StringBuilder();
            foreach (Medicine m in list)
            {
                AreaInfo a = AreaOf(site, m.Area);
                sb.Append("<div class=\"hm-pcard\" style=\"--ac:" + a.Color + ";--at:" + a.Tint + "\">");
                sb.Append("<div class=\"hm-pcard-img\"><span class=\"hm-tag\">" + HtmlEncode(a.Label) + "</span><img src=\"" + m.Image + "\" alt=\"" + HtmlEncode(m.Brand) + " pack\" loading=\"lazy\"></div>");
                sb.Append("<div class=\"hm-pcard-b\"><b class=\"hm-brand\">" + HtmlEncode(m.Brand) + "</b><span class=\"hm-gen\">" + HtmlEncode(m.Generic) + "</span>");
                sb.Append("<p>" + HtmlEncode(m.Indication) + "</p>");
                sb.Append("<div class=\"hm-meta\"><span>" + HtmlEncode(m.FormLabel) + "</span><span>Approved " + m.Year + "</span></div>");
                sb.Append("<div class=\"hm-plinks\"><a href=\"#\" onclick=\"return false;\">Prescribing information</a><a href=\"#\" onclick=\"return false;\">Patient information</a></div></div></div>");
            }
            return sb.ToString();
        }

        private static string NewsGrid(SiteData site, List<NewsItem> list)
        {
            if (list.Count == 0)
            {
                return "<div class=\"hm-empty\">No releases match these filters.</div>";
            }
            StringBuilder sb = new StringBuilder();
            foreach (NewsItem n in list)
            {
                string cat = site.NewsCats.Where(c => c.Key == n.Cat).Select(c => c.Label).FirstOrDefault() ?? n.Cat;
                sb.Append("<article class=\"hm-ncard\"><div class=\"hm-nmeta\"><span class=\"hm-ncat hm-c-" + n.Cat + "\">" + HtmlEncode(cat) + "</span><time>" + DateText(n.Date) + "</time></div>");
                sb.Append("<h3><a href=\"#\" onclick=\"return false;\">" + HtmlEncode(n.Title) + "</a></h3><p>" + HtmlEncode(n.Summary) + "</p>");
                sb.Append("<a class=\"hm-nmore\" href=\"#\" onclick=\"return false;\">Read more &rarr;</a></article>");
            }
            return sb.ToString();
        }

        private static string TrialList(SiteData site, List<Trial> list)
        {
            if (list.Count == 0)
            {
                return "<div class=\"hm-empty\">No studies match your search. Try fewer filters.</div>";
            }
            StringBuilder sb = new StringBuilder();
            foreach (Trial t in list)
            {
                AreaInfo a = AreaOf(site, t.Area);
                string locs = string.Join(", ", t.Locations.Take(2)) + (t.Locations.Count > 2 ? " +" + (t.Locations.Count - 2) + " more" : string.Empty);
                sb.Append("<div class=\"hm-trial\" style=\"--ac:" + a.Color + "\"><div class=\"hm-trial-top\">");
                sb.Append("<span class=\"hm-st hm-st-" + t.Status + "\">" + HtmlEncode(StatusLabel(site, t.Status)) + "</span>");
                sb.Append("<span class=\"hm-ph\">" + HtmlEncode(PhaseLabel(site, t.Phase)) + "</span><span class=\"hm-tid\">" + HtmlEncode(t.Id) + "</span></div>");
                sb.Append("<h3>" + HtmlEncode(t.Title) + "</h3><div class=\"hm-cond\"><i></i>" + HtmlEncode(t.Condition) + " &middot; " + HtmlEncode(a.Label) + "</div>");
                sb.Append("<div class=\"hm-facts\"><span>Ages " + HtmlEncode(t.Ages) + "</span><span>" + t.Enrollment.ToString("N0", CultureInfo.InvariantCulture) + " participants</span><span>" + HtmlEncode(locs) + "</span></div>");
                sb.Append("<button type=\"button\" class=\"hm-dbtn\" aria-expanded=\"false\" onclick=\"HomeJs.detail(this, '" + t.Id + "')\">Study details</button>");
                sb.Append("<div class=\"hm-detail\" id=\"hm-d-" + t.Id + "\"></div></div>");
            }
            return sb.ToString();
        }

        private static string Offices(SiteData site)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Office o in site.Offices)
            {
                sb.Append("<div class=\"hm-office\"><b>" + HtmlEncode(o.Name) + "</b><span>" + HtmlEncode(o.Line1) + "</span><span>" + HtmlEncode(o.Line2) + "</span><a href=\"#\" onclick=\"return false;\">" + HtmlEncode(o.Phone) + "</a></div>");
            }
            return sb.ToString();
        }
    }
}
