from flask import flash, render_template, request, redirect
from app import app
from app import db

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

import wtforms

from wtforms.widgets import HTMLString, html_params
class XEditableWidget(object):
    def __call__(self, field, **kwargs):
        # get Field from FieldList and create x-editable link based on it 
        subfield = field.pop_entry()
        value = kwargs.pop("value", "")
        
        kwargs.setdefault('data-role', 'x-editable')
        kwargs.setdefault('data-url', '/')
        
        kwargs.setdefault('id', field.matchid)
        kwargs.setdefault('name', field.name)
        kwargs.setdefault('href', '#')
        
        if not kwargs.get('pk'):
            raise Exception('pk required')
        kwargs['data-pk'] = kwargs.pop("pk")
        
        if isinstance(subfield, wtforms.StringField):
            kwargs['data-type'] = 'text'
        elif isinstance(subfield, wtforms.BooleanField):
            kwargs['data-type'] = 'select'
        elif isinstance(subfield, wtforms.RadioField):
            kwargs['data-type'] = 'select'
        elif isinstance(subfield, wtforms.SelectField):
            kwargs['data-type'] = 'select'
        elif isinstance(subfield, wtforms.DateField):
            kwargs['data-type'] = 'date'
        elif isinstance(subfield, wtforms.DateTimeField):
            kwargs['data-type'] = 'datetime'
        elif isinstance(subfield, wtforms.IntegerField):
            kwargs['data-type'] = 'number'
        elif isinstance(subfield, wtforms.TextAreaField):
            kwargs['data-type'] = 'textarea'
        else:
            raise Exception('Unsupported field type: %s' % (type(subfield),))
            
        return HTMLString('<a %s>%s</a>' % (html_params(**kwargs), value))


class XEditableForm(wtforms.Form):
    # min_entries=1 is required, because XEditableWidget needs at least 1 entry
    test1 = wtforms.FieldList(wtforms.StringField(), widget=XEditableWidget(), min_entries=1)
    test2 = wtforms.FieldList(wtforms.StringField(), widget=XEditableWidget(), min_entries=1)


@app.route('/matches2', methods=['POST', 'GET'])
def index():
    form = XEditableForm(request.form)
    if (request.method == "POST") and form.validate():
        for x in form:
            # last_index will be set if a field is submitted
            if getattr(x, 'last_index', None):
                model = models.Match.query.get(x.last_index)
                setattr(model, x.name, x.data.pop())
                db.session.commit()
    elif (request.method == "POST") and not form.validate():
        print("Errors", form.errors)
        
    for x in models.Match.query.all():
        print(x.test1, x.test2)
    return render_template('example.html', form=form)



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