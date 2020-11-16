rem setup local Windows 10 machine for development (but not run the code)
rem #1 - install C++ build tools: https://visualstudio.microsoft.com/visual-cpp-build-tools/
python -m pip install scrapy pillow mysqlclient Click flask flask-sqlalchemy Flask-WTF flask_table
python -m pip install scrapy_gui
echo enter into python shell:
echo import scrapy_gui
echo scrapy_gui.open_browser()
python