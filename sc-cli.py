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
    conn = MySQLdb.connect(db=DB_SETTINGS['db'],
                            user=DB_SETTINGS['user'], passwd=DB_SETTINGS['passwd'],
                            host=DB_SETTINGS['host'],
                            charset='utf8', use_unicode=True)
    cursor = conn.cursor()

@click.group()
def cli():
    1

@cli.command()
def clear():
    """Clear matches table"""
    click.echo('Clearing matches...')
    openDB()
    cursor.execute("DELETE FROM matches")
    conn.commit()
    conn.close()

@cli.command()
def run():
    """Run hardverapro spider"""
    click.echo('Run hardverapro with RX470...')
    os.chdir(os.path.join(Path.home(),'salescrawler'))
    #os.system("cd salescrawler ; scrapy crawl hardverapro -a searchterm=RX470")
    os.system("scrapy crawl hardverapro -a searchterm=RX470")

@cli.command()
def update():
    """Check for tool updates: re-clone tool, but keep DB and run crawlers"""
    click.echo('Update the tool...')
    shutil.rmtree(os.path.join(Path.home(),'salescrawler'))
    os.chdir(Path.home())
    os.system('git clone git@github.com:otapi/salescrawler.git')

if __name__ == '__main__':
    click.echo('SalesCrawler - Program to run regular searches on websites')
    cli()