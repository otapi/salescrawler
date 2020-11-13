import click
import os
import shutil
from pathlib import Path
import scrapy
from salesCrawlerScrapy.settings import DB_SETTINGS
import MySQLdb

conn = None
cursor = None
def openDB():
    global conn, cursor
    if not conn:
        conn = MySQLdb.connect(db=DB_SETTINGS['db'],
                                user=DB_SETTINGS['user'], passwd=DB_SETTINGS['passwd'],
                                host=DB_SETTINGS['host'],
                                charset='utf8', use_unicode=True)
        cursor = conn.cursor()

def closeDB():
    if conn:
        conn.close()

def insertDB(table, data):
    # insert a record into SQL based on a dict and returns the ID.
    openDB()
    sql = 'INSERT INTO '+table+' ({fields}) VALUES ({values})'
    fields = ', '.join(data.keys())
    values = ', '.join(["%s" for value in data.values()])
    composed_sql = sql.format(fields=fields, values=values)
    cursor.execute(composed_sql, data.values())
    conn.commit()
    return cursor.lastrowid

@click.group()
def general():
    pass

@general.command()
def clear():
    """Clear matches table"""
    click.echo('Clearing matches...')
    openDB()
    cursor.execute("DELETE FROM matches")
    conn.commit()

@general.command()
def run():
    """Run hardverapro spider"""
    click.echo('Run hardverapro with RX470...')
    os.chdir(os.path.join(Path.home(),'salescrawler'))
    #os.system("cd salescrawler ; scrapy crawl hardverapro -a searchterm=RX470")
    os.system("scrapy crawl hardverapro -a searchterm=RX470")

@general.command()
def update():
    """Check for tool updates: re-clone tool, but keep DB and run crawlers"""
    click.echo('Update the tool...')
    shutil.rmtree(os.path.join(Path.home(),'salescrawler'))
    os.chdir(Path.home())
    os.system('git clone git@github.com:otapi/salescrawler.git')

@click.group()
def crawler():
    pass

@crawler.command()
@click.argument('name')
def crawlerAdd(name):
    """Add a new crawler with NAME and return it's ID"""
    id = insertDB("crawlers", {
        "name": name
    })
    click.echo(f"Crawler inserted, ID: {id}")
    return id

@crawler.command()
@click.argument('crawlerid')
def crawlerDelete(crawlerid):
    """Delete a crawler with CRAWLERID, and also delete it's spiderbots and matches"""

@click.group()
def spiderbot():
    pass

@spiderbot.command()
@click.argument('spider')
@click.argument('crawlerid')
@click.option('-s', '--searchTerm', help="Search term", type=int)
@click.option('-l', '--fullink', help="Full link instead of a search term", type=int)
def spiderbotAdd(spider, crawlerid, searchTerm='', fullink=''):
    """Add a new spiderbot of SPIDER to crawler of CRAWLERID and return it's ID. Either searchTerm or fullink should be specified."""
    if not spider:
        raise Exception("A spider should be specified.")
    if not crawlerid:
        raise Exception("The owner clawlerid should be specified.")
    if searchTerm and searchTerm == "":
        searchTerm = None
    if fullink and fullink == "":
        searchTerm = None
    if not searchTerm and not fullink:
        raise Exception("Either searchTerm or fullink should be specified.")
    if searchTerm:
        fullink = None

    id = insertDB("spiderbots", {
        "spider": spider,
        "crawlerID": int(crawlerid),
        "searchTerm": searchTerm,
        "fullink": fullink
    })
    click.echo(f"Spiderbot inserted, ID: {id}")
    return id

@spiderbot.command()
@click.argument('crawlerid')
def spiderBotDelete(spiderbotid):
    """Delete a spiderbot with SPIDERBOTID, and also delete it's matches"""

cli = click.CommandCollection(sources=[general, crawler, spiderbot])

if __name__ == '__main__':
    click.echo('SalesCrawler - Program to run regular searches on websites')
    cli()
    closeDB()