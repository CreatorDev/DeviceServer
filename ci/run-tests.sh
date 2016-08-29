DEVICE_SERVER_IP=$1
LWM2M_SERVER_IP=$2
CONTENT_TYPE=$3
OUTPUT_FILENAME=$4

ORGANISATION_0_KEY=$5
ORGANISATION_0_SECRET=$6

ORGANISATION_1_KEY=$7
ORGANISATION_1_SECRET=$8
    
# Run tests and save results. NB: `dotnet test -xml results.xml` does not yet work on linux.

COMMAND="env DeviceServerTests:RestAPI:URI=http://$DEVICE_SERVER_IP:8080 \
             DeviceServerTests:RestAPI:ContentType=$CONTENT_TYPE \
             DeviceServerTests:RestAPI:Authentication:MasterKey=$ORGANISATION_0_KEY \
             DeviceServerTests:RestAPI:Authentication:MasterSecret=$ORGANISATION_0_SECRET \
             DeviceServerTests:RestAPI:Authentication:Key=$ORGANISATION_1_KEY \
             DeviceServerTests:RestAPI:Authentication:Secret=$ORGANISATION_1_SECRET \
             DeviceServerTests:LWM2MClient:URI=coaps://$LWM2M_SERVER_IP:5684 \
         mono bin/Debug/net451/debian.8-x64/dotnet-test-xunit.exe \
              bin/Debug/net451/debian.8-x64/DeviceServerTests.dll \
              -xml $OUTPUT_FILENAME \
              -parallel none"              

#create a temporary container to run the tests within
docker create -it \
	   --workdir /app/test/DeviceServerTests/ \
	   --name deviceserver-test-container \
       --net imaginationdeviceserver_DeviceServerNet \
	   deviceserver-test \
	   $COMMAND

docker start -a deviceserver-test-container || true

# copy test results out of container
docker cp deviceserver-test-container:/app/test/DeviceServerTests/$OUTPUT_FILENAME $OUTPUT_FILENAME || true

docker rm -f deviceserver-test-container
