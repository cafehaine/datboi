[ ! -d "screenshots" ] && mkdir screenshots
cd screenshots
while true
do
curl 127.0.0.1/screen.png > $(date +%Y-%m-%d-%H:%M:%S).png
sleep 300
done

