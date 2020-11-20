from wtforms import Form, StringField, SelectMultipleField, DecimalField, BooleanField
import sclogic

class SpiderbotForm(Form):
    spiders = SelectMultipleField('Select spiders:', choices=sclogic.getSpiders().keys())
    searchterm = StringField('Search Term:')
    fullink = StringField('Fullink:')

class CrawlerForm(Form):
    name = StringField('Name:')


