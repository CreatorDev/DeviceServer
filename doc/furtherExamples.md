
AccessKeysController.GetAccessKeys Json response []
-----------------------------------------------------  
```json
{
    "PageInfo": {
        "TotalCount": 3,
        "ItemsCount": 3,
        "StartIndex": 0
    },
    "Items": [
        {
            "Name": "Test Key",
            "Key": "8H8QhabSbtoRy9GK776u_k7cj3J0ozy322LQQRrh_hsozgAAIQBxvylxmVuzrc90S8c2_Vf_oTrKFnM58oVoxA",
            "Links": [
                {
                    "rel": "self",
                    "href": "http://localhost:8080/accesskeys/8H8QhabSbtoRy9GK776u_k7cj3J0ozy322LQQRrh_hsozgAAIQBxvylxmVuzrc90S8c2_Vf_oTrKFnM58oVoxA"
                },
                {
                    "rel": "update",
                    "href": "http://localhost:8080/accesskeys/8H8QhabSbtoRy9GK776u_k7cj3J0ozy322LQQRrh_hsozgAAIQBxvylxmVuzrc90S8c2_Vf_oTrKFnM58oVoxA"
                },
                {
                    "rel": "remove",
                    "href": "http://localhost:8080/accesskeys/8H8QhabSbtoRy9GK776u_k7cj3J0ozy322LQQRrh_hsozgAAIQBxvylxmVuzrc90S8c2_Vf_oTrKFnM58oVoxA"
                }
            ]
        },
        {
            "Name": "RolandsKey",
            "Key": "HEiJlbaHV83boSqTHYOvJb0Zx4Vxd9fNZ-Fgxklobxw-7pFtbzFwKZBCC5DCFKoerf-R6pZGTaKLHMnVNpyd4A",
            "Links": [
                {
                    "rel": "self",
                    "href": "http://localhost:8080/accesskeys/HEiJlbaHV83boSqTHYOvJb0Zx4Vxd9fNZ-Fgxklobxw-7pFtbzFwKZBCC5DCFKoerf-R6pZGTaKLHMnVNpyd4A"
                },
                {
                    "rel": "update",
                    "href": "http://localhost:8080/accesskeys/HEiJlbaHV83boSqTHYOvJb0Zx4Vxd9fNZ-Fgxklobxw-7pFtbzFwKZBCC5DCFKoerf-R6pZGTaKLHMnVNpyd4A"
                },
                {
                    "rel": "remove",
                    "href": "http://localhost:8080/accesskeys/HEiJlbaHV83boSqTHYOvJb0Zx4Vxd9fNZ-Fgxklobxw-7pFtbzFwKZBCC5DCFKoerf-R6pZGTaKLHMnVNpyd4A"
                }
            ]
        },
        {
            "Name": "RolandsKey123",
            "Key": "gy-5joCz-kHn1ZsPM04TmK8sg6aJ4Nm2jWzubl8VTyFpnv1d3YU4dmtEeUOk1ds9sP8WCTHEoXIUJafMkPwbPQ",
            "Links": [
                {
                    "rel": "self",
                    "href": "http://localhost:8080/accesskeys/gy-5joCz-kHn1ZsPM04TmK8sg6aJ4Nm2jWzubl8VTyFpnv1d3YU4dmtEeUOk1ds9sP8WCTHEoXIUJafMkPwbPQ"
                },
                {
                    "rel": "update",
                    "href": "http://localhost:8080/accesskeys/gy-5joCz-kHn1ZsPM04TmK8sg6aJ4Nm2jWzubl8VTyFpnv1d3YU4dmtEeUOk1ds9sP8WCTHEoXIUJafMkPwbPQ"
                },
                {
                    "rel": "remove",
                    "href": "http://localhost:8080/accesskeys/gy-5joCz-kHn1ZsPM04TmK8sg6aJ4Nm2jWzubl8VTyFpnv1d3YU4dmtEeUOk1ds9sP8WCTHEoXIUJafMkPwbPQ"
                }
            ]
        }
    ],
    "Links": [
        {
            "rel": "add",
            "href": "http://localhost:8080/accesskeys"
        }
    ]
}
```

