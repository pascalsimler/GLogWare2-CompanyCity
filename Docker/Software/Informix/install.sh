docker run -d \
  --name informix \
  -e LICENSE=accept \
  -p 9088:9088 \
  -p 27883:27883 \
  -v data:/opt/ibm/data \
icr.io/informix/informix-developer-database