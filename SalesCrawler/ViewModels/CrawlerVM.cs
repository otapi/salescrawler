using SalesCrawler.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SalesCrawler.ViewModels
{
    public class CrawlerVM : BaseVM
    {
        // https://github.com/tom-englert/DataGridExtensions
        // TaskCreationOptions.LongRunning

        int _NumberOfRunningBots = 10;
        public int NumberOfRunningBots
        {
            get { return _NumberOfRunningBots; }
            set { SetProperty(ref _NumberOfRunningBots, value); }
        }
        ObservableCollection<Scraper> _AvailableScrapers;
        public ObservableCollection<Scraper> AvailableScrapers
        {
            get { return _AvailableScrapers; }
            set { SetProperty(ref _AvailableScrapers, value); }
        }

        Dictionary<int, Type> ScraperClasses;
        public ObservableCollection<BotInfo> Bots { get; }

        int BotIndex;
        public CrawlerVM()
        {
            BotIndex = 0;
            ScanScraperClasses();

            Bots = new ObservableCollection<BotInfo>();
            Bots.CollectionChanged += OnCollectionChanged;

            
        }

        public int AddBot(ScraperSetting crawlerbotSetting)
        {
            var bot = Activator.CreateInstance(ScraperClasses[crawlerbotSetting.Scraper.ScraperId]) as Architecture.IScraper;
            bot.Init(crawlerbotSetting);
            BotIndex++;
            var bi = new BotInfo()
            {
                Message = "",
                Scraper = bot,
                Task = Task.Factory.StartNew(bot.Start),
                StatusMessage = StatusMessages[TaskStatus.Created],
                StartTime = DateTime.Now
            };
            bi.Task.ContinueWith((t) => {
                bi.FinishedTime = DateTime.Now;
                bi.StatusMessage = StatusMessages[t.Status];
                if (t.IsFaulted)
                {
                    bi.Message = t.Exception.Message;
                }
                bi.ElapsedMinutes = (bi.FinishedTime - bi.StartTime).TotalMinutes;
                RaisePropertyChanged(() => Bots);
            });
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

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(() => Bots);
        }

        static Dictionary<TaskStatus, string> StatusMessages = new Dictionary<TaskStatus, string>()
        {
            {TaskStatus.Canceled, "Canceled" },
            {TaskStatus.Created, "Running" },
            {TaskStatus.Faulted, "Faulted" },
            {TaskStatus.RanToCompletion, "Completed" },
            {TaskStatus.Running, "Running" },
            {TaskStatus.WaitingForActivation, "Running" },
            {TaskStatus.WaitingForChildrenToComplete, "Running" },
            {TaskStatus.WaitingToRun, "Running" },
        };
    }
}
