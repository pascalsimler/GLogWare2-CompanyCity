docker run --name mosquitto -d -p 1883:1883 -p 9001:9001 -v /home/glogware/docker/mosquitto/mosquitto.conf:/mosquitto/config/mosquitto.conf eclipse-mosquitto
