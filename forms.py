from wtforms import Form, StringField, SelectMultipleField, DecimalField, BooleanField
import sclogic

class SpiderbotForm(Form):
    spiders = SelectMultipleField('Select spiders:', choices=sclogic.getSpidersChoices())
    searchterm = StringField('Search term:')
    fullink = StringField('Fullink:')
    minprice = DecimalField('Minimum price:')
    maxprice = DecimalField('Maximum price:')

class CrawlerForm(Form):
    name = StringField('Name:')


