
![](images/img.png)

----

# Introduction to the device server REST API

*This guide is aimed at engineers who are developing an IoT application that uses the device server REST API.*  

The objective of this article is to introduce the device server REST API, its concepts, structure and use.
Also covered are best practices, standard and custom HTTP headers and API data types.


### Device server REST API features

* TLS secured connection  
* OAuth2 based authenticated access  
* Supports response filtering to reduce returned content size by including only explicitly requested links and/or fields  
* MIME media types reflect the OMA LWM2M object/resource model  


## The basics

### Definitions

**Resource** - A data element or value, relating specifically to, or resulting from, the operation of a device. A resource is exposed via LWM2M operations for consumption by a management application which may reside on the device itself, or form a component of a related system. Resources are defined for interoperability, being fully described in terms of data type, multiplicity, access type and operations. A temperature measurement device for example, may expose several resources such as manufacturer, serial number, device identifier, current temperature measurement value, current time stamp etc.  

**Resource definition** - A description of a resource. All resources must be defined prior to use. The definition process describes the resource to facilitate interoperability. Any applications that share a resource must be made aware of the same definition. Note that the resource definition does not contain any data, it merely describes the resource. For a resource to contain data it must first be instantiated. Several instances of a resource may coexist depending on application design.  

**Object** - A named container grouping one or more associated resources. Extending the temperature sensor example from above, all of the device's resources can be grouped together as a single object called 'Temperature sensor'. Similar to a  resource, an object is fully described in terms of multiplicity, access type and operations, and must be defined prior to use.  

**Object definition** - The description of an object, i.e. a collection of related resource definitions. All objects and the resources that they contain, must be defined prior to use. The definition process describes the object to facilitate interoperability. Any applications that share an object must be made aware of the same definition. Note that the object definition does not contain any data, it merely describes the object. For an object to contain data it must first be instantiated. Several instances of an object may coexist depending on application design. 

**Asset** - Any data or data resource that does not fall strictly into the category of object or resource as defined above. An example could be metrics information, which although a data asset, does not require an object or resource definition. The term *Asset* is used to avoid confusion between common data resources and *resources* as defined by the OMA LWM2M standard. 

### Overview

The purpose of the device server REST API is to help you to manage your IoT application devices, data objects and resources. It’s important to understand that the device server organizes all assets in a hierarchical structure that enables link based navigation starting at the API root URL. All Create, Read, Update, Delete and *Observe* processes will be performed using standard HTTP methods (GET, POST, PUT, DELETE) with a few embellishments for authentication and request/response body content type definition. CoAP *Observe* support is subscription based and notified via webhook. The REST API implements the MIME media type model which defines the structure of request and response content.

### Link based navigation
The device server API follows REST architecture, which dictates that all asset navigation is achieved by following links, inevitably arriving at the target resource.
As an example, if we perform an authenticated HTTP GET request on the root API URL, the device server responds with a set of links that are appropriate the level of access of the client making the request.  

**GET** /  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA

The *Authorization* statement is a part of the HTTP request header, used to pass access credentials along with the request. For more on this see the [*Authentication*](authentication.md) section. 

Here's a typical API root response content:

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

Each link structure has three attributes; *rel*, *href* and *type*: 
 
* The *rel* (link relationship) attribute identifies the relationship between the link content and the client  
* The *href* is the hypertext reference, a direct link to the asset described in rel  
* The *type* attribute defines the MIME media type of the response content retrieved (or represented) by the href. These type definitions play an important part in the object and resource handling process described later.  

This is the starting point for all device server operations. Since the REST API operates under HATEOAS constraints, only those resources which are possible in *the current server state* will be returned. Operations are achieved by following the appropriate link. As an example, a list of currently connected clients (devices), can be found by perfoming an authenticated HTTP GET request on the *clients* link.

**Important point** - The device server internal asset structure is subject to change. If you hard code any of the *href* links into your application, it may break as further server developments occur. By correctly following the link navigation process your application is protected from changes implemented on the server side.

