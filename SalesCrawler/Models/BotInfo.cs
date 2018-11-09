using SalesCrawler.Architecture;
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
        string _Message;
        public string Message
        {
            get { return _Message; }
            set { SetProperty(ref _Message, value); }
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

        IScraper _Scraper;
        public IScraper Scraper
        {
            get { return _Scraper; }
            set { SetProperty(ref _Scraper, value); }
        }

    }
}
