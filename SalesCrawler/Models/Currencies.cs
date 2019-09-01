using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesCrawler.Models
{
    public class Currencies
    {
        public enum Currency
        {
            USD,
            EUR,
            HUF
        }

        public static int ConvertPriceToHUF(Currency currency, double price)
        {
            switch (currency)
            {
                case Currencies.Currency.EUR:
                    return (int)price * 325;
                case Currencies.Currency.HUF:
                    return (int)price;
                case Currencies.Currency.USD:
                    return (int)price * 180;
                default:
                    App.PrintWarning($"Unkown currency: [{currency.ToString()}] with price ${price.ToString()}");
                    return 0;
            }
        }
    }
}
