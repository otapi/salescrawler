from wtforms import Form, StringField, SelectMultipleField, DecimalField, BooleanField
import sclogic

class SpiderbotForm(Form):
    spiders = SelectMultipleField('Select spiders:', choices=sclogic.getSpidersChoices(), default=sclogic.getSpiders.keys())
    searchterm = StringField('Search term:')
    fullink = StringField('Fullink:')
    minprice = StringField('Minimum price:')
    maxprice = StringField('Maximum price:')

class CrawlerForm(Form):
    name = StringField('Name:')


