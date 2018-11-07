using SalesCrawler.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SalesCrawler.ViewModels
{
    public class CrawlerVM : BaseVM
    {
        ObservableCollection<Crawlerbot> _AvailableScrapers;
        public ObservableCollection<Crawlerbot> AvailableScrapers
        {
            get { return _AvailableScrapers; }
            set { SetProperty(ref _AvailableScrapers, value); }
        }

        Dictionary<int, Type> ScraperClasses;
        Dictionary<int, Architecture.IScraper> Bots;

        int BotIndex;
        public CrawlerVM()
        {
            BotIndex = 0;
            ScanScraperClasses();
            Bots = new Dictionary<int, Architecture.IScraper>();
        }

        public int AddBot(CrawlerbotSetting crawlerbotSetting)
        {
            var bot = Activator.CreateInstance(ScraperClasses[crawlerbotSetting.Crawlerbot.CrawlerbotId]) as Architecture.IScraper;
            Bots.Add(BotIndex++, bot);
            return BotIndex;
        }



        void ScanScraperClasses()
        {
            ScraperClasses = new Dictionary<int, Type>();
            AvailableScrapers = new ObservableCollection<Crawlerbot>();
            var theList = Assembly.GetExecutingAssembly().GetTypes().ToList().Where(t => t.Namespace == "SalesCrawler.Scrapers" && !t.FullName.Contains("+")).ToList();
            foreach (Type t in theList)
            {
                var b = Activator.CreateInstance(t) as Architecture.IScraper;
                ScraperClasses.Add(b.Datasheet.CrawlerbotId, t);
                AvailableScrapers.Add(b.Datasheet);
            }
        }
    }
}