AccessKeysController.GetAccessKey Json response []
-----------------------------------------------------  
```json
{
    "Name": "Test Key",
    "Key": "8H8QhabSbtoRy9GK776u_k7cj3J0ozy322LQQRrh_hsozgAAIQBxvylxmVuzrc90S8c2_Vf_oTrKFnM58oVoxA",
    "Links": [
        {
            "rel": "self",
            "href": "http://localhost:8080/accesskeys/8H8QhabSbtoRy9GK776u_k7cj3J0ozy322LQQRrh_hsozgAAIQBxvylxmVuzrc90S8c2_Vf_oTrKFnM58oVoxA"
        },
        {
            "rel": "update",
            "href": "http://localhost:8080/accesskeys/8H8QhabSbtoRy9GK776u_k7cj3J0ozy322LQQRrh_hsozgAAIQBxvylxmVuzrc90S8c2_Vf_oTrKFnM58oVoxA"
        },
        {
            "rel": "remove",
            "href": "http://localhost:8080/accesskeys/8H8QhabSbtoRy9GK776u_k7cj3J0ozy322LQQRrh_hsozgAAIQBxvylxmVuzrc90S8c2_Vf_oTrKFnM58oVoxA"
        }
    ]
}
```

AccessKeysController.UpdateAccessKey Json request []
-----------------------------------------------------
```json
{
    "Name": "NewName"
}
```

ClientsController.GetClient Json response []
-----------------------------------------------------
```json
{
    "Name": "testROLANDB-HP",
    "Links": [
        {
            "rel": "self",
            "href": "http://localhost:8080/clients/oFIrQFrW8EWcZ5u7eGfrkw"
        },
        {
            "rel": "remove",
            "href": "http://localhost:8080/clients/oFIrQFrW8EWcZ5u7eGfrkw"
        },
        {
            "rel": "objecttypes",
            "href": "http://localhost:8080/clients/oFIrQFrW8EWcZ5u7eGfrkw/objecttypes",
            "type": "application/vnd.imgtec.objecttypes+json"
        },
        {
            "rel": "subscriptions",
            "href": "http://localhost:8080/clients/oFIrQFrW8EWcZ5u7eGfrkw/subscriptions",
            "type": "application/vnd.imgtec.subscriptions+json"
        },
        {
            "rel": "metrics",
            "href": "http://localhost:8080/clients/oFIrQFrW8EWcZ5u7eGfrkw/metrics",
            "type": "application/vnd.imgtec.metrics+json"
        }
    ]
}
```

ClientsController.GetObjectType Json response []
-----------------------------------------------------
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

ClientsController.AddObjectInstance Json response []
-----------------------------------------------------
```json
{
  "Links" : [
        {
            "rel": "self",
            "href": "http://localhost:8080/clients/oFIrQFrW8EWcZ5u7eGfrkw/objecttypes/qvwis3pLWECc9oe3tWW_ng/instances/1"
        },
        {
            "rel": "update",
            "href": "http://localhost:8080/clients/oFIrQFrW8EWcZ5u7eGfrkw/objecttypes/qvwis3pLWECc9oe3tWW_ng/instances/1"
        },
        {
            "rel": "remove",
            "href": "http://localhost:8080/clients/oFIrQFrW8EWcZ5u7eGfrkw/objecttypes/qvwis3pLWECc9oe3tWW_ng/instances/1"
        },
        {
            "rel": "subscriptions",
            "href": "http://localhost:8080/clients/oFIrQFrW8EWcZ5u7eGfrkw/objecttypes/qvwis3pLWECc9oe3tWW_ng/instances/1/subscriptions"
        },
        {
            "rel": "definition",
            "href": "http://localhost:8080/objecttypes/definitions/qvwis3pLWECc9oe3tWW_ng"
        }
  ],
  "ID":"5"
}
```

