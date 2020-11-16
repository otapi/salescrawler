# python3 salescrawler/sc-cli.py update
# cd ~/ ; python3 salescrawler/sc-cli.py update ; cd salescrawler
# scrapy crawl hardverapro -a searchterm=RX470 -a spiderbotid=7

# cd ~/ ; python3 salescrawler/sc-cli.py update ; cd ~/salescrawler ; FLASK_APP=main.py flask run



# cd ~/ ; rm -rf salescrawler ; git clone git@github.com:otapi/salescrawler.git ; cd salescrawler
cd ~/

echo Stop the server

echo Delete old folder
rm -rf salescrawler
echo Clone the Salescrawler
git clone git@github.com:otapi/salescrawler.git

echo run 
cd salescrawler
scrapy crawl hardverapro
scrapy crawl hardverapro -a searchterm=RX470
scrapy crawl hardverapro -a fullink=https://hardverapro.hu/aprok/keres.php?stext=RX470&county=&stcid=&settlement=&stmid=&minprice=1000&maxprice=30000&company=&cmpid=&user=&usrid=&selling=1&buying=1&stext_none=
read -p "Press any key to resume ..."
run.sh

