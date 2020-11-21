import scrapy

from salesCrawlerScrapy.helpers import Helpers
from salesCrawlerScrapy.items import ProductItem
import logging

class Teszvesz(scrapy.Spider):
    name = 'teszvesz'
    url_for_searchterm = 'https://www.teszvesz.hu/listings/index.php?q={searchterm}&qt=2&td=on&c=&p1={minprice}&p2={maxprice}&at=2&re=0&on=0&ap=0&tr=0&dc=0&pw=0&ds=&de=&us=&ub=&ob=16&obd=2&behat_search_item=Keres%C3%A9s'
                          
    def __init__(self, searchterm=None, fullink=None, spiderbotid = -1, maxpages=15, minprice=0, maxprice=Helpers.MAXPRICE, *args, **kwargs):
        super(Teszvesz, self).__init__(*args, **kwargs)
        if searchterm:
            self.start_urls = [Teszvesz.url_for_searchterm.format(searchterm=searchterm, minprice=minprice, maxprice=maxprice)]
            
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
        for item in response.xpath("//tr[@data-gtm-name]"):
            itemcount += 1
            logging.debug(f"Parsing item {itemcount}")
            
            yield ProductItem(
                location = None,
                title = item.xpath("@data-gtm-name").get(),
                price = Helpers.getNumber(item.xpath("@data-gtm-price").get()),
                seller = None,
                image_urls = Helpers.imageUrl(None, item.xpath(".//td[@class='listing-item-picture']/span/img/@src").get()),

                url = item.xpath(".//a[@class='itemlink']/@href").get(),

                
                extraid = item.xpath("@data-gtm-id").get(),
                currency = "HUF",
                
                spiderbotid = self.spiderbotid
            )

        next_page = response.xpath("//a[contains(img/@src, '/arw_frw.gif')]/@href").get()
        if next_page and self.scrapedpages<self.maxpages:
                self.scrapedpages += 1
                logging.debug(f"Next page (#{str(self.scrapedpages)} of {self.maxpages}): {next_page}")
                yield response.follow(next_page, self.parse)