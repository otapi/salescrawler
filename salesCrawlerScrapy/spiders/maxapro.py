import scrapy

from salesCrawlerScrapy.helpers import Helpers
from salesCrawlerScrapy.items import ProductItem
import logging

class Maxapro(scrapy.Spider):
    name = 'maxapro'
    url_for_searchterm = 'https://maxapro.hu/aprohirdetes/{searchterm}-order_priceasc'
                          
    def __init__(self, searchterm=None, fullink=None, spiderbotid = -1, maxpages=15, minprice=0, maxprice=Helpers.MAXPRICE, *args, **kwargs):
        super(Maxapro, self).__init__(*args, **kwargs)
        if searchterm:
            self.start_urls = [Maxapro.url_for_searchterm.format(searchterm=searchterm, minprice=minprice, maxprice=maxprice)]
            
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
        for item in response.xpath("//li[@class='srBlock']"):
            itemcount += 1
            logging.debug(f"Parsing item {itemcount}")
            
            yield ProductItem(
                title = Helpers.getString(item.xpath(".//div[@class='srData floatL']/div/h3/a/text()").get()),
                url = item.xpath(".//div[@class='srData floatL']/div/h3/a/@href").get(),
                seller = None,
                image_urls = Helpers.imageUrl(None, item.xpath(".//div[@class='srImg floatL']//img/@data-original").get()),
                extraid = item.xpath(".//div[@class='srData floatL']/div/h3/a/@href").get(),
                price = Helpers.getNumber(item.xpath(".//div[@class='srPrice']/text()").get()),
                currency = Helpers.getCurrency(item.xpath(".//div[@class='srPrice']/text()").get()),
                location = Helpers.getString(item.xpath(".//div[@class='location']/i/text()").get()),

                spiderbotid = self.spiderbotid,
                pageitemcount = itemcount,
                pagenumber = self.scrapedpages,
                pageurl = response.url
            )

        next_page = response.xpath("//div[@id='searchResultPagination']/a[contains(text(), 'Következő')]/@href").get()
        if next_page and self.scrapedpages<self.maxpages:
                self.scrapedpages += 1
                logging.debug(f"Next page (#{str(self.scrapedpages)} of {self.maxpages}): {next_page}")
                yield response.follow(next_page, self.parse)
        