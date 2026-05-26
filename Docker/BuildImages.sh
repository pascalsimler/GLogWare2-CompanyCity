set -e

esc=$'\033'
b="${esc}[30;107m"
r="${esc}[0m"

pathBackend="../Code/Services"
pathFrontend="../Code/Frontend"
pathRepository="repository"
pathSolution=".."

images=(
  "demoservice:$pathBackend/DemoService"
  "jobmanager:$pathBackend/JobManager"
  "reserve:$pathBackend/Reserve"
  "garbage:$pathBackend/Garbage"
  "bridgemanager:$pathBackend/BridgeManager"
  "bridgesimulator:$pathBackend/BridgeSimulator"
  "simulatorwebapp:$pathFrontend/SimulatorWebApp"
  "glogwarewebapp:$pathFrontend/GLogWareWebApp/GLogWareWebApp"
)

clear
rm -f "$pathRepository"/*.tar 2>/dev/null || true

choice="Z"

for item in "${images[@]}"; do

  name="${item%%:*}"
  path="${item#*:}"

  if [[ "$choice" != "A" && "$choice" != "I" ]]; then
    while true; do
      echo -e "Do you want to build the ${b}[${name}]${r} image? (${b}[Y]${r}=Yes, ${b}[N]${r}=No, ${b}[A]${r}=Yes to all, ${b}[I]${r}=No to all)"
      read -r choice
      choice=${choice^^}

      if [[ "$choice" == "Y" || "$choice" == "N" || "$choice" == "A" || "$choice" == "I" ]]; then
        break
      fi
    done
  fi

  echo -e "${b}[${name}]${r} image ..."

  if [[ "$choice" == "Y" || "$choice" == "A" ]]; then
    docker build -f "$path/Dockerfile" --force-rm -t "$name" "$pathSolution"
    docker image save "$name:latest" -o "$pathRepository/$name.tar"
  fi

done