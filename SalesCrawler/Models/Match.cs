using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesCrawler.Models
{
    public class Match
    {
        public int MatchId { get; set; }
        public ScraperSetting CrawlerbotSetting { get; set; }
        public string Seller { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }

        public double ActualPrice { get; set; }
        public Default.Currency Currency { get; set; }
        public int ActualPriceHUF { get; set; }

        public DateTime LastScannedDate { get; set; }
        /// <summary>
        /// User selects to see or hide this match
        /// </summary>
        public bool Visible { get; set; }
        /// <summary>
        /// False if this match cannot found anymore
        /// </summary>
        public bool Alive { get; set; }
        public ICollection<PriceHistory> PriceHistories { get; set; } = new List<PriceHistory>();
    }
}
