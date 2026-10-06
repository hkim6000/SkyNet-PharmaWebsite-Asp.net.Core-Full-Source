namespace Pharma.Models
{
    public class SiteData
    {
        public List<AreaInfo> Areas { get; set; } = new List<AreaInfo>();
        public List<KeyLabel> Phases { get; set; } = new List<KeyLabel>();
        public List<KeyLabel> Statuses { get; set; } = new List<KeyLabel>();
        public List<KeyLabel> NewsCats { get; set; } = new List<KeyLabel>();
        public List<KeyLabel> Depts { get; set; } = new List<KeyLabel>();
        public List<KeyLabel> Locs { get; set; } = new List<KeyLabel>();
        public List<Medicine> Products { get; set; } = new List<Medicine>();
        public List<PipelineItem> Programs { get; set; } = new List<PipelineItem>();
        public List<Trial> Trials { get; set; } = new List<Trial>();
        public List<NewsItem> News { get; set; } = new List<NewsItem>();
        public List<Quarter> Quarters { get; set; } = new List<Quarter>();
        public List<Leader> Leaders { get; set; } = new List<Leader>();
        public List<Milestone> History { get; set; } = new List<Milestone>();
        public List<Office> Offices { get; set; } = new List<Office>();
        public List<Job> Jobs { get; set; } = new List<Job>();
        public List<EventItem> Events { get; set; } = new List<EventItem>();
        public List<Filing> Filings { get; set; } = new List<Filing>();
        public string PriceStart { get; set; } = "2021-10-08";
        public List<decimal> Prices { get; set; } = new List<decimal>();
    }

    public class KeyLabel
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class AreaInfo
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Tint { get; set; } = string.Empty;
        public string Blurb { get; set; } = string.Empty;
    }

    public class Medicine
    {
        public string Id { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Generic { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string Form { get; set; } = string.Empty;
        public string FormLabel { get; set; } = string.Empty;
        public string Indication { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Image { get; set; } = string.Empty;
    }

    public class PipelineItem
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string Phase { get; set; } = string.Empty;
        public string Indication { get; set; } = string.Empty;
        public string Modality { get; set; } = string.Empty;
        public string Milestone { get; set; } = string.Empty;
    }

    public class Trial
    {
        public string Id { get; set; } = string.Empty;
        public string Program { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public string Phase { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<string> Locations { get; set; } = new List<string>();
        public List<string> Regions { get; set; } = new List<string>();
        public int Enrollment { get; set; }
        public string Start { get; set; } = string.Empty;
        public string Completion { get; set; } = string.Empty;
        public string Ages { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public List<string> Criteria { get; set; } = new List<string>();
    }

    public class NewsItem
    {
        public string Id { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Cat { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
    }

    public class Quarter
    {
        public string Name { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal NetIncome { get; set; }
        public decimal Eps { get; set; }
        public decimal Dividend { get; set; }
    }

    public class Leader
    {
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
    }

    public class Milestone
    {
        public string Year { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    public class Office
    {
        public string Name { get; set; } = string.Empty;
        public string Line1 { get; set; } = string.Empty;
        public string Line2 { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    public class Job
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Dept { get; set; } = string.Empty;
        public string Loc { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    public class EventItem
    {
        public string Date { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Info { get; set; } = string.Empty;
    }

    public class Filing
    {
        public string Date { get; set; } = string.Empty;
        public string Form { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }
}
