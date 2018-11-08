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
        // https://github.com/tom-englert/DataGridExtensions

        ObservableCollection<Scraper> _AvailableScrapers;
        public ObservableCollection<Scraper> AvailableScrapers
        {
            get { return _AvailableScrapers; }
            set { SetProperty(ref _AvailableScrapers, value); }
        }

        Dictionary<int, Type> ScraperClasses;
        ObservableCollection<BotInfo> _Bots;
        public ObservableCollection<BotInfo> Bots
        {
            get { return _Bots; }
            set { SetProperty(ref _Bots, value); }
        }
        
        int BotIndex;
        public CrawlerVM()
        {
            BotIndex = 0;
            ScanScraperClasses();
            Bots = new ObservableCollection<BotInfo>();
        }

        public int AddBot(ScraperSetting crawlerbotSetting)
        {
            var bot = Activator.CreateInstance(ScraperClasses[crawlerbotSetting.Scraper.ScraperId]) as Architecture.IScraper;
            BotIndex++;
            var bi = new BotInfo()
            {
                Id = BotIndex,
                Message = "",
                Scraper = bot,
                Status = BotInfo.STATUSES.New
            };
            Bots.Add(bi);
            return BotIndex;
        }

        void ScanScraperClasses()
        {
            ScraperClasses = new Dictionary<int, Type>();
            AvailableScrapers = new ObservableCollection<Scraper>();
            var theList = Assembly.GetExecutingAssembly().GetTypes().ToList().Where(t => t.Namespace == "SalesCrawler.Scrapers" && !t.FullName.Contains("+")).ToList();
            foreach (Type t in theList)
            {
                var b = Activator.CreateInstance(t) as Architecture.IScraper;
                ScraperClasses.Add(b.Datasheet.ScraperId, t);
                AvailableScrapers.Add(b.Datasheet);
            }
        }
    }
}
