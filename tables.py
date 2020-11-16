from flask_table import Table, Col

class Results(Table):
    matchid = Col('matchid', show=False)
    #image = Col('Image')
    title = Col('Title')
    seller = Col('Seller')
    url = Col('Url')
    description = Col('Description')
    price = Col('Price')
    location = Col('Location')
    hide = Col('Hidden')
    spiderbotid = Col('spiderbotid', show=False)