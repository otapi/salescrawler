using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesCrawler.Models;

namespace SalesCrawler.Architecture
{
    public interface IScraper
    {
        Scraper Datasheet { get; }
        void Start();
    }
}