### Content types
Device server REST API request and response content is structured in specific ways using a MIME media type approach to describe each asset type as it is presented or accepted. All content is formatted in JSON by default, but can be explicitly requested in XML.  
To define the structure of a requested resource you’ll use an *Accept* header in your HTTP request. As an example:  

**Accept:** application/vnd.imgtec.resourcecreated<span style="color:red;">+json</span>  

This tells the device server that you’re expecting a response that’s structured like this:  
```json
{
  "ResourceCreated": {
    "ID": "4",
    "Link": {
      "rel": "self",
      "href": "subscriptions/4"
    }
  }
}
``` 


If you need an XML format response just change the type suffix:  

**Accept:** application/vnd.imgtec.resourcecreated<span style="color:red;">+xml</span>  
```xml 
<ResourceCreated>
    <ID>4</ID>
    <Link rel="self" href="subscriptions/4"/>
</ResourceCreated> 
```

**Note.** If you don’t state an *Accept* response media type, the default type for the requested asset will be returned.
 
The device server provides MIME media types for every resource and management function, with the basic naming scheme:

*application/vnd.imgtec.xxxxx[-version]*  
or  
*application/vnd.oma.lwm2m.xxxxx[-version]*  


Where *xxxxx* is the identifier that’s unique to the requested asset at that level. For example:

* application/vnd.imgtec.client  - represents a single client  
* application/vnd.imgtec.clients  - represents a list of clients  
* application/vnd.imgtec.metrics - represents a list of metrics  
* application/vnd.imgtec.subscriptions - represents a list of observe subscriptions  
* application/vnd.oma.lwm2m.subscription - represents a *single* observe subscription  
 
Since there may be several versions of a particular MIME media type available, the *–version* suffix is provided to ensure that only the requested version is returned. 
In the vast majority of cases version changes will be backwards compatible so the version suffix usually isn’t required, however, if the version suffix is omitted, the latest version of the media type will be assumed.

A full list of content types, with examples, is available in *The device server MIME media type reference*.

In the HTTP response, assuming that the transaction was successful, the **Content-Type** header will show the MIME media type of the enclosed response i.e.

**Content-Type:** *application/vnd.imgtec.resourcecreated*

## The request process
Asset reads are achieved through an authenticated HTTP GET operation, which is guaranteed not to change the state of the accessed data. If a response body is specified, it is only returned if the operation is successful. In the case of a failure, the HTTP response code will give an indication of the issue.  

