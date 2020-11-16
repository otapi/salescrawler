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
def clear():
    """Clear matches table"""
    click.echo('Delete all matches...')
    openDB()
    cursor.execute("DELETE FROM matches")
    conn.commit()

def runall():
    """Run all active crawlers"""
    click.echo('Run all active crawlers...')
    for crawler in selectDB(f"SELECT * FROM crawlers WHERE active=True"):
        click.echo(f"Run crawler: {crawler['name']} ({crawler['crawlerid']})")
        runCrawler(crawlerid = crawler['crawlerid'])

def runCrawler(crawlerid):
    """Run all active spiderbots owned by crawlerid"""
    click.echo(f'Run all active spiders of crawler {crawlerid}...')
    for spiderbot in selectDB(f"SELECT * FROM spiderbots WHERE crawlerid={crawlerid} and active=True"):
        click.echo(f"Run spiderbot: {spiderbot['spiderbotid']}")
        click.echo(f"   searchterm: {spiderbot['searchterm']}")
        click.echo(f"      Fullink: {spiderbot['fullink']}")
        runSpider(spider = spiderbot['spider'], searchterm = spiderbot['searchterm'], fullink = spiderbot['fullink'], spiderbotid = spiderbot['spiderbotid'])
    openDB()
    cursor.execute(f"UPDATE crawlers SET lastrun = %s WHERE crawlerid={crawlerid}", (datetime.datetime.now(),))
    conn.commit()

def runSpider(spider, searchterm = None, fullink = None, spiderbotid = -1):
    """Run a SPIDER owned by spiderbotid"""
    click.echo(f'Run spider: {spider} of spiderbot {str(spiderbotid)}')
    if searchterm:
        search=f"searchterm={searchterm}"
    else:
        search=f"fullink={urllib.parse.quote(fullink)}"
    
    os.chdir(os.path.join(Path.home(),'salescrawler'))
    os.system(f"scrapy crawl {spider} -a {search} -a spiderbotid={str(spiderbotid)}")

def movetree(root_src_dir, root_target_dir):
    for src_dir, dirs, files in os.walk(root_src_dir):
        dst_dir = src_dir.replace(root_src_dir, root_target_dir)
        if not os.path.exists(dst_dir):
            os.makedirs(dst_dir)

        for file_ in files:
            src_file = os.path.join(src_dir, file_)
            dst_file = os.path.join(dst_dir, file_)
            if os.path.exists(dst_file):
                os.remove(dst_file)
                shutil.move(src_file, dst_dir)

def update():
    """Check for tool updates: re-clone tool, but keep DB and run crawlers"""
    click.echo('Update the tool...')

    click.echo('Saving ImagesStore folder...')
    source_dir = os.path.join(Path.home(),'/salescrawler/static/ImagesStore')
    target_dir = os.path.join(Path.home(),'~/savedImagesStore')
    movetree(source_dir, target_dir)
    input("Press Enter to continue...")

    click.echo('Get from github...')
    shutil.rmtree(os.path.join(Path.home(),'salescrawler'))
    os.chdir(Path.home())
    os.system('git clone git@github.com:otapi/salescrawler.git')
    input("Press Enter to continue...")

    click.echo('Restore ImagesStore folder...')
    movetree(source_dir, target_dir)

def getSpiders():
    """List available spiders"""
    click.echo('Available spiders:')
    click.echo(SPIDERS)
    return SPIDERS

# ----------------
# Crawler commands
# ----------------
def crawlerAdd(name):
    """Add a new crawler with NAME and return it's ID"""
    id = insertDB("crawlers", {
        "name": name
    })
    click.echo(f"Crawler inserted, ID: {id}")
    return id

def crawlerDelete(crawlerid):
    """Delete the crawler with crawlerid, and also delete it's spiderbots and matches"""
    click.echo(f"Delete crawler: {crawlerid}")
    for spiderbot in selectDB(f"SELECT spiderbotid FROM spiderbots WHERE crawlerid={crawlerid}"):
        spiderbotDelete(spiderbotid = spiderbot['spiderbotid'])
    cursor.execute(f"DELETE FROM crawlers WHERE crawlerid={crawlerid}")
    conn.commit()

# ------------------
# Spiderbot commands
# ------------------
def spiderbotAdd(spider, crawlerid, searchterm, fullink):
    """Add a new spiderbot of SPIDER to crawler of crawlerid and return it's ID. Either searchterm or fullink should be specified."""
    
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
        raise Exception("Either searchterm or fullink should be specified.")
    if searchterm:
        fullink = None

    id = insertDB("spiderbots", {
        "spider": spider,
        "crawlerid": int(crawlerid),
        "searchterm": searchterm,
        "fullink": fullink
    })
    click.echo(f"Spiderbot inserted, ID: {id}")
    return id

def spiderbotDelete(spiderbotid):
    """Delete a spiderbot with spiderbotid, and also delete it's matches"""
    click.echo(f"Delete spiderbot, ID: {spiderbotid}")
    openDB()
    cursor.execute(f"DELETE FROM matches WHERE spiderbotid={spiderbotid}")
    cursor.execute(f"DELETE FROM spiderbots WHERE spiderbotid={spiderbotid}")
    conn.commit()
