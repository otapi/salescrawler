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
        ObservableCollection<Crawlerbot> _AvailableBots;
        public ObservableCollection<Crawlerbot> AvailableBots
        {
            get { return _AvailableBots; }
            set { SetProperty(ref _AvailableBots, value); }
        }

        Dictionary<int, Type> BotClasses;
        Dictionary<CrawlerbotSetting, Architecture.ICrawlerbot> Bots;
        public CrawlerVM()
        {
            ScanBotClasses();
            Bots = new Dictionary<CrawlerbotSetting, Architecture.ICrawlerbot>();
        }

        public void AddBot(CrawlerbotSetting crawlerbotSetting)
        {
            var bot = Activator.CreateInstance(BotClasses[crawlerbotSetting.Crawlerbot.CrawlerbotId]) as Architecture.ICrawlerbot;
            Bots.Add(crawlerbotSetting, bot);
        }



        void ScanBotClasses()
        {
            BotClasses = new Dictionary<int, Type>();
            ObservableCollection<Crawlerbot> AvailableBots = new ObservableCollection<Crawlerbot>();
            var theList = Assembly.GetExecutingAssembly().GetTypes().ToList().Where(t => t.Namespace == "SalesCrawler.Crawlerbots").ToList();
            foreach (Type t in theList)
            {
                var b = Activator.CreateInstance(t) as Architecture.ICrawlerbot;
                BotClasses.Add(b.Datasheet.CrawlerbotId, t);
                AvailableBots.Add(b.Datasheet);
            }
        }
    }
}
