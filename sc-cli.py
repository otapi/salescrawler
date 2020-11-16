import click
import sclogic

# ----------------
# General commands
# ----------------
@click.group()
def cli():
    pass

@cli.command()
def clear():
    sclogic.clear()

@cli.command()
def runall():
    """Run all active crawlers"""
    sclogic.runall()

@cli.command()
@click.argument('crawlerid')
def runCrawler(crawlerid):
    """Run all active spiderbots owned by crawlerid"""
    sclogic.runCrawler(crawlerid)

@cli.command()
@click.argument('spider')
@click.argument('spiderbotid')
@click.option('-s', '--searchterm', help="Search term")
@click.option('-l', '--fullink', help="Full link instead of a search term")
def runSpider(spider, searchterm = None, fullink = None, spiderbotid = -1):
    """Run a SPIDER owned by spiderbotid"""
    sclogic.runSpider(spider, searchterm, fullink, spiderbotid)

@cli.command()
def update():
    """Check for tool updates: re-clone tool, but keep DB and run crawlers"""
    sclogic.update()

@cli.command()
def getSpiders():
    sclogic.getSpiders()

# ----------------
# Crawler commands
# ----------------
@cli.command()
@click.argument('name')
def crawlerAdd(name):
    """Add a new crawler with NAME and return it's ID"""
    sclogic.crawlerAdd(name)

@cli.command()
@click.argument('crawlerid')
def crawlerDelete(crawlerid):
    """Delete the crawler with crawlerid, and also delete it's spiderbots and matches"""
    sclogic.crawlerDelete(crawlerid)

# ------------------
# Spiderbot commands
# ------------------
@cli.command()
@click.argument('spider')
@click.argument('crawlerid')
@click.option('-s', '--searchterm', default='', help="Search term")
@click.option('-f', '--fullink', default='', help="Full link instead of a search term")
def spiderbotAdd(spider, crawlerid, searchterm, fullink):
    """Add a new spiderbot of SPIDER to crawler of crawlerid and return it's ID. Either searchterm or fullink should be specified."""
    sclogic.spiderbotAdd(spider, crawlerid, searchterm, fullink)    
    # call as:
    #   sc-cli.py spiderbotadd hardverapro 3 -f"testlink"
    #   sc-cli.py spiderbotadd hardverapro 3 -s"RX470"

@cli.command()
@click.argument('spiderbotid')
def spiderbotDelete(spiderbotid):
    """Delete a spiderbot with spiderbotid, and also delete it's matches"""
    sclogic.spiderbotDelete(spiderbotid)

if __name__ == '__main__':
    click.echo('SalesCrawler - Program to run regular searches on websites')
    cli()
    closeDB()