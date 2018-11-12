using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SalesCrawler
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static ViewModels.MatchesVM _MatchesVM;
        public static ViewModels.MatchesVM MatchesVM
        {
            get
            {
                if (_MatchesVM == null)
                {
                    _MatchesVM = new ViewModels.MatchesVM();
                }
                return _MatchesVM;
            }
        }
        /*
        static ViewModels.CrawlerVM _CrawlerVM;
        public static ViewModels.CrawlerVM CrawlerVM
        {
            get
            {
                if (_CrawlerVM == null)
                {
                    _CrawlerVM = new ViewModels.CrawlerVM();
                }
                return _CrawlerVM;
            }
        }
        */
        

        static Data.Database _DB;
        public static Data.Database DB
        {
            get
            {
                if (_DB == null)
                {
                    App.PrintNote($"[DatabaseHelper] start");

                    _DB = new Data.Database();
                    _DB.Database.Delete(); _DB = new Data.Database();
                }
                return _DB;
            }
        }

        public static object ServiceLocator { get; private set; }

        public static void PrintWarning(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesCrawler][Warning] {message}");
        }
        public static void PrintNote(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesCrawler][Note] {message}");
        }

    }
}
