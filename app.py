from flask import Flask
from flask_sqlalchemy import SQLAlchemy
from salesCrawlerScrapy.settings import DB_SETTINGS

app = Flask(__name__)

db=DB_SETTINGS['db']
user=DB_SETTINGS['user']
passwd=DB_SETTINGS['passwd']
host=DB_SETTINGS['host']

app.config['SQLALCHEMY_DATABASE_URI'] = f"mysql://{user}:{passwd}@{host}/{db}"
app.secret_key = "salescrawler"
db = SQLAlchemy(app)
