import scrapy

from tutorial.helpers import Helpers

class Hardverapro(scrapy.Spider):
    name = 'hardverapro'
    def start_requests(self):
        urls = [
            'https://hardverapro.hu/aprok/keres.php?stext=RX470&county=&stcid=&settlement=&stmid=&minprice=&maxprice=&company=&cmpid=&user=&usrid=&selling=1&buying=1&stext_none=',
        ]
        for url in urls:
            yield scrapy.Request(url=url, callback=self.parse)

    def parse(self, response):
        for item in response.xpath("//li[@class='media']"):
            yield {
                'title': item.xpath(".//h1/a/text()").get(),
                'seller': item.xpath(".//div[@class='uad-misc']/div[0]/a/text()").get(),
                'imageUrl': item.xpath("./a/img/@src").get(),
                'url': item.xpath(".//h1/a/@href").get(),
                ##'description': "",
                'price': Helpers.getNumber(item.xpath(".//div[@class='uad-info']/div[@class='uad-price']/text()").get()),
                'currency': "HUF",
                'location': item.xpath(".//div[@class='uad-info']/div[@class='uad-light']/text()").get(),
                }

        next_page = response.xpath("//li[@class='nav-arrow']/a[@rel='next']/@href").get()
        if next_page:
            yield response.follow(response.xpath("//li[@class='nav-arrow']/a[@rel='next']/@href").get(), self.parse)
        