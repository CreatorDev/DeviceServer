
![](images/img.png)
----

# Common device server operations


The objective of this article is to explain some of the operations that are commonly performed by clients accessing device server assets via the REST API.

## Prerequisites  
To get the most from this section you'll need to be familiar with basics of the device server REST API - see [*Introduction to the device server REST API*](DSRESTindex.md).  
**Note.** For the sake of brevity, the examples given below do not show the full link navigation from API root to the target asset. In practice, it's important *not* to hard code links because the internal architecture of the device server is subject to change. Always take the RESTful approach.

## Introduction
The device server is a LWM2M management server designed to be implemented alongside third party cloud services to integrate M2M capability into an IoT application.  
The device server interfaces securely with constrained device networks via the Constrained Application Protocol (CoAP) and aids device and application interoperability by supporting both IPSO registered smart object definitions and custom object definitions.  
Device management is enabled through the implementation of the Open Mobile Alliance LWM2M standard. The CoAP interface and all LWM2M functionality is abstracted by the device server libraries, so an intimate knowledge of LWM2M and CoAP is not required.  
The device server also supports the CoAP *observe* verb by providing notifications whenever an observed object or resource value changes. Observations are managed as subscriptions which supply a webhook URL to recieve the change notification.  

## Common operations

### Retrieving a list of connected clients
All operations begin with an HTTP GET request to the device server REST API base url...

**GET** /  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA

```json 

{
    "Links": [
        {
            "rel": "authenticate",
            "href": "http://localhost:8080/oauth/token",
            "type": "application/vnd.imgtec.accesskeys+json"
        },
        {
            "rel": "accesskeys",
            "href": "http://localhost:8080/accesskeys",
            "type": "application/vnd.imgtec.accesskeys+json"
        },
        {
            "rel": "configuration",
            "href": "http://localhost:8080/configuration",
            "type": "application/vnd.imgtec.configuration+json"
        },
        {
            "rel": "clients",
            "href": "http://localhost:8080/clients",
            "type": "application/vnd.imgtec.clients+json"
        },
        {
            "rel": "identities",
            "href": "http://localhost:8080/identities",
            "type": "application/vnd.imgtec.identities+json"
        },
        {
            "rel": "objectdefinitions",
            "href": "http://localhost:8080/objecttypes/definitions",
            "type": "application/vnd.imgtec.objectdefinitions+json"
        },
        {
            "rel": "subscriptions",
            "href": "http://localhost:8080/subscriptions",
            "type": "application/vnd.imgtec.subscriptions+json"
        },
        {
            "rel": "versions",
            "href": "http://localhost:8080/versions",
            "type": "application/vnd.imgtec.versions+json"
        }
    ]
}
```

Picking up the *clients* link...

**GET** /clients  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA  
**Accept:** application/vnd.imgtec.clients+json  

Notice that the request *Accept* header has the *+json* suffix? This tells the device server to respond with a JSON content type as below:  

[]: [ClientsController.GetClients.Response]
```json

{
    "PageInfo": {
        "TotalCount": 2,
        "ItemsCount": 2,
        "StartIndex": 0
    },
    "Items": [
        {
            "Links": [
                {
                    "rel": "self",
                    "href": "/clients/e-nAl7UHV0GkN9bkMLohfw"
                },
                {
                    "rel": "objecttypes",
                    "href": "/clients/e-nAl7UHV0GkN9bkMLohfw/objecttypes",
                    "type": "application/vnd.imgtec.objecttypes+json"
                },
                {
                    "rel": "subscriptions",
                    "href": "/clients/e-nAl7UHV0GkN9bkMLohfw/subscriptions",
                    "type": "application/vnd.imgtec.subscriptions+json"
                }
            ],
            "Name": "TestClient1"
        },
        {
            "Links": [
                {
                    "rel": "self",
                    "href": "/clients/DWT7xvnv3kmmyCnKiycaNw"
                },
                {
                    "rel": "objecttypes",
                    "href": "/clients/DWT7xvnv3kmmyCnKiycaNw/objecttypes",
                    "type": "application/vnd.imgtec.objecttypes+json"
                },
                {
                    "rel": "subscriptions",
                    "href": "/clients/DWT7xvnv3kmmyCnKiycaNw/subscriptions",
                    "type": "application/vnd.imgtec.subscriptions+json"
                }
            ],
            "Name": "TestClient2"
        }
    ]
}
```

Alternatively an XML content type can be specified with a *+xml* suffix on the *Accept* header:

