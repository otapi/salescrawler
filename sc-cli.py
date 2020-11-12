import click

@click.group()
def cli():
    a=1

@cli.command()
def run():
    """Run all active spiderBots"""
    click.echo('Running spiderBots...')

@cli.command()
def update():
    """Check for tool updates: re-clone tool, but keep DB and run crawlers"""
    click.echo('Update the tool...')

if __name__ == '__main__':
    click.echo('SalesCrawler - Program to run regular searches on websites')
    cli()