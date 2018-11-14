using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SalesCrawler.Models
{
    public class MatchData
    {
        public int MatchDataId { get; set; }
        public string Seller { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public byte[] ImageBinary { get; set; }
        [NotMapped]
        public BitmapImage Image
        {
            get
            {
                using (var ms = new System.IO.MemoryStream(ImageBinary))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                    return image;
                }
            }
        }


        public string Description { get; set; }

        public double ActualPrice { get; set; }
        public Currencies.Currency Currency { get; set; }
        public int ActualPriceHUF
        {
            get
            {
                return Currencies.ConvertPriceToHUF(Currency, ActualPrice);
            }
        }

        public bool IsAuction { get; set; }
        public string Location { get; set; }
        public DateTime Expire { get; set; }
        

    }
}
