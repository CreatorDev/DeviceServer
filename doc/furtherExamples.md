# Further Examples

[]: [AccessKeysController.GetAccessKeys.Response]
[]: [!generateXml]
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

[]: [AccessKeysController.GetAccessKey.Response]
[]: [!generateXml]
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

[]: [AccessKeysController.UpdateAccessKey.Request]
```json
{
    "Name": "NewName"
}
```

[]: [ClientsController.AddObjectInstance.Response]
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

[]: [ClientsController.GetObjectInstance.Response]
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

[]: [SubscriptionsController.GetSubscriptions.Response]
[]: [!generateXml]
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

[]: [SubscriptionsController.GetSubscriptions.Response.2]
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

[]: [SubscriptionsController.GetSubscriptions.Response.3]
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

[]: [SubscriptionsController.GetSubscription.Response]
[]: [!generateXml]
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

[]: [ConfigurationController.GetConfiguration.Response]
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

[]: [ConfigurationController.GetBootstrapConfiguration.Response]
```json
{
    "Url": "coaps://localhost:15684/"
}
```

[]: [IdentitiesController.GetIdentities.Response]
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

[]: [IdentitiesController.GetCertificates.Response]
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

[]: [IdentitiesController.GetPSKIdentities.Response]
[]: [!generateXml]
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

[]: [IdentitiesController.GetPSKIdentity.Response]
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

[]: [ObjectDefinitionsController.AddObjectDefinitions.Request]
[]: [ObjectDefinitionsController.AddObjectDefinitions.Request][application/vnd.imgtec.objectdefinition]
[]: [!generateXml]
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

[]: [ObjectDefinitionsController.AddObjectDefinitions.Response]
[]: [ObjectDefinitionsController.AddObjectDefinitions.Response][application/vnd.imgtec.resourcecreated]
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

[]: [ObjectDefinitionsController.AddObjectDefinitions.Request][application/vnd.imgtec.objectdefinitions]
[]: [!generateXml]
```json
{
  "Items": [
    {
      "ObjectID": "30397",
      "Name": "Device123",
      "MIMEType": "application/vnd.oma.lwm2m.device",
      "SerialisationName": "Device123",
      "Singleton": true,
      "Properties": [
        {
          "PropertyID": "0",
          "Name": "Manufacturer",
          "DataType": "String",
          "IsCollection": false,
          "IsMandatory": false,
          "Access": "Read",
          "SerialisationName": "Manufacturer"
        }
      ]
    }
  ]
}
```

[]: [ObjectDefinitionsController.AddObjectDefinitions.Response][application/vnd.imgtec.resourcescreated]
```json
{
    "Items": [
        {
            "ID": "srQux22fuEiyyjJd3Md30w",
            "Links": [
                {
                    "rel": "self",
                    "href": "http://localhost:8080/objecttypes/definitions/srQux22fuEiyyjJd3Md30w"
                }
            ]
        }
    ]
}
```

[]: [ObjectDefinitionsController.GetObjectDefinition.Response]
[]: [!generateXml]
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

[]: [ObjectDefinitionsController.UpdateObjectDefinition.Request]
[]: [!generateXml]
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
