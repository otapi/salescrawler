cd ~/

echo Stop the server

echo Delete old folder
rm -rf salescrawler
echo Clone the Salescrawler
git clone git@github.com:otapi/salescrawler.git

echo run 

read -p "Press any key to resume ..."