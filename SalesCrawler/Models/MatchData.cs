using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesCrawler.Models
{
    public class MatchData
    {
        public int MatchDataId { get; set; }
        public string Seller { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }

        public double ActualPrice { get; set; }
        public Currencies.Currency Currency { get; set; }
        public int ActualPriceHUF
        {
            get
            {
                return Currencies.ConvertPriceToHUF(Currency, ActualPrice);
            }
        }

        public bool IsAuction { get; set; }
        public string Location { get; set; }
        public DateTime Expire { get; set; }
        

    }
}
