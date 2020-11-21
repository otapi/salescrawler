import scrapy

from salesCrawlerScrapy.helpers import Helpers
from salesCrawlerScrapy.items import ProductItem
import logging

class Aprohirdetescom(scrapy.Spider):
    name = 'aprohirdetescom'
    url_for_searchterm = 'https://www.aprohirdetes.com/hu/_?s=price_asc&q={searchterm}&z=11&md=50&c=_'
                          
    def __init__(self, searchterm=None, fullink=None, spiderbotid = -1, maxpages=15, minprice=0, maxprice=Helpers.MAXPRICE, *args, **kwargs):
        super(Aprohirdetescom, self).__init__(*args, **kwargs)
        if searchterm:
            self.start_urls = [Aprohirdetescom.url_for_searchterm.format(searchterm=searchterm, minprice=minprice, maxprice=maxprice)]
            
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
        for item in response.xpath("//div[@data-hirdetes-id]"):
            itemcount += 1
            logging.debug(f"Parsing item {itemcount}")
            
            yield ProductItem(
                extraid = item.xpath("./@data-hirdetes-id").get(),    
                image_urls = Helpers.imageUrl(None, item.xpath(".//img/@src").get()),    
                url = response.urljoin(item.xpath(".//a[@class='tile-link']/@href").get()),
                title = item.xpath(".//img/@alt").get(),
                seller = None,
                price = Helpers.getNumber(item.xpath(".//span[@class='h4 mr-1 text-primary']/text()").get()),
                currency = "HUF",
                location = Helpers.getString(item.xpath(".//p[@class='mb-0 text-muted text-sm']/text()").get()),

                spiderbotid = self.spiderbotid
            )

        next_page = response.urljoin(response.xpath("//li[@class='page-item active']/a[@class='page-link' and i/@class='fa fa-angle-right']/@href").get())
        if next_page and self.scrapedpages<self.maxpages:
                self.scrapedpages += 1
                logging.debug(f"Next page (#{str(self.scrapedpages)} of {self.maxpages}): {next_page}")
                yield response.follow(next_page, self.parse)
        