**GET** /clients  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA  
**Accept:** application/vnd.imgtec.com.clients **+xml**  

```xml

<Clients>
    <PageInfo>
        <TotalCount>2</TotalCount>
        <ItemsCount>2</ItemsCount>
        <StartIndex>0</StartIndex>
    </PageInfo>
    <Items>
        <Client>
            <Links>
                <Link rel="self" href="/clients/e-nAl7UHV0GkN9bkMLohfw" />
                <Link rel="objecttypes" href="/clients/e-nAl7UHV0GkN9bkMLohfw/objecttypes" type="application/vnd.imgtec.objecttypes+xml" />
                <Link rel="subscriptions" href="/clients/e-nAl7UHV0GkN9bkMLohfw/subscriptions" type="application/vnd.imgtec.subscriptions+xml" />
            </Links>
        </Client>
        <Client>
            <Links>
                <Link rel="self" href="/clients/DWT7xvnv3kmmyCnKiycaNw" />
                <Link rel="objecttypes" href="/clients/DWT7xvnv3kmmyCnKiycaNw/objecttypes" type="application/vnd.imgtec.objecttypes+xml" />
                <Link rel="subscriptions" href="/clients/DWT7xvnv3kmmyCnKiycaNw/subscriptions" type="application/vnd.imgtec.subscriptions+xml" />
            </Links>
        </Client>
    </Items>
</Clients>
```

