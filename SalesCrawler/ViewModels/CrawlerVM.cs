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
        // TODO: adapt MVVM best practices? https://blog.rsuter.com/recommendations-best-practices-implementing-mvvm-xaml-net-applications/

        ObservableCollection<Scraper> _AvailableScrapers;
        public ObservableCollection<Scraper> AvailableScrapers
        {
            get { return _AvailableScrapers; }
            set { SetProperty(ref _AvailableScrapers, value); }
        }

        Dictionary<int, Type> ScraperClasses;
        public ObservableCollection<BotInfo> Bots { get; }

        public CrawlerVM()
        {
            ScanScraperClasses();

            Bots = new ObservableCollection<BotInfo>();
            Bots.CollectionChanged += OnCollectionChanged;

            
        }

        public void AddBot(ScraperSetting crawlerbotSetting)
        {
            var bot = Activator.CreateInstance(ScraperClasses[crawlerbotSetting.Scraper.ScraperIdentifier]) as ScraperBase;
            bot.Init(crawlerbotSetting);
            var bi = new BotInfo()
            {
                Name = crawlerbotSetting.Name,
                Message = "",
                Scraper = bot,
                Task = Task.Factory.StartNew(bot.StartBase, TaskCreationOptions.LongRunning),
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
                bi.ElapsedMinutes = ((int)((bi.FinishedTime - bi.StartTime).TotalMinutes)*10)/10;
                RaisePropertyChanged(() => Bots);
            });
            Bots.Add(bi);
        }

        

        void ScanScraperClasses()
        {
            ScraperClasses = new Dictionary<int, Type>();
            AvailableScrapers = new ObservableCollection<Scraper>();
            var theList = Assembly.GetExecutingAssembly().GetTypes().ToList().Where(t => t.Namespace == "SalesCrawler.Scrapers" && !t.FullName.Contains("+")).ToList();
            foreach (Type t in theList)
            {
                var b = Activator.CreateInstance(t) as Architecture.IScraper;
                ScraperClasses.Add(b.Datasheet.ScraperIdentifier, t);
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
