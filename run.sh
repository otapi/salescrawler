# python3 salescrawler/sc-cli.py update
# cd ~/ ; python3 salescrawler/sc-cli.py update ; cd salescrawler
# scrapy crawl hardverapro -a searchterm=RX470 -a spiderbotid=7

# cd ~/ ; python3 salescrawler/sc-cli.py update ; cd ~/salescrawler ; FLASK_APP=main.py flask run



# cd ~/ ; rm -rf salescrawler ; git clone git@github.com:otapi/salescrawler.git ; cd salescrawler
cd ~/
python3 salescrawler/sc-cli.py update
cd ~/salescrawler
xdg-open http://127.0.0.1:5000/ &
FLASK_APP=main.py flask run

