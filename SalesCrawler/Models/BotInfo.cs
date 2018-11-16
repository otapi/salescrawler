using SalesCrawler.Helpers;
using SalesCrawler.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesCrawler.Models
{
    public class BotInfo : BaseVM
    {
        string _Name;
        public string Name
        {
            get { return _Name; }
            set { SetProperty(ref _Name, value); }
        }

        ScraperSetting _Setting;
        public ScraperSetting Setting
        {
            get { return _Setting; }
            set { SetProperty(ref _Setting, value); }
        }

        DateTime _CreatedTime;
        public DateTime CreatedTime
        {
            get { return _CreatedTime; }
            set { SetProperty(ref _CreatedTime, value); }
        }

        DateTime _StartTime;
        public DateTime StartTime
        {
            get { return _StartTime; }
            set { SetProperty(ref _StartTime, value); }
        }
        DateTime _FinishedTime;
        public DateTime FinishedTime
        {
            get { return _FinishedTime; }
            set { SetProperty(ref _FinishedTime, value); }
        }
        double _ElapsedMinutes;
        public double ElapsedMinutes
        {
            get { return _ElapsedMinutes; }
            set { SetProperty(ref _ElapsedMinutes, value); }
        }
        string _StatusMessage;
        public string StatusMessage
        {
            get { return _StatusMessage; }
            set { SetProperty(ref _StatusMessage, value); }
        }
        public Task Task { get; set; }

        Helpers.ScraperBot _Scraper;
        public ScraperBot Scraper
        {
            get { return _Scraper; }
            set { SetProperty(ref _Scraper, value); }
        }

    }
}
