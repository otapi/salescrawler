using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesCrawler.Models;

namespace SalesCrawler.ViewModels
{
    public class MainWindowVM : BaseVM
    {
        public ObservableCollection<CrawlProfile> CrawlProfiles { get; set; }

        public MainWindowVM() : base()
        {
            CrawlProfiles = new ObservableCollection<CrawlProfile>()
            {
                new CrawlProfile()
                {
                    Name = "test1"
                },
                new CrawlProfile()
                {
                    Name = "test2"
                },
            };
        }
    }
}
