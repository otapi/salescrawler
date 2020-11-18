from wtforms import Form, StringField, SelectMultipleField, DecimalField, BooleanField
from salesCrawlerScrapy.settings import SPIDERS

class SpiderbotForm(Form):
    name = StringField('Name:')
    spiderbots = SelectMultipleField('Select spiders:', choices=SPIDERS)
    searchterm = StringField('Search Term:')
    fullink = StringField('Fullink:')

class CrawlerForm(Form):
    name = StringField('Name:')

