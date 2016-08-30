
![](img.png)  
---

# Device server metrics  

*This guide is aimed at engineers who are developing an IoT application that uses the device server REST API.*  

The objective of this article is to describe the different metrics offered by the device server and to demonstrate how to access them using the device server REST Api. For more information about the REST API see [the device server user guide](userGuide.md).

## Introduction

The device server keeps track of several performance figures which it makes available for management purposes. These figures are known as metrics, and are designed to give an overall perspective on how much data the device server processes, and by how many users and client devices.  


## Types of metric

There are two types of metric available:  

* Organisation metrics - figures that apply to the device server as a whole  
* Client metrics - figures that apply to a particular client within the device server's organisation  

Metrics are *read only* and are collated in real time. They are accessed via HTTP GET request and can be retrieved as a collection or by individual value.  

### Organisation metrics

Here's an example of a GET request to retrieve the full set of organisation metrics:

**GET** /metrics  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA  
**Accept:** application/vnd.imgtec.metrics+json  

Response:  
```json
{
    "PageInfo": {
        "TotalCount": 4,
        "ItemsCount": 4,
        "StartIndex": 0
    },
    "Items": [
        {
            "Name": "BytesSent",
            "Value": 7820,
            "Links": [
                {
                    "rel": "self",
                    "href": "http://localhost:8080/metrics/BytesSent"
                }
            ]
        },
        {
            "Name": "TransactionCount",
            "Value": 470,
            "Links": [
                {
                    "rel": "self",
                    "href": "http://localhost:8080/metrics/TransactionCount"
                }
            ]
        },
        {
            "Name": "BytesReceived",
            "Value": 22970,
            "Links": [
                {
                    "rel": "self",
                    "href": "http://localhost:8080/metrics/BytesReceived"
                }
            ]
        },
        {
            "Name": "NumberClients",
            "Value": 1,
            "Links": [
                {
                    "rel": "self",
                    "href": "http://localhost:8080/metrics/NumberClients"
                }
            ]
        }
    ]
}
```

It can be seen from the above example that the device server provides overall values for; the number of bytes sent and recieved, the number of transactions processed and the number of clients currently connected.  

To retrieve a single organisation metric the *metricID* string needs to be known. These can be seen in the above example and are listed below:

| metricID | Description |  
|-----|-----|  
| BytesSent | The overall number of bytes sent by the server |  
| BytesReceived | The overall number of bytes recieved by the server |  
| TransactionCount | The overall number of transactions processed by the server |  
| NumberClients | The number of clients *currently connected* to the server |  

Here's an example:

**GET** /metrics/BytesSent  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA  
**Accept:** application/vnd.imgtec.metric+json  

Response:

```json
{
    "Name": "BytesSent",
    "Value": 7820,
    "Links": [
        {
            "rel": "self",
            "href": "http://localhost:8080/metrics/BytesSent"
        }
    ]
}
```

### Client metrics

The device server provides similar metrics collated by client, with the exception of course of the *NumberClients* metric. 

Client metrics have the same accept data type as their organisation equivalents.

To retrieve a client metric, the clientID must be known. We'll start with a request for the full client metrics collection:


**GET** /clients/{clientID}/metrics  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA  
**Accept:** application/vnd.imgtec.metrics+json  

Response:

[]: [MetricsController.GetMetrics.Response]
```json
{
    "PageInfo": {
        "TotalCount": 3,
        "ItemsCount": 3,
        "StartIndex": 0
    },
    "Items": [
        {
            "Name": "BytesSent",
            "Value": 1334,
            "Links": [
                {
                    "rel": "self",
                    "href": "http://localhost:8080/clients/oFIrQFrW8EWcZ5u7eGfrkw/metrics/BytesSent"
                }
            ]
        },
        {
            "Name": "TransactionCount",
            "Value": 81,
            "Links": [
                {
                    "rel": "self",
                    "href": "http://localhost:8080/clients/oFIrQFrW8EWcZ5u7eGfrkw/metrics/TransactionCount"
                }
            ]
        },
        {
            "Name": "BytesReceived",
            "Value": 8336,
            "Links": [
                {
                    "rel": "self",
                    "href": "http://localhost:8080/clients/oFIrQFrW8EWcZ5u7eGfrkw/metrics/BytesReceived"
                }
            ]
        }
    ]
}
```

As with organisation metrics, client metrics can also be retrieved individually...

**GET** /clients/{clientID}/metrics/TransactionCount  
**Authorization:** Bearer 2YotnFZFEjr1zCsicMWpAA  
**Accept:** application/vnd.imgtec.metric+json  

Response:

[]: [MetricsController.GetMetric.Response]
```json
{
    "Name": "TransactionCount",
    "Value": 81,
    "Links": [
        {
            "rel": "self",
            "href": "http://localhost:8080/clients/oFIrQFrW8EWcZ5u7eGfrkw/metrics/TransactionCount"
        }
    ]
}
```

### Summary

* Management statistics (referred to a metrics), are collected in real time by the device server and are made available available for management purposes via the REST API.  
* Metrics are available at organisation level and at client level.  
* Metrics of either level can be retrieved as a complete collection, or by individual metric.  

----

----

