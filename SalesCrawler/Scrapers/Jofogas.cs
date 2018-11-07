﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesCrawler.Models;

namespace SalesCrawler.Scrapers
{
    public class Jofogas : ViewModels.CrawlerBotBase, Architecture.IScraper
    {
        public Crawlerbot Datasheet { get; } = new Crawlerbot()
        {
            CrawlerbotId = 1,
            Name = "Jofogas.hu",
        };

        public async Task StartAsync()
        {
            PrintNote("Start");

        }
    }
}