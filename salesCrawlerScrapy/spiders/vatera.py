import scrapy

from salesCrawlerScrapy.helpers import Helpers
from salesCrawlerScrapy.items import ProductItem
import logging

class Vatera(scrapy.Spider):
    name = 'vatera'
    url_for_searchterm = 'https://www.vatera.hu/listings/index.php?q={searchterm}&qt=2&td=on&scategory_231340=choose&c=&p1={minprice}&p2={maxprice}&at=2&tr8=&re=0&sdr=&pci=0&on=0&ap=0&tr=0&dc=0&pw=0&ds=&de=&us=&ub=&ob=16&obd=2&behat_search_item='
                          
    def __init__(self, searchterm=None, fullink=None, spiderbotid = -1, maxpages=15, minprice=0, maxprice=Helpers.MAXPRICE, *args, **kwargs):
        super(Vatera, self).__init__(*args, **kwargs)
        if searchterm:
            self.start_urls = [Vatera.url_for_searchterm.format(searchterm=searchterm, minprice=minprice, maxprice=maxprice)]
            
        if fullink:
            self.start_urls = [f'{fullink}']
        logging.debug(f"Start url is: {self.start_urls}")
        
        if type(spiderbotid) == str:
            self.spiderbotid = int(spiderbotid)
        else: 
            self.spiderbotid = spiderbotid
        
        self.maxpages=maxpages
        self.scrapedpages=0
    
    def parse(self, response):
        logging.debug(f"Parse started")
        itemcount = 0
        for item in response.xpath("//div[@class='gtm-impression prod']"):
            itemcount += 1
            logging.debug(f"Parsing item {itemcount}")
            
            location = item.xpath(".//div[contains(text(),'Termék helye:')]/text()").get()
            if location:
                Helpers.getString(location = location.replace("Termék helye:", ""))

            yield ProductItem(
                title = item.xpath("@data-gtm-name").get(),
                price = Helpers.getNumber(item.xpath("@data-gtm-price").get()),
                seller = Helpers.getString(item.xpath(".//span[@class='userrating']/a/text()").get()),
                image_urls = Helpers.imageUrl(None, item.xpath(".//div[@class='picbox']/img/@data-original").get()),
                url = item.xpath(".//a[@class='product_link']/@href").get(),
                location = location,
                extraid = item.xpath("@data-product-id").get(),
                currency = "HUF",
                
                spiderbotid = self.spiderbotid
            )

        next_page = response.xpath("//a[@aria-label='Következő oldal']/@href").get()
        if next_page and self.scrapedpages<self.maxpages:
                self.scrapedpages += 1
                logging.debug(f"Next page (#{str(self.scrapedpages)} of {self.maxpages}): {next_page}")
                yield response.follow(next_page, self.parse)
        