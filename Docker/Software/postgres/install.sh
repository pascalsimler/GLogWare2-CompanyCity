docker run --name postgres \
  -e POSTGRES_USER=admin \
  -e POSTGRES_PASSWORD=*Gudel1954* \
  -v /home/glogware/docker/postgres/data:/var/lib/postgresql/18 \
  -p 5432:5432 \
  -d postgres:18.1
