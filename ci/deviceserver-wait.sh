# waits for device server by polling the REST API entry point
printf "Waiting for Device Server"

DEVICE_SERVER_URI=$1

MAX_ATTEMPTS=30
COUNTER=0
while [  $COUNTER -lt $MAX_ATTEMPTS ]; do
  printf "."
  set +e
  curl -s $DEVICE_SERVER_URI > index.html
  rc=$?
  set -e
  if [ $rc -eq 0 ]; 
  then 
  	break
  fi
  COUNTER=`expr $COUNTER + 1`
  sleep 1
done

if [ $COUNTER -eq $MAX_ATTEMPTS ]; 
then
  echo "Device Server was not started successfully"
  exit 1
fi

echo "Device Server up"
