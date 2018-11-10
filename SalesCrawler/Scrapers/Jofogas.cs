﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SalesCrawler.Models;

namespace SalesCrawler.Scrapers
{
    public class Jofogas : ViewModels.CrawlerBotBase, Architecture.IScraper
    {
        public Scraper Datasheet { get; } = new Scraper()
        {
            ScraperIdentifier = 1,
            Name = "Jofogas.hu",
        };

        
        public async Task Start()
        {
            PrintNote("start");
            
            PrintNote("completed");
        }
    }
}
