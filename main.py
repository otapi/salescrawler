from flask import flash, render_template, request, redirect
from formencode import variabledecode
from app import app
from app import db

import forms
import models

@app.route('/', methods=['GET', 'POST'])
def index():
    if "run" in request.args:
        flash('run!')
    elif "update" in request.args:
        flash('update!')

    if request.method == 'POST':    
        postvars = variabledecode.variable_decode(request.form, dict_char='_')
        for k, v in postvars.items():
            match = models.Match.query.filter_by(matchid=int(k)).first()
            if "hide" in v and v["hide"] == "on":
                match.hide = True
            else:
                match.hide = False
        db.session.commit()

    matches = models.Match.query.filter_by(hide=False)
    return render_template('matches.html', matches=matches) 

@app.route('/searchform', methods=['GET', 'POST'])
def searchfrom():
    spider = forms.SpidersForm(request.form)
    if request.method == 'POST':
        return spider_results(spider)
    return render_template('search.html', form=spider)

"""
if __name__ == '__main__':
    import argparse
    parser = argparse.ArgumentParser()
    parser.add_argument('--debug', '-d', action='store_true')
    parser.add_argument('--port', '-p', default=5000, type=int)
    parser.add_argument('--host', default='0.0.0.0')

    args = parser.parse_args()
    app.run(args.host, args.port, debug=args.debug)
"""

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


@app.cli.command("clear")
def clear():
    """Clear matches table"""
    click.echo('Clearing matches...')
    openDB()
    cursor.execute("DELETE FROM matches")
    conn.commit()

@app.cli.command("runall")
@click.pass_context
def runall(ctx):
    """Run all active crawlers"""
    click.echo('Run all active crawlers...')
    for crawler in selectDB(f"SELECT * FROM crawlers WHERE active=True"):
        click.echo(f"Run crawler: {crawler['name']} ({crawler['crawlerid']})")
        ctx.invoke(runCrawler, crawlerid = crawler['crawlerid'])

@app.cli.command("runCrawler")
@click.argument('crawlerid')
@click.pass_context
def runCrawler(ctx, crawlerid):
    """Run all active spiderbots owned by crawlerid"""
    click.echo(f'Run all active spiders of crawler {crawlerid}...')
    for spiderbot in selectDB(f"SELECT * FROM spiderbots WHERE crawlerid={crawlerid} and active=True"):
        click.echo(f"Run spiderbot: {spiderbot['spiderbotid']}")
        click.echo(f"   searchterm: {spiderbot['searchterm']}")
        click.echo(f"      Fullink: {spiderbot['fullink']}")
        ctx.invoke(runSpider, spider = spiderbot['spider'], searchterm = spiderbot['searchterm'], fullink = spiderbot['fullink'], spiderbotid = spiderbot['spiderbotid'])
    openDB()
    cursor.execute(f"UPDATE crawlers SET lastrun = %s WHERE crawlerid={crawlerid}", datetime.datetime.now())
    conn.commit()

@app.cli.command("runSpider")
@click.argument('spider')
@click.argument('spiderbotid')
@click.option('-s', '--searchterm', help="Search term")
@click.option('-l', '--fullink', help="Full link instead of a search term")
def runSpider(spider, searchterm = None, fullink = None, spiderbotid = -1):
    """Run a SPIDER owned by spiderbotid"""
    click.echo(f'Run spider: {spider} of spiderbot {str(spiderbotid)}')
    if searchterm:
        search=f"searchterm={searchterm}"
    else:
        search=f"fullink={urllib.parse.quote(fullink)}"
    
    os.chdir(os.path.join(Path.home(),'salescrawler'))
    os.system(f"scrapy crawl {spider} -a {search} -a spiderbotid={str(spiderbotid)}")

@app.cli.command("update")
def update():
    """Check for tool updates: re-clone tool, but keep DB and run crawlers"""
    click.echo('Update the tool...')
    shutil.rmtree(os.path.join(Path.home(),'salescrawler'))
    os.chdir(Path.home())
    os.system('git clone git@github.com:otapi/salescrawler.git')

@app.cli.command("getSpiders")
def getSpiders():
    """List available spiders"""
    click.echo('Available spiders:')
    click.echo(SPIDERS)
    return SPIDERS

# ----------------
# Crawler commands
# ----------------
@app.cli.command("crawlerAdd")
@click.argument('name')
def crawlerAdd(name):
    """Add a new crawler with NAME and return it's ID"""
    id = insertDB("crawlers", {
        "name": name
    })
    click.echo(f"Crawler inserted, ID: {id}")
    return id

@app.cli.command("crawlerDelete")
@click.argument('crawlerid')
@click.pass_context
def crawlerDelete(ctx, crawlerid):
    """Delete the crawler with crawlerid, and also delete it's spiderbots and matches"""
    click.echo(f"Delete crawler: {crawlerid}")
    for spiderbot in selectDB(f"SELECT spiderbotid FROM spiderbots WHERE crawlerid={crawlerid}"):
        ctx.invoke(spiderbotDelete, spiderbotid = spiderbot['spiderbotid'])
    cursor.execute(f"DELETE FROM crawlers WHERE crawlerid={crawlerid}")
    conn.commit()

# ------------------
# Spiderbot commands
# ------------------
@app.cli.command("spiderbotAdd")
@click.argument('spider')
@click.argument('crawlerid')
@click.option('-s', '--searchterm', default='', help="Search term")
@click.option('-f', '--fullink', default='', help="Full link instead of a search term")
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

@app.cli.command("spiderbotDelete")
@click.argument('spiderbotid')
def spiderbotDelete(spiderbotid):
    """Delete a spiderbot with spiderbotid, and also delete it's matches"""
    click.echo(f"Delete spiderbot, ID: {spiderbotid}")
    openDB()
    cursor.execute(f"DELETE FROM matches WHERE spiderbotid={spiderbotid}")
    cursor.execute(f"DELETE FROM spiderbots WHERE spiderbotid={spiderbotid}")
    conn.commit()
