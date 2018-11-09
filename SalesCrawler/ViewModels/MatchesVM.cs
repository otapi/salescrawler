using SalesCrawler.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace SalesCrawler.ViewModels
{
    public class MatchesVM : BaseVM
    {
        // https://docs.microsoft.com/en-us/visualstudio/data-tools/create-a-simple-data-application-with-wpf-and-entity-framework-6?view=vs-2017
        // https://blog.tonysneed.com/2014/05/28/real-world-mvvm-with-entity-framework-and-asp-net-web-api/
        public ObservableCollection<Match> Matches { get; }

        public MatchesVM()
        {
            Matches = new ObservableCollection<Match>();
            Matches.CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(() => Matches);
        }
    }
}
