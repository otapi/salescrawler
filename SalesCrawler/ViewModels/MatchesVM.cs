﻿using SalesCrawler.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Windows.Input;

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

            LoadItemsCommand.Execute(null);
        }

        private ICommand _LoadItemsCommand;
        public ICommand LoadItemsCommand
        {
            get
            {
                return _LoadItemsCommand ?? (_LoadItemsCommand = new CommandHandler(async (param) => await LoadItemsCommandExecute(null), !IsBusy));
            }
        }
        public async Task LoadItemsCommandExecute(object param = null)
        {
            IsBusy = true;
            Matches.Clear();
            foreach (var m in await App.DB.GetMatches())
            {
                Matches.Add(m);
            }
            IsBusy = false;
        }
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(() => Matches);
        }
    }
}
