from app import app
from flask import flash, render_template, request, redirect

from forms import SpidersForm
import models

@app.route('/', methods=['GET', 'POST'])
def index():
    spider = SpidersForm(request.form)
    if request.method == 'POST':
        return spider_results(spider)
    return render_template('index.html', form=spider)
    

@app.route('/results')
def results():
    qry = db_session.query(Match)
    results = qry.all()
        
    if not results:
        flash('No results found!')
        return redirect('/')
    else:
        # display results
        table = Results(results)
        table.border = True
        return render_template('results.html', table=table)

if __name__ == '__main__':
    app.run()