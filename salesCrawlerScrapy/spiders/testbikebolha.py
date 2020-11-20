import scrapy

from salesCrawlerScrapy.helpers import Helpers
from salesCrawlerScrapy.items import ProductItem
import logging

class Testbikebolha(scrapy.Spider):
    name = 'testbikebolha'
    url_for_searchterm = 'https://bolha.testbike.hu/aprok?q={searchterm}&p=1&s=cheap'
                          
    def __init__(self, searchterm=None, fullink=None, spiderbotid = -1, maxpages=15, minprice=0, maxprice=Helpers.MAXPRICE, *args, **kwargs):
        super(Testbikebolha, self).__init__(*args, **kwargs)
        if searchterm:
            self.start_urls = [Testbikebolha.url_for_searchterm.format(searchterm=searchterm, minprice=minprice, maxprice=maxprice)]
            
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
        for item in response.xpath("//figure"):
            itemcount += 1
            logging.debug(f"Parsing item {itemcount}")
            
            yield ProductItem(
                title = Helpers.getString(item.xpath("./div[@class='relative']/div/a/@title").get()),
                url = response.urljoin(item.xpath("./div[@class='relative']/div/a/@href").get()),
                image_urls = Helpers.imageUrl(None, item.xpath("./div/div/a/img/@src").get()),
                extraid = item.xpath("./div[@class='relative']/div/a/@href").get(),
                seller = None,
                price = Helpers.getNumber(item.xpath(".//span[@class='d_block']/text()").get()),
                currency = Helpers.getCurrency(item.xpath(".//span[@class='d_block']/text()").get()),
                location = Helpers.getString(item.xpath(".//span[@class='d_block fw_bold color_light']/text()").get()),

                spiderbotid = self.spiderbotid
            )

        next_page = response.xpath("//a[i/@class='fa fa-angle-right d_inline_m']/@href").get()
        if next_page and self.scrapedpages<self.maxpages:
                self.scrapedpages += 1
                logging.debug(f"Next page (#{str(self.scrapedpages)} of {self.maxpages})")
                yield response.follow(next_page, self.parse)