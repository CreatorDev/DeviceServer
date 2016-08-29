
![](images/img.png)
----
## Device authentication

### Introduction

Client connection to the device server CoAP interface requires that the client possesses a valid device certificate or a pre-shared key (PSK). 

On installation the device server database will contain admin key and secret tokens used to generate access keys and device certificates which will be associated with your organisation.


### Obtaining device certificates
Having logged into the device server using the admin credentials, certificates are acquired from the *identities/certificates* endpoint of the device server REST API.
 
No POST request content is required for this operation.

The returned certificate is associated with the device server's owner organisation: 

**POST** /identities/certificates
  
Response:

[]: [IdentitiesController.AddCertificate.Response]
```json
  
{
    "CertificateFormat":"PEM",
    "RawCertificate:":"
-----BEGIN RSA PRIVATE KEY-----
MIIBOwIBAAJBAKumg5Xw6meJdICQqDgLwPuc5m64RjaGRPZp1dQr5SfZC/aeC8hS
Fh10QQBxTDZ0sZO4hluYs/ySyfqsGxSEBW8CAwEAAQJBAJNa9lmRIVnS6Z25EYBT
rs8SCcwBC92g9LDpeHSVXjsOtDE97gniuabqLp8roTcgUe2lpqrGniCe2YzQBh0M
JlkCIQDWdpX30tsRfDNFzwwqmtgEweuLllZmt7E8oNs83suSqwIhAMzlNIXzqyzB
YQ/44nvLWuRsYyUA9g3g/FDWx9KPmrhNAiEAkPvQPJ62bKarPulPD0po+SZj55T1
yjEqx5LzQ75Wn0sCIFf4iOaCSho7JNtgto0EMJAmYSMAGKS41lw2+zBSa9F5AiA1
2Nmm9gamFLhJ/fwpnzSM3sYisLuDtSboZQbE7vCMpw==
-----END RSA PRIVATE KEY-----
-----BEGIN CERTIFICATE-----
MIIBbDCCARagAwIBAgIJAJBmLEGbhpXsMA0GCSqGSIb3DQEBBQUAMBExDzANBgNV
BAMTBndpZmlyZTAeFw0xNjA0MTAyMzU5MjRaFw00MzA4MjcyMzU5MjRaMBExDzAN
BgNVBAMTBndpZmlyZTBcMA0GCSqGSIb3DQEBAQUAA0sAMEgCQQCrpoOV8OpniXSA
kKg4C8D7nOZuuEY2hkT2adXUK+Un2Qv2ngvIUhYddEEAcUw2dLGTuIZbmLP8ksn6
rBsUhAVvAgMBAAGjUTBPMAsGA1UdDwQEAwIEMDATBgNVHSUEDDAKBggrBgEFBQcD
ATArBgNVHREEJDAiggl3aWZpcmVfZWaCCXdpZmlyZV9lY4cEwKgBGYcEwKgBGTAN
BgkqhkiG9w0BAQUFAANBAJGVejII4UMb+OjCI5dyMfimrFBiUiQcomZfdW09mlRP
k6c7X+axbhXpZ6Zi6YuJgDiT+cfFH8cDjO7V0+JpuU0=
-----END CERTIFICATE-----"
}

```


**Note.** A certificate must be generated *for each device* that will connect to the device server. Each device certificate has a uniquely generated GUID included in it which will serve to identify the device. The certificate must be embedded into the device's application code or may be recorded by the device as part of its configuration process.  


### Authentication via pre-shared key (PSK)

The use of PSKs is also supported as an alternative to device certificates. 
Having logged into the device server using the admin credentials, PSKs are acquired from the */identities/psk* endpoint of the device server REST API.

No POST request content is required for this operation.
 
The returned PSK is associated with the device server's owner organisation: 

**POST** /identities/psk

returns:

[]: [IdentitiesController.AddPSKIdentity.Response]
```json
{
    "Identity": "PwjWTw_TyUaVEEGHgtnzJA",
    "Secret": "3ACF33D72D32629BED85F383D7D8F82C48CD33819E0D114C3BC27439761920DB",
    "Links": [
        {
            "rel": "self",
            "href": "http://delmet-hp:8080/identities/psk/PwjWTw_TyUaVEEGHgtnzJA"
        },
        {
            "rel": "remove",
            "href": "http://delmet-hp:8080/identities/psk/PwjWTw_TyUaVEEGHgtnzJA"
        }
    ]
}
```

As with the certificate, the PSK is unique to the device and should be embedded into the device's application code.


### The bootstrap sequence

A bootstrap server is a trusted server that accepts connections from a device, validates the device's authentication credentials and then redirects the device to a suitable management server. Although not essential, the use of a bootstrap server allows greater flexibility when more than one management/device server is involved in an IoT application.
The bootstrapping process follows the same sequence regardless of whether certificates or PSKs are used.

![](images/Bootstrap_server_process_descriptions_100dpi.png)

In larger and more widespread applications there may be several geographically distant device servers available. The bootstrap server may then have the additional purpose of selecting a suitable LWM2M device server based on location. This would typically involve a higher level PSK or cetrificate management function which coordinates authentication credentials between the bootstrap and device servers.

![](images/Bootstrap_server_process_descriptions_with_management_layer_100dpi.png)

**Note.** The identity and server management layer is not provided with the Creator IoT framework.

### Summary

Here are some key points to take away:

* All client CoAP transactions must be authorised using validated certificate or PSK based credentials.  
* PSKs and/or certificates are obtained using administrative rights via HTTP POST request to the *identities* endpoint of the REST API.  
* A trusted intermediate bootstrap server may be used to validate PSKs and certificates and to redirect devices to their appropriate device management server.
* All PSKs and certificates are unique to the organisation and to the device. A PSK or certificate must therefore be generated for each device. 


----

----



