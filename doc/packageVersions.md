
![](images/img.png)
----

# Device server package versions

*This guide is aimed at engineers who are developing an IoT application that uses the device server REST API.*


## Prerequisites  
To get the most from this document you'll need to be familiar with basics of the device server REST API - see [*Introduction to the device server REST API*](DSRESTindex.md).  

**Note.** For the sake of brevity, any examples given below will not show the full link navigation from API root to the target asset. In practice, it's important *not* to hard code links because the internal architecture of the device server is subject to change. Always take the RESTful approach.

## Introduction

The device server provides an API that returns a list of the component packages that make up the server. Packages are grouped by server build number and the package version is provided.

This is a read only service.

## Example


**GET** /versions  

Returns...

[VersionsController.GetVersions.Response]
```json
{
  "Versions": {
    "Build": "201604191118",
    "Components": {
      "Component": [
        {
          "Name": ".NET framework",
          "Version": "4.6.1"
        },
        {
          "Name": "nginx",
          "Version": "1.9.14"
        },
        {
          "Name": "RabbitMQ",
          "Version": "3.6.1"
        },
        {
          "Name": "MongoDB",
          "Version": "3.2"
        }
      ]
    }
  }
}

```


----

----
