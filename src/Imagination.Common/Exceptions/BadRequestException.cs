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
using System.Text;

namespace Imagination
{
    [global::System.Serializable]
    public class BadRequestException : Exception
    {
        private List<string> _InvalidFields;

        public BadRequestException() {}
        public BadRequestException(string message) : base(message) {}
		public BadRequestException(string message, string invalidField) : base(message) { AddInvalidField(invalidField); }
		public BadRequestException(string message, Exception inner) : base(message, inner) {}
		protected BadRequestException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) {_InvalidFields = new List<string>(); }

        private string _ErrorCode;

        public string ErrorCode
        {
            get { return _ErrorCode; }
            set { _ErrorCode = value; }
        }

        public List<string> InvalidFields
        {
            get { return _InvalidFields; }
        }

        public void AddInvalidField(string fieldName)
        {
			if (_InvalidFields == null)
				_InvalidFields = new List<string>(); 
            if (!_InvalidFields.Contains(fieldName))
                _InvalidFields.Add(fieldName);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
			if (_ErrorCode != null)
				sb.Append(" ErrorCode= " + _ErrorCode);
            if (_InvalidFields != null && _InvalidFields.Count > 0)
            {
                sb.Append("\nInvalidFields= ");

                for (int index = 0; index < _InvalidFields.Count; index++)
                {
                    sb.Append(_InvalidFields[index]);
                    if (index < _InvalidFields.Count - 1)
                        sb.Append(",");
                }
            }
			if (sb.Length > 0)
				sb.Append("\n");
			sb.Append(base.ToString());
            return sb.ToString();
        }
    }
}
