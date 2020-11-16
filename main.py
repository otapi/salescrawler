from flask import flash, render_template, request, redirect
from formencode import variabledecode
from app import app
from app import db

import forms
import models

import sclogic

@app.route('/', methods=['GET', 'POST'])
def index():
    if "run" in request.args:
        flash('run...')
        sclogic.runall()
        flash('run finished!')
    elif "update" in request.args:
        flash('update!')

    if request.method == 'POST':    
        postvars = variabledecode.variable_decode(request.form, dict_char='_')
        for k, v in postvars.items():
            match = models.Match.query.filter_by(matchid=int(k)).first()
            if "hide" in v and v["hide"] == "on":
                match.hide = True
            else:
                match.hide = False
        db.session.commit()

    matches = models.Match.query.filter_by(hide=False)
    return render_template('matches.html', matches=matches) 

@app.route('/searchform', methods=['GET', 'POST'])
def searchfrom():
    spider = forms.SpidersForm(request.form)
    if request.method == 'POST':
        return spider_results(spider)
    return render_template('search.html', form=spider)

if __name__ == '__main__':
    import argparse
    parser = argparse.ArgumentParser()
    parser.add_argument('--debug', '-d', action='store_true')
    parser.add_argument('--port', '-p', default=5000, type=int)
    parser.add_argument('--host', default='0.0.0.0')

    args = parser.parse_args()
    app.run(args.host, args.port, debug=args.debug)
