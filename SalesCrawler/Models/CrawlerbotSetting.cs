using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesCrawler.Models
{
    public class CrawlerbotSetting
    {
        public int CrawlerbotSettingId { get; set; }
        public string Name { get; set; }
        public bool IsSearchPatternURL { get; set; }
        public string SearchPattern { get; set; }

        public Crawlerbot Crawlerbot { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
