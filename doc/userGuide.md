![](images/img.png)
----

# The LWM2M device management server user guide  
The device server is a self-hosted LWM2M device bootstrap/management server with CoAP and REST interfaces. The purpose of the device server is to manage the provisioning, authentication and connectivity of IoT devices, to query and supply resource information, and to service CoAP *Observe* notifications.  
 
![](images/CreatorDev_IoT_framework_device_server_overview_100dpi.png)  
  

The device server acts as a bridge between CoAP/LWM2M on the device network side, and HTTP/REST on the internet side, allowing web and mobile applications to manage device connectivity and to utilise device side properties and resources.

## Contents

* [Introduction](devServer.md)  
* [Installation and deployment](devServerInstallation.md)  
* [The device server REST API](DSRESTindex.md)  
    * [Client authentication](authentication.md)  
	* [Device authentication](devauth.md)  
	* [Response optimisation](ResponseOptimization.md)  
	* [Common operations](commonDeviceServerOperations.md)  
	* [Subscribing to CoAP observations](subscribingToObservations.md)  
	* [Metrics](metrics.md)  
	* [Package versions](packageVersions.md)  

----

----
