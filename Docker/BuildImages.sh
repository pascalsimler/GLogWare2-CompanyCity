clear
BACKEND_PATH=../Code/Services
FRONTEND_PATH=../Code/Frontend
REPOSITORY_PATH=repository
SOLUTION_PATH=..

rm ${REPOSITORY_PATH}/*.tar

docker build -f ${BACKEND_PATH}/DemoService/Dockerfile --force-rm -t demoservice ${SOLUTION_PATH}
docker build -f ${BACKEND_PATH}/BridgeManager/Dockerfile --force-rm -t bridgemanager ${SOLUTION_PATH}
docker build -f ${BACKEND_PATH}/BridgeSimulator/Dockerfile --force-rm -t bridgesimulator ${SOLUTION_PATH}

docker build -f ${FRONTEND_PATH}/SimulatorWebApp/Dockerfile --force-rm -t simulatorwebapp ${SOLUTION_PATH}
docker build -f ${FRONTEND_PATH}/GLogWareWebApp/GLogWareWebApp/Dockerfile --force-rm -t glogwarewebapp ${SOLUTION_PATH}

docker image save demoservice:latest -o ${REPOSITORY_PATH}/demoservice.tar
docker image save bridgemanager:latest -o ${REPOSITORY_PATH}/bridgemanager.tar
docker image save bridgesimulator:latest -o ${REPOSITORY_PATH}/bridgesimulator.tar
docker image save simulatorwebapp:latest -o ${REPOSITORY_PATH}/simulatorwebapp.tar
docker image save glogwarewebapp:latest -o ${REPOSITORY_PATH}/glogwarewebapp.tar