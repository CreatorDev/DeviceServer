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
	internal class Command
	{
		protected static Dictionary<string, Command> _Commands = new Dictionary<string, Command>();

		public string Name { get; set; }

		public List<string> Parameters { get; private set; }

		public Command()
		{
			Parameters = new List<string>();
		}


		public virtual void Execute()
		{

		}

		public virtual void Help()
		{

		}

		public static Command GetCommand(string command)
		{
			Command result;
			if (_Commands.TryGetValue(command.ToLower(), out result))
			{
				if (result.Parameters != null)
					result.Parameters.Clear();
			}
			return result;
		}

		public static Command Parse(string commandLine)
		{
			Command result = null;
			int position = commandLine.IndexOf(' ');
			string commmand;
			string args;
			if (position == -1)
			{
				commmand = commandLine;
				args = string.Empty;
			}
			else
			{
				commmand = commandLine.Substring(0, position);
				args = commandLine.Substring(position + 1);
			}
			Command command;
			if (_Commands.TryGetValue(commmand.ToLower(), out command))
			{
				if (command.Parameters != null)
					command.Parameters.Clear();
				command.ParseArgs(args);
				result = command;
			}
			return result;
		}

		protected void ParseArgs(string args)
		{
			StringBuilder arg = new StringBuilder();
			bool insideLiteral = false;
			char previousElement = char.MinValue;
			foreach (char element in args)
			{
				bool addToken = false;
				if (insideLiteral)
				{
					if ((element == '"') && (previousElement != '\\'))
					{
						insideLiteral = false;
						addToken = true;
					}
					else if ((element == '"') && (previousElement == '\\'))
					{
						arg.Append(element);
					}
					else if ((element == '\\') && (previousElement != '\\'))
					{

					}
					else if ((element == '\\') && (previousElement == '\\'))
					{
						arg.Append(element);
						previousElement = ' ';
						continue;
					}
					else
						arg.Append(element);
				}
				else
				{
					switch (element)
					{
						case '"':
							addToken = true;
							insideLiteral = true;
							break;
						case ' ':
						case '\r':
						case '\n':
						case '\t':
							addToken = true;
							break;
						default:
							arg.Append(element);
							break;
					}

				}
				if (addToken && (arg.Length > 0))
				{
					if (Parameters == null)
						Parameters = new List<string>();
					Parameters.Add(arg.ToString());
					arg.Length = 0;
				}
				previousElement = element;
			}
			if (arg.Length > 0)
			{
				if (Parameters == null)
					Parameters = new List<string>();
				Parameters.Add(arg.ToString());
			}
		}

		public static void RegisterCommand(Command command)
		{
			if (command.Name != null)
			{
				string name = command.Name.ToLower();
				_Commands.Add(name, command);
			}
		}

	}
}
