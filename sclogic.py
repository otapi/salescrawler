import click
import os
import shutil
from pathlib import Path
from salesCrawlerScrapy.settings import IMAGES_STORE
import datetime
import urllib.parse

from salesCrawlerScrapy import spiders
import inspect
import logging

from app import db
import models

# ----------------
# General commands
# ----------------
def clear():
    """Clear matches table"""
    models.Match.query.delete()
    shutil.rmtree(os.path.join(Path.home(),'salescrawler', IMAGES_STORE))
    db.session.commit()
    
    
def runall():
    """Run all active crawlers"""
    click.echo('Run all active crawlers...')
    for crawler in models.Crawler.query.filter_by(active=True):
        click.echo(f"Run crawler: {crawler.name} ({crawler.crawlerid})")
        runCrawler(crawlerid = crawler.crawlerid)

def runCrawler(crawlerid):
    """Run all active spiderbots owned by crawlerid"""
    click.echo(f'Run all active spiders of crawler {crawlerid}...')
    for spiderbot in models.Spiderbot.query.filter_by(crawlerid=crawlerid, active=True):
        runSpider(spider = spiderbot.spider, searchterm = spiderbot.searchterm, fullink = spiderbot.fullink, spiderbotid = spiderbot.spiderbotid, crawlerid = crawlerid, minprice=spiderbot.minprice, maxprice=spiderbot.maxprice)
    cr = models.Crawler.query.filter_by(crawlerid=crawlerid).first()
    cr.lastrun = datetime.datetime.now()
    db.session.commit()

def connectvpn():
    """ Connect to protonVPN"""
    os.system("sudo protonvpn c -f")

def disconnectvpn():
    """ Connect to protonVPN"""
    os.system("sudo protonvpn d")

def runSpider(spider, spiderbotid, crawlerid, searchterm, fullink, minprice, maxprice):
    """Run a SPIDER owned by spiderbotid"""
    click.echo(f'Run spider: {spider} of spiderbot {str(spiderbotid)}')
    if fullink:
        search=f'"fullink={urllib.parse.quote(fullink)}"'
    else:
        search=f'"searchterm={searchterm}"'
    
    plus = ""
    if minprice:
        plus = f" -a minprice={str(int(minprice))}"
    if maxprice:
        plus += f" -a maxprice={str(int(maxprice))}"
    
    os.chdir(os.path.join(Path.home(),'salescrawler'))
    runcode = f"scrapy crawl {spider} -a {search} -a spiderbotid={str(spiderbotid)} -a crawlerid={str(crawlerid)}{plus}"
    click.echo(f"cmd: "+runcode)
    os.system(runcode)
    db.session.commit()
    
    # autohide if overpriced
    spiderbot = models.Spiderbot.query.filter_by(spiderbotid=spiderbotid).first()
    maxprice = spiderbot.crawler.maxprice
    if maxprice:
        found = False
        for match in models.Match.query.filter_by(spiderbotid=spiderbotid).filter_by(hide=False).all():
            if match.price and match.price > maxprice:
                match.hide = True
                db.session.commit()
                found = True
        if found:
            click.echo(f'Some matches were autohidden due higher price than set in the crawler.')

    # autohide if belowpriced. Will hide even None if specified
    spiderbot = models.Spiderbot.query.filter_by(spiderbotid=spiderbotid).first()
    minprice = spiderbot.crawler.minprice
    if minprice:
        found = False
        for match in models.Match.query.filter_by(spiderbotid=spiderbotid).filter_by(hide=False).all():
            if (not match.price) or match.price < minprice:
                match.hide = True
                db.session.commit()
                found = True
        if found:
            click.echo(f'Some matches were autohidden due lower or missing price than set in the crawler.')
    click.echo(f'Completed spider: {spider} of spiderbot {str(spiderbotid)}')

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

    disconnectvpn()
    click.echo('Saving ImagesStore folder...')
    source_dir = os.path.join(Path.home(),'salescrawler', IMAGES_STORE)
    target_dir = os.path.join(Path.home(),'savedImagesStore')
    movetree(source_dir, target_dir)

    click.echo('Get from github...')
    shutil.rmtree(os.path.join(Path.home(),'salescrawler'))
    os.chdir(Path.home())
    os.system('git clone git@github.com:otapi/salescrawler.git')

    click.echo('Restore ImagesStore folder...')
    movetree(target_dir, source_dir)

def getSpiders():
    """Dict of available spiders"""
    ret = {}
    for name, obj in inspect.getmembers(spiders):
        if inspect.isclass(obj):
            ret[obj.name] = obj
    return ret

def getSpidersChoices():
    """Dict of available spiders"""
    ret = []
    for spidername in getSpiders().keys():
        c = (spidername, spidername)
        ret.append(c)
    return ret

# ----------------
# Crawler commands
# ----------------
def crawlerAdd(name):
    """Add a new crawler with NAME and return it's ID"""
    cr = models.Crawler()
    cr.name = name
    db.session.add(cr)
    db.session.commit()
    click.echo(f"Crawler inserted, ID: {cr.crawlerid}")
    return cr.crawlerid

def crawlerDelete(crawlerid):
    """Delete the crawler with crawlerid, and also delete it's spiderbots and matches"""
    click.echo(f"Delete crawler: {crawlerid}")
    for spiderbot in models.Spiderbot.query.filter_by(crawlerid=crawlerid):
        spiderbotDelete(spiderbotid = spiderbot.spiderbotid)
    models.Crawler.query.filter_by(crawlerid=crawlerid).delete()
    db.session.commit()

# ------------------
# Spiderbot commands
# ------------------
def spiderbotAdd(spider, crawlerid, searchterm, fullink, minprice, maxprice):
    """Add a new spiderbot of SPIDER to crawler of crawlerid and return it's ID. Either searchterm or fullink should be specified."""
    click.echo(f"Add spider {spider}")
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
    
    sb = models.Spiderbot()
    sb.spider = spider
    sb.crawlerid = int(crawlerid)
    sb.searchterm = searchterm
    sb.fullink = fullink
    sb.minprice = minprice
    sb.maxprice = maxprice

    if searchterm:
        sb.fullinkref = getSpiders()[sb.spider].url_for_searchterm.format(searchterm=searchterm, minprice=int(minprice), maxprice=int(maxprice))
    
    db.session.add(sb)
    db.session.commit()
        
    click.echo(f"Spiderbot inserted, ID: {sb.spiderbotid}")
    return sb.spiderbotid

def spiderbotDelete(spiderbotid):
    """Delete a spiderbot with spiderbotid, and also delete it's matches"""
    click.echo(f"Delete spiderbot, ID: {spiderbotid}")
    for match in models.Match.query.filter_by(spiderbotid=spiderbotid).all():
        lcl = os.path.join(Path.home(),'salescrawler', 'static', match.image)
        if os.path.exists(lcl):
                os.remove(lcl)
    models.Match.query.filter_by(spiderbotid=spiderbotid).delete()
    models.Spiderbot.query.filter_by(spiderbotid=spiderbotid).delete()
    db.session.commit()