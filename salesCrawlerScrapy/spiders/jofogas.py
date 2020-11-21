import scrapy

from salesCrawlerScrapy.helpers import Helpers
from salesCrawlerScrapy.items import ProductItem
import logging

class Jofogas(scrapy.Spider):
    name = 'jofogas'
    url_for_searchterm = 'https://www.jofogas.hu/magyarorszag?f=a&max_price={maxprice}&min_price={minprice}&q={searchterm}&sp=1'

    def __init__(self, searchterm=None, fullink=None, spiderbotid = -1, maxpages=15, minprice=0, maxprice=Helpers.MAXPRICE, *args, **kwargs):
        super(Jofogas, self).__init__(*args, **kwargs)
        if searchterm:
            self.start_urls = [Jofogas.url_for_searchterm.format(searchterm=searchterm, minprice=minprice, maxprice=maxprice)]
            
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
        for item in response.xpath("//div//div[@class='contentArea']"):
            itemcount += 1
            logging.debug(f"Parsing item {itemcount}")
            
            link = item.xpath(".//h3[@class='item-title']/a")
            if len(item.xpath(".//div[contains(text(),'Kiszállítás folyamatban')]").getall()) == 0:
                yield ProductItem(
                    title = Helpers.getString(link.xpath("text()").get()),
                    seller = None,
                    image_urls = Helpers.imageUrl(response, item.xpath(".//meta[@itemprop='image']/@content").get()),
                    url = response.urljoin(link.xpath("@href").get()),
                    extraid = link.xpath("@href").get(),
                    price = Helpers.getNumber(item.xpath(".//span[@class='price-value']/@content").get()),
                    currency = Helpers.getCurrency(item.xpath(".//span[@class='currency']/text()").get()),
                    location = Helpers.getString(item.xpath(".//section[@class='reLiSection cityname ']/text()").get()),

                    spiderbotid = self.spiderbotid
                )

        next_page = response.xpath("//a[@class='ad-list-pager-item ad-list-pager-item-next active-item js_hist_li js_hist jofogasicon-right']/@href").get()
        if next_page and self.scrapedpages<self.maxpages:
                self.scrapedpages += 1
                logging.debug(f"Next page (#{str(self.scrapedpages)} of {self.maxpages}): {next_page}")
                yield response.follow(next_page, self.parse)
        