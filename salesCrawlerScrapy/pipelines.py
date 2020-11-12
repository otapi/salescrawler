# Define your item pipelines here
#
# Don't forget to add your pipeline to the ITEM_PIPELINES setting
# See: https://docs.scrapy.org/en/latest/topics/item-pipeline.html


# useful for handling different item types with a single interface
from itemadapter import ItemAdapter
import MySQLdb
import os
from pathlib import Path
import logging

class DatabasePipeline:

    def __init__(self, db, user, passwd, host):
        self.db = db
        self.user = user
        self.passwd = passwd
        self.host = host

    ignoreFields = [
        'image_urls',
        'images'
    ]

    def process_item(self, item, spider):
        # Inserts fields only of assigned ones - to use default from SQL DB.
        sql = 'INSERT INTO matches ({fields}) VALUES ({values})'
        # keep only non-empty fields
        data = {}
        for field in item:
            if item[field] and field not in DatabasePipeline.ignoreFields:
                data[field] = item[field]
        
        if item['images']:
            imgfile = os.path.join(Path.home(),'salescrawler/ImagesStore', item['images'][0]['path']) 
            with open(imgfile, 'rb') as file:
                binaryData = file.read()
            os.remove(imgfile) 
            data['image'] = binaryData

        fields = ', '.join(data.keys())
        values = ', '.join(["%s" for value in data.values()])
        composed_sql = sql.format(fields=fields, values=values)
        
        self.cursor.execute(composed_sql, data.values())
        self.conn.commit()
        return item

    def open_spider(self, spider):
        self.conn = MySQLdb.connect(db=self.db,
                                    user=self.user, passwd=self.passwd,
                                    host=self.host,
                                    charset='utf8', use_unicode=True)
        self.cursor = self.conn.cursor()

    def close_spider(self, spider):
        self.conn.close()

    @classmethod
    def from_crawler(cls, crawler):
        db_settings = crawler.settings.getdict("DB_SETTINGS")
        if not db_settings: # if we don't define db config in settings
            raise Exception('MySQL DB settings are missing from settings.py') # then reaise error
        db = db_settings['db']
        user = db_settings['user']
        passwd = db_settings['passwd']
        host = db_settings['host']
        return cls(db, user, passwd, host) # returning pipeline instance