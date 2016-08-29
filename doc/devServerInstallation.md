
![](images/img.png)
----

## Device server installation and deployment

The aim of this guide is to describe the installation processes for the Creator device server.

The Creator device server has been provided both as a complete open source project and as a set of Docker containers. 

### Installation using Docker


**We've assumed that you already have Docker installed and ready to go and that you have some background knowledge regarding its use. If not, go [here](https://www.docker.com/) for details.**


The simplest installation method is to *pull* the current device server Docker containers, in which case you only need to download the relevant Docker compose file, [docker-compose.yaml](https://gitlab.flowcloud.systems/FlowM2M/DeviceServer/raw/master/docker-compose.yml) into a local working directory.


If you're planning to build a device server Docker image from source you'll need to locally clone the [device server GitHub repository](git@gitlab.flowcloud.systems:FlowM2M/DeviceServer.git). 


### Setting up a domain for https

On your workstation you'll also need a domain which resolves to the IP address of the machine that's running Docker. We suggest DNS hi-jacking `deviceserver.mymachine.com`.

The Docker *compose* command references the *DEVICESERVER_HOSTNAME* environment variable to determine its 
domain name so you'll need to assign this...
```
export DEVICESERVER_HOSTNAME=deviceserver.mymachine.com  
```

Alternatively you can create a more permanent hostname assignment by creating a `.env` file with the following content:   

```
DEVICESERVER_HOSTNAME=deviceserver.mymachine.com
```

If you're using Linux you'll need to edit */etc/host* to resolve the *deviceserver.mymachine.com* domain by adding...


```
127.0.0.1       localhost deviceserver.mymachine.com
```

If you're using Windows you'll need to use the IP address of your virtual machine. In your Docker terminal use *docker-machine inspect* to find this...

```
$ docker-machine inspect
{
    "ConfigVersion": 3,
    "Driver": {
        "IPAddress": "192.168.99.100",
	(...)
}
```

Then open Notepad as an administrator and edit `C:\Windows\System32\drivers\etc\hosts` to include the new IP address...

```
192.168.99.100 deviceserver.mymachine.com
```

### SSL certification

The device server includes a *Fabio* load balancer which terminates HTTPs connections and which requires an SSL certificate to operate.




You can produce a self-signed certificate from scratch, or use an existing Private Key and CSR if you have one. For either option you'll need to have openssl installed, if you are using Windows the docker quickstart terminal, (usually installed with docker) or git bash will provide this.

**To generate self-signed certification for `*.mymachine.com`**

**Note.** When using a self-signed certificate you will need to tell your browser, or other client, to ignore that these certificates are not signed by a trusted authority.


Generate a Private Key and a CSR...
```
mkdir -p docker/ssl
COMMON_NAME=*.mymachine.com
openssl req -newkey rsa:2048 -nodes -keyout docker/ssl/domain_key.pem -subj "/CN=${COMMON_NAME}" -out docker/ssl/domain_csr.pem
```

Then generate a self-signed certificate from the Private Key and CSR...
```
openssl x509 -signkey docker/ssl/domain_key.pem -in docker/ssl/domain_csr.pem -req -days 365 -out docker/ssl/domain_cert.pem
```

Load the certificate into a temporary docker volume...
  

```
docker volume create --name certs
docker run -d --name certstool -v certs:/ssl alpine /bin/true
docker cp docker/ssl/domain_cert.pem certstool:/ssl/cert.pem
docker cp docker/ssl/domain_key.pem certstool:/ssl/key.pem
docker rm certstool
```


You can now download the docker images and launch the docker stack...
```
docker-compose pull
docker-compose up -d
```
###Building a Docker image from the device server source code


If you want to build from source code, or implement additions to device server functionality, you'll need to rebuild the docker image. Clone the Git repository and navigate to the local *DeviceServer* directory, then run the following:

```
docker-compose build deviceserver-build
docker-compose up -d
```


### Test the service


[https://deviceserver.mymachine.com](https://deviceserver.mymachine.com)  



----

----
