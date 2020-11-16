from flask import flash, render_template, request, redirect
from app import app

import forms
import models
import tables


@app.route('/', methods=['GET', 'POST'])
def index():
    if "run" in request.args:
        flash('run!')

    elif "update" in request.args:
        flash('update!')

    qry = db.session.query(models.Match)
    results = qry.all()
    table = tables.Results(results)
    table.border = True

    return render_template('index.html', table=table)

@app.route('/matches', methods=['GET', 'POST'])
def matches():
  matches = db.get('matches')

  if request.method == 'POST':    
    postvars = variabledecode.variable_decode(request.form, dict_char='_')
    for k, v in postvars.iteritems():
        member = [m for m in matches if m["matchid"] == int(k)][0]
        member['hide'] = v["hide"]
    db.set('matches', matches)
    db.dump()
  return render_template('matches.html', matches=matches) 

@app.route('/searchform', methods=['GET', 'POST'])
def searchfrom():
    spider = forms.SpidersForm(request.form)
    if request.method == 'POST':
        return spider_results(spider)
    return render_template('search.html', form=spider)
    

@app.route('/results')
def results():
    qry = db.session.query(models.Match)
    results = qry.all()
        
    if not results:
        flash('No results found!')
        return redirect('/')
    else:
        # display results
        table = tables.Results(results)
        table.border = True
        return render_template('results.html', table=table)

if __name__ == '__main__':
    import argparse
    parser = argparse.ArgumentParser()
    parser.add_argument('--debug', '-d', action='store_true')
    parser.add_argument('--port', '-p', default=5000, type=int)
    parser.add_argument('--host', default='0.0.0.0')

    args = parser.parse_args()
    app.run(args.host, args.port, debug=args.debug)