![](doc/images/img.png)
----

# docker-deviceserver

## Running the device server
To run the device-server stack you must have installed docker-engine version 1.10 or higher 
and docker-compose version 1.6 or higher.

For each application, see the respective installation documentation:

* [install docker-engine][de]
* [install docker-compose][dc]

Alternatively you may choose to create a new Virtual machine following the instructions
below...

### Create a new Virtual Machine running Docker

#### Install Virtualbox and Vagrant on your Host

* [Download and install Virtual box] [vb]
* [Download and install Vagrant] [vg]

#### Create an Ubuntu VM using Vagrant
```
C:\>mkdir vm
C:\>cd vm
C:\vm>vagrant init bento/ubuntu-16.04
A `Vagrantfile` has been placed in this directory. You are now
ready to `vagrant up` your first virtual environment! Please read
the comments in the Vagrantfile as well as documentation on
`vagrantup.com` for more information on using Vagrant.
```

```
C:\vm>notepad Vagrantfile
```

Uncomment the line...  

```
  config.vm.network "public_network"
```

```
C:\vm>vagrant up
Bringing machine 'default' up with 'virtualbox' provider...
==> default: Importing base box 'bento/ubuntu-16.04'...

...
...

==> default: Waiting for machine to boot. This may take a few minutes...
    default: SSH address: 127.0.0.1:2200
    default: SSH username: vagrant
    default: SSH auth method: private key

C:\vm>vagrant ssh (or use putty to ssh to the "SSH address" listed)
```

#### Set-up docker and docker-compose

```
$ sudo -s
# apt-get update
# apt-get install git curl apt-transport-https ca-certificates
# apt-key adv --keyserver hkp://p80.pool.sks-keyservers.net:80 --recv-keys 58118E89F3A912897C070ADBF76221572C52609D
# echo "deb https://apt.dockerproject.org/repo ubuntu-xenial main" > /etc/apt/sources.list.d/docker.list
# apt-get update
# apt-get install -y docker-engine
# service docker start
# usermod -aG docker vagrant
# curl -L https://github.com/docker/compose/releases/download/1.7.1/docker-compose-`uname -s`-`uname -m` > /usr/local/bin/docker-compose
# chmod a+x /usr/local/bin/docker-compose
# exit
```

logout of the VM and then log back in.


#### Download the device-server source code
```
$ git clone git@gitlab.flowcloud.systems:FlowM2M/DeviceServer.git
(enter your ldap username/password)
$ cd DeviceServer
```

### Build and run device-server stack 

To run the stack, make sure you are up to date with git master and run:

```sh
git pull origin master
docker-compose up -d
```

### Remove device-server stack

```sh
docker-compose down
```

## Accessible Services
The stack runs in it's own virtual network, the following services are
accessible externally.

* CoAP - ports 5683, 5684, 15683, 15684
* Consul - port 8500
* Device Server REST API - routed via fabio to port 80
* Fabio - port 80, 81

### CoAP
This is the protocol designed for machine-to-machine (M2M) applications so that
your device can talk with the server directly.

* CoAP is mapped to the host directly and can be found via ports 5683, 5684, 
  15683 and 15684

### Consul
Consul manages the DNS for the stack and as each container is bound via the
registrator container, each service is made available by DNS. By navigating to
http://YOUR_VM_OR_DOCKER_HOST_IP:8500 you will be presented with the Consul UI
which allows you to see all the services known to consul.

* Consul frontend can be found via port 8500

### Device Server
The device server RESTful API, routed via fabio and found on port 80.

* The device server is routed via fabio and can be found via port 80

### Fabio
Fabio reads the Key/Value store provided by Consul. As services become 
registered against consul with a health check and a specially defined urlprefix
key/value entry, Fabio add a route. Fabio seeks to provide a zero-conf 
 load balancing HTTP(S) router for deploying microservices managed by consul.

* The Fabio frontend is available via port 81
* The device server is routed via fabio and can be found via port 80

[vb]: <https://www.virtualbox.org/wiki/Downloads/>
[vg]: <https://www.vagrantup.com/downloads.html>
[de]: <https://docs.docker.com/engine/installation/>
[dc]: <https://docs.docker.com/compose/install/>


# Testing

### Building the test environment image

```sh
docker build -f docker/Dockerfile.DeviceServerTest -t deviceserver-test .
```

### Start the device server stack (if it is not already running)

```sh
git pull origin master
docker-compose up -d
```

### Retrieve the IP addresses of the required services

```sh
DEVICE_SERVER_IP=`docker inspect --format \
'{{ .NetworkSettings.Networks.deviceserver_DeviceServerNet.IPAddress }}' \
deviceserver_webservice-deviceserver_1`

LWM2M_SERVER_IP=`docker inspect --format \
'{{ .NetworkSettings.Networks.deviceserver_DeviceServerNet.IPAddress }}' \
deviceserver_lwm2m-server_1`
```

Ensure the two variables exist:

```sh
echo "DEVICE_SERVER_IP: $DEVICE_SERVER_IP"
echo "LWM2M_SERVER_IP: $LWM2M_SERVER_IP"
```

### Inserting organisations required by tests:

Set the following environment variables to values of your choice:

ORGANISATION_0_KEY=your_organisation_0_key_here

ORGANISATION_0_SECRET=your_organisation_0_secret_here

ORGANISATION_1_KEY=your_organisation_1_key_here

ORGANISATION_1_SECRET=your_organisation_1_secret_here

Run the following commands if the organisations do not exist.

```sh
docker exec imaginationdeviceserver_mongo_1 mongo localhost/Organisations --eval \
        'db.AccessKey.insert({"_id":"'"$ORGANISATION_0_KEY"'", "OrganisationID": NumberInt(0), "Name": "FunctionalTests", "Secret": "'"$ORGANISATION_0_SECRET"'"})'

docker exec imaginationdeviceserver_mongo_1 mongo localhost/Organisations --eval \
        'db.AccessKey.insert({"_id":"'"$ORGANISATION_1_KEY"'", "OrganisationID": NumberInt(1), "Name": "FunctionalTests", "Secret": "'"$ORGANISATION_1_SECRET"'"})'
```
     
### Running the test script

This script will create a container in which to run the tests, start dotnet-test, gather the results as an xml file and copy it out of the container. 


```sh
./ci/run-tests.sh $DEVICE_SERVER_IP $LWM2M_SERVER_IP json results_json.xml \
$ORGANISATION_0_KEY $ORGANISATION_0_SECRET $ORGANISATION_1_KEY $ORGANISATION_1_SECRET
```

The third argument `json`, specifies the content type which will be used for requests and responses, and can either be xml or json.
The fourth argument `results_json.xml`, is the XML file where results will be saved and can be any filename that you choose.  

### Remove device-server stack

```sh
docker-compose down
```




