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
        return hashlib.md5(bytes(text, 'utf-8')).hexdigest()

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
        data['hash'] = self.getHash(item['spiderbotid'], item['title'], item['seller'], item['extraid'])
        data['updated'] = self.updateDateTime

        # load images into blob
        """
        if item['images']:
            imgfile = os.path.join(Path.home(),'salescrawler/ImagesStore', item['images'][0]['path']) 
            with open(imgfile, 'rb') as file:
                binaryData = file.read()
            os.remove(imgfile) 
            data['image'] = binaryData
        """
        if item['images']:
            data['image'] = os.path.join('/ImagesStore', item['images'][0]['path']) 
        else:
            data['image'] = ""

        # check for already existing matches
        self.cursor.execute(f"SELECT price, shown, hide, hidedat FROM matches WHERE hash='{data['hash']}'")
        olddata = self.cursor.fetchall()
        if len(olddata)>0:
            if ('price' in data and data['price'] == olddata[0][0]) or ('price' not in data and not olddata[0][0]):
                data['shown'] = olddata[0][1]
                data['hide'] = olddata[0][2]
                data['hidedat'] = olddata[0][3]
            self.cursor.execute(f"DELETE FROM matches WHERE hash='{data['hash']}'")
            self.conn.commit()

        # wrap up and commit the insert
        fields = ', '.join(data.keys())
        values = ', '.join(["%s" for value in data.values()])
        composed_sql = sql.format(fields=fields, values=values)
        self.cursor.execute(composed_sql, data.values())
        self.conn.commit()        
        if data['spiderbotid'] not in self.spiderbotids:
            self.spiderbotids.append(data['spiderbotid'])
        return item

    def open_spider(self, spider):
        self.conn = MySQLdb.connect(db=self.db,
                                    user=self.user, passwd=self.passwd,
                                    host=self.host,
                                    charset='utf8', use_unicode=True)
        self.cursor = self.conn.cursor()
        self.updateDateTime = datetime.datetime.now()
        self.spiderbotids = []

    def close_spider(self, spider):
        for spiderbotid in self.spiderbotids:
            self.cursor.execute("DELETE FROM matches WHERE timestampdiff(MINUTE, updated, %s) <> 0 AND spiderbotid = %s", (self.updateDateTime, spiderbotid))
        self.conn.commit()
        self.conn.close()

    @classmethod
    def from_crawler(cls, crawler):
        db_settings = crawler.settings.getdict("DB_SETTINGS")
        if not db_settings: # if we don't define db config in settings
            raise Exception('MySQL DB settings are missing from settings.py') # then raise error
        db = db_settings['db']
        user = db_settings['user']
        passwd = db_settings['passwd']
        host = db_settings['host']
        return cls(db, user, passwd, host) # returning pipeline instance