cls
SET BACKEND_PATH=..\Code\Services
SET REPOSITORY_PATH=repository
SET SOLUTION_PATH=..

del %REPOSITORY_PATH%/*.tar

docker build -f %BACKEND_PATH%/DemoService/Dockerfile --force-rm -t demoservice %SOLUTION_PATH%

docker image save demoservice:latest -o %REPOSITORY_PATH%/demoservice.tar
