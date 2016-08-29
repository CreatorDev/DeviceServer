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
	internal class DisplayResourceCommand : Command
	{
		private Client _Client;

		public DisplayResourceCommand(Client client)
			: base()
		{
			Name = "show";
			_Client = client;
		}


		private int CompareResources(IResource x, IResource y)
		{
			int result;
			int xValue;
			int yValue;
			if (int.TryParse(x.Name, out xValue) && int.TryParse(y.Name, out yValue))
				result = xValue.CompareTo(yValue);
			else
				result = x.Name.CompareTo(y.Name);
			return result;
		}

		public override void Execute()
		{
			if (Parameters.Count > 0)
			{
				IResource resource = _Client.GetResource(Parameters[0]);
				if (resource != null)
				{
					List<IResource> items = resource.Children.ToArray().ToList();
					items.Sort(CompareResources);
					foreach (IResource item in items)
					{
						LWM2MResources resources = item as LWM2MResources;
						if (resources == null)
						{
							LWM2MResource itemResource = item as LWM2MResource;
							if (itemResource == null)
								Console.Write(item.Name.PadRight(33, ' '));
							else
							{
								Console.Write(item.Name.PadRight(3, ' '));
								if (string.IsNullOrEmpty(itemResource.Description))
									Console.Write(new string(' ',30));
								else
									Console.Write(itemResource.Description.PadRight(30, ' '));
							}
							Console.Write(":");
							Console.WriteLine(item.ToString());
						}
						else
						{
							Console.Write(item.Name.PadRight(3, ' '));
							Console.Write(resources.Description);
							Console.WriteLine();
							List<IResource> children = resources.Children.ToArray().ToList();
							children.Sort(CompareResources);
							foreach (IResource childItem in children)
							{
								Console.Write("\t\t");
								Console.Write(childItem.Name);
								Console.Write("\t:");
								Console.WriteLine(childItem.ToString());
							}
						}
					}
				}
			}
			else
			{
				IResource resource = _Client.GetResource(null);
				if (resource != null)
				{
					List<IResource> items = resource.Children.Where((e) => { return e.Visible; }).ToArray().ToList();
					items.Sort(CompareResources);
					foreach (IResource item in items)
					{
						List<IResource> children = item.Children.ToArray().ToList();
						children.Sort(CompareResources);
						if (children.Count > 0)
						{
							foreach (IResource childItem in children)
							{
								Console.Write("  ");
								Console.Write(item.Name);
								Console.Write("/");
								Console.WriteLine(childItem.Name);
							}
						}
						else
						{
							Console.Write("  ");
							Console.WriteLine(item.Name);
						}
					}
				}
			}
		}

		public override void Help()
		{
			Console.WriteLine("show [url] eg show 20001/0");
			Console.WriteLine("   if you don't provide url it will list objects");
		}

	}
}
