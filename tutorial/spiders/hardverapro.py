import scrapy

class Hardverapro(scrapy.Spider):
    name = 'hardverapro'
    def start_requests(self):
        urls = [
            'https://hardverapro.hu/aprok/keres.php?stext=RX470&county=&stcid=&settlement=&stmid=&minprice=&maxprice=&company=&cmpid=&user=&usrid=&selling=1&buying=1&stext_none=',
        ]
        for url in urls:
            yield scrapy.Request(url=url, callback=self.parse)

    def parse(self, response):
        for item in response.xpath('//li[@class='media']'):
            yield {
                'title': item.css('//h1/a/text()').get()
                }

        #for next_page in response.css('a.next-posts-link'):
        #    yield response.follow(next_page, self.parse)