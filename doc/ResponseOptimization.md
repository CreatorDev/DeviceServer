
![](images/img.png)
----

# Device server REST API response optimisation

*This guide is aimed at engineers who are developing an IoT application that uses the device server REST API.*

The objective of this article is to introduce the practice of REST API response optimisation, including response content filtering by link and by field.

## Prerequisites  
To get the most from this document you'll need to be familiar with basics of the device server REST API - see [*Introduction to the device server REST API*](DSRESTindex.md).

## Introduction  
Device resource constraints, bandwidth cost and network latency are of major concern to IoT application developers, and the ability to reduce the content of a request response to only those links and fields that are absolutely required, is extremely beneficial in terms of minimising the resources needed to transport and to process it. This article aims to introduce the means by which the response content returned by the device server REST API can be filtered to return a targeted field and/or link subset.

## The problem

Here's a response to a request for a list of object types associated with a particular client:

**GET** /clients/Gjq2Ot3ZQ0u_oF5DKHIxhg/objecttypes  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA  
**Accept:** application/vnd.oma.lwm2m.objects+json

```json
{
 "PageInfo": {
    "TotalCount": 8,
    "ItemsCount": 8,
    "StartIndex": 0
  },
  "Items": [
    {
     "ObjectTypeID": "6",
     "Links": [
       {
        "rel": "self",
        "href": "http://localhost:8080/clients/Gjq2Ot3ZQ0u_oF5DKHIxhg/objecttypes/6"
       },
       {
        "rel": "definition",
        "href": "http://localhost:8080/objecttypes/definitions/eJzqR3mDIrpg",
        "type": "application/vnd.imgtec.objectdefinition+json"
       },
       {
        "rel": "instances",
        "href": "http://localhost:8080/clients/Gjq2Ot3ZQ0u_oF5DKHIxhg/objecttypes/eJzqR3mDIrpg/instances",
        "type": "application/vnd.oma.lwm2m.location+json"
       }
         ]
    },
    {
     "ObjectTypeID": "5",
     "Links": [
       {
        "rel": "self",
         "href": "http://localhost:8080/clients/Gjq2Ot3ZQ0u_oF5DKHIxhg/objecttypes/5"
       },
       {
        "rel": "definition",
        "href": "http://localhost:8080/objecttypes/definitions/CqooNZRckzzQ",
        "type": "application/vnd.imgtec.objectdefinition+json"
       },
       {
        "rel": "instances",
        "href": "http://localhost:8080/clients/Gjq2Ot3ZQ0u_oF5DKHIxhg/objecttypes/CqooNZRckzzQ/instances",
        "type": "application/vnd.oma.lwm2m.firmwareupdate+json"
       }
         ]
     },
     {
      "ObjectTypeID": "4",
      "Links": [
        {
         "rel": "self",
         "href": "http://localhost:8080/clients/Gjq2Ot3ZQ0u_oF5DKHIxhg/objecttypes/4"
        },
        {
         "rel": "definition",
         "href": "http://localhost:8080/objecttypes/definitions/8OMJBhBsiww",
         "type": "application/vnd.imgtec.objectdefinition+json"
        },
        {
         "rel": "instances",
         "href": "http://localhost:8080/clients/Gjq2Ot3ZQ0u_oF5DKHIxhg/objecttypes/8OMJBhBsiww/instances",
         "type": "application/vnd.oma.lwm2m.connectivitymonitoring+json"
        }
         ]
      },
      {
       "ObjectTypeID": "3",
       "Links": [
         {
          "rel": "self",
          "href": "http://localhost:8080/clients/Gjq2Ot3ZQ0u_oF5DKHIxhg/objecttypes/3"
         },
         {
          "rel": "definition",
          "href": "http://localhost:8080/objecttypes/definitions/k5iZ6Gc47Q",
          "type": "application/vnd.imgtec.objectdefinition+json"
         },
         {
          "rel": "instances",
          "href": "http://localhost:8080/clients/Gjq2Ot3ZQ0u_oF5DKHIxhg/objecttypes/k5iZ6Gc47Q/instances",
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
      },
      {
       "ObjectTypeID": "20005"
      }
   ]
}
```

The above object definition lists only eight object types but there's no upper limit to how many objects a client may contain, so the above structure could have been considerably larger, which puts pressure on bandwidth usage, response latency and processing power, especially in the case of the more constrained devices.

A further issue is that of data redundancy. What if we only need to list the *self* link for each object type? Clearly two of the three links in each object type would be redundant in this case.

What's required then is the means to *filter the server response such that only the desired content links and fields are returned*.

## The solution
### Link filtering 

Starting with a simple example, we'll make a GET request to the API root:

**GET** /  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA  

