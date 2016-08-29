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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Imagination.Model
{
	public class ObjectTypes : IEnumerable<ObjectType>, IEnumerable
	{
		private List<ObjectType> _ObjectTypeList = new List<ObjectType>();
		private Dictionary<int, ObjectType> _ObjectTypes = new Dictionary<int, ObjectType>();

		public int Count { get { return _ObjectTypeList.Count; } }
        public ObjectType this[int index] { get { return _ObjectTypeList[index]; } }
        
        public void AddObjectType(ObjectType objectType)
		{
			if (!_ObjectTypes.ContainsKey(objectType.ObjectTypeID))
			{
				_ObjectTypeList.Add(objectType);
				_ObjectTypes.Add(objectType.ObjectTypeID, objectType);
			}
		}

		public ObjectType GetObjectType(int objectTypeID)
		{
			ObjectType result;
			_ObjectTypes.TryGetValue(objectTypeID, out result);
			return result;
		}

		public void Parse(string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				ObjectType objectType = null;
				StringBuilder idText = new StringBuilder();
				StringBuilder path = new StringBuilder();
				bool instance = false;
				string rootPath = string.Empty;
                string potentialRootPath = null;
				foreach (char item in text)
				{
					if (item == '<')
					{
						idText.Length = 0;
						path.Length = 0;
						instance = false;
					}
					else if (item == '>')
					{
                        potentialRootPath = path.ToString();
						if (idText.Length > 0)
						{
							int id;
							if (int.TryParse(idText.ToString(), out id))
							{
								if (instance)
								{
									objectType.Instances.Add(id);
								}
								else
								{
									_ObjectTypes.TryGetValue(id, out objectType);
									if (objectType == null)
									{
										objectType = new ObjectType();
										objectType.Path = string.Concat(rootPath, path.ToString()); // path.ToString(0, path.Length - idText.Length));
										objectType.ObjectTypeID = id;
										_ObjectTypeList.Add(objectType);
										_ObjectTypes.Add(id, objectType);
									}
								}
							}
							else
							{
                                //rootPath = path.ToString(0, path.Length - idText.Length);
                                //rootPath = path.ToString();
							}
						}
                        path.Length = 0;
						objectType = null;
					}
                    else if (item == ',')
                    {
						if (path.ToString().Contains(";rt=\"oma.lwm2m\""))
						{
							if ((potentialRootPath != null) && (string.Compare(potentialRootPath, "/") != 0))
							{
								rootPath = potentialRootPath;
							}
						}
                    }
                    else if (item == '/')
                    {
                        if (idText.Length > 0)
                        {
                            int id;
                            if (int.TryParse(idText.ToString(), out id))
                            {
                                _ObjectTypes.TryGetValue(id, out objectType);
                                if (objectType == null)
                                {
                                    objectType = new ObjectType();
                                    objectType.Path = string.Concat(rootPath, path.ToString()); // path.ToString(0, path.Length - idText.Length));
                                    objectType.ObjectTypeID = id;
                                    _ObjectTypeList.Add(objectType);
                                    _ObjectTypes.Add(id, objectType);
                                }
                                instance = true;
                            }
                        }
                        idText.Length = 0;
                        path.Append(item);
                    }
                    else
                    {
                        path.Append(item);
                        idText.Append(item);
                    }
				}
			}
		}

        public void Serialise(Stream stream)
        {                
            IPCHelper.Write(stream, _ObjectTypeList.Count);
			foreach (ObjectType item in _ObjectTypeList)
			{
				item.Serialise(stream);
			}
        }

        public static ObjectTypes Deserialise(Stream stream)
        {
            ObjectTypes result = new ObjectTypes();
            int count = IPCHelper.ReadInt32(stream);
            if (count > 0)
            {
                for (int index = 0; index < count; index++)
                {
                    ObjectType objectType = ObjectType.Deserialise(stream);                    
                    result.AddObjectType(objectType);
                }
                result._ObjectTypeList.Sort(ObjectType.Compare);
            }
            return result;
        }

		public IEnumerator<ObjectType> GetEnumerator()
		{
			return _ObjectTypeList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _ObjectTypeList.GetEnumerator();
		}


        public static int Compare(ObjectTypes x, ObjectTypes y)
        {
            int result = 0;
            if ((x == null) && (y != null))
                result = 1;
            else if ((x != null) && (y == null))
                result = -1;
            else if (x != null)
            {
                result = x._ObjectTypeList.Count.CompareTo(y._ObjectTypeList.Count);
                if (result == 0)
                {
                    foreach (ObjectType xItem in x._ObjectTypeList)
                    {
                        bool found = false;
                        foreach (ObjectType yItem in y._ObjectTypeList)
                        {
                            if (xItem.ObjectTypeID == yItem.ObjectTypeID)
                            {
                                found = ObjectType.Compare(xItem, yItem) == 0;
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
