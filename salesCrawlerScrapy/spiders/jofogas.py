import scrapy

from salesCrawlerScrapy.helpers import Helpers
from salesCrawlerScrapy.items import ProductItem
import logging

class Jofogas(scrapy.Spider):
    name = 'jofogas'
    url_for_searchterm = 'https://www.jofogas.hu/magyarorszag?q={searchterm}&sp=1'

    def __init__(self, searchterm=None, fullink=None, spiderbotid = -1, maxpages=15, *args, **kwargs):
        super(Jofogas, self).__init__(*args, **kwargs)
        if searchterm:
            self.start_urls = [Jofogas.url_for_searchterm.format(searchterm=searchterm)]
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
        for item in response.xpath("//div//div[@class='contentArea']"):
            logging.debug(f"Parsing item")
            link = item.xpath(".//h3[@class='item-title']/a")
            if len(item.xpath(".//div[contains(text(),'Kiszállítás folyamatban')]").getall()) == 0:
                yield ProductItem(
                    title = link.xpath("text()").get(),
                    seller = None,
                    image_urls = Helpers.imageUrl(response, item.xpath(".//picture/img/@src").get()),
                    url = response.urljoin(link.xpath("@href").get()),
                    price = Helpers.getNumber(item.xpath(".//h3[@class='item-price']/text()").get()),
                    currency = Helpers.getCurrency(item.xpath(".//span[@class='currency']/text()").get()),
                    location = item.xpath(".//section[@class='reLiSection cityname']/text()").get(),

                    spiderbotid = self.spiderbotid,
                    extraid = None
                )

        next_page = response.xpath("//a[@class='ad-list-pager-item ad-list-pager-item-next active-item js_hist_li js_hist jofogasicon-right']/@href").get()
        if next_page:
            if self.scrapedpages<self.maxpages:
                self.scrapedpages += 1
                logging.debug(f"Next page (#{str(self.scrapedpages)} of {self.maxpages})")
                yield response.follow(next_page, self.parse)
        