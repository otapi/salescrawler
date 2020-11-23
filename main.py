from flask import flash, render_template, request, redirect, url_for
from formencode import variabledecode
from app import app
from app import db

import models
import forms
import sclogic

@app.route('/', methods=['GET', 'POST'])
def index():
    return index_engine()

@app.route('/filtered/<spiderbotids>', methods=['GET', 'POST'])
def index_filtered(spiderbotids):
    return index_engine(hidematches = True, spiderbotids=spiderbotids)

@app.route('/all', methods=['GET', 'POST'])
def index_all():
    return index_engine(hidematches = False)

@app.route('/saved', methods=['GET', 'POST'])
def index_saved():
    return index_engine(hidematches = False, onlysaved = False)

def index_engine(hidematches = True, onlysaved = False, spiderbotids = None):
    if "run" in request.args:
        flash('run...')
        sclogic.runall()
        flash('run finished!')
    elif "clear" in request.args:
        flash('update...')
        sclogic.clear()
        flash('updated!')
    elif "connectvpn" in request.args:
        flash('connect to VPN...')
        sclogic.connectvpn()
        flash('VPN completed')
    elif "showall" in request.args:
        return redirect(url_for('index_all'))
    elif "showsaved" in request.args:
        return redirect(url_for('index_saved'))
    elif "refresh" in request.args:
        return redirect(url_for('index'))

    crawlers = models.Crawler.query.order_by(models.Crawler.name).all()
    
    if spiderbotids:
        matches = None
        for spiderbotid in spiderbotids:
            rm = models.Match.query.filter_by(spiderbotid=spiderbotid).all()
            if matches:
                matches = matches + rm
            else:
                matches = rm
        flash('Matches are filtered for one Crawler...')

    else:
        if hidematches:
            if onlysaved:
                matches = models.Match.query.filter_by(hide=False, saved=True).order_by(models.Match.price).all()
            else:
                matches = models.Match.query.filter_by(hide=False).order_by(models.Match.price).all()
        else:
            if onlysaved:
                matches = models.Match.query.filter_by(saved=True).order_by(models.Match.price).all()
            else:
                matches = models.Match.query.order_by(models.Match.price).all()
        
    return render_template('main.html', matches=matches, crawlers=crawlers) 

@app.route('/match-update', methods=['GET', 'POST'])
def match_update():
    if request.method == 'POST':    
        postvars = variabledecode.variable_decode(request.form, dict_char='_')
        for k, v in postvars.items():
            match = models.Match.query.filter_by(matchid=int(k)).first()
            match.hide = True if ("hide" in v and v["hide"] == "on") else False
            match.saved = True if ("saved" in v and v["saved"] == "on") else False
        db.session.commit()
    return redirect('/')

@app.route('/crawler-update', methods=['GET', 'POST'])
def crawler_update():
    if request.method == 'POST':    
        spiderbotids = []
        postvars = variabledecode.variable_decode(request.form, dict_char='_')
        for ids, values in postvars.items():
            selected = True if "selected" in values and values["selected"] == "on" else False
            id = int(ids) if ids.isnumeric() else None
            if id:  
                if selected:          
                    if 'delete_button' in request.form:
                        flash('Delete crawler...')
                        sclogic.crawlerDelete(id)
                        flash('Delete crawler finished!')
                    elif 'filter_button' in request.form:
                        for spiderbot in models.Spiderbot.query.filter_by(crawlerid=id).all():
                            spiderbotids.append(spiderbot.spiderbotid)
                elif 'save_button' in request.form:
                    print("itt: "+values["maxprice"])
                    crawler = models.Crawler.query.filter_by(crawlerid=id).first()
                    crawler.active = True if ("active" in values and values["active"] == "on") else False
                    crawler.name = values["name"]
                    crawler.runcadence = float(values["runcadence"]) 
                    crawler.maxprice = float(values["maxprice"]) if values["maxprice"] and values["maxprice"] != '' and float(values["maxprice"]) !=0 else None
                    crawler.minprice = float(values["minprice"]) if values["minprice"] and values["minprice"] != '' and float(values["minprice"]) !=0 else None
                    db.session.commit()
        if len(spiderbotids)>0:
            return redirect(url_for('index_filtered', spiderbotids=spiderbotids))

    return redirect('/')

