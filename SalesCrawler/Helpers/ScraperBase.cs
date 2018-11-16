using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesCrawler.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Drawing;
using System.Net;
using System.IO;
using SalesCrawler.ViewModels;

namespace SalesCrawler.Helpers
{
    public class ScraperBase
    {
        public static DateTime NEVEREXPIRE = new DateTime(2100, 1, 1);
        protected IWebDriver driver;
        
        public void Init(IWebDriver driver)
        {
            this.driver = driver;
        }
        protected double StripToDouble(string text)
        {
            // TODO: check and locale spec
            var s = System.Text.RegularExpressions.Regex.Replace(text, @"[^\d.,]", "");
            if (s == "" || s == "." || s == ",")
            {
                return 0;
            }
            else
            {
                return double.Parse(s);
            }
        }

        protected int StripToInt(string text)
        {
            var s = System.Text.RegularExpressions.Regex.Replace(text, @"[^\d]", "");
            if (s == "")
            {
                return 0;
            }
            else
            {
                return int.Parse(s);
            }
        }

        protected string StripToLetters(string text)
        {
            var s = System.Text.RegularExpressions.Regex.Replace(text, @"[\d-]", string.Empty);
            s = s.Replace("&nbsp;", " ");
            return s.Trim();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="by"></param>
        /// <param name="timeout">seconds</param>
        protected void Waitfor(By by, int timeout = 10)
        {
            var w = new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
            w.Until(c => c.FindElement(by));
        }
        protected void Wait(int timeout = 1)
        {
            System.Threading.Thread.Sleep(timeout * 1000);
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Y coordinate</returns>
        protected int ScrollTo(IWebElement element, int offsetY = 0)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript($"window.scroll(0, {element.Location.Y+offsetY});");
            return element.Location.Y;
        }

        protected void ScrollToBottom()
        {
            ((IJavaScriptExecutor)driver).ExecuteScript($"window.scrollTop = window.scrollHeight;");
        }

        protected byte[] GetImage(string url)
        {
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);
            WebResponse myResp = myReq.GetResponse();

            byte[] b = null;
            using (Stream stream = myResp.GetResponseStream())
            using (MemoryStream ms = new MemoryStream())
            {
                int count = 0;
                do
                {
                    byte[] buf = new byte[1024];
                    count = stream.Read(buf, 0, 1024);
                    ms.Write(buf, 0, count);
                } while (stream.CanRead && count > 0);
                b = ms.ToArray();
            }
            return b;
        }
        protected byte[] TakeScreenshot(IWebElement element)
        {

            driver.SwitchTo();
            driver.Manage().Window.Maximize();

            int offs = 100;
            int yoffset = ScrollTo(element, -offs);

            // Todo: cache the screenshot with Y coordinates.
            Byte[] byteArray = ((ITakesScreenshot)driver).GetScreenshot().AsByteArray;
            System.Drawing.Bitmap screenshot = new System.Drawing.Bitmap(new System.IO.MemoryStream(byteArray));

            System.Drawing.Rectangle croppedImage = new System.Drawing.Rectangle(element.Location.X, offs, element.Size.Width, element.Size.Height+offs);
            screenshot = screenshot.Clone(croppedImage, screenshot.PixelFormat);

            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(screenshot, typeof(byte[]));
        }

        protected Currencies.Currency GetCurrency(string text)
        {

            switch (StripToLetters(text))
            {
                case "Ft":
                case "":
                case "INGYENES":
                    return Currencies.Currency.HUF;
                default:
                    if (text.StartsWith("Ft"))
                    {
                        return Currencies.Currency.HUF;
                    }
                    throw new Exception("Unkown currency: " + text);
            }
        }
    }
}

