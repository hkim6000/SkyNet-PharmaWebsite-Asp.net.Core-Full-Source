using System.Globalization;
using System.Text;
using System.Text.Json;
using Pharma.Models;
using SkyNet;

namespace Pharma.codes
{
    public class Investors : WebPage
    {
        public override async Task OnInitialized()
        {
            HtmlDoc.SetTitle("Investor Relations | Calvera Therapeutics");
            HtmlDoc.AddMetaElement("viewport", "width=device-width, initial-scale=1");
            HtmlDoc.AddMetaElement("description", "Stock information, quarterly results, events and filings.");

            SiteData site = await LoadSite();
            HtmlDoc.HtmlBodyText = HtmlDoc.HtmlBodyText
                .Replace("{plhd_quote}", Quote(site))
                .Replace("{plhd_chart}", Chart(site, "1y"))
                .Replace("{plhd_kstats}", KeyStats(site))
                .Replace("{plhd_results}", Results(site))
                .Replace("{plhd_events}", Events(site))
                .Replace("{plhd_filings}", Filings(site));
        }

        public async Task<ApiResponse> Chart()
        {
            ApiResponse response = new ApiResponse();
            SiteData site = await LoadSite();
            string range = Pick(GetDataValue("range"), new[] { "6m", "1y", "5y" });
            response.SetElementContents("iv-chart", Chart(site, range == string.Empty ? "1y" : range));
            return response;
        }

        private static string Quote(SiteData site)
        {
            List<decimal> p = site.Prices;
            decimal last = p[p.Count - 1];
            decimal prev = p[p.Count - 2];
            decimal ch = last - prev;
            decimal pct = prev == 0 ? 0 : ch / prev * 100m;
            string cls = ch >= 0 ? "iv-up" : "iv-down";
            return "<b>$" + Money(last) + "</b><span class=\"" + cls + "\">" + (ch >= 0 ? "+" : "&minus;") + Money(Math.Abs(ch)) + " (" + (ch >= 0 ? "+" : "&minus;") + Math.Abs(pct).ToString("0.00", CultureInfo.InvariantCulture) + "%) this week</span>";
        }