The above response contains a list of the currently connected clients with some paging information added in cases where the list is long enough to prove unwieldy. For more information on paging, see [*Introduction to the Device Server REST API*](DSRESTindex.md#paging-information).

### Retrieving details of an individual client

Clients may be retrieved individually by supplying the clientID:  

**GET** /clients/e-nAl7UHV0GkN9bkMLohfw  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA  
**Accept:** application/vnd.imgtec.com.client+json  

Response:

[]: [ClientsController.GetClient.Response]
```json
{
    "Name": "testClient1",
    "Links": [
        {
            "rel": "self",
            "href": "http://localhost:8080/clients/e-nAl7UHV0GkN9bkMLohfw"
        },
        {
            "rel": "remove",
            "href": "http://localhost:8080/clients/e-nAl7UHV0GkN9bkMLohfw"
        },
        {
            "rel": "objecttypes",
            "href": "http://localhost:8080/clients/e-nAl7UHV0GkN9bkMLohfw/objecttypes",
            "type": "application/vnd.imgtec.objecttypes+json"
        },
        {
            "rel": "subscriptions",
            "href": "http://localhost:8080/clients/e-nAl7UHV0GkN9bkMLohfw/subscriptions",
            "type": "application/vnd.imgtec.subscriptions+json"
        },
        {
            "rel": "metrics",
            "href": "http://localhost:8080/clients/e-nAl7UHV0GkN9bkMLohfw/metrics",
            "type": "application/vnd.imgtec.metrics+json"
        }
    ]
}
```

### Retrieving a list of a client's supported object types

From the client list above we can now pick up the *objecttypes* link to see which object types are supported by (defined for) a particular client...

**GET** /clients/e-nAl7UHV0GkN9bkMLohfw/objecttypes  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA  
**Accept:** application/vnd.imgtec.objecttypes+json  
 
Response:

[]: [ClientsController.GetObjectTypes.Response]
```json
{
    "PageInfo": {
        "TotalCount": 7,
        "ItemsCount": 7,
        "StartIndex": 0
    },
    "Items": [
        {
            "ObjectTypeID": "6",
            "Links": [
                {
                    "rel": "self",
                    "href": "http://localhost:8080/clients/e-nAl7UHV0GkN9bkMLohfw/objecttypes/6"
                },
                {
                    "rel": "definition",
                    "href": "http://localhost:8080/objecttypes/definitions/O4a7t9cvhEuJzqR3mDIrpg",
                    "type": "application/vnd.imgtec.objectdefinition+json"
                },
                {
                    "rel": "instances",
                    "href": "http://localhost:8080/clients/e-nAl7UHV0GkN9bkMLohfw/objecttypes/O4a7t9cvhEuJzqR3mDIrpg/instances",
                    "type": "application/vnd.oma.lwm2m.location+json"
                }
            ]
        },
        {
            "ObjectTypeID": "5",
            "Links": [
                {
                    "rel": "self",
                    "href": "http://localhost:8080/clients/e-nAl7UHV0GkN9bkMLohfw/objecttypes/5"
                },
                {
                    "rel": "definition",
                    "href": "http://localhost:8080/objecttypes/definitions/fbcRX48kTkCqooNZRckzzQ",
                    "type": "application/vnd.imgtec.objectdefinition+json"
                },
                {
                    "rel": "instances",
                    "href": "http://localhost:8080/clients/e-nAl7UHV0GkN9bkMLohfw/objecttypes/fbcRX48kTkCqooNZRckzzQ/instances",
                    "type": "application/vnd.oma.lwm2m.firmwareupdate+json"
                }
            ]
        },
        {
            "ObjectTypeID": "4",
            "Links": [
                {
                    "rel": "self",
                    "href": "http://localhost:8080/clients/e-nAl7UHV0GkN9bkMLohfw/objecttypes/4"
                },
                {
                    "rel": "definition",
                    "href": "http://localhost:8080/objecttypes/definitions/W4-Tt8gW4E-8OMJBhBsiww",
                    "type": "application/vnd.imgtec.objectdefinition+json"
                },
                {
                    "rel": "instances",
                    "href": "http://localhost:8080/clients/e-nAl7UHV0GkN9bkMLohfw/objecttypes/W4-Tt8gW4E-8OMJBhBsiww/instances",
                    "type": "application/vnd.oma.lwm2m.connectivitymonitoring+json"
                }
            ]
        },
        {
            "ObjectTypeID": "3",
            "Links": [
                {
                    "rel": "self",
                    "href": "http://localhost:8080/clients/e-nAl7UHV0GkN9bkMLohfw/objecttypes/3"
                },
                {
                    "rel": "definition",
                    "href": "http://localhost:8080/objecttypes/definitions/y42v8ZgiZUG6k5iZ6Gc47Q",
                    "type": "application/vnd.imgtec.objectdefinition+json"
                },
                {
                    "rel": "instances",
                    "href": "http://localhost:8080/clients/e-nAl7UHV0GkN9bkMLohfw/objecttypes/y42v8ZgiZUG6k5iZ6Gc47Q/instances",
                    "type": "application/vnd.oma.lwm2m.device+json"
                }
            ]
        },
        {
            "ObjectTypeID": "20001"
        },
        {
            "ObjectTypeID": "20000"
        },
        {
            "ObjectTypeID": "15"
        }
    ]
}


```

The above response offers a list of object types associated with a particular client.  

Object types are identified by an ObjectTypeID and may be retrieved individually by stating the ObjectTypeID in the request:  

**GET** /clients/e-nAl7UHV0GkN9bkMLohfw/objecttypes/qvwis3pLWECc9oe3tWW_ng  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA  
**Accept:** application/vnd.imgtec.objecttype+json  

[]: [ClientsController.GetObjectType.Response]
```json
{
    "ObjectTypeID": "15",
    "Links": [
        {
            "rel": "self",
            "href": "http://localhost:8080/clients/oFIrQFrW8EWcZ5u7eGfrkw/objecttypes/qvwis3pLWECc9oe3tWW_ng"
        },
        {
            "rel": "definition",
            "href": "http://localhost:8080/objecttypes/definitions/qvwis3pLWECc9oe3tWW_ng",
            "type": "application/vnd.imgtec.objectdefinition+json"
        },
        {
            "rel": "instances",
            "href": "http://localhost:8080/clients/oFIrQFrW8EWcZ5u7eGfrkw/objecttypes/qvwis3pLWECc9oe3tWW_ng/instances",
            "type": "application/vnd.oma.lwm2m.devicecapability+json"
        }
    ]
}
```

 You can see from the above that links are provided to retrieve the object definition and a list of any instances of the object. We'll take a look at object definitions before moving on to managing object instances.

### Managing object definitions
Object definitions can be created, retrieved, updated and deleted using HTTP POST, GET, PUT and DELETE requests respectively.
An object is defined in terms of:  

* An object type identifier (ideally from an IPSO object definition)  
* An object name  
* An object MIME media type (for REST API purposes this describes the structure of the object definition)  
* Whether it is capable of multiple instances  
* Whether it is mandatory (must have one or more instances)  
* A list of one or more associated resources  


#### Creating an object definition  
When creating a new object definition the definition data must be structured as described by the appropriate MIME media type and passed in the body content of an HTTP POST request to the *objecttypes/definitions* link. The device server returns a *ResourceCreated* data type containing the new object definition id.

We can retrieve an object definition by performing an HTTP GET request on the *objecttypes/definitions* link, stating the objecttype ID...  

**GET** /objecttypes/definitions/O4a7t9cvhEuJzqR3mDIrpg  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA  
 
Response:

[]: [ObjectDefinitionsController.GetObjectDefinitions.Response]
```json 

{
    "ObjectDefinitionID": "O4a7t9cvhEuJzqR3mDIrpg",
    "ObjectID": "6",
    "Name": "Location",
    "MIMEType": "application/vnd.oma.lwm2m.location",
    "SerialisationName": "Location",
    "Singleton": true,
    "Properties": [
        {
            "PropertyDefinitionID": "DzegSL2c5E-ROW_nmEIuPw",
            "PropertyID": "0",
            "Name": "Latitude",
            "DataType": "String",
            "Units": "Deg",
            "IsCollection": false,
            "IsMandatory": true,
            "Access": "Read",
            "SerialisationName": "Latitude"
        },
        {
            "PropertyDefinitionID": "hF0slYEbiE242srNs65MDw",
            "PropertyID": "1",
            "Name": "Longitude",
            "DataType": "String",
            "Units": "Deg",
            "IsCollection": false,
            "IsMandatory": true,
            "Access": "Read",
            "SerialisationName": "Longitude"
        },
        {
            "PropertyDefinitionID": "YOR75kn4jEKmQmKpBjaP0w",
            "PropertyID": "2",
            "Name": "Altitude",
            "DataType": "String",
            "Units": "m",
            "IsCollection": false,
            "IsMandatory": false,
            "Access": "Read",
            "SerialisationName": "Altitude"
        },
        {
            "PropertyDefinitionID": "BmPOcs37-UmJ-MVpWIHEYw",
            "PropertyID": "3",
            "Name": "Uncertainty",
            "DataType": "String",
            "Units": "m",
            "IsCollection": false,
            "IsMandatory": false,
            "Access": "Read",
            "SerialisationName": "Uncertainty"
        },
        {
            "PropertyDefinitionID": "0vl1Df0-p0GseUK0pI0UUA",
            "PropertyID": "4",
            "Name": "Velocity",
            "DataType": "Opaque",
            "Units": "Refers to 3GPP GAD specs",
            "IsCollection": false,
            "IsMandatory": false,
            "Access": "Read",
            "SerialisationName": "Velocity"
        },
        {
            "PropertyDefinitionID": "SMKNbFi35UaIGJKwOrQGlg",
            "PropertyID": "5",
            "Name": "Timestamp",
            "DataType": "DateTime",
            "DataTypeLength": 1,
            "MinValue": "0",
            "MaxValue": "6",
            "IsCollection": false,
            "IsMandatory": true,
            "Access": "Read",
            "SerialisationName": "Timestamp"
        }
    ],
    "Links": [
        {
            "rel": "self",
            "href": "http://localhost:8080/objecttypes/definitions/O4a7t9cvhEuJzqR3mDIrpg"
        },
        {
            "rel": "update",
            "href": "http://localhost:8080/objecttypes/definitions/O4a7t9cvhEuJzqR3mDIrpg"
        },
        {
            "rel": "remove",
            "href": "http://localhost:8080/objecttypes/definitions/O4a7t9cvhEuJzqR3mDIrpg"
        }
    ]
}
```

From the above it can be seen that the *properties* of the object definition actually represent the object's *resource* collection. Each property is fully defined by:  

* An identifier  
* A name  
* A data type (integer, float, ...)  
* Accessibility (Read, Write, ReadWrite and Execute)  
* Whether the property is mandatory (must have one or more instances)  
* Whether the property may have more than one instance (determined by the *IsCollection* field)  


Remember that while the above structure defines an object and its associated properties, it can't contain actual data values until it is instantiated, and then *only the value of the resource* is accessible. A resource's type and accessibility are implied by its definition so that when the data is shared, the receiving function can interpret it correctly.  


#### Updating an object definition  
The update process is achieved by overwriting the original definition using an HTTP PUT request with the updated definition in the request body. An update request does not return any response content. Success is determined from the HTTP response code.


#### Deleting an object definition  
Removing an object definition requires an HTTP DELETE request to the appropriate */objecttypes/definitions/{id}* link. No response content is returned from a delete operation. Success is determined from the HTTP rsponse code.

### Managing object instances
An object may have one or more instances (as stated by its definition), each of which will contain a set of resource instances holding real data values. When we manage  objects and resource values, we're actually managing instances of those  objects and resources.
Object instances can be created, read, updated and deleted *within the constraints of the object definition* (a single instance of a mandatory object cannot be deleted for example, nor can multiple instances be created where the object definition states otherwise).
Where an object is defined as being mandatory, the device server will assume that at least one instance of the object will always exist on the associated client.

#### Listing object instances
To get a list of instances associated with a particular object definition we use an HTTP GET request targeting both the client, and the particular instance of the object definition. Below is an example:

**GET** /clients/{id}/objecttypes/{definitionid}/instances  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA 

[]: [ClientsController.GetObjectInstances.Response]
```json
{
  "Locations": {
    "Link": {
      "-rel": "add",
      "-href": "/clients/{id}/objecttypes/{definitionid}/instances"
    },
    "Items": {
      "Location": {
        "Link": [
          {
            "-rel": "self",
            "-href": "/clients/{id}/objecttypes/{definitionid}/instances/{id}"
          },
          {
            "-rel": "update",
            "-href": "/clients/{id}/objecttypes/{definitionid}/instances/{id}"
          },
          {
            "-rel": "remove",
            "-href": "/clients/{id}/objecttypes/{definitionid}/instances/{id}"
          },
          {
            "-rel": "subscriptions",
            "-href": "/clients/{id}/objecttypes/{definitionid}/instances/{id}/subscriptions"
          }
        ],
        "InstanceID": "0",
        "Latitude": "-41.6",
        "Longitude": "170.23",
        "Timestamp": "2016-04-14T03:48:00Z"
      }
    }
  }
}
```
You can see that the above instance of a *location* object holds real resource data and has its own instance identifier. Note that in this object instance there are several single instance  resources (InstanceID, Latitude, Longitude and Timestamp). Since the location object can have multiple instances it has been returned as an item in a list, even though there's only one current instance.
In addition to the object information, the device server returns the links required to manage it, including *update* and *remove* options. The *subscriptions* link is of particular interest because it's used to support the CoAP *observe* function. This is covered later in *Subscribing to observations*.

#### Creating object instances
Because multiple instances of the above object are allowed we can create a new instance using an HTTP POST request with the new instance values in the request body content:

**POST** /clients/{id}/objecttypes/{definitionid}/instances  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA  
**Content-Type:** application/vnd.oma.lwm2m.locations+json

Here's the body content:
[]: [ClientsController.AddObjectInstance.Request]
```json
{
  "Locations": {
    "Items": {
      "Location": {
        "Latitude": "-26.45",
        "Longitude": "123.45",
        "Timestamp": "2016-05-07T11:56:00Z"
      }
    }
  }
}
```

The device server returns a *resourceCreated* response containing a new instanceID.

#### Updating object instances
To update an object instance use an HTTP PUT request with the updated instance values in the request body content. To do this you'll also need the target instanceID for the object that you're updating.
To perform an update we'll use an HTTP PUT request with the updated values in the request body content.

**PUT** /clients/{id}/objecttypes/{definitionid}/instances  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA  
**Content-Type:** application/vnd.oma.lwm2m.locations+json

Here's the body content:

[]: [ClientsController.UpdateObjectInstance.Request]
```json
{
  "Locations": {
    "Items": {
      "Location": {
        "Latitude": "-34.56",
        "Longitude": "131.54",
        "Timestamp": "2016-05-07T11:56:00Z"
      }
    }
  }
}
```

The delete request doesn't return a response body. Success (or otherwise), is indicated by the HTTP request response code:

| Code | Description |  
|-----|-----|  
| 200 | Request completed OK |  
| 401 | Unauthorised request |  
| 404 | Target not found |  

#### Deleting object instances

To delete an object instance you'll need to know the object's instance ID. Following the links from the API root to the client's objecttypes definition list and then to the appropriate instance...

**DELETE** /clients/{id}/objecttypes/{definitionid}/instances/{id}  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA  

The delete request doesn't return a response body. Success (or otherwise), is indicated by the HTTP request response code.


## Summary

Here are a few key points to take away...  

* The device server's clients are devices and applications  
* A client will have one or more data  objects associated with it  
* An object is a named collection of resources  
* A resource is a fully described data item, with a specified type, access method and instance controls  
* An object and/or resource must be defined on both the client and the device server in order to share data  
* A definition *describes* data, an instance *contains* data   
* The device server returns a full list of currently connected clients and all the objects associated with them  
* Definitions and instances can be managed via the device server REST API  
* The *observe* verb is implemented via subscription to client, object and resource events  
* Subscribed observes are notified via webhook  
 

----

----

