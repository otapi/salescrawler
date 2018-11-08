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
        ObservableCollection<Crawlerbot> _Crawlerbots;
        public ObservableCollection<Crawlerbot> Crawlerbots
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
            var collection = items.Cast<Crawlerbot>();
            
            foreach (var item in collection)
            {
                CrawlerbotSetting setting = new CrawlerbotSetting()
                {
                    Name = $"SimpleSearch - {item.Name}",
                    SearchPattern = TextToSearch,
                    Crawlerbot = item
                };
                App.CrawlerVM.AddBot(setting);
            }
        }

        public SimpleSearchVM()
        {
            Crawlerbots = new ObservableCollection<Crawlerbot>();
            foreach(var c in App.CrawlerVM.AvailableScrapers)
            {
                Crawlerbots.Add(c);
            }
        }
    }
}
