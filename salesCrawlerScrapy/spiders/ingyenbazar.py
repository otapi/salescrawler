import scrapy

from salesCrawlerScrapy.helpers import Helpers
from salesCrawlerScrapy.items import ProductItem
import logging

class Ingyenbazar(scrapy.Spider):
    name = 'ingyenbazar'
    url_for_searchterm = 'https://www.ingyenbazar.hu/search/&search={searchterm}&kategoria=0&sub=&location=&city=&distance=30&from={minprice}&to={maxprice}&sort=price'
                          
    def __init__(self, searchterm=None, fullink=None, spiderbotid = -1, maxpages=15, minprice=0, maxprice=Helpers.MAXPRICE, *args, **kwargs):
        super(Ingyenbazar, self).__init__(*args, **kwargs)
        if searchterm:
            self.start_urls = [Ingyenbazar.url_for_searchterm.format(searchterm=searchterm, minprice=minprice, maxprice=maxprice)]
            
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
        for item in response.xpath("//div[@class='indent']"):
            itemcount += 1
            logging.debug(f"Parsing item {itemcount}")
            
            yield ProductItem(
                title = Helpers.getString(item.xpath(".//a[@class='product_name']/text()").get()),
                url = response.urljoin(item.xpath(".//a[@class='product_name']/@href").get()),
                extraid = item.xpath(".//a[@class='product_name']/@href").get(),
                seller = None,
                price = Helpers.getNumber(item.xpath(".//div[@class='price']/b/text()").get()),
                currency = Helpers.getNumber(item.xpath(".//div[@class='price']/b/text()").get()),
                location = Helpers.getString(item.xpath(".//div[@class='price']/i/b[1]/text()").get()),
                image_urls = Helpers.imageUrl(response, item.xpath(".//a[@class='product_name']/img/@src").get()),

                spiderbotid = self.spiderbotid
            )

        next_page = response.xpath("//a[@class='next']/@href").get()
        if next_page and self.scrapedpages<self.maxpages:
                self.scrapedpages += 1
                logging.debug(f"Next page (#{str(self.scrapedpages)} of {self.maxpages}): {next_page}")
                yield response.follow(next_page, self.parse)
        