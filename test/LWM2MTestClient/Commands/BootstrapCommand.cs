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

namespace Imagination.LWM2M
{
	internal class BootstrapCommand : Command
	{
		private Client _Client;

		public BootstrapCommand(Client client)
			: base()
		{
			Name = "bootstrap";
			_Client = client;
		}

		public override void Help()
		{
			Console.WriteLine("bootstap [url] eg bootstrap coap://we-dev-lwm2m1.we.imgtec.org:15685");
		}

		public override void Execute()
		{
			if (Parameters.Count > 0)
			{
				_Client.Disconnect();
				Console.Write("Bootstraping (");
				Console.Write(Parameters[0]);
				Console.Write(") .. ");
				if (_Client.Bootstrap(Parameters[0]))
				{
					Console.WriteLine("Complete");
					Console.Write("Connect to server (");
					if (_Client.ConnectToServer())
					{
						Console.WriteLine("Complete");
					}
				}
				else
				{
					Console.WriteLine("Failed");
				}
			}
		}
	}
}