ClientsController.GetObjectInstance Json response []
-----------------------------------------------------
```json
{
    "Links": [
        {
            "rel": "self",
            "href": "http://localhost:8080/clients/oFIrQFrW8EWcZ5u7eGfrkw/objecttypes/kjRafR5BX0CvBdcGWHWuiQ/instances/0"
        },
        {
            "rel": "update",
            "href": "http://localhost:8080/clients/oFIrQFrW8EWcZ5u7eGfrkw/objecttypes/kjRafR5BX0CvBdcGWHWuiQ/instances/0"
        },
        {
            "rel": "remove",
            "href": "http://localhost:8080/clients/oFIrQFrW8EWcZ5u7eGfrkw/objecttypes/kjRafR5BX0CvBdcGWHWuiQ/instances/0"
        },
        {
            "rel": "subscriptions",
            "href": "http://localhost:8080/clients/oFIrQFrW8EWcZ5u7eGfrkw/objecttypes/kjRafR5BX0CvBdcGWHWuiQ/instances/0/subscriptions"
        },
        {
            "rel": "definition",
            "href": "http://localhost:8080/objecttypes/definitions/kjRafR5BX0CvBdcGWHWuiQ"
        }
    ],
    "InstanceID": "0",
    "Manufacturer": "Open Mobile Alliance",
    "ModelNumber": "Lightweight M2M Client",
    "SerialNumber": "345000123",
    "FirmwareVersion": "1.0",
    "AvailablePowerSources": [
        5,
        1
    ],
    "PowerSourceVoltages": [
        5000,
        3800
    ],
    "PowerSourceCurrents": [
        5000,
        3800
    ],
    "BatteryLevel": 100,
    "MemoryFree": 15,
    "ErrorCodes": [
        0
    ],
    "CurrentTime": "2016-07-07T03:43:20Z",
    "UTCOffset": "+12:00",
    "SupportedBindingandModes": "U"
}
```

SubscriptionsController.GetSubscriptions Json response [] (/subscriptions)
-----------------------------------------------------
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

SubscriptionsController.GetSubscriptions Json response [] (/clients/{clientID}/subscriptions)
-----------------------------------------------------
```json
{
    "PageInfo": {
        "TotalCount": 1,
        "ItemsCount": 1,
        "StartIndex": 0
    },
    "Items": [
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
            "href": "http://localhost:8080/clients/oFIrQFrW8EWcZ5u7eGfrkw/subscriptions"
        }
    ]
}
```

SubscriptionsController.GetSubscriptions Json response [] (/clients/{clientID}/objectTypes/{objectTypeID}/instances/{instanceID}/subscriptions)
-----------------------------------------------------
```json
{
    "PageInfo": {
        "TotalCount": 1,
        "ItemsCount": 1,
        "StartIndex": 0
    },
    "Items": [
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
            "href": "http://localhost:8080/clients/oFIrQFrW8EWcZ5u7eGfrkw/objecttypes/kjRafR5BX0CvBdcGWHWuiQ/instances/0/subscriptions"
        }
    ]
}
```

SubscriptionsController.GetSubscription Json response []
-----------------------------------------------------
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

ConfigurationController.GetConfiguration Json response []
-----------------------------------------------------
```json
{
    "Links": [
        {
            "rel": "bootstrap",
            "href": "http://localhost:8080/configuration/bootstrap",
            "type": "application/vnd.imgtec.bootstrap+json"
        }
    ]
}
```

ConfigurationController.GetBootstrapConfiguration Json response []
-----------------------------------------------------
```json
{
    "Url": "coaps://localhost:15684/"
}
```

IdentitiesController.GetIdentities Json response []
-----------------------------------------------------
```json
{
    "Links": [
        {
            "rel": "psk",
            "href": "http://localhost:8080/identities/psk",
            "type": "application/vnd.imgtec.pskidentities+json"
        },
        {
            "rel": "certificate",
            "href": "http://localhost:8080/identities/certificates",
            "type": "application/vnd.imgtec.certificates+json"
        }
    ]
}
```

IdentitiesController.GetCertificates Json response []
-----------------------------------------------------
```json
{
    "Links": [
        {
            "rel": "add",
            "href": "http://localhost:8080/identities/certificates"
        }
    ]
}
```

IdentitiesController.GetPSKIdentities Json response []
-----------------------------------------------------
```json
{
    "PageInfo": {
        "TotalCount": 1,
        "ItemsCount": 1,
        "StartIndex": 0
    },
    "Items": [
        {
            "Identity": "oFIrQFrW8EWcZ5u7eGfrkw",
            "Links": [
                {
                    "rel": "self",
                    "href": "http://localhost:8080/identities/psk/oFIrQFrW8EWcZ5u7eGfrkw"
                },
                {
                    "rel": "remove",
                    "href": "http://localhost:8080/identities/psk/oFIrQFrW8EWcZ5u7eGfrkw"
                }
            ]
        }
    ],
    "Links": [
        {
            "rel": "add",
            "href": "http://localhost:8080/identities/psk"
        }
    ]
}
```

