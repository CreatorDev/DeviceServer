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
using Imagination.Model;
using Imagination.LWM2M;

namespace Imagination.Model
{
	internal class LWM2MClient : Client
	{
        private static int _DataFormat = TlvConstant.CONTENT_TYPE_TLV;

		public const int REQUEST_TIMEOUT = 20000;

		public System.Net.EndPoint Address { get; set; }

		public CoAP.Net.IEndPoint EndPoint { get; set; }

        public static int DataFormat { get { return _DataFormat; } set { _DataFormat = value; } }

        public ClientMetrics Metrics { get; private set; }

        private class ObserveRequest
        {
            public Request Request { get; set; }

            public ObjectDefinition ObjectDefinition { get; set; }

            public Object Object { get; set; }

            public PropertyDefinition PropertyDefinition { get; set; }
        }

        private List<ObserveRequest> _ObserveRequests;

        public LWM2MClient()
        {
            Metrics = new ClientMetrics();
        }

		public void Cancel()
		{

		}
        
        public void CancelObserve(ObjectType objectType, string instanceID, string resourceID, bool useReset)
        {
			if (_ObserveRequests != null)
			{
				string uriPath;
				if (string.IsNullOrEmpty(instanceID))
					uriPath = objectType.Path;
				else if (string.IsNullOrEmpty(resourceID))
					uriPath = string.Concat(objectType.Path, "/", instanceID);
				else
					uriPath = string.Concat(objectType.Path, "/", instanceID, "/", resourceID);
				int index = 0;
				while (index < _ObserveRequests.Count)
				{
					if (_ObserveRequests[index].Request.UriPath == uriPath)
					{
						Request observeRequest = _ObserveRequests[index].Request;
						_ObserveRequests.RemoveAt(index);
						if (observeRequest != null)
						{
							if (useReset)
							{
								observeRequest.IsCancelled = true;
							}
							else
							{
								Request request = new Request(Method.GET);
								request.Accept = _DataFormat;
								request.Destination = observeRequest.Destination;
								request.EndPoint = observeRequest.EndPoint;
								request.UriPath = observeRequest.UriPath;
								request.MarkObserveCancel();
                                SendRequest(request);
                                observeRequest.IsCancelled = true;
							}
						}

					}
					else
						index++;
				}
			}
        }

		public void Observe( ObjectDefinition objectDefinition, ObjectType objectType, string instanceID, PropertyDefinition propertyDefinition)
        {
            Request request;            
            if (propertyDefinition == null)
                request = NewGetRequest(objectType, instanceID, null);
            else
                request = NewGetRequest(objectType, instanceID, propertyDefinition.PropertyID);
            request.MarkObserve();
            if (_ObserveRequests == null)
                _ObserveRequests = new List<ObserveRequest>();
			bool found = false;
			for (int index = 0; index < _ObserveRequests.Count; index++)
			{
				if (_ObserveRequests[index].Request.UriPath == request.UriPath)
				{
					found = true;
					break;
				}
			}
			if (!found)
			{
				_ObserveRequests.Add(new ObserveRequest() { Request = request, ObjectDefinition = objectDefinition, PropertyDefinition = propertyDefinition });
				request.Respond += new EventHandler<ResponseEventArgs>(ObserveResponse);
                SendRequest(request);
            }
        }

        public Request SendRequest(Request request)
        {
            request.Respond += new EventHandler<ResponseEventArgs>(UpdateMetrics);
            return request.Send();
        }

        void UpdateMetrics(object sender, ResponseEventArgs e)
        {
            lock (this)
            {
                Request request = sender as Request;
                if (request != null)
                {
                    Metrics.BytesSent.Value += request.Bytes.Length;
                    Metrics.BytesReceived.Value += e.Response.PayloadSize;
                    ++Metrics.TransactionCount.Value;
                    BusinessLogicFactory.Events.MetricsUpdate(this);
                    // TODO: How often should metrics be updated?
                    // TODO: Handle retransmit, rejected etc
                }
            }
        }

