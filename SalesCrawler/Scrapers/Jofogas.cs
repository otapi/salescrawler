using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SalesCrawler.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace SalesCrawler.Scrapers
{
    // https://medium.com/the-andela-way/introduction-to-web-scraping-using-selenium-7ec377a8cf72
    // https://javabeginnerstutorial.com/selenium/selenium-tutorial/
    // https://github.com/AngleSharp/AngleSharp over htmlagilitypack
    // http://scraping.pro

    public class Jofogas : ViewModels.ScraperBase, Architecture.IScraper
    {
        public Scraper Datasheet { get; } = new Scraper()
        {
            ScraperIdentifier = 1,
            Name = "Jofogas.hu"
        };

        
        override public async Task Start()
        {
            if (!Setting.IsSearchPatternURL)
            {
                driver.Navigate().GoToUrl("https://www.jofogas.hu/");
                //Wait().Until(ExpectedConditions.ElementToBeClickable(By.Id("CybotCookiebotDialogBodyButtonAccept")));
                driver.FindElement(By.Id("CybotCookiebotDialogBodyButtonAccept")).Click();

                //Wait().Until(c => c.FindElement(By.Id("index-search")));
                //Wait().Until(ExpectedConditions.ElementToBeClickable(By.Id("index-search")));
                driver.FindElement(By.Id("index-search")).Click();
                driver.FindElement(By.Id("index-search")).Clear();
                driver.FindElement(By.Id("index-search")).SendKeys(Setting.SearchPattern);
                driver.FindElement(By.XPath("//button[@type='submit']//i[@class='mdi mdi-magnify']")).Click();
            }
            else
            {
                driver.Navigate().GoToUrl(Setting.SearchPattern);
                //Wait().Until(ExpectedConditions.ElementToBeClickable(By.Id("CybotCookiebotDialogBodyButtonAccept")));
                driver.FindElement(By.Id("CybotCookiebotDialogBodyButtonAccept")).Click();

            }
            //Wait().Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='ad-list-pager-item ad-list-pager-item-next active-item js_hist_li js_hist jofogasicon-right']")));
            Wait().Until(c => c.FindElement(By.XPath("//a[@class='ad-list-pager-item ad-list-pager-item-next active-item js_hist_li js_hist jofogasicon-right']")));

            foreach (var item in driver.FindElements(By.XPath("//div//div[@class='contentArea']")))
            {
                await SaveMatch(
                    seller: null,
                    title: item.FindElement(By.XPath(".//h3[@class='item-title']/a")).Text,
                    url: item.FindElement(By.XPath(".//h3[@class='item-title']/a")).GetAttribute("href"),
                    imageUrl: item.FindElement(By.XPath(".//img")).GetAttribute("style").Replace("background-image: url(\"", "").Replace("\");", ""),
                    description: null,
                    actualPrice: StripToInt(item.FindElement(By.XPath(".//h3[@class='item-price']")).Text),
                    currency: GetCurrency(item.FindElement(By.XPath(".//span[@class='currency']")).Text)
                    );
            };
            
            PrintNote("completed");
        }
        
        Default.Currency GetCurrency(string text)
        {
            switch(text.Replace("&nbsp;", "").Trim())
            {
                case "Ft":
                case "":
                    return Default.Currency.HUF;
                default:
                    throw new Exception("Unkown currency: " + text);
            }
        }
        
    }
}