        private static string Chart(SiteData site, string range)
        {
            List<decimal> pts = Range(site.Prices, range);
            int offset = site.Prices.Count - pts.Count;
            DateTime start = DateTime.ParseExact(site.PriceStart, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            decimal min = pts.Min();
            decimal max = pts.Max();
            decimal pad = (max - min) * 0.08m + 0.5m;
            min -= pad;
            max += pad;
            double W = 1000, H = 300, L = 56, B = 30;
            StringBuilder line = new StringBuilder();
            for (int i = 0; i < pts.Count; i++)
            {
                double x = L + i * (W - L - 10) / (pts.Count - 1);
                double y = 10 + (double)((max - pts[i]) / (max - min)) * (H - B - 10);
                line.Append(F(x) + "," + F(y) + " ");
            }
            string poly = line.ToString().Trim();
            bool up = pts[pts.Count - 1] >= pts[0];
            string col = up ? "#12707A" : "#C2536B";
            StringBuilder sb = new StringBuilder();
            sb.Append("<svg viewBox=\"0 0 1000 300\" role=\"img\" aria-label=\"CVRT share price, " + range.ToUpperInvariant() + "\">");
            sb.Append("<defs><linearGradient id=\"iv-cg\" x1=\"0\" y1=\"0\" x2=\"0\" y2=\"1\"><stop offset=\"0\" stop-color=\"" + col + "\" stop-opacity=\"0.22\"/><stop offset=\"1\" stop-color=\"" + col + "\" stop-opacity=\"0\"/></linearGradient></defs>");
            for (int g = 0; g <= 4; g++)
            {
                double y = 10 + g * (H - B - 10) / 4;
                decimal v = max - (max - min) * g / 4;
                sb.Append("<line x1=\"" + F(L) + "\" y1=\"" + F(y) + "\" x2=\"990\" y2=\"" + F(y) + "\" stroke=\"#DDE6EA\"/>");
                sb.Append("<text x=\"" + F(L - 8) + "\" y=\"" + F(y + 4) + "\" text-anchor=\"end\" font-size=\"12\" fill=\"#5B6B75\">$" + v.ToString("0", CultureInfo.InvariantCulture) + "</text>");
            }
            for (int k = 0; k <= 4; k++)
            {
                int i = (int)Math.Round(k * (pts.Count - 1) / 4.0);
                double x = L + i * (W - L - 10) / (pts.Count - 1);
                string label = start.AddDays(7 * (offset + i)).ToString(range == "5y" ? "MMM yyyy" : "MMM d", CultureInfo.InvariantCulture);
                sb.Append("<text x=\"" + F(x) + "\" y=\"292\" text-anchor=\"" + (k == 0 ? "start" : k == 4 ? "end" : "middle") + "\" font-size=\"12\" fill=\"#5B6B75\">" + label + "</text>");
            }
            sb.Append("<polygon points=\"" + F(L) + "," + F(H - B) + " " + poly + " 990," + F(H - B) + "\" fill=\"url(#iv-cg)\"/>");
            sb.Append("<polyline points=\"" + poly + "\" fill=\"none\" stroke=\"" + col + "\" stroke-width=\"2.5\" stroke-linejoin=\"round\"/>");
            string[] lastXY = poly.Substring(poly.LastIndexOf(' ') + 1).Split(',');
            sb.Append("<circle cx=\"" + lastXY[0] + "\" cy=\"" + lastXY[1] + "\" r=\"5\" fill=\"" + col + "\"/>");
            sb.Append("</svg>");
            decimal pct = pts[0] == 0 ? 0 : (pts[pts.Count - 1] - pts[0]) / pts[0] * 100m;
            sb.Append("<div class=\"iv-chart-cap\">" + range.ToUpperInvariant() + " change: <b class=\"" + (up ? "iv-up" : "iv-down") + "\">" + (up ? "+" : "") + pct.ToString("0.0", CultureInfo.InvariantCulture) + "%</b> &middot; weekly closes</div>");
            return sb.ToString();
        }

        private static string F(double v)
        {
            return v.ToString("0.#", CultureInfo.InvariantCulture);
        }

        private static string KeyStats(SiteData site)
        {
            List<decimal> y = Range(site.Prices, "1y");
            decimal last = y[y.Count - 1];
            decimal eps = site.Quarters.Skip(Math.Max(0, site.Quarters.Count - 4)).Sum(q => q.Eps);
            decimal div = site.Quarters[site.Quarters.Count - 1].Dividend * 4;
            decimal cap = last * 1.22m;
            string[,] items =
            {
                { "52-week high", "$" + Money(y.Max()) },
                { "52-week low", "$" + Money(y.Min()) },
                { "Market cap", "$" + cap.ToString("0.0", CultureInfo.InvariantCulture) + "B" },
                { "P/E (TTM)", eps == 0 ? "&mdash;" : (last / eps).ToString("0.0", CultureInfo.InvariantCulture) },
                { "Dividend yield", (div / last * 100m).ToString("0.00", CultureInfo.InvariantCulture) + "%" }
            };
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < items.GetLength(0); i++)
            {
                sb.Append("<div><span>" + items[i, 0] + "</span><b>" + items[i, 1] + "</b></div>");
            }
            return sb.ToString();
        }

        private static string Results(SiteData site)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<thead><tr><th>Quarter</th><th>Revenue</th><th>Net income</th><th>Diluted EPS</th><th>Dividend</th><th>Revenue growth</th></tr></thead><tbody>");
            List<Quarter> qs = site.Quarters.AsEnumerable().Reverse().ToList();
            for (int i = 0; i < qs.Count; i++)
            {
                Quarter q = qs[i];
                string growth = "&mdash;";
                if (i + 1 < qs.Count && qs[i + 1].Revenue != 0)
                {
                    decimal g = (q.Revenue - qs[i + 1].Revenue) / qs[i + 1].Revenue * 100m;
                    growth = "<span class=\"" + (g >= 0 ? "iv-up" : "iv-down") + "\">" + (g >= 0 ? "+" : "") + g.ToString("0.0", CultureInfo.InvariantCulture) + "%</span>";
                }
                sb.Append("<tr><td>" + HtmlEncode(q.Name) + "</td><td>$" + q.Revenue.ToString("0.00", CultureInfo.InvariantCulture) + "B</td>");
                sb.Append("<td>$" + q.NetIncome.ToString("0.00", CultureInfo.InvariantCulture) + "B</td><td>$" + Money(q.Eps) + "</td><td>$" + Money(q.Dividend) + "</td><td>" + growth + "</td></tr>");
            }
            sb.Append("</tbody>");
            return sb.ToString();
        }

