from app import db

# Crawlers are the set up of batch of spiders for searching an item (key term)
class Crawler(db.Model):
    __tablename__ = "crawlers"
    db.Column()
    crawlerId = db.Column(db.Integer, primary_key=True)
    # Should it run?
    active = db.Column(db.Boolean)
    name = db.Column(db.String)
    # hours - How frequent should it run?
    runCadence: Int
    # When run last time?
    lastRun = db.Column(db.String)

    def __repr__(self):
        return '<Crawler %r>' % self.name

# Spiders with specified search term and owner of crawler
class Spiderbot(db.Model):
    __tablename__ = "spiderbots"
    spiderbotId = db.Column(db.Integer, primary_key=True)
    # Item or key term to search
    searchTerm = db.Column(db.String)
    # Full url for search (instead of searchTerm)
    fullink = db.Column(db.String)
    # Name of the spider
    spider = db.Column(db.String)
    # Should it run?
    active = db.Column(db.Boolean)
    # foreign key to crawler
    crawlerId = db.Column(db.Integer, db.ForeignKey("crawlers.crawlerId"))
    crawler = db.relationship("Crawler", backref=db.backref(
        "spiderbots"), lazy=True)

    def __repr__(self):
        return '<Spiderbot %r>' % self.spiderbotId

# This is where the results go from spiders
class Match(db.Model):
    __tablename__ = "matches"
    matchId = db.Column(db.Integer, primary_key=True)
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
    isAuction = db.Column(db.Boolean)
    # Geography location of the item
    location = db.Column(db.String)
    # If the matched item expire
    expire = db.Column(db.String)
    # When updated this record last time?
    updated = db.Column(db.String)
    # Was already show to user?
    shown = db.Column(db.Boolean)
    # Hide this match from the user
    hide = db.Column(db.Boolean)
    # When did user hided this match?
    hidedAt = db.Column(db.String)
    # Hash code of the match (filled via pipeline)
    hash = db.Column(db.String)
    # Extra ID field to identify the match
    extraId = db.Column(db.String)
    # foreign key to spiderbot
    spiderbotId = db.Column(db.Integer, db.ForeignKey("spiders.spiderbotId"))
    spiderbot = db.relationship("Spiderbot", backref=db.backref(
        "spiderbots"), lazy=True)

    def __repr__(self):
        return '<Match %r>' % self.title
