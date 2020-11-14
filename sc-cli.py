import click
import os
import shutil
from pathlib import Path
import scrapy
from salesCrawlerScrapy.settings import DB_SETTINGS
from salesCrawlerScrapy.settings import SPIDERS
import MySQLdb
import datetime
import urllib.parse

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
@click.pass_context
def run(ctx):
    """Run all active crawlers"""
    click.echo('Run all active crawlers...')
    for crawler in selectDB(f"SELECT * FROM crawlers WHERE active=True"):
        click.echo(f"Run crawler: {crawler['name']} ({crawler['crawlerID']})")
        ctx.invoke(runCrawler, crawlerid = crawler['crawlerID'])

@cli.command()
@click.argument('crawlerid')
@click.pass_context
def runCrawler(ctx, crawlerid):
    """Run all active spiderbots owned by CRAWLERID"""
    click.echo(f'Run all active spiders of crawler {crawlerid}...')
    for spiderbot in selectDB(f"SELECT * FROM spiderbots WHERE crawlerID={crawlerid} and active=True"):
        click.echo(f"Run spiderbot: {spiderbot['spiderbotID']}")
        click.echo(f"   SearchTerm: {spiderbot['searchTerm']}")
        click.echo(f"      Fullink: {spiderbot['fullink']}")
        ctx.invoke(runSpider, spider = spiderbot['spider'], searchterm = spiderbot['searchTerm'], fullink = spiderbot['fullink'], spiderbotid = spiderbot['spiderbotID'])
    openDB()
    cursor.execute(f"UPDATE crawlers SET lastrun = %s WHERE crawlerID={crawlerid}", datetime.datetime.now())
    conn.commit()

@cli.command()
@click.argument('spider')
@click.argument('spiderbotid')
@click.option('-s', '--searchterm', help="Search term")
@click.option('-l', '--fullink', help="Full link instead of a search term")
def runSpider(spider, searchterm = None, fullink = None, spiderbotid = -1):
    """Run a SPIDER owned by SPIDERBOTID"""
    click.echo(f'Run spider: {spider} of spiderbot {str(spiderbotid)}')
    if searchterm:
        search=f"searchterm={searchterm}"
    else:
        search=f"fullink={urllib.parse.quote(fullink)}"
    
    os.chdir(os.path.join(Path.home(),'salescrawler'))
    os.system(f"scrapy crawl {spider} -a {search} -a spiderbotid={str(spiderbotid)}")

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
@click.argument('crawlerid')
@click.pass_context
def crawlerDelete(ctx, crawlerid):
    """Delete the crawler with CRAWLERID, and also delete it's spiderbots and matches"""
    click.echo(f"Delete crawler: {crawlerid}")
    for spiderbot in selectDB(f"SELECT spiderbotID FROM spiderbots WHERE crawlerID={crawlerid}"):
        ctx.invoke(spiderbotDelete, spiderbotid = spiderbot['spiderbotID'])
    cursor.execute(f"DELETE FROM crawlers WHERE crawlerID={crawlerid}")
    conn.commit()

# ------------------
# Spiderbot commands
# ------------------
@cli.command()
@click.argument('spider')
@click.argument('crawlerid')
@click.option('-s', '--searchterm', default='', help="Search term")
@click.option('-f', '--fullink', default='', help="Full link instead of a search term")
def spiderbotAdd(spider, crawlerid, searchterm, fullink):
    """Add a new spiderbot of SPIDER to crawler of CRAWLERID and return it's ID. Either searchTerm or fullink should be specified."""
    
    # call as:
    #   sc-cli.py spiderbotadd hardverapro 3 -f"testlink"
    #   sc-cli.py spiderbotadd hardverapro 3 -s"RX470"
    if not spider:
        raise Exception("A spider should be specified.")
    if not crawlerid:
        raise Exception("The owner clawlerid should be specified.")
    if searchterm and searchterm == "":
        searchterm = None
    if fullink and fullink == "":
        searchterm = None
    if not searchterm and not fullink:
        raise Exception("Either searchTerm or fullink should be specified.")
    if searchterm:
        fullink = None

    id = insertDB("spiderbots", {
        "spider": spider,
        "crawlerID": int(crawlerid),
        "searchTerm": searchterm,
        "fullink": fullink
    })
    click.echo(f"Spiderbot inserted, ID: {id}")
    return id

@cli.command()
@click.argument('spiderbotid')
def spiderbotDelete(spiderbotid):
    """Delete a spiderbot with SPIDERBOTID, and also delete it's matches"""
    click.echo(f"Delete spiderbot, ID: {spiderbotid}")
    openDB()
    cursor.execute(f"DELETE FROM matches WHERE spiderbotID={spiderbotid}")
    cursor.execute(f"DELETE FROM spiderbots WHERE spiderbotID={spiderbotid}")
    conn.commit()

if __name__ == '__main__':
    click.echo('SalesCrawler - Program to run regular searches on websites')
    cli()
    closeDB()