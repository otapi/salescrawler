from wtforms import Form, StringField, SelectField
from salesCrawlerScrapy.settings import SPIDERS

class SpidersForm(Form):
    select = SelectField('Select spider:', choices=SPIDERS)
    search = StringField('')