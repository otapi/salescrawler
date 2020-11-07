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
pip3 install scrapy