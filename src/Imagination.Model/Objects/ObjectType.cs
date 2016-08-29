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
using System.IO;

namespace Imagination.Model
{
	public class ObjectType
	{
		public string Path { get; set; }

		public int ObjectTypeID { get; set; }

		public List<int> Instances { get; set; }

		public ObjectType()
		{
			Instances = new List<int>();
		}

        public void Serialise(Stream stream)
        {
            IPCHelper.Write(stream, Path);
            IPCHelper.Write(stream, ObjectTypeID);
            IPCHelper.Write(stream, Instances.Count);
            foreach (int item in Instances)
            {
                IPCHelper.Write(stream, item);                
            }
        }


        public static ObjectType Deserialise(Stream stream)
        {
            ObjectType result = new ObjectType();
            result.Path = IPCHelper.ReadString(stream);
            result.ObjectTypeID = IPCHelper.ReadInt32(stream);
            int count = IPCHelper.ReadInt32(stream);
            if (count > 0)
            {
                for (int index = 0; index < count; index++)
                {
                    int item = IPCHelper.ReadInt32(stream);
                    result.Instances.Add(item);
                }
            }
            return result;
        }

        internal static int Compare(ObjectType x, ObjectType y)
        {
            int result = x.ObjectTypeID.CompareTo(y.ObjectTypeID);
            if (result == 0)
            {
                result = x.Instances.Count.CompareTo(y.Instances.Count);
                if (result == 0)
                {
                    foreach (int xItem in x.Instances)
                    {
                        bool found = false;
                        foreach (int yItem in y.Instances)
                        {
                            if (xItem == yItem)
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            result = 1;
                            break;
                        }
                    }
                }
            }
            return result;
        }
    }
}
