
![](images/img.png)
----

## Device server installation and deployment

The aim of this guide is to describe the installation processes for the Creator device server.

The Creator device server has been provided both as a complete open source project and as a set of Docker images.

### Installation using Docker


The simplest installation method is use `docker-compose` to deploy the public docker images

You will need to setup the following environment variables

```
export DEVICESERVER_HOSTNAME=<resolvable hostname of server>
export RABBITMQ_USERNAME=<any old username>
export RABBITMQ_PASSWORD=<nice random password>
```
### Setting up the certificates


In order to generate certificates the device server needs a certificate authority. Below are the commands to create this.
Run the following commands from the `docker/ds` directory (note that CSR generation will require some interactive input):
```
openssl ecparam -genkey -name prime256v1 -out Root.key
openssl req -new -sha256 -key Root.key -out csr.csr
openssl req -x509 -sha256 -days 3650 -key Root.key -in csr.csr -out Root.crt
```
```
openssl ecparam -genkey -name prime256v1 -out CA.key
openssl req -new -sha256 -key CA.key -out CA.csr
openssl ca -config root.cnf -notext -md sha256 -days 3650 -in CA.csr -out CA.crt
```
```
openssl ecparam -genkey -name prime256v1 -out LWM2MBootstrap.key
openssl req -new -sha256 -key LWM2MBootstrap.key -out bootstrap.csr
openssl ca -config intermediate.cnf -notext -md sha256 -days 730 -in bootstrap.csr -out LWM2MBootstrap.crt
openssl ec -in LWM2MBootstrap.key -pubout -out LWM2MBootstrap.pub
```
```
openssl ecparam -genkey -name prime256v1 -out LWM2MServer.key
openssl req -new -sha256 -key LWM2MServer.key -out server.csr
openssl ca -config intermediate.cnf -notext -md sha256 -days 730 -in server.csr -out LWM2MServer.crt
openssl ec -in LWM2MServer.key -pubout -out LWM2MServer.pub
```
The following pem files are the ones actually used by the device server
```
cat Root.key Root.crt > Root.pem
cat CA.key CA.crt > CA.pem
cat LWM2MBootstrap.key LWM2MBootstrap.pub LWM2MBootstrap.crt > LWM2MBootstrap.pem
cat LWM2MServer.key LWM2MServer.pub LWM2MServer.crt > LWM2MServer.pem
```
Verify the bootstrap and server certificates
```
openssl verify -verbose -CAfile <(cat Root.pem CA.pem) LWM2MServer.pem
openssl verify -verbose -CAfile <(cat Root.pem CA.pem) LWM2MBootstrap.pem
```
The device server rest api sits behind nginx which should be setup with an SSL certificate

If you can, use  https://letsencrypt.org or use an existing certificate for the hostname you are using. If not generate a self signed certificate in the following directory
```
openssl dhparam -out dhparam.pem 2048
```
```
docker/dhparam.pem
docker/ssl/key.pem
docker/ssl/cert.pem
```


### Deploying

You can now bring up the docker stack...
```
docker-compose up -d
```
Check the output of `docker ps` to make sure the status of all the containers is up

### Setup an initial key and secret

Generate a couple of nice long random strings to use as a key and secret, for example
```
2Jy6bdbSNlf7XgO441PM8Z1FRV5IoVhu01KEbyvtVJb6FSOEIz7w49zQ3bsW0LCAUdEeVq7q324xqW029ehOkz
jWS9lH6U695311I052p5i03977l9R29rV6VnvAd06Xl6p1U8PCLPaJlzw1pLhQgS83j5L62Xl4n339HJjI5279
```
Insert these into the mongo database
```
    docker exec deviceserver_mongo_1 mongo localhost/Organisations --eval \
        'db.AccessKey.insert({"_id":"XXX", "OrganisationID": NumberInt(0), "Name": "admin", "Secret": "YYY"})'
```
Where XXX and YYY are your new key/secret

### Test the service


`https://<resolvable hostname of server>`

### Building the Docker images from the device server source code


If you want to build from source code, or implement additions to device server functionality, you'll need to rebuild the docker images
```
cd build
make
docker images
```


----

----
