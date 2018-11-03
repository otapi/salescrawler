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
        /// <summary>
        /// If null, DefaultUrl is used with SeachPattern
        /// If not null, SearchPattern is ignored
        /// </summary>
        public string CustomUrl { get; set; }
        /// <summary>
        /// Ignored if CustomUrl is null
        /// </summary>
        public string SearchPattern { get; set; }

        public Crawlerbot Crawlerbot { get; set; }
    }
}