@app.route('/new_crawler', methods=['GET', 'POST'])
def new_crawler():
    form = forms.CrawlerForm(request.form)
    if request.method == 'POST' and form.validate():
        if not ('from_main' in request.form):
            sclogic.crawlerAdd(form.name.data)
            flash('Crawler created successfully!')
            return redirect('/')

    return render_template('new_crawler.html', form=form)

@app.route('/spiderbots/<crawlerid>', methods=['GET', 'POST'])
def spiderbots(crawlerid):
    spiderbots = models.Spiderbot.query.filter_by(crawlerid=crawlerid).order_by(models.Spiderbot.spiderbotid).all()
    crawler = models.Crawler.query.filter_by(crawlerid=crawlerid).first()
    return render_template('spiderbots.html', spiderbots=spiderbots, crawler=crawler)

def isfloat(value):
    try:
        float(value)
        return True
    except ValueError:
        return False


@app.route('/new-spiderbots/<crawlerid>', methods=['GET', 'POST'])
def new_spiderbots(crawlerid):
    crawler = models.Crawler.query.filter_by(crawlerid=crawlerid).first()
    
    form = forms.SpiderbotForm(request.form)
    if request.method == 'POST' and form.validate():
        if not ('from_spiderbots' in request.form):
            flash('Create spiders...')
            for spider in form.spiders.data:
                minprice = float(form.minprice.data) if isfloat(form.minprice.data) and float(form.minprice.data) !=0 else None
                maxprice = float(form.maxprice.data) if isfloat(form.maxprice.data) and float(form.maxprice.data) !=0 else None
                sclogic.spiderbotAdd(spider, crawlerid, form.searchterm.data, form.fullink.data, minprice, maxprice)
            flash('Spiders created successfully!')
            return redirect(url_for('spiderbots', crawlerid=crawlerid))

    if crawler.maxprice:
        form.maxprice.data = str(crawler.maxprice)
    if crawler.minprice:
        form.minprice.data = str(crawler.minprice)
    form.spiders.data = sclogic.getSpiders().keys()
    return render_template('new_spiderbot.html', form=form, crawler = crawler)


@app.route('/spiderbot-update/<crawlerid>', methods=['GET', 'POST'])
def spiderbot_update(crawlerid):
    if request.method == 'POST':    
        postvars = variabledecode.variable_decode(request.form, dict_char='_')
        for ids, values in postvars.items():
            selected = True if "selected" in values and values["selected"] == "on" else False
            id = int(ids) if ids.isnumeric() else None
            if id:            
                if 'delete_button' in request.form:
                    if selected:
                        flash('Delete spiderbot...')
                        sclogic.spiderbotDelete(id)
                        flash('Delete spiderbot finished!')
                elif 'save_button' in request.form:
                    spiderbot = models.Spiderbot.query.filter_by(spiderbotid=id).first()
                    spiderbot.active = True if ("active" in values and values["active"] == "on") else False
                    spiderbot.searchterm = None if values["searchterm"] == "" or values["searchterm"] == "None" else values["searchterm"]
                    spiderbot.fullink = None if values["fullink"] == "" or values["fullink"] == "None" else values["fullink"]
                    spiderbot.minprice = float(values["minprice"]) if isfloat(values["minprice"]) and float(values["minprice"]) !=0 else None
                    spiderbot.maxprice = float(values["maxprice"]) if isfloat(values["maxprice"]) and float(values["maxprice"]) !=0 else None
        db.session.commit()
    return redirect(url_for('spiderbots', crawlerid=crawlerid))

if __name__ == '__main__':
    import argparse
    parser = argparse.ArgumentParser()
    parser.add_argument('--debug', '-d', action='store_true')
    parser.add_argument('--port', '-p', default=5000, type=int)
    parser.add_argument('--host', default='0.0.0.0')

    args = parser.parse_args()
    app.run(args.host, args.port, debug=args.debug)
