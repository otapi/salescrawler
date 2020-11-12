import click
import os
import shutil
from pathlib import Path
import scrapy

@click.group()
def cli():
    1

@cli.command()
def run():
    """Run hardverapro spider"""
    click.echo('Run hardverapro with RX470...')
    #os.chdir(os.path.join(Path.home(),'salescrawler'))
    os.system("cd salescrawler ; scrapy crawl hardverapro -a searchterm=RX470")

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