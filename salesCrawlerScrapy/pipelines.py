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
        'image_urls'
    ]

    def process_item(self, item, spider):
        # Inserts fields only of assigned ones - to use default from SQL DB.
        sql = 'INSERT INTO table_name ({fields}) VALUES ({values})'
        data = {}
        for field in item:
            if item[field] and field not in DatabasePipeline.ignoreFields:
                if field == 'images':
                    data[field] = '@@IMAGE@@'
                else:
                    data[field] = item[field]
        
        fields = ', '.join(data.keys())
        values = ', '.join(['"{0}"'.format(value) for value in data.values()])
        composed_sql = sql.format(fields=fields, values=values)
        
        if item['images']:
            composed_sql = composed_sql.replace('"@@IMAGE@@"', '%s')
            imgfile = os.path.join(Path.home(),'salescrawler/ImagesStore', item['images'][0]['path']) 
            with open(imgfile, 'rb') as file:
                binaryData = file.read()
            
            os.remove(imgfile) 
            insert_blob_tuple = (binaryData)
            logging.warning(composed_sql)
            self.cursor.execute(composed_sql, insert_blob_tuple)
        else:
            self.cursor.execute(composed_sql)
        
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