Which returns...

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

Now let's imagine that you only intend to use the *clients* link. You can specifically request only that link by adding a custom *X-Links* request header. Here's how it's done:

**GET** /  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA  
<span style="color:red;">**X-Links:** clients</span>  


Which returns...
```json

{
    "Links": [
        {
            "rel": "clients",
            "href": "http://localhost:8080/clients",
            "type": "application/vnd.imgtec.clients+json"
        }
    ]
}

```

If you need more than one link, use a comma delimited list of the links required:

**GET** /  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA  
<span style="color:red;">**X-Links:** clients, versions</span>  

Which returns...
```json
 {
    "Links": [
        {
            "rel": "clients",
            "href": "http://localhost:8080/clients",
            "type": "application/vnd.imgtec.clients+json"
        },
        {
            "rel": "versions",
            "href": "http://localhost:8080/versions",
            "type": "application/vnd.imgtec.versions+json"
        }
    ]
}
```

Setting the *X-Links* value to a non existent link (for example *X-Links: none*) removes *all* links, in this case resulting in an empty response.

### Field filtering  

Starting with a GET request for object definition metadata:

**GET** /objecttypes/definitions  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA  
**Accept:** application/vnd.oma.lwm2m.objects+json

```json
{
  "ObjectsMetadata": {
    "Items": {
      "ObjectMetadata": {
        "Link": {
          "rel": "self",
          "href": "https://xxxxx.com/devices/objects/metadata/NXv_eoXt10G9-l3LlDzRug",
          "type": "application/vnd.imgtec.com.objectmetadata+json"
        },
        "ObjectID": "20020",
        "Name": "DemoObject",
        "MIMEType": "application/vnd.oma.lwm2m.object",
        "SerialisationName": "DemoObject",
        "Singleton": "True",
        "Properties": {
          "Property": [
            {
              "PropertyID": "0",
              "Name": "Button1State",
              "DataType": "Integer",
              "IsCollection": "False",
              "IsMandatory": "False",
              "Access": "Read",
              "SerialisationName": "Button1State"
            },
            {
              "PropertyID": "1",
              "Name": "Button2State",
              "DataType": "Integer",
              "IsCollection": "False",
              "IsMandatory": "False",
              "Access": "Read",
              "SerialisationName": "Button2State"
            },
            {
              "PropertyID": "2",
              "Name": "Led1",
              "DataType": "Integer",
              "IsCollection": "False",
              "IsMandatory": "False",
              "Access": "ReadWrite",
              "SerialisationName": "Led1"
            },
            {
              "PropertyID": "3",
              "Name": "Led2",
              "DataType": "Integer",
              "IsCollection": "False",
              "IsMandatory": "False",
              "Access": "ReadWrite",
              "SerialisationName": "Led2"
            }
          ]
        }
      }
    }
  }
}
```

This response is composed almost entirely of fields rather than links so we'll use the *X-Fields* header to reduce the response content to something more manageable. We'll start with just the object ID and name for each listed object:

**GET** /objecttypes/definitions  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA  
**Accept:** application/vnd.oma.lwm2m.objects+json  
<span style="color:red;">**X-Fields:** Items(ObjectID, Name)</span>  

Which returns...
```json
{
  "ObjectsMetadata": {
    "Items": {
      "ObjectMetadata": {
        "Link": {
          "rel": "self",
          "href": "https://xxxxx.com/devices/objects/metadata/NXv_eoXt10G9-l3LlDzRug",
          "type": "application/vnd.imgtec.com.objectmetadata+json"
        },
        "ObjectID": "20020",
        "Name": "DemoObject"
      }
    }
  }
}
```


#### Filtering nested fields
In the above example we used the sub-selector syntax: `X-Fields: Items(ObjectID, Name)`, which returned the target fields (ObjectID and Name) contained in each item, (note that *Items* is a collection of type *Item* and each *Item* is named *ObjectMetadata*). 

If we now wish to add fields from the *Properties* field set, we'll need to subset further:

`X-Fields: Items(ObjectID, Name, Properties(PropertyID,Name))`

Here, we've asked for the *ObjectID* and *Name* fields as before, but also added the *Properties* field and asked for a subset of *PropertyID* and *Name*. We'll also remove the one remaining link using `X-Links: none`. This is how our request looks:

**GET** /objecttypes/definitions 
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA  
**Accept:** application/vnd.oma.lwm2m.objects+json  
<span style="color:red;">**X-Fields:** Items(ObjectID, Name, Properties(PropertyID,Name))  
**X-Links:** none</span>  

