docker run \
   -e "ACCEPT_EULA=Y" \
   -e "MSSQL_SA_PASSWORD=*Gudel1954*" \
   -e "TZ=Europe/Zurich" \
   -p 1433:1433 \
   --name mssql \
   --hostname mssql \
   -v /home/glogware/docker/mssql/data:/var/opt/mssql \
   -v /etc/localtime:/etc/localtime:ro \
   -v /etc/timezone:/etc/timezone:ro \
   -d \
mcr.microsoft.com/mssql/server:2022-latest
