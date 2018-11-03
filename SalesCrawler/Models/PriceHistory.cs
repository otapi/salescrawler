using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesCrawler.Models
{
    public class PriceHistory
    {
        public int PriceHistoryId { get; set; }
        public DateTime Date { get; set; }
        public int PriceHUF { get; set; }
    }
}
