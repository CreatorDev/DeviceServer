#!/bin/bash
#
# register the object definitions from a json file 
# using the REST API
##

if [ ! -z $1 ] 
then
     WEBSERVICE=$1
else
     WEBSERVICE=http://localhost:8080
fi

JSON_FILE=ObjectDefinitions.json

curl -vX POST ${WEBSERVICE}/objecttypes/definitions \
     -d @${JSON_FILE} \
     --header "Content-Type: application/vnd.imgtec.objectdefinitions+json" \
     --header "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJPcmdJRCI6IjAiLCJleHAiOjE0OTA5NTgwMDB9.zw-laBQRc7Xcxo5excZQeiWRy67UqUq5SNP64U__NxE"
