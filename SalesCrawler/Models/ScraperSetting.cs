using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesCrawler.Models
{
    public class ScraperSetting
    {
        public int ScraperSettingId { get; set; }
        public string Name { get; set; }
        public bool IsSearchPatternURL { get; set; }
        public string SearchPattern { get; set; }

        public Scraper Scraper { get; set; }
        public CrawlProfile CrawlProfile { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
