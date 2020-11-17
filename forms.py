from wtforms import Form, StringField, SelectField, DecimalField, BooleanField
from salesCrawlerScrapy.settings import SPIDERS

class SpiderbotForm(Form):
    select = SelectField('Select spider:', choices=SPIDERS)
    search = StringField('Name:')

class CrawlerForm(Form):
    name = StringField('Name:')

