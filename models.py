from app import db

# Crawlers are the set up of batch of spiders for searching an item (key term)
class Crawler(db.Model):
    __tablename__ = "crawlers"
    db.Column()
    crawlerid = db.Column(db.Integer, primary_key=True)
    # Should it run?
    active = db.Column(db.Boolean, default=True)
    name = db.Column(db.String, nullable=False, unique=True)
    # hours - How frequent should it run?
    runcadence = db.Column(db.Integer(), default=1)
    # When run last time?
    lastrun = db.Column(db.DateTime)

    def __repr__(self):
        return '<Crawler %r>' % self.name

# Spiders with specified search term and owner of crawler
class Spiderbot(db.Model):
    __tablename__ = "spiderbots"
    spiderbotid = db.Column(db.Integer, primary_key=True)
    # Item or key term to search
    searchterm = db.Column(db.String)
    # Full url for search (instead of searchterm)
    fullink = db.Column(db.String)
    # Name of the spider
    spider = db.Column(db.String)
    # Should it run?
    active = db.Column(db.Boolean, default=True)
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
    title = db.Column(db.String)
    seller = db.Column(db.String)
    # Picture of the item
    image = db.Column(db.String)
    url = db.Column(db.String)
    description = db.Column(db.String)
    # Actual price
    price = db.Column(db.Float)
    currency = db.Column(db.String)
    isauction = db.Column(db.Boolean, default=False)
    # Geography location of the item
    location = db.Column(db.String)
    # If the matched item expire
    expire = db.Column(db.String)
    # When updated this record last time?
    updated = db.Column(db.DateTime)
    # Was already show to user?
    shown = db.Column(db.Boolean, default=False)
    # Hide this match from the user
    hide = db.Column(db.Boolean, default=False)
    # When did user hided this match?
    hidedat = db.Column(db.String)
    # Hash code of the match (filled via pipeline)
    hash = db.Column(db.String)
    # Extra ID field to identify the match
    extraid = db.Column(db.String)
    # foreign key to spiderbot
    spiderbotid = db.Column(db.Integer, db.ForeignKey("spiderbots.spiderbotid"))
    spiderbot = db.relationship("Spiderbot", backref=db.backref(
        "spiderbots"), lazy=True)

    def __repr__(self):
        return '<Match %r>' % self.title
