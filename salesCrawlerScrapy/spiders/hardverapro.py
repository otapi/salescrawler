import scrapy

from salesCrawlerScrapy.helpers import Helpers
from salesCrawlerScrapy.items import ProductItem

class Hardverapro(scrapy.Spider):
    name = 'hardverapro'
    def __init__(self, searchterm=None, fullink=None, spiderbotID = -1, *args, **kwargs):
        super(Hardverapro, self).__init__(*args, **kwargs)
        if searchterm:
            self.start_urls = [f'https://hardverapro.hu/aprok/keres.php?stext={searchterm}&county=&stcid=&settlement=&stmid=&minprice=&maxprice=&company=&cmpid=&user=&usrid=&selling=1&buying=1&stext_none=']
        if fullink:
            self.start_urls = [f'{fullink}']
        if type(spiderbotID) == str:
            self.spiderbotID = int(spiderbotID)
        else: 
            self.spiderbotID = spiderbotID

    
    def parse(self, response):
        for item in response.xpath("//li[@class='media']"):
            yield ProductItem(
                title = item.xpath(".//h1/a/text()").get(),
                seller = item.xpath(".//div[@class='uad-misc']/div/a/text()").get(),
                image_urls = [response.urljoin(item.xpath("./a/img/@src").get())],
                url = response.urljoin(item.xpath(".//h1/a/@href").get()),
                price = Helpers.getNumber(item.xpath(".//div[@class='uad-info']/div[@class='uad-price']/text()").get()),
                currency = 'HUF',
                location = item.xpath(".//div[@class='uad-info']/div[@class='uad-light']/text()").get(),
                spiderbotID = self.spiderbotID,
                extraID = None
            )

        next_page = response.xpath("//li[@class='nav-arrow']/a[@rel='next']/@href").get()
        if next_page:
            yield response.follow(response.xpath("//li[@class='nav-arrow']/a[@rel='next']/@href").get(), self.parse)
        