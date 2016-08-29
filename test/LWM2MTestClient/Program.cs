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
using System.Reflection;
using CoAP;
using CoAP.Net;
using CoAP.Server;
using Imagination.LWM2M.Resources;

namespace Imagination.LWM2M
{
	class Program
	{
		static void Main(string[] args)
		{
			Client client = null;
			try
			{
				Assembly assembly = Assembly.GetExecutingAssembly();
				System.Diagnostics.FileVersionInfo fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
				Console.WriteLine(string.Concat("TestClient Version - ", fileVersionInfo.FileVersion));
				CoAP.Log.LogManager.Level = CoAP.Log.LogLevel.None;
				client = new Client(55863);
				client.LoadResources();
                client.Start();
                Command.RegisterCommand(new PSKCommand(client));
                Command.RegisterCommand(new CertificateFileCommand(client));
                BootstrapCommand bootstrapCommand = new BootstrapCommand(client);
				Command.RegisterCommand(bootstrapCommand);
                ConnectCommand connectCommand = new ConnectCommand(client);
                Command.RegisterCommand(connectCommand);
                Command.RegisterCommand(new EchoCommand());
				Command.RegisterCommand(new FCAPCommand(client));
				Command.RegisterCommand(new HelpCommand());
				Command.RegisterCommand(new SaveCommand(client));
				Command.RegisterCommand(new SetResourceCommand(client));				
				Command.RegisterCommand(new DisplayResourceCommand(client));				
				Command.RegisterCommand(new Command() { Name = "quit" });
                Command.RegisterCommand(new Command() { Name = "exit" });

				bool bootstrap = true;
				if (client.HaveBootstrap())
				{
					Console.Write("Connect to server (");
					if (client.ConnectToServer())
					{
						Console.WriteLine("Complete");
						bootstrap = false;
					}
					else
						Console.WriteLine("Failed");
				}
				if (bootstrap)
				{
					//bootstrapCommand.Parameters.Add("coap://delmet-hp.we.imgtec.org:15685");
					//bootstrapCommand.Parameters.Add("coap://we-dev-lwm2m1.we.imgtec.org:15685");
					//bootstrapCommand.Execute();
				}
				Console.WriteLine("Type quit to stop the LWM2M client (type help to see other commmands).");
				while (true)
				{
					Console.Write('>');
					Command command = Command.Parse(Console.ReadLine());
					if (command != null)
					{
						if ((string.Compare(command.Name, "quit", true) == 0) || (string.Compare(command.Name, "exit", true) == 0))
							break;
						else
						{
							command.Execute();
						}
					}
				}

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
                Console.WriteLine("Press enter to exit...");
                Console.ReadLine();
			}
			finally
			{
				if (client != null)
				{
					client.Stop();
					client.SaveResources();
				}
			}

		}
	}
}
