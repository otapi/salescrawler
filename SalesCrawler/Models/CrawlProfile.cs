using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesCrawler.Models
{
    public class CrawlProfile
    {
        public int CrawlProfileId { get; set; }
        public string Name { get; set; }

        public ICollection<ScraperSetting> ScraperSettings { get; set; } = new List<ScraperSetting>();
        public ICollection<Match> Matches { get; set; } = new List<Match>();

        public override string ToString()
        {
            return Name;
        }
    }
}
