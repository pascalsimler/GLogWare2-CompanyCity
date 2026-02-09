clear
BACKEND_PATH=../Code/Services
REPOSITORY_PATH=repository
SOLUTION_PATH=..

rm ${REPOSITORY_PATH}/*.tar

docker build -f ${BACKEND_PATH}/DemoService/Dockerfile --force-rm -t demoservice ${SOLUTION_PATH}

docker image save demoservice:latest -o ${REPOSITORY_PATH}/demoservice.tar
