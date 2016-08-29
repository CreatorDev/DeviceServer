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
using System.IO;

namespace Imagination
{
	internal class Trace
	{
		private static string _LogFile;
		private static TTracePriority _TracePriority;

		static Trace()
		{
            if (Environment.OSVersion.Platform == PlatformID.Unix)
                _LogFile = @"/var/log/databasenotification.log";
            else
				_LogFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DatabaseNotification.Log");
			_TracePriority = TTracePriority.High;
		}

		/// <summary>
		/// Level of detail to report in trace
		/// </summary>
		public static TTracePriority TracePriority
		{
			get
			{
				return _TracePriority;
			}
			set
			{
				_TracePriority = value;
			}
		}


		/// <summary>
		/// Write Text to trace
		/// </summary>
		/// <param name="priority">priority level of text</param>
		/// <param name="text">test to write</param>
		public static void Write(TTracePriority priority, string text)
		{
			if (priority <= TracePriority)
			{
                try
                {
                    string logDirectory = Path.GetDirectoryName(_LogFile);
                    if (!Directory.Exists(logDirectory))
                        Directory.CreateDirectory(logDirectory);
                    text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + text;
                    File.AppendAllText(_LogFile, text);
                }
                catch
                {

                }
			}
		}

		/// <summary>
		/// Write text to trace and auto-add new line charaters
		/// </summary>
		/// <param name="priority">priorit level of text</param>
		/// <param name="text">test to write</param>
		public static void WriteLine(TTracePriority priority, string text)
		{
			Write(priority, text + Environment.NewLine);
		}
	}
}
