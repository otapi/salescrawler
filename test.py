#from salesCrawlerScrapy.spiders.hardverapro import Hardverapro
from salesCrawlerScrapy import spiders
import inspect

print(spiders.Hardverapro().name)
for name, obj in inspect.getmembers(spiders):
        if inspect.isclass(obj):
            print(obj)
            print(obj().name)
