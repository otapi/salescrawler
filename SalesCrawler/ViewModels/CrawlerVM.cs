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
using System.Windows.Input;

namespace SalesCrawler.ViewModels
{
    public class CrawlerVM : BaseVM
    {
        // https://github.com/tom-englert/DataGridExtensions
        // TODO: adapt MVVM best practices? https://blog.rsuter.com/recommendations-best-practices-implementing-mvvm-xaml-net-applications/

        
        ObservableCollection<Models.Scraper> _AvailableScrapers;
        public ObservableCollection<Models.Scraper> AvailableScrapers
        {
            get { return _AvailableScrapers; }
            set { SetProperty(ref _AvailableScrapers, value); }
        }

        private ICommand _GetDetailsCommand;
        public ICommand GetDetailsCommand
        {
            get
            {
                return _GetDetailsCommand ?? (_GetDetailsCommand = new CommandHandler(async (match) => await GetDetailsCommandExecute(match), !IsBusy));
            }
        }
        public async Task GetDetailsCommandExecute(object matchObj)
        {
            var match = matchObj as Match;
            AddBotsUpdateDetails(new List<Match>()
            {
                match
            });
        }

        Dictionary<string, Type> ScraperClasses;
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

        public void AddBotScrapeList(ScraperSetting crawlerbotSetting)
        {
            var bot = new ScraperBot(ScraperClasses[crawlerbotSetting.Scraper.ScraperIdentifier]);
            var bi = new BotInfo()
            {
                Name = crawlerbotSetting.Name,
                Setting = crawlerbotSetting,
                Scraper = bot,
                Task = null,
                StatusMessage = StatusMessages[TaskStatus.Created],
                CreatedTime = DateTime.Now,
                TaskType = BotInfo.TaskTypes.ScrapeList
            };
            StartBotTask(bi);
            Bots.Add(bi);
        }

        public void AddBotsUpdateDetails(List<Match> matches)
        {
            foreach (var scraperSetting in from m in matches
                                           group m by new
                                           {
                                               m.ScraperSetting
                                           } into gm
                                           select gm.Key.ScraperSetting)


            {
                
                var bot = new ScraperBot(ScraperClasses[scraperSetting.Scraper.ScraperIdentifier]);
                var bi = new BotInfo()
                {
                    Name = scraperSetting.Name,
                    Setting = scraperSetting,
                    Scraper = bot,
                    Task = null,
                    StatusMessage = StatusMessages[TaskStatus.Created],
                    CreatedTime = DateTime.Now,
                    TaskType = BotInfo.TaskTypes.UpdateDetails,
                    Matches = (from m in matches
                              where m.ScraperSetting == scraperSetting
                              select m).ToList()
                };
                StartBotTask(bi);
                Bots.Add(bi);
            }
        }


        private void StartBotTask(BotInfo bi)
        {
            
            if (IsBusy)
            {
                App.PrintNote("Only one bot is allowed to run at once.");
                return;
            }
            IsBusy = true;
            bi.StartTime = DateTime.Now;
            switch (bi.TaskType)
            {
                case BotInfo.TaskTypes.ScrapeList:
                    bi.Task = Task.Factory.StartNew(() => bi.Scraper.ScrapeListBase(driver, bi.Setting), TaskCreationOptions.LongRunning);
                    break;
                case BotInfo.TaskTypes.UpdateDetails:
                    bi.Task = Task.Factory.StartNew(() => bi.Scraper.UpdateMatchDetails(driver, bi.Matches, bi.Setting), TaskCreationOptions.LongRunning);
                    break;
                default:
                    throw new Exception("Unkown tasktype");
            }
            bi.StatusMessage = StatusMessages[TaskStatus.Running];
            bi.Task.ContinueWith((t) => {
                bi.FinishedTime = DateTime.Now;
                bi.StatusMessage = StatusMessages[t.Status];
                if (t.IsFaulted)
                {
                    App.PrintWarning($"[{bi.Name}] {t.Exception.Message}");
                }
                bi.ElapsedMinutes = ((int)((bi.FinishedTime - bi.StartTime).TotalMinutes) * 10) / 10;
                RaisePropertyChanged(() => Bots);

                IsBusy = false;
                
                var b = GetNextBotFromQueue();
                if (b==null)
                {
                    CloseDriver();
                } else
                {

                    StartBotTask(b);
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
            ScraperClasses = new Dictionary<string, Type>();
            AvailableScrapers = new ObservableCollection<Models.Scraper>();
            var theList = Assembly.GetExecutingAssembly().GetTypes().ToList().Where(t => t.Namespace == "SalesCrawler.Scrapers" && !t.FullName.Contains("+")).ToList();
            foreach (Type t in theList)
            {
                var b = Activator.CreateInstance(t) as Helpers.IScraper;
                b.Datasheet.ScraperIdentifier = t.FullName;
                ScraperClasses.Add(b.Datasheet.ScraperIdentifier, t);
                AvailableScrapers.Add(b.Datasheet);
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(() => Bots);
        }

        static readonly Dictionary<TaskStatus, string> StatusMessages = new Dictionary<TaskStatus, string>()
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
