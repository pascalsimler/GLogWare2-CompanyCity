clear
REPOSITORY_PATH=repository

docker image rm demoservice:latest

docker image load -i ${REPOSITORY_PATH}/demoservice.tar