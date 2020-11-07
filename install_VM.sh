# Run this script on a fresh install of VM (Ubuntu) or run the commands manually
# Disable auto-lock: https://techdirectarchive.com/2020/05/05/how-to-disable-automatic-screen-lock-in-ubuntu-linux/
sudo dpkg --configure -a
sudo apt-get update
sudo apt-get dist-upgrade
sudo apt-get install git

# Generate the SSH key 
ssh-keygen -t rsa -b 4096
echo Copy the content of your public SSH key, it is the file /home/otapi/.ssh/id_rsa.pub by default
cat /home/otapi/.ssh/id_rsa.pub
echo Paste the content into your GitHub/BitBucket account on the SSH key section
read -p "Press any key to resume ..."
echo First clone
git clone git@github.com:otapi/salescrawler.git
sudo apt install python3-pip
sudo apt-get install python3 python3-venv python3-dev python3-pip libxml2-dev libxslt1-dev zlib1g-dev libffi-dev libssl-dev

echo Create virtual env
python3 -m venv env
echo Activate your env: 
source env/bin/activate
echo istall Scrapy with pip
pip3 install scrapy