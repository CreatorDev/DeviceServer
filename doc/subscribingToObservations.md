
![](images/img.png)
----

# Observation management

The objective of this article is to describe the different types of change observation, and the correct method of subscribing to changes in observed values and states.

## Prerequisites  
To get the most from this document you'll need to be familiar with basics of the device server REST API - see [*Introduction to the device server REST API*](DSRESTindex.md).  

**Note.** For the sake of brevity, the examples given below do not show the full link navigation from API root to the target asset. In practice, it's important *not* to hard code links because the internal architecture of the device server is subject to change. Always take the RESTful approach.

## Introduction
The ability to keep track of client connectivity and resource value changes in real time without the use of prolonged polling requests is a major benefit provided by the device server which enables a client to subscribe to asynchronous notifications triggered by a range of client, object and resource events.  
In Constrained Application Protocol (CoAP) terms, the act of monitoring a change event is known as *observation*, such that, when a client observes a resource, the client will be notified immediately if the state or value of that resource changes.

## How does it work?

The device server exposes a notification subscription service via its REST API and returns event triggered notifications via HTTP POST request to a supplied web hook URL.

## Which events can be observed?

Observable events fall into three categories:  

1. **Connectivity events**  
    * Client connects  
    * Client disconnects  
    * Client details are updated  
    * Client connection expires or times out  
2. **Object events**  
    * New object instantiated   
    * Object instance resource changes  
3. **Resource value events**  
    * Resource value changes  

Subscribers to the above events will recieve a notification each time the event occurs until such time that the subscription is cancelled.

A typical client event notification identifies the client, the time of the event, and the type of event that has occurred:
```json
{
  "Notifications": {
    "Items": {
      "Notification": {
        "SubscriptionType": "ClientConnected",
        "TimeTriggered": "2016-04-28T17:38:22",
        "Links": [
          {
            "rel": "subscription",
            "href": "subscriptions/4"
          },
          {
            "rel": "client",
            "href": "/clients/{id}"
          }
        ]
      }
    }
  }
}
```

A typical resource observation notification identifies the object instance, the client and the affected resource, also offering the time of the event and the new resource value:
```json
{
  "Notification": {
    "SubscriptionType": "Observation",
    "TimeTriggered": "2016-04-28T17:38:22",
    "Links": [
      {
        "rel": "subscription",
        "href": "subscriptions/4"
      },
      {
        "rel": "client",
        "href": "/clients/{id}"
      },
      {
        "rel": "object",
        "href": "/clients/{id}/objecttypes/{definitionid}/instances/{instanceid}"
      },
      {
        "rel": "definition",
        "href": "/objecttypes/definitions/{definitionid}"
      }
    ],
    "Value": {
      "Location": { "Latitude": "-41.6" }
    }
  }
}
```

## Retrieving a list of current subscriptions

A list of the current subscriptions can be retrieved using an HTTP GET request:

**GET** /subscriptions  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA  
**Accept:** application/vnd.imgtec.subscriptions+json  

Response:  
[]: [SubscriptionsController.GetSubscriptions.Response]

```json
{
    "PageInfo": {
        "TotalCount": 2,
        "ItemsCount": 2,
        "StartIndex": 0
    },
    "Items": [
        {
            "SubscriptionType": "ClientConnected",
            "Url": "https://webhook-url/notification",
            "AcceptContentType": "application/json",
            "Links": [
                {
                    "rel": "self",
                    "href": "http://localhost:8080/subscriptions/R1X6PD8sMEiS8bfzmGk4dA"
                },
                {
                    "rel": "update",
                    "href": "http://localhost:8080/subscriptions/R1X6PD8sMEiS8bfzmGk4dA"
                },
                {
                    "rel": "remove",
                    "href": "http://localhost:8080/subscriptions/R1X6PD8sMEiS8bfzmGk4dA"
                }
            ]
        },
        {
            "SubscriptionType": "Observation",
            "Url": "http://localhost:8000",
            "AcceptContentType": "application/xml",
            "Links": [
                {
                    "rel": "self",
                    "href": "http://localhost:8080/subscriptions/sqKdHWw5UUadn6t5KvrD_g"
                },
                {
                    "rel": "update",
                    "href": "http://localhost:8080/subscriptions/sqKdHWw5UUadn6t5KvrD_g"
                },
                {
                    "rel": "remove",
                    "href": "http://localhost:8080/subscriptions/sqKdHWw5UUadn6t5KvrD_g"
                }
            ]
        }
    ],
    "Links": [
        {
            "rel": "add",
            "href": "http://localhost:8080/subscriptions"
        }
    ]
}
```

A single subscription can also be retrieved using the subscription identifier:  

**GET** /subscriptions/sqKdHWw5UUadn6t5KvrD_g  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA  
**Accept:** application/vnd.imgtec.subscription+json  

Response:  
[]: [SubscriptionsController.GetSubscription.Response]

```json
{
    "SubscriptionType": "Observation",
    "Url": "http://localhost:8000",
    "AcceptContentType": "application/xml",
    "Links": [
        {
            "rel": "self",
            "href": "http://localhost:8080/subscriptions/sqKdHWw5UUadn6t5KvrD_g"
        },
        {
            "rel": "update",
            "href": "http://localhost:8080/subscriptions/sqKdHWw5UUadn6t5KvrD_g"
        },
        {
            "rel": "remove",
            "href": "http://localhost:8080/subscriptions/sqKdHWw5UUadn6t5KvrD_g"
        }
    ]
}
```

## Setting up an observation subscription  

Observations are subscribed to by posting a notification request, MIME type *application/vnd.oma.lwm2m.subscription*, to the *subscriptions* link of the observed property. Subscriptions can be registered at several levels depending on the event that's being observed:  

