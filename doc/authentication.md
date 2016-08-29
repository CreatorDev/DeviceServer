
![](images/img.png)
----

# Device server REST API request authentication

*This guide is aimed at engineers who are developing an IoT application that uses the device server REST API.*

The objective of this article is to explain authentication requirements for clients accessing the device server assets via the REST API.

## Prerequisites  
To get the most from this document you'll need to be familiar with basics of the device server REST API. See [*Introduction to the device server REST API*](DSRESTindex.md).

Authentication is a prerequisite for all communications with the device server REST API and its associated resources.

## Device server interfaces

The device server exposes two interfaces; HTTP/REST API and CoAP/LWM2M. In general, devices will use the CoAP interface and all requests will be authenticated by *device certificate*.  All other applications (such as those on mobile devices) and web services will use the REST API and authenticate using an *access token*.

### REST API authentication  
Use of the device server REST API requires that each HTTP request is authenticated by a valid access token.

Acquiring an access token is a multi-stage process:  

* Obtain key and secret tokens (once only) from the developer console  
* Use the key and secret to obtain a unique access_token and a refresh_token  

The valid access_token is then submitted in the *Authorization* request header of each REST API request.  

## Key and secret tokens

On installation the device server database will contain admin key and secret tokens used to generate access keys and device certificates which will be associated with your organisation.  

Having logged into the device server, the desired organisation name can be posted to the *accesskeys* API endpoint:  

**POST** /accesskeys

[]: [AccessKeysController.AddAccessKey.Request]
```json  
{
    "Name":"My Organisation Name"
}
```
The device server returns a user level key and secret which is associated with the named organisation:  

[]: [AccessKeysController.AddAccessKey.Response]
```json
{
  "Links" : [
    {"rel":"self","href":"/accesskeys/1"}
  ],
  "Name":"My Access Keys",
  "Key":"CDqDz71VY1103192cdjz91la091la91j1199HHRand0mVa191luel193aajl193chd19ld9174jcjaMTBdjkloH009rXDEGqnAwa",
  "Secret": "l7U2LhKfcp95LpvAmEhPSoItcf_z9_gKZd8Ew3h0OwK32K12fTzKvuTiksnqp4sHo_BOvZTuqJftKsDzZ8p0dg"
}
```

**Note.** *Creating access keys as an admin level user will return keys for a new organisation. Creating keys as a logged in user will generate new keys for the organisation that the user is associated with.*

## Access and refresh tokens  

Here's a typical response from the device server root API which has been accessed *without authentication*:  
```json

{
    "Links": [
        {
            "rel": "authenticate",
            "href": "http://localhost:8080/oauth/token",
            "type": "application/vnd.imgtec.accesskeys+json"
        },
        {
            "rel": "versions",
            "href": "http://localhost:8080/versions",
            "type": "application/vnd.imgtec.versions+json"
        }
    ]
}
```

The above response is limited to a *versions* link, which offers information about the package versions used in the current server build, and the *authenticate* link, which is used to generate access and refresh tokens from the *key* and *secret* tokens.  


The access_token and refresh_token are obtained by submitting key and secret tokens via HTTP POST request to the *authenticate* link of the REST API. The device server will then return a valid access_token and a refresh_token...

**POST** /oauth/token  
**ContentType:** application/x-www-form-urlencoded  


The key and secret tokens are passed as request body content:  

[]: [AuthenticationController.CreateAccessToken.Request]
```form
grant_type=password&username={key}&password={secret}
```

The device server returns...

[]: [AuthenticationController.CreateAccessToken.Response]
```json
{
  "oAuthToken": {
    "access_token": "2YotnFZFEjr1zCsicMWpAA",
    "token_type": "Bearer",
    "expires_in": "3600",
    "refresh_token": "tGzv3JOkF0XG5Qx2TlKWIA"
  }
}
```


## Authenticating an HTTP request


Below is the header from a typical *authenticated* HTTP request: 
```` 
Request Headers
User-Agent :   
Accept-encoding :   gzip, deflate
Authorization : Bearer 2YotnFZFEjr1zCsicMWpAA
Connection :   Keep-Alive
Content-Type :   application/vnd.imgtec.com.{0}
Host :   xx-xxx.com
````
The first thing to notice is the *Authorization* header which states the access_token value and the token type. These values identify the requesting client to the device server. Any request that’s received by the server with an invalid Authorization header will be rejected with a request status of 401 - unauthorised.

The format for the authorization header is:

**Authorization:** {*token type*} {*access_token*}  

In most cases the token type will be *Bearer*.

Here's a typical response from the device server root API with the correct authentication, now containing information relevant to the authenticated client:  

[]: [APIEntryController.GetEntryPoint.Response]
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
            "rel": "metrics",
            "href": "http://localhost:8080/metrics",
            "type": "application/vnd.imgtec.metrics+json"
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



## The access_token
The access_token is short lived (usually 24 hours), but can be renewed on or before expiry by presenting a valid refresh_token to the device server *authenticate* link in exchange for a new access_token.

## The refresh_token
A refresh_token is used to renew an expired or current access_token. The refresh_token has a long life, often up to two years. A refresh_token can also be renewed before expiry using the device server authenticate API. If the current refresh_token expires without renewal the client must obtain a new one.  

## Replacing an expired access_token
A replacement access_token is acquired by sending a valid refresh_token to the device server *authenticate* API in exchange for a new access_token, and optionally, a new refresh_token.

Beginning with a GET request to the API root URL...

**GET** /  

The device server responds with:

````json

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
````

Then extract the *authenticate* link. Note that the MIME media type associated with this request will be *application/vnd.imgtec.com.oauthtoken*.
 
Generate an HTTP POST request to the *authenticate* link with a valid refresh_token in the request body:

**POST** /oauth/token  
**Content-Type:** application/x-www-form-urlencoded  
**Accept:** application/vnd.imgtec.com.oauthtoken+json

Here's the POST request body content:

[]: [AuthenticationController.CreateAccessToken.Request.2]
```body
grant_type=refresh_token&refresh_token={refresh_token}
``` 

The device server returns:

[]: [AuthenticationController.CreateAccessToken.Response.2]
```json

Response: 201 OK
ContentType: application/vnd.imgtec.com.oauthtoken+json
 
{
       "access_token":"2YotnFZFEjr1zCsicMWpAA",
       "token_type":"Bearer",
       "expires_in":3600,
       "refresh_token":"tGzv3JOkF0XG5Qx2TlKWIA"
}
```
You can see that a replacement access_token with a 24 hour expiry has been returned, along with a new refresh_token.  


## Summary

Here are some key points to take away:

* All device server REST API transactions via HTTP must be authorised using OAuth2 based credentials which are passed in an Authorization header in the HTTP request header section.  
* Key and secret tokens are obtained from the developer console and submitted via HTTP POST request to the *authenticate* link of the REST API in exchange for an access_token and a refresh_token.  
* In general, devices register with the device server using certificates whereas mobile and web applications interface via the REST API using access tokens.  
* An access_token is short lived but can be renewed by POSTing a refresh_token to the REST API *authenticate* endpoint.  
* If a refresh_token expires, new key and secret tokens must be obtained and submitted via HTTP POST request to the *authenticate* link of the REST API.  

----

----