Which returns...
```json
{
  "ObjectsMetadata": {
    "Items": {
      "ObjectMetadata": {
        "ObjectID": "20020",
        "Name": "DemoObject",
        "Properties": {
          "Property": [
            {
              "PropertyID": "0",
              "Name": "Button1State"
            },
            {
              "PropertyID": "1",
              "Name": "Button2State"
            },
            {
              "PropertyID": "2",
              "Name": "Led1"
            },
            {
              "PropertyID": "3",
              "Name": "Led2"
            }
          ]
        }
      }
    }
  }
}
```

**Note.** The sub-selector syntax can also be used in the *X-Links* header to select links that lie within a nested structure. An example of this, along with several other use cases, is given in *further examples* below.

## Filter syntax

The X-Fields and the X-Links filters use the same syntax and rules:

* A comma separated list of item names that must be returned i.e. clientID, clientName, …
* In the case of nested fields or links within objects, a sub-selector can be used to request a specific set of sub-fields by placing expressions in parentheses i.e. Items(ObjectID,Name).
* If the X-Fields or the X-Links request header is missing or empty it will not be applied so *all* fields or links will be returned.
* If the X-Fields header contains a non-existing value, no fields will be returned.
* If the X-Links header contains a non-existing value, no links will be returned.
* X-Fields has precedence over X-Links such that if X-Fields excludes an object none of its links will be shown, even if the link is explicitly included in X-Links.

----

## Further examples
To filter a list of fields using a sub-selector:

**GET** /examples/1  

Full response:  
```json
{
  "Example": {
    "Links": [
      {
        "rel": "self",
        "href": "/examples/1"
      },
      {
        "rel": "update",
        "href": "/examples/1"
      }
    ],
    "ExampleID": "1",
    "Name": "Property Filter",
    "ExampleType": "Demo",
    "ChildElement": {
      "Links": [
        {
          "rel": "self",
          "href": "/examples/1/child/1"
        },
        {
          "rel": "update",
          "href": "/examples/1/child/1"
        }
      ],
      "ID": "1",
      "Name": "Child 1"
    }
  }
}
```

With filtering:

**GET** /examples/1  
**X-Fields:** ExampleType, ChildElement(Name)

Filtered response:  
```json
{
  "Example": {
    "Links": [
      {
        "rel": "self",
        "href": "/examples/1"
      },
      {
        "rel": "update",
        "href": "/examples/1"
      }
    ],
    "ExampleType": "Demo",
    "ChildElement": {
      "Links": [
        {
          "rel": "self",
          "href": "/examples/1/child/1"
        },
        {
          "rel": "update",
          "href": "/examples/1/child/1"
        }
      ],
      "Name": "Child 1"
    }
  }
}
```

A combined filter of Fields and Links with sub-selector:

**GET** /examples  

Full response:  
```json
{
  "Examples": {
    "Link": {
      "rel": "add",
      "href": "/examples"
    },
    "Items": {
      "Example": [
        {
          "Links": [
            {
              "rel": "self",
              "href": "/examples/1"
            },
            {
              "rel": "update",
              "href": "/examples/1"
            }
          ],
          "ExampleID": "1",
          "Name": "Field Filter",
          "ExampleType": "Demo"
        },
        {
          "Links": [
            {
              "rel": "self",
              "href": "/examples/2"
            },
            {
              "rel": "update",
              "href": "/examples/2"
            }
          ],
          "ExampleID": "2",
          "Name": "Link Filter",
          "ExampleType": "Demo"
        }
      ]
    }
  }
}
```
 
With filtering:

**GET** /examples  
**X-Fields:** Items(ExampleID, Name)  
**X-Links:** add, Items(self)

Filtered response: 
```json
{
  "Examples": {
    "Link": {
      "rel": "add",
      "href": "/examples"
    },
    "Items": {
      "Example": [
        {
          "Links": {
            "rel": "self",
            "href": "/examples/1"
          },
          "ExampleID": "1",
          "Name": "Field Filter"
        },
        {
          "Links": {
            "rel": "self",
            "href": "/examples/2"
          },
          "ExampleID": "2",
          "Name": "Link Filter"
        }
      ]
    }
  }
}
```

----

## Summary
Here are a few key points to take away:

* Due to some IoT device limitations it’s beneficial to reduce request response content, bandwidth usage and network latency as much as possible.
* Device server REST API responses may return a high level of content redundancy which can be significantly reduced with appropriate filtering of fields and links.
* Filtering is accomplished by the use of the X-Fields and X-Links custom request headers.
* X-Fields contains a comma delimited list of the desired response fields.
* X-Links contains a comma delimited list of the desired response links.
* A missing filter header will not be applied. All of the fields or links will be returned.
* If a non-existing link or field is listed, no fields or links will be returned.
* Nested fields, or links within objects or fields, can be referenced using the sub-setting syntax.


----

----

