import scrapy

class BlogSpider(scrapy.Spider):
    name = 'blogspider'
    def start_requests(self):
        urls = [
            'https://blog.scrapinghub.com',
        ]
        for url in urls:
            yield scrapy.Request(url=url, callback=self.parse)

    def parse(self, response):
        for title in response.css('.post-header>h2'):
            yield {'title': title.css('a ::text').get()}

        for next_page in response.css('a.next-posts-link'):
            yield response.follow(next_page, self.parse)