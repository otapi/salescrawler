import scrapy

from salesCrawlerScrapy.helpers import Helpers
from salesCrawlerScrapy.items import ProductItem
import logging

class Hardverapro(scrapy.Spider):
    name = 'hardverapro'
    url_for_searchterm = 'https://hardverapro.hu/aprok/keres.php?stext={searchterm}&county=&stcid=&settlement=&stmid=&minprice=&maxprice=&company=&cmpid=&user=&usrid=&selling=1&buying=1&stext_none='

    def __init__(self, searchterm=None, fullink=None, spiderbotid = -1, maxpages=15, *args, **kwargs):
        super(Hardverapro, self).__init__(*args, **kwargs)
        if searchterm:
            self.start_urls = [Hardverapro.url_for_searchterm.format(searchterm=searchterm)]
            logging.debug(f"Start url is: {self.start_urls}")
        if fullink:
            self.start_urls = [f'{fullink}']

        if type(spiderbotid) == str:
            self.spiderbotid = int(spiderbotid)
        else: 
            self.spiderbotid = spiderbotid
        
        self.maxpages=maxpages
        self.scrapedpages=0
    
    def parse(self, response):
        logging.debug(f"Parse started")
        for item in response.xpath("//li[@class='media']"):
            logging.debug(f"Parsing item")
            yield ProductItem(
                title = item.xpath(".//h1/a/text()").get(),
                seller = item.xpath(".//div[@class='uad-misc']/div/a/text()").get(),
                image_urls = Helpers.imageUrl(response, item.xpath("./a/img/@src").get()),
                url = response.urljoin(item.xpath(".//h1/a/@href").get()),
                price = Helpers.getNumber(item.xpath(".//div[@class='uad-info']/div[@class='uad-price']/text()").get()),
                currency = 'HUF',
                location = item.xpath(".//div[@class='uad-info']/div[@class='uad-light']/text()").get(),
                spiderbotid = self.spiderbotid,
                extraid = None
            )

        next_page = response.xpath("//li[@class='nav-arrow']/a[@rel='next']/@href").get()
        if next_page:
            if self.scrapedpages<self.maxpages:
                self.scrapedpages += 1
                logging.debug(f"Next page (#{str(self.scrapedpages)} of {self.maxpages})")
                yield response.follow(next_page, self.parse)
        