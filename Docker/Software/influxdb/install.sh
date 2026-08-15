docker run -d --name=influxdb \
  -p 8086:8086 \
  -v data:/var/lib/influxdb \
  -e INFLUXDB_ADMIN_USER=admin \
  -e INFLUXDB_ADMIN_PASSWORD=*Gudel1954* \
  influxdb
