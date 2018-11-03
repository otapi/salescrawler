using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesCrawler.Architecture
{
    interface ICrawlerbot
    {
        Models.Crawlerbot Datasheet { get; set; }
    }
}
