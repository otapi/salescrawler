import scrapy
from salesCrawlerScrapy.helpers import Helpers
from salesCrawlerScrapy.items import ProductItem

import logging

class Aprohirdetesingyenhu(scrapy.Spider):
    name = 'aprohirdetesingyenhu'
    url_for_searchterm = 'https://aprohirdetesingyen.hu/osszes-hirdetes/kereses--{searchterm}'
                          
                          
    def __init__(self, searchterm=None, fullink=None, spiderbotid = -1, maxpages=15, minprice=0, maxprice=Helpers.MAXPRICE, *args, **kwargs):
        super(Aprohirdetesingyenhu, self).__init__(*args, **kwargs)
        if searchterm:
            self.start_urls = [Aprohirdetesingyenhu.url_for_searchterm.format(searchterm=searchterm, minprice=minprice, maxprice=maxprice)]
            
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
        request = scrapy.Request(url="https://aprohirdetesingyen.hu/ajax/listazas-frissitese/rendezes-osszes-hirdetes/4",
                                headers={'referer': response.url},
                                callback=self.parse_sorted)
        yield request

    def parse_sorted(self, response):
        logging.debug(f"Parse storted")
        itemcount = 0

        for item in response.xpath("//div[@class='h0_elem']"):
            itemcount += 1
            logging.debug(f"Parsing item {itemcount}")
            
            yield ProductItem(
                title = Helpers.getString(item.xpath(".//div[@ class='h_nev']//a/text()").get()),
                url = response.urljoin(item.xpath(".//div[@class='h_info']/a[@style='color:#427CB3']/@href").get()),
                image_urls = Helpers.imageUrl(None, item.xpath(".//div[@class='h_left_inner']//img/@src").get()),
                extraid = item.xpath(".//div[@class='h_info']/a[@style='color:#427CB3']/@href").get(),
                seller = None,
                price = Helpers.getNumber(item.xpath(".//div[@class='h_ar']/text()").get()),
                currency = Helpers.getCurrency(item.xpath(".//div[@class='h_ar']/text()").get()),
                location = None,

                spiderbotid = self.spiderbotid
            )

        next_page = response.xpath("//a[@class = 'h_oldal_kovetkezo']/@href").get()
        if next_page and self.scrapedpages<self.maxpages:
                self.scrapedpages += 1
                logging.debug(f"Next page (#{str(self.scrapedpages)} of {self.maxpages}): {next_page}")
                yield response.follow(next_page, self.parse_sorted)
        