        private static string Events(SiteData site)
        {
            StringBuilder sb = new StringBuilder();
            foreach (EventItem e in site.Events)
            {
                DateTime d = DateTime.ParseExact(e.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                sb.Append("<div class=\"iv-event\"><span class=\"iv-cal\"><small>" + d.ToString("MMM", CultureInfo.InvariantCulture) + "</small><b>" + d.Day + "</b></span>");
                sb.Append("<div><b>" + HtmlEncode(e.Title) + "</b><span>" + HtmlEncode(e.Info) + "</span></div></div>");
            }
            return sb.ToString();
        }

        private static string Filings(SiteData site)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Filing f in site.Filings)
            {
                sb.Append("<a class=\"iv-filing\" href=\"#\"><span class=\"iv-ftag\">" + HtmlEncode(f.Form) + "</span><span><b>" + HtmlEncode(f.Title) + "</b><small>" + HtmlEncode(DateText(f.Date)) + "</small></span></a>");
            }
            return sb.ToString();
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
                sb.Append("<div class=\"iv-sg-none\">No results for &ldquo;" + HtmlEncode(q) + "&rdquo;</div>");
            }
            else
            {
                foreach (string r in rows.Take(7))
                {
                    sb.Append(r);
                }
                sb.Append("<div class=\"iv-sg-foot\">" + total + (total == 1 ? " result" : " results") + " across medicines, pipeline, trials and news</div>");
            }
            response.SetElementContents("iv-sugg", sb.ToString());
            response.ExecuteScript("InvestorsJs.openSugg();");
            return response;
        }

        private static string Sugg(string type, string color, string title, string sub, string href)
        {
            return "<a class=\"iv-sg\" href=\"" + href + "\"><span class=\"iv-sg-t\" style=\"--ac:" + color + "\">" + type + "</span><span><b>" + HtmlEncode(title) + "</b><small>" + HtmlEncode(sub) + "</small></span></a>";
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
            sb.Append("<button type=\"button\" class=\"iv-chip" + (active == string.Empty ? " iv-act" : string.Empty) + "\" onclick=\"InvestorsJs.chip(this, '" + group + "', '')\">All</button>");
            foreach (KeyLabel k in items)
            {
                sb.Append("<button type=\"button\" class=\"iv-chip" + (k.Key == active ? " iv-act" : string.Empty) + "\" onclick=\"InvestorsJs.chip(this, '" + group + "', '" + k.Key + "')\">" + HtmlEncode(k.Label) + "</button>");
            }
            return sb.ToString();
        }

        private static string ProductGrid(SiteData site, List<Medicine> list)
        {
            if (list.Count == 0)
            {
                return "<div class=\"iv-empty\">No medicines in this area yet.</div>";
            }
            StringBuilder sb = new StringBuilder();
            foreach (Medicine m in list)
            {
                AreaInfo a = AreaOf(site, m.Area);
                sb.Append("<div class=\"iv-pcard\" style=\"--ac:" + a.Color + ";--at:" + a.Tint + "\">");
                sb.Append("<div class=\"iv-pcard-img\"><span class=\"iv-tag\">" + HtmlEncode(a.Label) + "</span><img src=\"" + m.Image + "\" alt=\"" + HtmlEncode(m.Brand) + " pack\" loading=\"lazy\"></div>");
                sb.Append("<div class=\"iv-pcard-b\"><b class=\"iv-brand\">" + HtmlEncode(m.Brand) + "</b><span class=\"iv-gen\">" + HtmlEncode(m.Generic) + "</span>");
                sb.Append("<p>" + HtmlEncode(m.Indication) + "</p>");
                sb.Append("<div class=\"iv-meta\"><span>" + HtmlEncode(m.FormLabel) + "</span><span>Approved " + m.Year + "</span></div>");
                sb.Append("<div class=\"iv-plinks\"><a href=\"#\" onclick=\"return false;\">Prescribing information</a><a href=\"#\" onclick=\"return false;\">Patient information</a></div></div></div>");
            }
            return sb.ToString();
        }

        private static string NewsGrid(SiteData site, List<NewsItem> list)
        {
            if (list.Count == 0)
            {
                return "<div class=\"iv-empty\">No releases match these filters.</div>";
            }
            StringBuilder sb = new StringBuilder();
            foreach (NewsItem n in list)
            {
                string cat = site.NewsCats.Where(c => c.Key == n.Cat).Select(c => c.Label).FirstOrDefault() ?? n.Cat;
                sb.Append("<article class=\"iv-ncard\"><div class=\"iv-nmeta\"><span class=\"iv-ncat iv-c-" + n.Cat + "\">" + HtmlEncode(cat) + "</span><time>" + DateText(n.Date) + "</time></div>");
                sb.Append("<h3><a href=\"#\" onclick=\"return false;\">" + HtmlEncode(n.Title) + "</a></h3><p>" + HtmlEncode(n.Summary) + "</p>");
                sb.Append("<a class=\"iv-nmore\" href=\"#\" onclick=\"return false;\">Read more &rarr;</a></article>");
            }
            return sb.ToString();
        }

        private static string TrialList(SiteData site, List<Trial> list)
        {
            if (list.Count == 0)
            {
                return "<div class=\"iv-empty\">No studies match your search. Try fewer filters.</div>";
            }
            StringBuilder sb = new StringBuilder();
            foreach (Trial t in list)
            {
                AreaInfo a = AreaOf(site, t.Area);
                string locs = string.Join(", ", t.Locations.Take(2)) + (t.Locations.Count > 2 ? " +" + (t.Locations.Count - 2) + " more" : string.Empty);
                sb.Append("<div class=\"iv-trial\" style=\"--ac:" + a.Color + "\"><div class=\"iv-trial-top\">");
                sb.Append("<span class=\"iv-st iv-st-" + t.Status + "\">" + HtmlEncode(StatusLabel(site, t.Status)) + "</span>");
                sb.Append("<span class=\"iv-ph\">" + HtmlEncode(PhaseLabel(site, t.Phase)) + "</span><span class=\"iv-tid\">" + HtmlEncode(t.Id) + "</span></div>");
                sb.Append("<h3>" + HtmlEncode(t.Title) + "</h3><div class=\"iv-cond\"><i></i>" + HtmlEncode(t.Condition) + " &middot; " + HtmlEncode(a.Label) + "</div>");
                sb.Append("<div class=\"iv-facts\"><span>Ages " + HtmlEncode(t.Ages) + "</span><span>" + t.Enrollment.ToString("N0", CultureInfo.InvariantCulture) + " participants</span><span>" + HtmlEncode(locs) + "</span></div>");
                sb.Append("<button type=\"button\" class=\"iv-dbtn\" aria-expanded=\"false\" onclick=\"InvestorsJs.detail(this, '" + t.Id + "')\">Study details</button>");
                sb.Append("<div class=\"iv-detail\" id=\"iv-d-" + t.Id + "\"></div></div>");
            }
            return sb.ToString();
        }

        private static string Offices(SiteData site)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Office o in site.Offices)
            {
                sb.Append("<div class=\"iv-office\"><b>" + HtmlEncode(o.Name) + "</b><span>" + HtmlEncode(o.Line1) + "</span><span>" + HtmlEncode(o.Line2) + "</span><a href=\"#\" onclick=\"return false;\">" + HtmlEncode(o.Phone) + "</a></div>");
            }
            return sb.ToString();
        }
    }
}
