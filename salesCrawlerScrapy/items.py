# Define here the models for your scraped items
#
# See documentation in:
# https://docs.scrapy.org/en/latest/topics/items.html

import scrapy


class ProductItem(scrapy.Item):
    # define the fields for your item here like:
    # name = scrapy.Field()
    title  = scrapy.Field()
    seller = scrapy.Field()
    url = scrapy.Field()
    description = scrapy.Field()
    price = scrapy.Field()
    currency = scrapy.Field()
    location = scrapy.Field()
    images = scrapy.Field()
    image_urls = scrapy.Field()
    extraID = scrapy.Field()
    hash = scrapy.Field()
    pass
