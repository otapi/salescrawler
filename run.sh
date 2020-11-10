# cd ~/ ; rm -rf salescrawler ; git clone git@github.com:otapi/salescrawler.git ; cd salescrawler ; scrapy crawl blogspider
cd ~/

echo Stop the server

echo Delete old folder
rm -rf salescrawler
echo Clone the Salescrawler
git clone git@github.com:otapi/salescrawler.git

echo run 
cd salescrawler
scrapy crawl blogspider
read -p "Press any key to resume ..."
run.sh
