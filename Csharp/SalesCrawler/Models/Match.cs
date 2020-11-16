using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesCrawler.Models
{
    public class Match
    {
        public int matchid { get; set; }
        public DateTime LastScannedDate { get; set; }

        public ScraperSetting ScraperSetting { get; set; }
        public MatchData MatchData { get; set;}
        public ICollection<PriceHistory> PriceHistories { get; set; } = new List<PriceHistory>();
        /// <summary>
        /// User selects to see or hide this match
        /// </summary>
        public bool Visible { get; set; } = true;
        /// <summary>
        /// False if this match cannot found anymore
        /// </summary>
        public bool Alive { get; set; } = true;
    }
}