| HTTP response code | Description |  
|-----|-----|  
| 200 | OK |  
| 201 | Created OK |  
| 400 | Bad request - See the BadRequest MIME type in [*Tables*](DSRESTindex.md#tables) below |  
| 401 | Unauthorised |  
| 403 | Not allowed |  
| 404 | Resource not found |  
| 409 | Conflict |  
| 410 | Gone |  
| 500 | Server error |  
| 501 | Not implemented |  
| 503 | Server busy |  
| 507 | Database not available |  
||||  


In the case of HTTP response code *400 – Bad request*, additional information will be provided in the response body in the MIME media type *application/vnd.imgtec.badrequest*. More detail is provided at the end of this document.
 
### Best practice
**Always use the right tool for the job.**   


* The GET method for read operations  
* The POST method for create operations  
* The PUT method for update operations  
* The DELETE method for remove operations  



**Follow the links to the target asset starting from the root URL.**  
Locating and using data will involve several HTTP requests beginning at the API entry point and ending at the target asset, object or resource. As an example, to access a list of clients:

**GET** /  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA  
**Accept:** application/vnd.imgtec.api+json  

The device server returns:

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
The *clients* link returns a list of clients (devices) that are currently connected to the device server, so if we perform an HTTP GET request on the clients link:

**GET** /clients  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA  
**Accept:** application/vnd.imgtec.clients+json
 
Here’s the response.
```json

{
    "PageInfo": {
        "TotalCount": 1,
        "ItemsCount": 1,
        "StartIndex": 0
      },
    "Items": [
        {
            "Links": [
                {
                    "rel": "self",
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
                }
              ]
         }
     ]
   
}
```

### Paging information

The above response contains a `PageInfo` tag for use in cases where the returned list is excessively long and unwieldy. A populated PageInfo tag looks something like this:  


```json
{
    "PageInfo": {
        "TotalCount": 7,
        "ItemsCount": 7,
        "StartIndex": 0,
        "Links": [
          {
            "rel": "first",
            "href": "/clients"
          },
          {
            "rel": "previous",
            "href": "/clients"
          },
          {
            "rel": "last",
            "href": "/clients"
          },
          {
            "rel": "next",
            "href": "/clients"
          }
        ]
     }
}
```

When required, paging information will offer a `TotalCount` of the number of items that qualify for the list, the `ItemsCount` or number of items in the current page, and the `StartIndex`, the current page number (starting at zero).  

Where applicable, additional links in the paging information are used to step through long lists a page at a time, choosing from:  

* First - the first page in the list  
* Last - the final page in the list  
* Previous - the page before the current page  
* Next - the page after the current page  

Where a large response can be anticipated, the *pageSize* and *startIndex* values may be used as parameters in the request query to indicate the required number of items in each page and the startpoint index in the list respectively:

**GET** /clients<span style="color:red;">?pageSize=10&startIndex=20</span>  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA  
**Accept:** application/vnd.imgtec.clients+json  

The above request would return ten items per page starting at page twenty.

| name | type | use | description |  
|-----|-----|-----|-----|  
| startIndex | integer | optional | Page number of the result set. If not supplied, defaults to 0 |  
| pageSize | integer | optional | Number of items to be returned per page. If not supplied, defaults to 10 |  
||||| 


### Response optimisation  

From the above examples it can be seen that some common REST API requests could return a significant amount of content. In cases where the requesting device is in any way constrained, with limited memory, bandwidth or processing power for example, even the use of paging may prove unsuitable. 
For this reason a further method is provided by which to optimize a request so that it returns only those fields and/or links required by the device or application. For a full explanation see the [*Response optimisation*](ResponseOptimization.md) section.

### Security

The device server REST API uses several strategies aimed at providing operational security and user privacy.  

##### Authenticated HTTP requests

All HTTP requests are required to be authenticated with valid access credentials. Authentication is more fully described in the [*Authentication*](authentication.md) section.

##### Authorisation

Authorisation is achieved using access credentials which identify the client and enables the device server to apply an appropriate access policy. The access credentials are passed in the *Authorization* header of an HTTP request.

##### SSL

Device server assets may contain sensitive data, so a secure connection is mandatory for most operations. It is strongly recommended that all transactions are performed under a secured connection. All device server CoAP transactions are secured using DTLS.

### Summary
Here are some key points to take away.  

* The device server REST API is designed to help you navigate the asset hierarchy.  
* The primary tool of REST API navigation is the resource link.  
* Don’t hard code link structures into an application because the device server is subject to change.   
* Each navigation link is composed of a hypermedia reference and a MIME media type definition which defines the format and structure of the response content returned by the link.  
* You can specify the format in which your resource is returned by stating a MIME media type in the *Accept* header of your HTTP request.  
* The default format for request and response content is JSON but XML can be specified by adding the +xml suffix to the MIME media type declaration in the *Accept* or *Content-Type* headers.  
* If you don’t state an *Accept* media type, the default type and version for the requested asset will be returned, but as a rule of best practice you should always specify your accepted media type.  
* Response content can be optimized to return only those links and fields that are required by the requesting device or application.  


----


## Tables
This section presents structures and values that are commonly associated with device server REST API operations.
### HTTP headers
The standard and custom HTTP headers used by the REST API.
#### Standard headers
| Header Name | Description |  
|-----|-----|  
| Accept | Indicates to the server what media type(s) this client is prepared to accept. |  
| Accept-Language | Indicates acceptable languages for response and defaults to en-GB. Should be set on every request for UI related translation to occur. |  
| Content-Length | Defines the size of the message response body. |  
| Content-Type | Describes the representation and syntax of the request message body. Required on POST or PUT requests that contain a message body. |  
| Host | Identifies the host receiving the message and is required to allow support of multiple origin hosts at a single IP address. |  
| Authorization | Used to hold user access tokens (obtained from a call to the user authorisation API). Mandatory for all web service API's that require authentication. |  
||||  



#### Custom headers

| Header Name | Description |  
|-----|-----|  
| X-Client-IPAddress | Used to hold the IP address of a web sites client, and should be set on every request. |  
| X-Client-RequestId | Used to hold a client specific request Id. If the web service detects this header in the request then it is automatically added to the http response headers, thus enabling a client that’s operating asynchronously to match request and responses. |  
| X-Culture | Holds the culture ([ISO 639 2 alpha language code]-[ISO 3166 2 alpha country code]) of a web sites client. If specified, this overrides the http Accept-Language setting. Used in cases where the client cannot control the Accept-Language header. |  
| X-Fields | Used in the request header to specify a comma delimited list of the fields to be returned. Used for response optimisation |  
| X-Links | Used in the request header to specify a comma delimited list of the links to be returned. Used for response optimisation |  
||||  



### Commonly used link relationships (rel attributes)

| Link relationship | Description |
|-----|-----|
| self | Indicates a HTTP GET method for reading the current resource or more details about the current resource (specified by href) |  
| next | Indicates an HTTP GET method for reading the next page of items |  
| previous | Indicates an HTTP GET method for reading the previous page of items |  
| first | Indicates an HTTP GET method for reading the first page of items |  
| last | Indicates an HTTP GET method for reading the last page of items |  
| remove | Indicates an HTTP DELETE method for deleting the current asset |  
| update | Indicates an HTTP PUT method for updating the current asset |  
| add | Indicates an HTTP POST method for adding a new asset to the asset specified by href |  
||||  



### API supported data types

| Type name | Description |
|-----|-----|
| ID | An asset, object or resource identifier. This may be an identifier containing alphanumeric characters. The maximum length should not be assumed and may be up to 512 bytes. |  
| Token | An authorisation token. This may be an identifier containing alphanumeric characters. The maximum length should not be assumed and may be up to 1024 bytes. |  
| Integer | A 16, 32 or 64 bit signed or unsigned integer. |  
| String | A UTF8 encoded string. |  
| Boolean | A boolean value, represented as a string with the values 'True' or 'False' (case insensitive). |  
| DateTime | An XML formatted DateTime string, in the form `yyyy-MM-ddTHH:mm:ss.fffffff` |  
| Enum | An enumerated type. |  
| Simplecontainer | A container or list, for a collection of items of a specific type. |  
| CurrencyAmount | Indicates a currency amount, defining a currency identifier and an amount (in units of the lowest currency denominator e.g. cents) |  
||||  



### The BadRequest media type

In the case of an HTTP 400, the response Content-Type will be set to BadRequest. The `ErrorCode` element of the response will indicate the type of error. A common error code across all API's is the InvalidFields error code. When this is set, there will be a collection of `Field` elements in the response to indicate which fields in the request body were invalid.
```json
{
  "BadRequest": {
    "ErrorCode": "1",
    "ErrorMessage": "Invalid type",
    "InvalidFields": { "Field": "clientName" },
    "ExternalErrorCode": "1"
  }
}
```


#### BadRequest element descriptions  


| Element | Type | Description |  
|-----|-----|-----|  
| ErrorCode | integer | Mandatory - one of the values outlined in bad request response codes above. |  
| ErrorMessage | string | Optional - a description of the error. |  
| InvalidFields | array | Optional - If present array field elements identify which of the fields in the request were invalid. |  
| Field | string | Optional - If present specifies an invalid field in the request were invalid. |  
||||  

<br>

#### BadRequest codes

| Code | Description |  
|-----|-----|  
| 1 | InvalidFields |  
||||  


----

----

