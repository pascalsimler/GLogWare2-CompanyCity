docker run -d \
  --name mysql \
  -v /home/glogware/docker/mysql/data:/var/lib/mysql \
  -e MYSQL_ROOT_PASSWORD=*Gudel1954* \
  -e TZ=Europe/Zurich \
  -p 3306:3306 \
mysql:latest \
  --lower_case_table_names=1 \
