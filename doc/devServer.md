![](images/img.png)
----


## The device server  

The device server is a LWM2M management server designed to be implemented alongside third party cloud services to integrate M2M capability into an IoT application. The device server exposes two secure interfaces; REST API/HTTPs and LWM2M/CoAP.  


![](images/Awa_LWM2M_device_server_positioning_150dpi.png)


The device server interfaces securely with LWM2M device networks via the Constrained Application Protocol (CoAP) and aids device and application interoperability by supporting both IPSO registered smart object definitions and custom object definitions.  

Device management is enabled through the implementation of the Open Mobile Alliance LWM2M standard. The CoAP interface and all LWM2M functionality is abstracted by the device server libraries, so intimate knowledge of LWM2M and CoAP is not required.  

Web and mobile applications interface with the device server via an authenticated REST API with a single entry point URL.

Since LWM2M relies on CoAP for communications, the device server acts as a bridge between the CoAP and HTTP protocols, allowing devices and applications which are outside the LWM2M device network to query resource and connectivity data using HTTP via the device server's REST API. No resource states are cached by the server, so all resource queries are propagated via CoAP directly to the targeted device/s. The device server does cache client connectivity status however, so this data is returned directly.  


![](images/Device_server_process_descriptions_100dpi.png)


The CoAP *Observe* verb is supported by registering (subscribing to) an observation with the device server which will POST a notification to a specified web hook when the value or state of the observed resource meets the desired criteria.


![](images/Device_server_process_descriptions_subscribe_100dpi.png)


Because observation subscriptions are also registered with the relevant LWM2M client, it's clear from the above model that scalability issues could occur in cases where a great many subscriptions are made to the same resource if the subscribed client is severely memory constrained (effectively the device will have all its application memory commandeered by subscription registrations). To counter this possibility only one resource subscription of a given subscription type is registered with the LWM2M client. Multiple subscriptions of the same type and to the same resource will be collated and brokered by the device server.

----

----



