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
using System.ComponentModel;

namespace Imagination.Model
{
	[Serializable]
	public class ModelBase : INotifyPropertyChanged
	{
		protected TObjectState _ObjectState;
		protected int _UpdateID;
		private bool _UpdateState;

		public event EventHandler ObjectStateChanged;
		public event PropertyChangedEventHandler PropertyChanged;

		public ModelBase()
		{
			_UpdateState = true;
		}

		public TObjectState ObjectState
		{
			get { return _ObjectState; }
			set
			{
				bool changed = (_ObjectState != value);
				_ObjectState = value;
				if (changed)
					OnObjectStateChanged(this, null);
			}
		}

		public int UpdateID
		{
			get { return _UpdateID; }
			set { _UpdateID = value; }
		}

		public virtual void ClearState()
		{
			this.ObjectState = TObjectState.NotChanged;
			_UpdateState = true;
		}

		protected virtual void OnObjectStateChanged(object sender, EventArgs e)
		{
			if (ObjectStateChanged != null)
			{
				ObjectStateChanged(sender, e);
			}
		}

		protected void RaisePropertyChanged(String info)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(info));
			}
		}


		protected bool CheckPropertyChanged(IComparable originalValue, IComparable newValue)
		{
			bool result = false;
			if ((originalValue == null) && (newValue != null))
				result = true;
			else if ((originalValue != null) && (newValue == null))
				result = true;
			else if ((originalValue != null) && (originalValue.CompareTo(newValue) != 0))
				result = true;
			if (_UpdateState && (_ObjectState == TObjectState.NotChanged) && result)
				this.ObjectState = TObjectState.Update;
			return result;
		}

		protected bool CheckPropertyChanged(int? originalValue, int? newValue)
		{
			bool result = false;
			if (originalValue.HasValue && newValue.HasValue && (originalValue.Value != newValue.Value))
				result = true;
			else if (originalValue.HasValue && !newValue.HasValue)
				result = true;
			else if (!originalValue.HasValue && newValue.HasValue)
				result = true;
			if (_UpdateState && (_ObjectState == TObjectState.NotChanged) && result)
				this.ObjectState = TObjectState.Update;
			return result;
		}

		protected bool CheckPropertyChanged(bool? originalValue, bool? newValue)
		{
			bool result = false;
			if (originalValue.HasValue && newValue.HasValue && (originalValue.Value != newValue.Value))
				result = true;
			else if (originalValue.HasValue && !newValue.HasValue)
				result = true;
			else if (!originalValue.HasValue && newValue.HasValue)
				result = true;
			if (_UpdateState && (_ObjectState == TObjectState.NotChanged) && result)
				this.ObjectState = TObjectState.Update;
			return result;
		}

		protected bool CheckPropertyChanged(double? originalValue, double? newValue)
		{
			bool result = false;
			if (originalValue.HasValue && newValue.HasValue && (originalValue.Value != newValue.Value))
				result = true;
			else if (originalValue.HasValue && !newValue.HasValue)
				result = true;
			else if (!originalValue.HasValue && newValue.HasValue)
				result = true;
			if (_UpdateState && (_ObjectState == TObjectState.NotChanged) && result)
				this.ObjectState = TObjectState.Update;
			return result;
		}

		protected bool CheckPropertyChanged(DateTime? originalValue, DateTime? newValue)
		{
			bool result = false;
			if (originalValue.HasValue && newValue.HasValue && (originalValue.Value != newValue.Value))
				result = true;
			else if (originalValue.HasValue && !newValue.HasValue)
				result = true;
			else if (!originalValue.HasValue && newValue.HasValue)
				result = true;
			if (_UpdateState && (_ObjectState == TObjectState.NotChanged) && result)
				this.ObjectState = TObjectState.Update;
			return result;
		}

		protected bool CheckPropertyChanged(byte[] originalValue, byte[] newValue)
		{
			bool result = false;
			if (originalValue != null && newValue != null)
			{
				if (originalValue.Length != newValue.Length)
					result = true;
				else
				{
					for (int index = 0; index < originalValue.Length; index++)
					{
						if (originalValue[index] != newValue[index])
						{
							result = true;
							break;
						}
					}
				}
			}
			else if (originalValue != null && newValue == null)
				result = true;
			else if (originalValue == null && newValue != null)
				result = true;
			if (_UpdateState && (_ObjectState == TObjectState.NotChanged) && result)
				this.ObjectState = TObjectState.Update;
			return result;
		}

	}
}
