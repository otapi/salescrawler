using SalesCrawler.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Threading;
using SalesCrawler.Helpers;

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

        IWebDriver _driver;
        public IWebDriver driver
        {
            get
            {
                if (_driver == null)
                {
                    var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Google\\Chrome\\User Data\\";

                    ChromeOptions options = new ChromeOptions();
                    options.AddArgument("user-data-dir=" + userProfile);
                    options.AddArgument("--start-maximized");
                    options.AddUserProfilePreference("profile.default_content_setting_values.notifications", 2);
                    //options.AddArgument("--incognito");
                    _driver = new ChromeDriver(options);
                }
                return _driver;

            }

        }

        public CrawlerVM()
        {
            ScanScraperClasses();

            Bots = new ObservableCollection<BotInfo>();
            Bots.CollectionChanged += OnCollectionChanged;

            IsBusy = false;
            
        }

        public void AddBot(ScraperSetting crawlerbotSetting)
        {
            var bot = Activator.CreateInstance(ScraperClasses[crawlerbotSetting.Scraper.ScraperIdentifier]) as ScraperBase;
            var bi = new BotInfo()
            {
                Name = crawlerbotSetting.Name,
                Message = "",
                Setting = crawlerbotSetting,
                Scraper = bot,
                Task = null,
                StatusMessage = StatusMessages[TaskStatus.Created],
                CreatedTime = DateTime.Now
            };
            StartBotScrapeLists(bi);
            Bots.Add(bi);
        }

        private void StartBotScrapeLists(BotInfo bi)
        {
            
            if (IsBusy)
            {
                App.PrintNote("Only one bot is allowed to run at once.");
                return;
            }
            IsBusy = true;
            bi.StartTime = DateTime.Now;
            bi.Task = Task.Factory.StartNew(() => bi.Scraper.ScrapeListBase(driver, bi.Setting), TaskCreationOptions.LongRunning);
            bi.StatusMessage = StatusMessages[TaskStatus.Running];
            bi.Task.ContinueWith((t) => {
                bi.FinishedTime = DateTime.Now;
                bi.StatusMessage = StatusMessages[t.Status];
                if (t.IsFaulted)
                {
                    bi.Message = t.Exception.Message;
                }
                bi.ElapsedMinutes = ((int)((bi.FinishedTime - bi.StartTime).TotalMinutes) * 10) / 10;
                RaisePropertyChanged(() => Bots);
                IsBusy = false;
                Thread.Sleep(1000);
                
                var b = GetNextBotFromQueue();
                if (b==null)
                {
                    //CloseDriver();
                } else
                {

                    StartBotScrapeLists(b);
                }
            });
        }
        
        private void CloseDriver()
        {
            driver.Quit();
            driver.Dispose();
            _driver = null;
        }
        private BotInfo GetNextBotFromQueue()
        {
            foreach (var b in Bots)
            {
                if (b.Task == null)
                {
                    return b;
                }
            }
            return null;
        }

        void ScanScraperClasses()
        {
            ScraperClasses = new Dictionary<int, Type>();
            AvailableScrapers = new ObservableCollection<Scraper>();
            var theList = Assembly.GetExecutingAssembly().GetTypes().ToList().Where(t => t.Namespace == "SalesCrawler.Scrapers" && !t.FullName.Contains("+")).ToList();
            foreach (Type t in theList)
            {
                var b = Activator.CreateInstance(t) as Helpers.IScraper;
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
            {TaskStatus.Created, "Waiting" },
            {TaskStatus.Faulted, "Faulted" },
            {TaskStatus.RanToCompletion, "Completed" },
            {TaskStatus.Running, "Running" },
            {TaskStatus.WaitingForActivation, "Waiting" },
            {TaskStatus.WaitingForChildrenToComplete, "Running" },
            {TaskStatus.WaitingToRun, "Waiting" },
        };
    }
}
