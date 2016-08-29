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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Imagination.Model
{
	public class ServiceEventMessage
	{
		private Guid _MessageID;
		private string _ResponseRoutingKey;
        private string _Queue;
		private long _TimeStamp;
		private Dictionary<string, object> _Parameters;
		private Guid _DeliveryID;
        private TMessagePublishMode _MessagePublishMode;
		private DateTime? _QueueAfterTime;

		public ServiceEventMessage()
		{
			_Parameters = new Dictionary<string, object>();
		}

		public Guid MessageID
		{
			get { return _MessageID; }
			set { _MessageID = value; }
		}

		public Dictionary<string, object> Parameters
		{
			get { return _Parameters; }
		}

        [JsonIgnore]
        public string Queue
		{
			get { return _Queue; }
			set { _Queue = value; }
		}

		public string ResponseRoutingKey
		{
			get { return _ResponseRoutingKey; }
			set { _ResponseRoutingKey = value; }
		}

		public long TimeStamp
		{
			get { return _TimeStamp; }
			set { _TimeStamp = value; }
		}
        
        public TMessagePublishMode MessagePublishMode
		{
            get { return _MessagePublishMode; }
            set { _MessagePublishMode = value; }
		}

		public DateTime? QueueAfterTime
		{
			get { return _QueueAfterTime; }
			set { _QueueAfterTime = value; }
		}

        [JsonIgnore]
        internal Guid DeliveryID
		{
			get { return _DeliveryID; }
			set { _DeliveryID = value; }
		}

        public void AddParameter(string key, string value)
        {
            _Parameters.Add(key, value);
        }

        public void AddParameter(string key, object value)
        {
            _Parameters.Add(key, value);
        }

    }
}