IdentitiesController.GetPSKIdentity Json response []
-----------------------------------------------------
```json
{
    "Identity": "oFIrQFrW8EWcZ5u7eGfrkw",
    "Links": [
        {
            "rel": "self",
            "href": "http://localhost:8080/identities/psk/oFIrQFrW8EWcZ5u7eGfrkw"
        },
        {
            "rel": "remove",
            "href": "http://localhost:8080/identities/psk/oFIrQFrW8EWcZ5u7eGfrkw"
        }
    ]
}
```

MetricsController.GetMetrics Json response []   (/metrics)
-----------------------------------------------------
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

MetricsController.GetMetric Json response []   (/metrics/{metricID})
-----------------------------------------------------
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

MetricsController.GetMetrics Json response  [] (/clients/{clientID}/metrics)
-----------------------------------------------------
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

MetricsController.GetMetric Json response [] (/clients/{clientID}/metrics/metricID)
-----------------------------------------------------
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

ObjectDefinitionsController.AddObjectDefinitions Json request [] (Single definition)  NB application/vnd.imgtec.objectdefinition+json
-----------------------------------------------------
```json
{
    "ObjectID": "50234",
    "Name": "DemoObject",
    "MIMEType": "application/vnd.oma.lwm2m.ext.demoobject",
    "SerialisationName": "DemoObject",
    "Singleton": true,
    "Properties": [
        {
            "PropertyID": "0",
            "Name": "Button1State",
            "DataType": "Integer",
            "IsCollection": false,
            "IsMandatory": false,
            "Access": "Read",
            "SerialisationName": "Button1State"
        }
    ]
}
```
ObjectDefinitionsController.AddObjectDefinitions Json response [] (Single definition)
-----------------------------------------------------
```json
{
    "ID": "Nws4ePaG2U-CewSxDEZT0A",
    "Links": [
        {
            "rel": "self",
            "href": "http://localhost:8080/objecttypes/definitions/Nws4ePaG2U-CewSxDEZT0A"
        }
    ]
}
```
ObjectDefinitionsController.AddObjectDefinitions Json request [] (collection)  NB application/vnd.imgtec.objectdefinitions+json
-----------------------------------------------------
ObjectDefinitionsController.AddObjectDefinitions Json response [] (collection)
-----------------------------------------------------
ObjectDefinitionsController.GetObjectDefinition Json response []
-----------------------------------------------------
```json
{
    "ObjectDefinitionID": "Nws4ePaG2U-CewSxDEZT0A",
    "ObjectID": "50234",
    "Name": "DemoObject",
    "MIMEType": "application/vnd.oma.lwm2m.ext.demoobject",
    "SerialisationName": "DemoObject",
    "Singleton": true,
    "Properties": [
        {
            "PropertyDefinitionID": "fI1EdLZHKEeFnnEVaX1KGQ",
            "PropertyID": "0",
            "Name": "Button1State",
            "DataType": "Integer",
            "IsCollection": false,
            "IsMandatory": false,
            "Access": "Read",
            "SerialisationName": "Button1State"
        }
    ],
    "Links": [
        {
            "rel": "self",
            "href": "http://localhost:8080/objecttypes/definitions/Nws4ePaG2U-CewSxDEZT0A"
        },
        {
            "rel": "update",
            "href": "http://localhost:8080/objecttypes/definitions/Nws4ePaG2U-CewSxDEZT0A"
        },
        {
            "rel": "remove",
            "href": "http://localhost:8080/objecttypes/definitions/Nws4ePaG2U-CewSxDEZT0A"
        }
    ]
}
```
ObjectDefinitionsController.UpdateObjectDefinition Json request []
-----------------------------------------------------
```json
{
    "ObjectID": "50234",
    "Name": "DemoObject1",
    "MIMEType": "application/vnd.oma.lwm2m.ext.demoobject",
    "SerialisationName": "DemoObject1",
    "Singleton": true,
    "Properties": [
        {
            "PropertyID": "0",
            "Name": "Button1State",
            "DataType": "Integer",
            "IsCollection": false,
            "IsMandatory": false,
            "Access": "Read",
            "SerialisationName": "Button1State"
        }
    ]
}
```

---
