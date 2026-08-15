docker run -d \
	--name keycloak \
	-p 8443:8443 \
	-v keycloak_data:/opt/keycloak/data \
	-e "KC_BOOTSTRAP_ADMIN_USERNAME=admin" \
	-e "KC_BOOTSTRAP_ADMIN_PASSWORD=admin" \
	-e "KC_DB=mssql" \
	-e "KC_DB_URL=jdbc:sqlserver://mssql.glogware.com:1433;databaseName=keycloak;loginTimeout=30;encrypt=true;trustServerCertificate=true" \
	-e "KC_DB_USERNAME=sa" \
	-e "KC_DB_PASSWORD=*Gudel1954*" \
	-e "KC_HOSTNAME=keycloak.glogware.com" \
	-e "KC_HTTPS_KEY_STORE_FILE=/opt/keycloak/conf/keycloak.p12" \
	-e "KC_HTTPS_KEY_STORE_PASSWORD=*Gudel1954*" \
  	-v /home/psimler/dev/docker/keycloak/keycloak.p12:/opt/keycloak/conf/keycloak.p12 \
	quay.io/keycloak/keycloak:latest start --hostname keycloak.glogware.com