        void ObserveResponse(object sender, ResponseEventArgs e)
        {
            lock (this)
            {
#if DEBUG
                Console.WriteLine("NOTIFY Recieved");
#endif
                Request request = sender as Request;
                if (request != null)
                {
                    Console.WriteLine(request.UriPath);
                    ObserveRequest observeRequest = null;
                    for (int index = 0; index < _ObserveRequests.Count; index++)
                    {
                        if (_ObserveRequests[index].Request.UriPath == request.UriPath)
                        {
                            observeRequest = _ObserveRequests[index];
                            break;
                        }
                    }
                    string[] objectInstanceResource = request.UriPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    if (observeRequest != null)
                    {
#if DEBUG
                        if (observeRequest.PropertyDefinition != null)
                            Console.WriteLine(observeRequest.PropertyDefinition.Name);
#endif
                        if ((e.Response.ContentType == MediaType.TextPlain) || (e.Response.ContentType == TlvConstant.CONTENT_TYPE_PLAIN))
                        {
#if DEBUG
                            string payload = Encoding.UTF8.GetString(e.Response.Payload);
                            Console.WriteLine(payload);
#endif
                        }
                        else if (e.Response.ContentType == TlvConstant.CONTENT_TYPE_TLV)
                        {
                            Object lwm2mObject = null;
                            if (e.Response.Payload != null)
                            {
                                TlvReader reader = new TlvReader(e.Response.Payload);
                                lwm2mObject = ObjectUtils.ParseObject(observeRequest.ObjectDefinition, reader);
                            }

                            if ((lwm2mObject != null) && (objectInstanceResource.Length > 1) && (lwm2mObject.InstanceID == null))
                            {
                                // Instance ID not in TLV, take from request URI
                                lwm2mObject.InstanceID = objectInstanceResource[1];
                            }

                            if ((lwm2mObject != null) && (lwm2mObject.Properties.Count > 0))
                            {
#if DEBUG
                                foreach (Property item in lwm2mObject.Properties)
                                {
                                    if (observeRequest.PropertyDefinition == null || item.PropertyDefinitionID == observeRequest.PropertyDefinition.PropertyDefinitionID)
                                    {
                                        if (item.Value != null)
                                        {
                                            Console.WriteLine(item.Value.Value);
                                        }
                                        else if (item.Values != null)
                                        {
                                            string[] values = new string[item.Values.Count];
                                            int index = 0;
                                            foreach (PropertyValue value in item.Values)
                                            {
                                                Console.WriteLine(value.Value);
                                                values[index++] = value.Value;
                                            }
                                        }
                                        break;
                                    }
                                }
#endif
                            }
                            if (lwm2mObject != null)
                                BusinessLogicFactory.Events.ObservationNotify(this, lwm2mObject);
                        }
                    }
                }
            }
        }

		public Request NewGetRequest(ObjectType objectType, string instanceID)
		{
			Request result = new Request(Method.GET);
			//result.Accept = TlvConstant.CONTENT_TYPE_TLV;
            result.Accept = _DataFormat;
			SetupRequest(result, objectType, instanceID, null);
			return result;
		}

        public Request NewGetRequest(ObjectType objectType, string instanceID, string resourceID)
		{
			Request result = new Request(Method.GET);
			//result.Accept = TlvConstant.CONTENT_TYPE_TLV;
            result.Accept = _DataFormat;
			//if (_DataFormat == MediaType.TextPlain)
			//    result.Accept = TlvConstant.CONTENT_TYPE_PLAIN;
            SetupRequest(result, objectType, instanceID, resourceID);
			return result;
		}


        public Request NewPostRequest(ObjectType objectType, string instanceID, int contentType, byte[] data)
		{
            return NewPostRequest(objectType, instanceID, null, contentType, data);
		}

        public Request NewPostRequest(ObjectType objectType, string instanceID, string resourceID, int contentType, byte[] data)
		{
			Request result = new Request(Method.POST);
			SetupRequest(result, objectType, instanceID, resourceID);
			result.Payload = data;
			if (contentType != -1)
				result.ContentType = contentType;
			return result;
		}

        public Request NewPutRequest(ObjectType objectType, string instanceID, string resourceID, int contentType, byte[] data)
		{
			Request result = new Request(Method.PUT);
			//if (contentType == MediaType.TextPlain)
			//    contentType = TlvConstant.CONTENT_TYPE_PLAIN;
			SetupRequest(result, objectType, instanceID, resourceID);
			result.Payload = data;
			if (contentType != -1)
				result.ContentType = contentType;
			return result;
		}

		public Request NewDeleteRequest(ObjectType objectType, string instanceID)
		{
			return NewDeleteRequest(objectType, instanceID, null);
		}

		public Request NewDeleteRequest(ObjectType objectType, string instanceID, string resourceID)
		{
			Request result = new Request(Method.DELETE);
			SetupRequest(result, objectType, instanceID, resourceID);
			return result;
		}

		private void SetupRequest(Request request, ObjectType objectType, string instanceID, string resourceID)
		{
			request.Destination = this.Address;
			request.EndPoint = this.EndPoint;
			if (string.IsNullOrEmpty(instanceID))
				request.UriPath = objectType.Path;
			else if (string.IsNullOrEmpty(resourceID))
				request.UriPath = string.Concat(objectType.Path, "/", instanceID);
			else
				request.UriPath = string.Concat(objectType.Path, "/", instanceID, "/", resourceID);
		}


	}
}
