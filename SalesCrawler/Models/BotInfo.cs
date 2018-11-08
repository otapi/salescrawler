using SalesCrawler.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesCrawler.Models
{
    public class BotInfo
    {
        public int Id { get; set; }
        public IScraper Scraper { get; set; }
        public STATUSES Status { get; set; }
        public string StatusMessage { get
            {
                return StatusMessages[Status];
            }
        }
        public string Message { get; set; }

        public enum STATUSES : int
        {
            New = 0,
            Paused = 1,
            Running = 2,
            Completed = 3,
            StoppedWithError = 4        
        }

        static Dictionary<STATUSES, string> StatusMessages = new Dictionary<STATUSES, string>()
        {
            {STATUSES.New, "New" },
            {STATUSES.Paused, "Paused" },
            {STATUSES.Running, "Running" },
            {STATUSES.Completed, "Completed" },
            {STATUSES.StoppedWithError, "Stopped with Error" },
        };
    }
}
