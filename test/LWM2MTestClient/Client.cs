/***********************************************************************************************************************
 Copyright (c) 2016, Imagination Technologies Limited and/or its affiliated group companies.
 All rights reserved.

 Redistribution and use in source and binary forms, with or without modification, are permitted provided that the
 following conditions are met:
     1. Redistributions of source code must retain the above copyright notice, this list of conditions and the
        following disclaimer.
     2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the
        following disclaimer in the documentation and/or other materials provided with the distribution.
     3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote
        products derived from this software without specific prior written permission.

 THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
 DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
 SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
 WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE 
 USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
***********************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoAP;
using CoAP.Net;
using CoAP.Server;
using CoAP.Server.Resources;
using Imagination.LWM2M.Resources;
using System.IO;
using System.Security.Cryptography;

namespace Imagination.LWM2M
{
	public class Client : IMessageDeliverer
	{
        private const string RESOURCES_CACHE = "ResourcesCache";
		private IMessageDeliverer _MessageDeliverer;
        private FlowClientChannel _Channel;
		private CoAPEndPoint _EndPoint;
        private CoapServer _CoapServer = new CoapServer();
		private IResource _Root;
        private SecurityResources _SecurityResources = new SecurityResources();
		private ServerResources _ServerResources = new ServerResources();
		private BootsrapCompleteResource _BootsrapComplete = new BootsrapCompleteResource();
		private System.Timers.Timer _Timer = new System.Timers.Timer();
		private System.Net.EndPoint _ServerEndPoint;
		private string _Location;

        public string ClientID { get; private set; }

        public Client(int port)
		{
			_MessageDeliverer = _CoapServer.MessageDeliverer;
			_CoapServer.MessageDeliverer = this;
			//_EndPoint = new CoAPEndPoint(port, CoapConfig.Default);

            //_EndPoint = new CoAPEndPoint(new FlowClientSecureChannel(port), CoapConfig.Default);
            //_EndPoint = new CoAPEndPoint(new FlowChannel(port), CoapConfig.Default);

            _Channel = new FlowClientChannel(port);

            UseCertificateFile("Client.pem");

            _EndPoint = new CoAPEndPoint(_Channel, CoapConfig.Default);
            _CoapServer.AddEndPoint(_EndPoint);
			_CoapServer.Add(_SecurityResources);
			_CoapServer.Add(_ServerResources);
			_CoapServer.Add(_BootsrapComplete);
			_Root = _BootsrapComplete.Parent;
			_Timer.Interval = 30000;
			_Timer.Elapsed += new System.Timers.ElapsedEventHandler(_Timer_Elapsed);
		}

        public void UsePSK(string identity, string secret)
        {
            _Channel.CertificateFile = null;
            _Channel.PSKIdentity = identity;
            _Channel.PSKSecret = secret;
            Console.WriteLine("Using PSK");
            Console.WriteLine(identity);
            Console.WriteLine(secret);
        }

        public void UseCertificateFile(string filename)
        {
            _Channel.CertificateFile = filename;
            _Channel.PSKIdentity = null;
            _Channel.PSKSecret = null;
            Console.WriteLine("Using Certificate: " + filename);
        }

        public void AddResources(IResource resource)
		{
			if ((resource != _SecurityResources) && (resource != _ServerResources))
				_CoapServer.Add(resource);
		}

		//System.Net.IPAddress.Parse("fe80::18be:e89d:e85f:278%12") //15685
		public bool Bootstrap(string url)
		{
			bool result = false;
			_Timer.Stop();
			Uri uri = new Uri(url);
			foreach (System.Net.IPAddress address in System.Net.Dns.GetHostAddresses(uri.DnsSafeHost))
			{
				CoapClient coapClient = new CoapClient();
				coapClient.EndPoint = _EndPoint;
				Request request = new Request(Method.POST);
				int port = 5683;
				if (string.Compare(uri.Scheme, "coaps", true) ==0)
					port = 5684;
				if (uri.Port > 0)
					port = uri.Port;
				request.Destination = new System.Net.IPEndPoint(address, port);
				request.UriPath = "/bs";
				request.UriQuery = "ep=test";
				Response response = coapClient.Send(request);
				if (response != null && response.StatusCode == StatusCode.Changed)
				{
					result = _BootsrapComplete.Wait(30000);
					break;
				}
			}	



			return result;
		}


		public bool ConnectToServer()
		{
			bool result = false;
			//now register to server
			foreach (SecurityResource item in _SecurityResources.Children)
			{
				Console.Write(item.ServerURI);
				Console.Write(") .. ");
				Uri uri = new Uri(item.ServerURI);
				foreach (System.Net.IPAddress address in System.Net.Dns.GetHostAddresses(uri.DnsSafeHost))
				{
                    bool secure =false;
                    int port = 5683;
                    if (string.Compare(uri.Scheme, "coaps", true) == 0)
                    {
                        secure = true;
                        port = 5684;
                    }
                    if (uri.Port > 0)
                        port = uri.Port;
                    if (_Channel.Secure != secure)
                    {
                        _Channel.Stop();
                        _Channel.Secure = secure;
                        _Channel.Start();
                    }
					CoapClient coapClient = new CoapClient();
                    coapClient.EndPoint = _EndPoint;
					Request request = new Request(Method.POST);
					request.Destination = new System.Net.IPEndPoint(address, port);
					request.UriPath = "/rd";
					request.UriQuery = string.Concat("ep=test",Environment.MachineName,"&lt=35");
					StringBuilder payLoad = new StringBuilder();
					foreach (IResource objectType in _Root.Children)
					{
						if (objectType.Visible)
						{
							bool instances = false;
							foreach (IResource instance in objectType.Children)
							{
								if (payLoad.Length > 0)
									payLoad.Append(',');
								payLoad.Append('<');
								payLoad.Append('/');
								payLoad.Append(objectType.Name);
								payLoad.Append('/');
								payLoad.Append(instance.Name);
								payLoad.Append('>');
								instances = true;
							}
							if (!instances)
							{
								if (payLoad.Length > 0)
									payLoad.Append(',');
								payLoad.Append('<');
								payLoad.Append('/');
								payLoad.Append(objectType.Name);
								payLoad.Append('>');
							}
						}
					}
					request.PayloadString = payLoad.ToString();
					request.ContentType = (int)MediaType.ApplicationLinkFormat;
					Response response = coapClient.Send(request);
					if (response != null && response.StatusCode == StatusCode.Created)
					{
						string location = response.LocationPath;

                        int position = location.LastIndexOf('/');
                        if (position > 0)
                        {
                            ClientID = location.Substring(position + 1);
                        }

						result = true;
						_ServerEndPoint = request.Destination;
						_Location = location;
						_Timer.Start();
						break;
					}
				}
			}
			return result;
		}

        public bool ConnectToServer(string serverURI)
        {
            bool result = false;
            SecurityResource securityResource;
            if (_SecurityResources.Children.Count() > 0)
            {
                securityResource = _SecurityResources.Children.First() as SecurityResource;            
            }
            else
            {
                securityResource = new SecurityResource("1");
                _SecurityResources.Add(securityResource);
            }
            securityResource.ServerURI = serverURI;
            if (serverURI.StartsWith("coaps"))
            {
                if (!string.IsNullOrEmpty(_Channel.PSKIdentity))
                {
                    securityResource.SecurityMode = TSecurityMode.PreSharedKey;
                    securityResource.ClientPublicKey = Encoding.UTF8.GetBytes(_Channel.PSKIdentity);
                    securityResource.SecretKey = StringUtils.HexStringToByteArray(_Channel.PSKSecret);
                }
                else if (!string.IsNullOrEmpty(_Channel.CertificateFile))
                {
                    securityResource.SecurityMode = TSecurityMode.Certificate;
                }
            }
            else
            {
                securityResource.SecurityMode = TSecurityMode.NoSecurity;
            }
            result = ConnectToServer();
            return result;
        }


		void IMessageDeliverer.DeliverRequest(Exchange exchange)
		{
			Request request = exchange.Request;
			IResource resource = FindResource(request.UriPaths);
			if (resource == null)
			{
				if ((request.Method == Method.GET) || (request.Method == Method.DELETE))
				{
					exchange.SendResponse(new Response(StatusCode.NotFound));
				}
				else
				{
					resource = FindParentResource(request.UriPaths);
					if (resource == null)
					{
						exchange.SendResponse(new Response(StatusCode.NotFound));
					}
					else
					{
						resource.HandleRequest(exchange);
					}
				}
			}
			else
				_MessageDeliverer.DeliverRequest(exchange);		
			
		}

		void IMessageDeliverer.DeliverResponse(Exchange exchange, CoAP.Response response)
		{
			if (exchange.Request == null)
				throw new ArgumentException("Request should not be empty.", "exchange");
			exchange.Request.Response = response;
		}

		public void Disconnect()
		{
			if (!string.IsNullOrEmpty(_Location))
			{
				CoapClient coapClient = new CoapClient();
				coapClient.EndPoint = _EndPoint;
				Request request = new Request(Method.DELETE);
				request.Destination = _ServerEndPoint;
				request.UriPath = _Location;
				coapClient.SendAsync(request);
				_Location = null;
			}
		}

		private IResource FindResource(IEnumerable<String> paths)
		{
			IResource result = _Root;
			using (IEnumerator<String> ie = paths.GetEnumerator())
			{
				while (ie.MoveNext() && result != null)
				{
					result = result.GetChild(ie.Current);
				}
			}
			return result;
		}

		private IResource FindParentResource(IEnumerable<String> paths)
		{
			IResource result = null;
			IResource current = _Root;
			using (IEnumerator<String> ie = paths.GetEnumerator())
			{
				while (ie.MoveNext() && current != null)
				{
					result = current;
					current = current.GetChild(ie.Current);
				}
			}
			return result;
		}

		
		public IResource GetResource(string path)
		{			
			IEnumerable<String> paths;
			if (path == null)
				paths = new List<string>();
			else
				paths = path.Split('/');
			return FindResource(paths);
		}

		public IResource GetParentResource(string path)
		{
			IEnumerable<String> paths;
			if (path == null)
				paths = new List<string>();
			else
				paths = path.Split('/');
			return FindParentResource(paths);
		}

		public bool HaveBootstrap()
		{
			return _SecurityResources.Children.Count() > 0;
		}
		
		public void LoadResources()
		{
			if (Directory.Exists(RESOURCES_CACHE))
			{
				foreach (string directory in Directory.GetDirectories(RESOURCES_CACHE))
				{
					string id = Path.GetFileName(directory);
					int objectID;
					if (int.TryParse(id, out objectID))
					{
						LWM2MResources resources = null;
						switch (objectID)
						{
							case 0:
								resources = _SecurityResources;
								break;
							case 1:
								resources = _ServerResources;
								break;
							case 3:
								resources = new DeviceResources();
								break;
							case 4:
								resources = new ConnectivityMonitoringResources();
								break;
							case 5:
								resources = new FirmwareUpdateResources();
								break;
							case 6:
								resources = new LocationResources();
								break;
							case 7:
								resources = new ConnectivityStatisticsResources();
								break;
							case 15:
								resources = new DeviceCapabilityResources();
								break;
							case 20000:
								resources = new FlowObjectResources();
								break;
							case 20001:
								resources = new FlowAccessResources();
								break;
							case 20005:
								resources = new FlowCommandResources();
								EventHandler<ChildCreatedEventArgs> handler = (s, e) =>
								{
									FlowCommandResource flowCommandResource = e.Resource as FlowCommandResource;
									if (flowCommandResource != null)
									{
										flowCommandResource.Updated += new EventHandler(FlowCommand_Updated);
										FlowCommand_Updated(flowCommandResource, null);
									}
								};
								resources.ChildCreated += handler;
								break;
							default:
								break;
						}
						if (resources != null)
						{
							foreach (string fileName in Directory.GetFiles(directory, "*.tlv"))
							{
								LWM2MResource resource = null;
								switch (objectID)
								{
									case 0:
                                        resource = new SecurityResource(Path.GetFileNameWithoutExtension(fileName));
                                        break;
									case 1:
										resource = new ServerResource(Path.GetFileNameWithoutExtension(fileName));
										break;
									case 3:
										resource = new DeviceResource();
										break;
									case 4:
										resource = new ConnectivityMonitoringResource();
										break;
									case 5:
										resource = new FirmwareUpdateResource();
										break;
									case 6:
										resource = new LocationResource();
										break;
									case 7:
										resource = new ConnectivityStatisticsResource();
										break;
									case 15:
										resource = new DeviceCapabilityResource(Path.GetFileNameWithoutExtension(fileName));
										break;
									case 20000:
										resource = new FlowObjectResource();
										resource.Updated += new EventHandler(FlowObject_Updated);
										break;
									case 20001:
										resource = new FlowAccessResource();
										break;
									case 20005:
										resource = new FlowCommandResource(Path.GetFileNameWithoutExtension(fileName));
										resource.Updated += new EventHandler(FlowCommand_Updated);
										break;
									default:
										break;
								}
								if (resource != null)
								{
									using(Stream stream = File.OpenRead(fileName))
									{
										TlvReader reader = new TlvReader(stream);
										if (resource.Deserialise(reader))
											resources.Add(resource);
									}

									if (objectID == 0)
                                    {
                                        SecurityResource securityResource = resource as SecurityResource;
                                        if (securityResource.SecurityMode == TSecurityMode.PreSharedKey)
                                        {
                                            UsePSK(System.Text.Encoding.UTF8.GetString(securityResource.ClientPublicKey), StringUtils.HexString(securityResource.SecretKey));
                                        }
                                    }
                                    else if (objectID == 20000)
                                    {
                                        FlowObjectResource flowObjectResource = resource as FlowObjectResource;
                                        if (flowObjectResource != null)
                                            flowObjectResource.TenantHash = null;
                                    }
                                }
							}
							this.AddResources(resources);
						}
					}
				}
			}
			else
				LoadDefaultResources();
		}


		void FlowCommand_Updated(object sender, EventArgs e)
		{
			FlowCommandResource flowCommandResource = sender as FlowCommandResource;
			if (flowCommandResource != null)
			{
				if (flowCommandResource.Status.HasValue && flowCommandResource.Status.Value == 2)
				{
					Command command = Command.GetCommand(flowCommandResource.CommandTypeID);
					if (command == null)
					{
						flowCommandResource.Status = 5;
						flowCommandResource.ErrorContentType = "plain/text";
						flowCommandResource.ErrorContent = "Not found";
						flowCommandResource.Changed();
					}
					else
					{
						flowCommandResource.Status = 3;
						flowCommandResource.Changed();
						if (flowCommandResource.ParameterValue != null)
						{							
							foreach (StringResource item in flowCommandResource.ParameterValue.Children)
							{
								command.Parameters.Add(item.Value);
							}
						}
						try
						{
							command.Execute();
							flowCommandResource.Status = 4;
							flowCommandResource.ResultContentType = "plain/text";
							flowCommandResource.ResultContent = "Success";
							flowCommandResource.Changed();
						}
						catch
						{
							flowCommandResource.Status = 5;
							flowCommandResource.ErrorContentType = "plain/text";
							flowCommandResource.ErrorContent = "Exception";
							flowCommandResource.Changed();
						}
					}
				}
			}
		}

		void FlowObject_Updated(object sender, EventArgs e)
		{
			FlowObjectResource flowObjectResource = sender as FlowObjectResource;
			if (flowObjectResource != null)
			{
				if ((flowObjectResource.TenantChallenge != null) && (flowObjectResource.HashIterations != null))
				{
					string tenantSecret = "getATTtDsNBpBRnMsN7GoQ==";
					HMACSHA256 hmac = new HMACSHA256();
					hmac.Key = Convert.FromBase64String(tenantSecret);
					byte[] hash = hmac.ComputeHash(flowObjectResource.TenantChallenge);
					for (int index = 1; index < flowObjectResource.HashIterations.Value; index++)
					{
						hash = hmac.ComputeHash(hash);
					}
					flowObjectResource.TenantHash = hash;
					flowObjectResource.Changed();
				}
			}
		}


		public void LoadDefaultResources()
		{
			DeviceResources deviceResources = new DeviceResources();
			IntegerResources availablePowerSources = new IntegerResources(string.Empty);
			availablePowerSources.Add(new IntegerResource("0") { Value = 1 });
			availablePowerSources.Add(new IntegerResource("1") { Value = 5 });

			IntegerResources powerSourceVoltage = new IntegerResources(string.Empty);
			powerSourceVoltage.Add(new IntegerResource("0") { Value = 3800 });
			powerSourceVoltage.Add(new IntegerResource("1") { Value = 5000 });

			IntegerResources powerSourceCurrent = new IntegerResources(string.Empty);
			powerSourceCurrent.Add(new IntegerResource("0") { Value = 3800 });
			powerSourceCurrent.Add(new IntegerResource("1") { Value = 5000 });

			IntegerResources errorCode = new IntegerResources(string.Empty);
			errorCode.Add(new IntegerResource("0") { Value = 0 });

			deviceResources.Add(new DeviceResource() { Manufacturer = "Open Mobile Alliance", ModelNumber = "Lightweight M2M Client", SerialNumber = "345000123", FirmwareVersion = "1.0", AvailablePowerSources = availablePowerSources, PowerSourceVoltages = powerSourceVoltage, PowerSourceCurrents = powerSourceCurrent, BatteryLevel = 100, MemoryFree = 15, ErrorCodes = errorCode, CurrentTime = DateTime.UtcNow, UTCOffset = "+12:00", SupportedBindingandModes = "U" });

			ConnectivityMonitoringResources connectivityMonitoringResources = new ConnectivityMonitoringResources();
			IntegerResources availableNetworkBearer = new IntegerResources(string.Empty);
			availableNetworkBearer.Add(new IntegerResource("0") { Value = 0 });

			StringResources ipAddresses = new StringResources(string.Empty);
			ipAddresses.Add(new StringResource("0") { Value = "192.168.148.18" });

			StringResources routerIPAddresse = new StringResources(string.Empty);
			routerIPAddresse.Add(new StringResource("0") { Value = "192.168.148.1" });

			StringResources apn = new StringResources(string.Empty);
			apn.Add(new StringResource("0") { Value = "internet" });

			connectivityMonitoringResources.Add(new ConnectivityMonitoringResource() { NetworkBearer = 0, AvailableNetworkBearers = availableNetworkBearer, RadioSignalStrength = 92, LinkQuality = 2, IPAddresses = ipAddresses, RouterIPAddresses = routerIPAddresse, LinkUtilization = 5, APNs = apn });
			this.AddResources(deviceResources);
			this.AddResources(connectivityMonitoringResources);
			this.AddResources(new FirmwareUpdateResources());
			FlowObjectResources flowObjectResources = new FlowObjectResources();
			FlowObjectResource flowObjectResource = new FlowObjectResource() { DeviceType = "EVOKE Flow", TenantID = 1 };
			flowObjectResource.Updated +=  new EventHandler(FlowObject_Updated);
			flowObjectResources.Add(flowObjectResource);
			
			this.AddResources(flowObjectResources);
			this.AddResources(new FlowAccessResources());

			FlowCommandResources flowCommandResources = new FlowCommandResources();
			EventHandler<ChildCreatedEventArgs> handler = (s, e) =>
			{
				FlowCommandResource flowCommandResource = e.Resource as FlowCommandResource;
				if (flowCommandResource != null)
				{
					flowCommandResource.Updated += new EventHandler(FlowCommand_Updated);
					FlowCommand_Updated(flowCommandResource, null);
				}
			};
			flowCommandResources.ChildCreated += handler;
			this.AddResources(flowCommandResources);

			LocationResources locationResources = new LocationResources();
			locationResources.Add(new LocationResource() { Latitude = "-41.0", Longitude = "174.0", Altitude = "150", Uncertainty = "1.0", Velocity = new byte[] { 0, 45, 0, 0 }, Timestamp = DateTime.UtcNow });
			this.AddResources(locationResources);

			DeviceCapabilityResources deviceCapabilityResources = new DeviceCapabilityResources();
			deviceCapabilityResources.Add(new DeviceCapabilityResource("0") { Attached = true, Enabled = true, Group = 0, Description = "Temp", Property = "2;3" });
			deviceCapabilityResources.Add(new DeviceCapabilityResource("1") { Attached = true, Enabled = true, Group = 1, Description = "Control", Property = "1;2" });
			deviceCapabilityResources.Add(new DeviceCapabilityResource("2") { Attached = true, Enabled = false, Group = 2, Description = "Bluetooth", Property = "0" });
			deviceCapabilityResources.Add(new DeviceCapabilityResource("3") { Attached = false, Enabled = false, Group = 2, Description = "WiFi", Property = "2" });

			this.AddResources(deviceCapabilityResources);
		}

		public void SaveResources()
		{
			foreach (IResource resource in _Root.Children)
			{
				LWM2MResources lwm2mResources = resource as LWM2MResources;
				if (lwm2mResources != null)
				{
					string directory = Path.Combine(RESOURCES_CACHE, lwm2mResources.Name);
					if (!Directory.Exists(directory))
					{
						Directory.CreateDirectory(directory);
					}
					foreach (LWM2MResource item in lwm2mResources.Children)
					{
						string fileName = Path.Combine(directory, string.Concat(item.Name, ".tlv"));
						using(Stream stream = File.Create(fileName,4096))
						{
							TlvWriter writer = new TlvWriter(stream);
							item.Serialise(writer);
							stream.Flush();
						}
					}

				}
			}

		}

		private void SendUpdate()
		{
			if (!string.IsNullOrEmpty(_Location))
			{
				CoapClient coapClient = new CoapClient();
				coapClient.EndPoint = _EndPoint;
				Request request = new Request(Method.PUT);
				request.Destination = _ServerEndPoint;
				request.UriPath = _Location;
				request.UriQuery = "lt=35";
				coapClient.SendAsync(request);
			}
		}


		public void Start()
		{
			_CoapServer.Start();
        }


		public void Stop()
		{
			_Timer.Stop();
			Disconnect();
            _CoapServer.Stop();
		}



		void _Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			SendUpdate();
		}


	}
}
