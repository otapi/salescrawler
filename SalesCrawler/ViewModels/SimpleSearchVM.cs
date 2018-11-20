using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SalesCrawler.Models;

namespace SalesCrawler.ViewModels
{
    class SimpleSearchVM : BaseVM
    {
        CrawlerVM CrawlerVM;
        ObservableCollection<Scraper> _Crawlerbots;
        public ObservableCollection<Scraper> Crawlerbots
        {
            get { return _Crawlerbots; }
            set { SetProperty(ref _Crawlerbots, value); }
        }

        string _TextToSearch;
        public string TextToSearch
        {
            get { return _TextToSearch; }
            set { SetProperty(ref _TextToSearch, value); }
        }

        int? _MinPrice;
        public int? MinPrice
        {
            get { return _MinPrice; }
            set { SetProperty(ref _MinPrice, value); }
        }
        int? _MaxPrice;
        public int? MaxPrice
        {
            get { return _MaxPrice; }
            set { SetProperty(ref _MaxPrice, value); }
        }
        private ICommand _SearchCommand;
        public ICommand SearchCommand
        {
            get
            {
                return _SearchCommand ?? (_SearchCommand = new CommandHandler((param) => SearchCommandExecute(param), !IsBusy));
            }
        }
        public void SearchCommandExecute(object selectedItems)
        {
            System.Collections.IList items = (System.Collections.IList)selectedItems;
            var collection = items.Cast<Scraper>();
            
            foreach (var item in collection)
            {
                ScraperSetting setting = new ScraperSetting()
                {
                    Name = $"SimpleSearch - {item.Name}",
                    SearchPattern = TextToSearch,
                    Scraper = item,
                    PagesToScrape = 5,
                    MinPrice = MinPrice,
                    MaxPrice = MaxPrice
                };
                CrawlerVM.AddBotScrapeList(setting);
            }

        }
        
        private ICommand _SearchTestCommand;
        public ICommand SearchTestCommand
        {
            get
            {
                return _SearchTestCommand ?? (_SearchTestCommand = new CommandHandler((param) => SearchTestCommandExecute(param), !IsBusy));
            }
        }
        public void SearchTestCommandExecute(object selectedItems)
        {
            System.Collections.IList items = (System.Collections.IList)selectedItems;
            var collection = items.Cast<Scraper>();

            foreach (var item in collection)
            {
                ScraperSetting setting = new ScraperSetting()
                {
                    Name = $"SimpleSearch - {item.Name}",
                    SearchPattern = (TextToSearch == null ? "kerékpár" : TextToSearch),
                    Scraper = item,
                    DoOnlyTest = true,
                    PagesToScrape = 2,
                    MinPrice = MinPrice,
                    MaxPrice = MaxPrice
                };
                CrawlerVM.AddBotScrapeList(setting);
            }

        }

        public SimpleSearchVM(CrawlerVM crawlerVM)
        {
            CrawlerVM = crawlerVM;
            Crawlerbots = new ObservableCollection<Scraper>();
            foreach(var c in CrawlerVM.AvailableScrapers)
            {
                Crawlerbots.Add(c);
            }
        }
    }
}
