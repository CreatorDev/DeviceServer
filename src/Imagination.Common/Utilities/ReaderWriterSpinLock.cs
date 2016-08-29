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
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
// We use plenty of interlocked operations on volatile fields below.  Safe.
#pragma warning disable 0420

namespace Imagination
{
	/// <summary>
	/// A very lightweight reader/writer lock.  It uses a single word of memory, and
	/// only spins when contention arises (no events are necessary).
	/// </summary>

	public class ReaderWriterSpinLock
	{

		private volatile int m_writer;
		private volatile ReadEntry[] m_readers = new ReadEntry[Environment.ProcessorCount * 16];
		[StructLayout(LayoutKind.Sequential, Size = 128)]
		struct ReadEntry
		{
			internal volatile int m_taken;
		}

		private int ReadLockIndex
		{
			get { return Thread.CurrentThread.ManagedThreadId % m_readers.Length; }
		}

		public void EnterReadLock()
		{
			SPW sw = new SPW();
			int tid = ReadLockIndex;
			// Wait until there are no writers.
			while (true)
			{
				while (m_writer == 1) sw.SpinOnce();
				// Try to take the read lock.
				Interlocked.Increment(ref m_readers[tid].m_taken);
				if (m_writer == 0)
				{
					// Success, no writer, proceed.
					break;
				}
				// Back off, to let the writer go through.
				Interlocked.Decrement(ref m_readers[tid].m_taken);
			}
		}


		public bool TryEnterReadLock(int timeOut)
		{
			bool result = false;
			SPW sw = new SPW();
			int tid = ReadLockIndex;
			// Wait until there are no writers.
			DateTime endTime = DateTime.Now.AddMilliseconds(timeOut);
			while (DateTime.Now < endTime)
			{
				while (m_writer == 1) sw.SpinOnce();
				// Try to take the read lock.
				Interlocked.Increment(ref m_readers[tid].m_taken);
				if (m_writer == 0)
				{
					// Success, no writer, proceed.
					result = true;
					break;
				}
				// Back off, to let the writer go through.
				Interlocked.Decrement(ref m_readers[tid].m_taken);
			}
			return result;
		}

		public void EnterWriteLock()
		{
			SPW sw = new SPW();
			while (true)
			{
				if (m_writer == 0 && Interlocked.Exchange(ref m_writer, 1) == 0)
				{
					// We now hold the write lock, and prevent new readers.
					// But we must ensure no readers exist before proceeding.
					for (int i = 0; i < m_readers.Length; i++)
						while (m_readers[i].m_taken != 0) sw.SpinOnce();
					break;
				}
				// We failed to take the write lock; wait a bit and retry.
				sw.SpinOnce();
			}
		}

		public bool TryEnterWriteLock(int timeOut)
		{
			bool result = false;
			SPW sw = new SPW();
			DateTime endTime = DateTime.Now.AddMilliseconds(timeOut);
			while (DateTime.Now < endTime)
			{
				if (m_writer == 0 && Interlocked.Exchange(ref m_writer, 1) == 0)
				{
					// We now hold the write lock, and prevent new readers.
					// But we must ensure no readers exist before proceeding.
					for (int i = 0; i < m_readers.Length; i++)
						while (m_readers[i].m_taken != 0) sw.SpinOnce();
					result = true;
					break;
				}
				// We failed to take the write lock; wait a bit and retry.
				sw.SpinOnce();
			}
			return result;
		}

		public void ExitReadLock()
		{
			// Just note that the current reader has left the lock.
			Interlocked.Decrement(ref m_readers[ReadLockIndex].m_taken);
		}

		public void ExitWriteLock()
		{
			// No need for a CAS.
			m_writer = 0;
		}
	}



	struct SPW
	{
		private int m_count;

		internal void SpinOnce()
		{
			if (m_count++ > 32)
			{
				Thread.Sleep(0);
			}
			else if (m_count > 12)
			{
				Thread.Sleep(5);
			}
			else
			{
				Thread.SpinWait(2 << m_count);
			}
		}
	}
}