* **Connectivity events** are observed at topmost level under */subscriptions*  
* **Object events** are observed at the client level under */clients/{id}/subscriptions*  
* **Resource events** are observed at the object level under */clients/{id}/objecttypes/{definitionid}/instances/{id}/subscriptions*  

As an example, to request a notification whenever a *client* connects to the device server, a subscription is posted at the topmost level stating the type of event to be notified, in this case *ClientConnected*, and providing a web hook URL as the target for the notification...

**POST** /subscriptions  
**Content-Type**: application/vnd.oma.lwm2m.subscription+json  

Request body content:

[]: [SubscriptionsController.AddSubscription.Request]
```json
{
    "SubscriptionType": "ClientConnected",
    "Url": "https://webhook-url/notification"  
}
```

The device server responds with an HTTP response code 201 (success) and a *ResourceCreated* MIME data type in the response body specifying the ID of, and a link to, the new subscription...

[]: [SubscriptionsController.AddSubscription.Response]
```json
{
    "ID": "vwAZe5UaXEij17WWAotVyw",
    "Links": [
        {
            "rel": "self",
            "href": "http://localhost:8080/subscriptions/vwAZe5UaXEij17WWAotVyw"
        }
    ]
}
```
A notification will be posted to the stated web hook whenever any client connects to the device server. A complete list of subscription types is tabled below.

Setting up a *resource* observation subscription follows a similar pattern but in this case we're targeting a particular instance of an object on a specific client:

**POST** /clients/{id}/objecttypes/{definitionid}/instances/{id}/subscriptions  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA  
**Content-Type:** application/vnd.oma.lwm2m.subscription+json

Here's the POST request body content:
```json 
{
    "SubscriptionType": "Observation",         
    "AcceptContentType": "application/json",   
    "Property": "Name or ID",                  
    "Url": "https://webhook-url/notification", 
    "Attributes": {
      "Pmin": "5",
      "Pmax": " ",
      "Step": " ",
      "LessThan": " ",
      "GreaterThan": " "
    }
}
```


The *SubscriptionType* element may contain one of the following types:  


| SubscriptionType | description |  
|-----|-----|  
| Observation | Notifies on a resource value change |  
| ClientConnected | Notifies when a client connects to the device server |  
| ClientDisconnected  | Notifies when a client disconnects from the device server |  
| ClientUpdated | Notifies when a client's details change |  
| ClientConnectionExpired | Notifies when a client's device server session expires |  
|||  


The *AcceptContentType* element defines the content format of the notification, currently either *application/json* or *application/xml*. If no *AcceptContentType* is specified, the notification will be of the same type as the subscription setup request.  

The *property* element identifies the resource being observed. The value of *property* may be either the resource ID, or the resource SerialisationName.

There is also an optional *Attributes* object that may be defined to customise the event trigger. The attributes elements are described below:

| Attribute | Description |  
|-----|-----|  
| Pmin | *Minimum period.* The miminum time, in seconds, that the client *must* wait between notifications. If a Resource value has to be notified during the defined 'quiet period', the notification *must* be sent immediately that the quiet period expires. If the Pmin value is not specified the mimimum period is defined by the default minimum period set in the device server.|  
| Pmax | *Maximum period.* The maximum time, in seconds, that the client *may* wait between notifications. When the maximum period expires after the previous notification, a new notification *must* be sent. If Pmax is not specified the maximum period is defined by the default maximum period set in the device server. The maximum period *must not* be smaller than the minimum period. |  
| Step | The Step attribute defines a minimum observed value change between notifications. When the Step value is specified the resource value change condition will occur when the resource value variation, since the previous notification, is equal to, or greater than, the Step attribute value. |  
| LessThan | Defines a low threshhold value for the observed resource. When *LessThan* is specified the LWM2M client *must* notify the server each time the observed value crosses the *LessThan* value. The pmin and pmax rules must still be respected. |  
| GreaterThan | Defines a high threshhold value for the observed resource. When *GreaterThan* is specified the LWM2M client *must* notify the server each time the observed value crosses the *GreaterThan* value. The pmin and pmax rules must still be respected. |  
|||    


A successful subscription returns a *ResourceCreated* response body containing a subscription ID.  

Object events such as an object instance creation, are notified via the *ClientUpdated* subscription which reports the revised object structure of the client. 

## Updating a subscription
A subscription can be updated by sending an HTTP PUT request containing new or partial subscription details to an existing subscription identifier endpoint:

**Note**. Only the webhook URL element and the *Attributes* object can be updated. No other fields may be modified without deleting the subscription and then recreating it.

**PUT** /subscriptions/4  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA  

[]: [SubscriptionsController.UpdateSubscription.Request]
```json
{
  "Url": "https://alternative_webhook-url/notification"
}
```

No response body is returned by an update operation. The success (or otherwise), of the operation is determined from the HTTP response code:

| HTTP response code | Description |  
|-----|-----|  
| 200 | OK |  
| 201 | Created OK |  
| 400 | Bad request - See the BadRequest MIME type in the Tables section. |  
| 401 | Unauthorised |  
| 403 | Not allowed |  
| 404 | Resource not found |  
| 409 | Conflict |  
| 410 | Gone |  
| 500 | Server error |  
| 501 | Not implemented |  
| 503 | Server busy |  
| 507 | Database not available |  


## Cancelling a subscription

An existing subscription is cancelled by sending an HTTP DELETE request to the subscription identifier endpoint:

**DELETE** /subscriptions/4  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA  
  
As with the update process, no response body is returned from a delete operation. Success (or otherwise) is determined from the HTTP response code.


----

----
