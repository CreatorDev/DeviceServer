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
using CoAP.Server.Resources;
using Imagination.LWM2M.Resources;

namespace Imagination.LWM2M
{
	internal class SetResourceCommand : Command
	{
		private Client _Client;

		public SetResourceCommand(Client client)
			: base()
		{
			Name = "set";
			_Client = client;
		}

		public override void Execute()
		{
			if (Parameters.Count > 1)
			{
                if (Parameters[0].StartsWith("/"))
                    Parameters[0] = Parameters[0].Substring(1);
				LWM2MResource resource = _Client.GetResource(Parameters[0]) as LWM2MResource;
				if (resource == null)
				{
					IResource parentResource = _Client.GetParentResource(Parameters[0]);

					resource = parentResource as LWM2MResource;

                    if (resource == null && parentResource.Parent != null && String.IsNullOrEmpty(parentResource.Parent.Path))
                    {
                        // object instance does not exist
                        LWM2MResources resources = parentResource as LWM2MResources;
                        string instanceID = Parameters[0].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[1];
                        parentResource = resource = resources.CreateResource(instanceID);

                        resources.ModifiedResource = resource;

                        resources.Changed();
                        //(parentResource as LWM2MResource).Changed();
                        resource = null;
                    }

					if (resource == null)
					{
						LWM2MResources resources = parentResource as LWM2MResources;

						if (resources != null)
						{
							string[] paths = Parameters[0].Split('/');
							string name = paths[paths.Length - 1];
							LWM2MResource childResource = null;
							BooleanResources booleanResources = resources as BooleanResources;
							if (booleanResources == null)
							{
								DateTimeResources dateTimeResources = resources as DateTimeResources;
								if (dateTimeResources == null)
								{
									FloatResources floatResources = resources as FloatResources;
									if (floatResources == null)
									{
										IntegerResources integerResources = resources as IntegerResources;
										if (integerResources == null)
										{
											OpaqueResources opaqueResources = resources as OpaqueResources;
											if (opaqueResources == null)
											{
												StringResources stringResources = resources as StringResources;
												if (stringResources == null)
												{

												}
												else
													childResource = new StringResource(name);
											}
											else
												childResource = new OpaqueResource(name);
										}
										else
											childResource = new IntegerResource(name);
									}
									else
										childResource = new FloatResource(name);
								}
								else
									childResource = new DateTimeResource(name);
							}
							else
								childResource = new BooleanResource(name);


							if (childResource != null)
							{
								childResource.SetValue(Parameters[1]);
								resources.Add(childResource);
							}
							
						}
					}
					else
					{
						
					}

				}
				else
				{
					resource.SetValue(Parameters[1]);
					resource.Changed();
                    LWM2MResource parent = resource.Parent as LWM2MResource;
                    if (parent != null)
                    {
                        parent.Changed();
                        LWM2MResources grandparent = parent.Parent as LWM2MResources;
                        if (grandparent != null)
                        {
                            grandparent.Changed();
                        }
                    }
                }
			}
		}

		public override void Help()
		{
			Console.WriteLine("set [url] [value] eg set 20001/0/1 Test");
		}
	}
}
