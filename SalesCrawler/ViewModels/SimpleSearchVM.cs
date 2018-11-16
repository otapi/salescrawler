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
                    Scraper = item
                };
                CrawlerVM.AddBot(setting);
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
                    PagesToScrape = 2
                };
                CrawlerVM.AddBot(setting);
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
