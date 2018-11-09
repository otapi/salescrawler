using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesCrawler.Models;

namespace SalesCrawler.ViewModels
{
    public class CrawlerBotBase
    {
        public ScraperSetting Setting { get; set; }
        
        public void Init(ScraperSetting crawlerbotSetting)
        {
            Setting = crawlerbotSetting;
        }

        protected void PrintNote(string message)
        {
            App.PrintNote($"[{Setting.Name}] ${message}");
        }

        protected void PrintWarning(string message)
        {
            App.PrintWarning($"[{Setting.Name}] ${message}");
        }
    }
}
