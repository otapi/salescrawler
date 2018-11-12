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
