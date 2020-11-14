import click
import os
import shutil
from pathlib import Path
import scrapy
from salesCrawlerScrapy.settings import DB_SETTINGS
from salesCrawlerScrapy.settings import SPIDERS
import MySQLdb
import datetime

# -------
# Helpers
# -------
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

def selectDB(selectSQL):
    # return a dict from a select SQL
    openDB()
    cursor.execute(selectSQL)
    desc = cursor.description
    column_names = [col[0] for col in desc]
    data = [dict(zip(column_names, row))  
        for row in cursor.fetchall()]
    return data

# ----------------
# General commands
# ----------------
@click.group()
def cli():
    pass

@cli.command()
def clear():
    """Clear matches table"""
    click.echo('Clearing matches...')
    openDB()
    cursor.execute("DELETE FROM matches")
    conn.commit()

@cli.command()
def run():
    """Run all active crawlers"""
    click.echo('Run all active crawlers...')
    for crawler in selectDB(f"SELECT * FROM crawlers WHERE active=True"):
        click.echo(f"Run crawler: {crawler['name']} ({crawler['crawlerID']})")
        runCrawler(crawler['crawlerID'])

@cli.command()
@click.argument('crawlerID')
def runCrawler(crawlerID):
    """Run all active spiderbots owned by CRAWLERID"""
    click.echo(f'Run all active spiders of crawler {crawlerID}...')
    for spiderbot in selectDB(f"SELECT * FROM spiderbots WHERE crawlerID={crawlerID} and active=True"):
        click.echo(f"Run spiderbot: {spiderbot['spiderbotID']}")
        click.echo(f"   SearchTerm: {spiderbot['searchTerm']}")
        click.echo(f"      Fullink: {spiderbot['fullink']}")
        runSpider(spiderbot['spider'], spiderbot['searchTerm'], spiderbot['fullink'], spiderbot['spiderbotID'])
    openDB()
    cursor.execute(f"UPDATE crawlers SET lastrun = %s WHERE crawlerID={crawlerID}", datetime.datetime.now())
    conn.commit()

@cli.command()
@click.argument('spider')
@click.argument('spiderbotID')
@click.option('-s', '--searchTerm', help="Search term")
@click.option('-l', '--fullink', help="Full link instead of a search term")
def runSpider(spider, searchTerm = None, fullink = None, spiderbotID = -1):
    """Run a SPIDER owned by SPIDERBOTID"""
    click.echo(f'Run spider: {spider}')
    if searchTerm:
        search=f"searchTerm={searchTerm}"
    else:
        search=f"fullink={fullink}"
    
    os.chdir(os.path.join(Path.home(),'salescrawler'))
    os.system(f"scrapy crawl {spider} -a {search} -a spiderbotID={str(spiderbotID)}")

@cli.command()
def update():
    """Check for tool updates: re-clone tool, but keep DB and run crawlers"""
    click.echo('Update the tool...')
    shutil.rmtree(os.path.join(Path.home(),'salescrawler'))
    os.chdir(Path.home())
    os.system('git clone git@github.com:otapi/salescrawler.git')

@cli.command()
def getSpiders():
    """List available spiders"""
    click.echo('Available spiders:')
    click.echo(SPIDERS)
    return SPIDERS

# ----------------
# Crawler commands
# ----------------
@cli.command()
@click.argument('name')
def crawlerAdd(name):
    """Add a new crawler with NAME and return it's ID"""
    id = insertDB("crawlers", {
        "name": name
    })
    click.echo(f"Crawler inserted, ID: {id}")
    return id

@cli.command()
@click.argument('crawlerID')
def crawlerDelete(crawlerID):
    """Delete the crawler with CRAWLERID, and also delete it's spiderbots and matches"""
    click.echo(f"Delete crawler: {crawlerID}")
    for spiderbot in selectDB(f"SELECT spiderbotID FROM spiderbots WHERE crawlerID={crawlerID}"):
        spiderbotDelete(spiderbot['spiderbotID'])
    cursor.execute(f"DELETE FROM crawlers WHERE crawlerID={crawlerID}")
    conn.commit()

# ------------------
# Spiderbot commands
# ------------------
@cli.command()
@click.option('-s', '--searchTerm', help="Search term")
@click.option('-l', '--fullink', help="Full link instead of a search term")
@click.argument('spider')
@click.argument('crawlerid')
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

@cli.command()
@click.argument('spiderbotID')
def spiderbotDelete(spiderbotID):
    """Delete a spiderbot with SPIDERBOTID, and also delete it's matches"""
    click.echo(f"Delete spiderbot, ID: {spiderbotID}")
    openDB()
    cursor.execute(f"DELETE FROM matches WHERE spiderbotID={spiderbotID}")
    cursor.execute(f"DELETE FROM spiderbots WHERE spiderbotID={spiderbotID}")
    conn.commit()

if __name__ == '__main__':
    click.echo('SalesCrawler - Program to run regular searches on websites')
    cli()
    closeDB()