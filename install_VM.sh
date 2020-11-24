# Run this script on a fresh install of VM (Ubuntu) or run the commands manually
# Disable auto-lock: https://techdirectarchive.com/2020/05/05/how-to-disable-automatic-screen-lock-in-ubuntu-linux/
sudo dpkg --configure -a
sudo apt-get update
sudo apt-get dist-upgrade
sudo apt-get install git

# Generate the SSH key 
ssh-keygen -t rsa -b 4096
echo Copy the content of your public SSH key, it is the file ~/.ssh/id_rsa.pub by default
cat ~/.ssh/id_rsa.pub
echo Paste the content into your GitHub/BitBucket account on the SSH key section
read -p "Press any key to resume ..."
echo First clone
git clone git@github.com:otapi/salescrawler.git


echo install MySQL
sudo apt-get install mysql-server
echo secure the MySQL
sudo mysql_secure_installation
echo 
echo Enter sql user details
echo CREATE USER 'salescrawler'@'localhost' IDENTIFIED BY 'password';
echo Then grant access
echo GRANT CREATE, ALTER, DROP, INSERT, UPDATE, DELETE, SELECT, REFERENCES, RELOAD on *.* TO 'salescrawler'@'localhost' WITH GRANT OPTION;
echo then flush
echo FLUSH PRIVILEGES;
echo then exit
mysql -u root -p
echo Check status
systemctl status mysql.service

sudo apt-get install python3 python3-pip python3-venv python3-dev python3-pip libxml2-dev libxslt1-dev zlib1g-dev libffi-dev libssl-dev default-libmysqlclient-dev

#echo Create virtual env
#python3 -m venv env
#echo Activate your env: 
#source env/bin/activate
# pillow?

echo install python packages
pip3 install scrapy mysqlclient Click flask flask-sqlalchemy formencode

echo Install MySQL Workbench
sudo snap install mysql-workbench-community

echo Copy and Run SQL query generated from 'SQLdb.vuerd.json' to create database scheme
echo start with 'USE salescrawler;'
mysql -u salescrawler -p

echo VPN setup
echo see https://protonvpn.com/support/linux-vpn-tool/
sudo apt install -y openvpn dialog python3-pip python3-setuptools
sudo pip3 install protonvpn-cli
sudo protonvpn init
echo Add as new line
echo otapi   ALL=(ALL) NOPASSWD:/usr/local/bin/protonvpn c -f
sudo visudo

cp ~/salescrawler/run.sh ~/Desktop/run.sh
chmod +x ~/Desktop/run.sh
echo add this line to crontab:
echo @reboot ~/Desktop/run.sh
crontab -e


cp ~/salescrawler/run.sh.desktop ~/.config/autostart/run.sh.desktop

echo goto to Startup Applications and add home/run.sh
read -r input
echo add Terminal=true
nano ~/.config/autostart/run.sh.desktop


echo autologin in ubuntu
echo goto to settings / users and enable autologin
read -r input

echo autologin in ubuntu
echo goto to settings / users and enable autologin



