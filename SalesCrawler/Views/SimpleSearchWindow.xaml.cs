using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SalesCrawler.Views
{
    /// <summary>
    /// Interaction logic for SimpleSearchWindow.xaml
    /// </summary>
    public partial class SimpleSearchWindow : Window
    {
        public SimpleSearchWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var view = new MatchesWindow();
            
            view.Show();
        }

        private void LBScrapers_Loaded(object sender, RoutedEventArgs e)
        {
            ((ListBox)sender).SelectAll();
        }
    }
}
