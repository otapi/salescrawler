rem setup local Windows 10 machine for development (but not run the code)
rem #1 - install C++ build tools: https://visualstudio.microsoft.com/visual-cpp-build-tools/
python -m pip install scrapy pillow mysqlclient Click flask flask-sqlalchemy Flask-WTF formencode

python -m pip uninstall jupiter-client
python -m pip uninstall pyzmq
python -m pip install pyzmq
python -m pip install jupiter-client

python -m pip install jedi
python -m pip install scrapy_gui
echo enter into python shell:
echo import scrapy_gui
echo scrapy_gui.open_browser()
python