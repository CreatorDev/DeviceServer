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

namespace Imagination
{
	public static class Singleton<T> where T : class
	{
		private static volatile T _Instance;
		private static object _Lock = new object();

		static Singleton()
		{
			lock (_Lock)
			{
                Type type = typeof(T);
				try
				{
					ConstructorInfo ci = typeof(T).GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
					if (ci == null)
					{
#if !SILVERLIGHT
                        ApplicationEventLog.WriteEntry("Flow", string.Format("Invoke Error: GetConstructor returned null for type {0} .", type.Name), System.Diagnostics.EventLogEntryType.Error);
#endif
#if DEBUG
						System.Diagnostics.Debugger.Break();
#endif
					}
					else
					{
						_Instance = (T)ci.Invoke(new object[0]);
                    }
				}
				catch (Exception ex)
				{
#if !SILVERLIGHT
					ApplicationEventLog.WriteEntry("Flow", string.Format("Invoke Error for type: {0}\n{1}", type.Name, ex), System.Diagnostics.EventLogEntryType.Error);
#endif
#if DEBUG
					System.Diagnostics.Debugger.Break();
#endif
				}
			}
		}

		/// <summary>
		/// Returns the only instance of the specified Bll generic type T
		/// </summary>
		public static T Instance
		{
			get { return _Instance; }
		}
	}
}
