clear
REPOSITORY_PATH=repository

docker image rm demoservice:latest
docker image rm bridgesimulator:latest
docker image rm bridgemanager:latest

docker image load -i ${REPOSITORY_PATH}/demoservice.tar
docker image load -i ${REPOSITORY_PATH}/bridgesimulator.tar
docker image load -i ${REPOSITORY_PATH}/bridgemanager.tar