from flask import flash, render_template, request, redirect
from app import app
from app import db

import forms
import models
import tables

@app.route('/', methods=['GET', 'POST'])
def index():
    print("itt")

    for k in request.args.keys():
        print(k)
    if "run" in request.form:
        print('run!')
        pass
    elif "update" in request.form:
        print('update!')
        pass

    qry = db.session.query(models.Match)
    results = qry.all()
    table = tables.Results(results)
    table.border = True

    return render_template('index.html', table=table)

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
    app.run()