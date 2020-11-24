from app import db

# Crawlers are the set up of batch of spiders for searching an item (key term)
class Crawler(db.Model):
    __tablename__ = "crawlers"
    db.Column()
    crawlerid = db.Column(db.Integer, primary_key=True)
    # Should it run?
    active = db.Column(db.Boolean, default=True)
    name = db.Column(db.String(255), nullable=False, unique=True)
    # User notes
    notes = db.Column(db.String(2550))
    # hours - How frequent should it run?
    runcadence = db.Column(db.Integer(), default=True)
    # When run last time?
    lastrun = db.Column(db.DateTime)
    # Autohide matches priced above this
    maxprice = db.Column(db.Float)
    # Autohide matches priced above this
    minprice = db.Column(db.Float)

    def __repr__(self):
        return '<Crawler %r>' % self.name

# Spiders with specified search term and owner of crawler
class Spiderbot(db.Model):
    __tablename__ = "spiderbots"
    spiderbotid = db.Column(db.Integer, primary_key=True)
    # Item or key term to search
    searchterm = db.Column(db.String(255))
    # Full url for search (instead of searchterm)
    fullink = db.Column(db.String(2550))
    # Calculated reference full url for search (instead of searchterm)
    fullinkref = db.Column(db.String(2550))
    # Name of the spider
    spider = db.Column(db.String(255))
    # Should it run?
    active = db.Column(db.Boolean, default=True)
    # minprice
    minprice = db.Column(db.Float)
    # maxprice
    maxprice = db.Column(db.Float)

    # foreign key to crawler
    crawlerid = db.Column(db.Integer, db.ForeignKey("crawlers.crawlerid"))
    crawler = db.relationship("Crawler", backref=db.backref(
        "spiderbots"), lazy=True)

    def __repr__(self):
        return '<Spiderbot %r>' % self.spiderbotid

# This is where the results go from spiders
class Match(db.Model):
    __tablename__ = "matches"
    matchid = db.Column(db.Integer, primary_key=True)
    # Title of the matched item
    title = db.Column(db.String(255))
    seller = db.Column(db.String(255))
    # Picture of the item
    image = db.Column(db.String(255))
    url = db.Column(db.String(2550))
    description = db.Column(db.String(255))
    # Actual price
    price = db.Column(db.Float)
    currency = db.Column(db.String(10))
    isauction = db.Column(db.Boolean, default=False)
    # Geography location of the item
    location = db.Column(db.String(255))
    # If the matched item expire
    expire = db.Column(db.DateTime)
    # When updated this record last time?
    updated = db.Column(db.DateTime)
    # Was already show to user?
    shown = db.Column(db.Boolean, default=False)
    # Hide this match from the user
    hide = db.Column(db.Boolean, default=False)
    # When did user hided this match?
    hidedat = db.Column(db.DateTime)
    # Hash code of the match (filled via pipeline)
    hash = db.Column(db.String(255))
    # Extra ID field to identify the match
    extraid = db.Column(db.String(255))
    # itemcount on the page found
    pageitemcount = db.Column(db.Integer)
    # number of page where found
    pagenumber = db.Column(db.Integer)
    # url of the page on found
    pageurl = db.Column(db.String(2550))
    # Saved for later?
    saved = db.Column(db.Boolean, default=False)


    # foreign key to spiderbot
    spiderbotid = db.Column(db.Integer, db.ForeignKey("spiderbots.spiderbotid"))
    spiderbot = db.relationship("Spiderbot", backref=db.backref(
        "spiderbots"), lazy=True)

    def __repr__(self):
        return '<Match %r>' % self.title

db.create_all()