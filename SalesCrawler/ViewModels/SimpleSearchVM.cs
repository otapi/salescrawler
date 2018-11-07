using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
