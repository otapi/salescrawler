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
import hashlib
import datetime

class DatabasePipeline:

    def __init__(self, db, user, passwd, host):
        self.db = db
        self.user = user
        self.passwd = passwd
        self.host = host

    def getHash(self, *args):
        """Get hash of the contained arguments. Empty/None arguments skipped.
        """
        text = ""
        for elem in args :
            if elem:
                text = text+"_"+str(elem)
        return hashlib.md5(bytes(text, 'utf-8') )

    ignoreFields = [
        'image_urls',
        'images'
    ]

    def process_item(self, item, spider):
        # Inserts fields only of assigned ones - to use default from SQL DB.
        sql = 'INSERT INTO matches ({fields}) VALUES ({values})'
        data = {}
        
        # keep only non-empty fields
        for field in item:
            if item[field] and field not in DatabasePipeline.ignoreFields:
                data[field] = item[field]
        
        # common fields
        data['hash'] = self.getHash(item['spiderbotID'], item['title'], item['seller'], item['extraID'])
        data['updated'] = datetime.datetime.now()

        # load images into blob
        if item['images']:
            imgfile = os.path.join(Path.home(),'salescrawler/ImagesStore', item['images'][0]['path']) 
            with open(imgfile, 'rb') as file:
                binaryData = file.read()
            os.remove(imgfile) 
            data['image'] = binaryData

        # check for already existing matches
        self.cursor.execute(f"SELECT price, shown, hide, hidedAt FROM matches WHERE hash='{data['hash']}'")
        olddata = self.cursor.fetchall()
        if len(olddata)>0:
            if data['price'] == olddata[0][0]:
                data['shown'] = olddata[0][1]
                data['hide'] = olddata[0][2]
                data['hidedAt'] = olddata[0][3]
            self.cursor.execute(f"DELETE FROM matches WHERE hash='{data['hash']}")
            self.conn.commit()

        # wrap up and commit the insert
        fields = ', '.join(data.keys())
        values = ', '.join(["%s" for value in data.values()])
        composed_sql = sql.format(fields=fields, values=values)
        print(f"Componsed_SQL: {composed_sql}")
        print(f"  data: {data}")
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