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
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using CoAP.Channel;

namespace Imagination.LWM2M
{
	public class FlowChannel : IChannel
	{
		private class RawData
		{
			public byte[] Data;
			public System.Net.EndPoint EndPoint;
		}

		private class UDPSocket : IDisposable
		{
			public readonly Socket Socket;
			public readonly byte[] Buffer;

			public UDPSocket(AddressFamily addressFamily, int bufferSize)
			{
				Socket = new Socket(addressFamily, SocketType.Dgram, ProtocolType.Udp);
                if (addressFamily == AddressFamily.InterNetworkV6)
                {
                    Socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, true);
                }
                Buffer = new byte[bufferSize];
			}

			public void Dispose()
			{
				Socket.Close();
			}
		}

		public const int DEFAULT_RECEIVE_PACKET_SIZE = 4096;

		private int _ReceiveBufferSize;
		private int _SendBufferSize;
		private int _ReceivePacketSize = DEFAULT_RECEIVE_PACKET_SIZE;
		private int _Port;
		private System.Net.EndPoint _LocalEndPoint;
		private UDPSocket _Socket;
		private UDPSocket _SocketIPv4;
		private int _Running;
		private int _Writing;
		private readonly ConcurrentQueue<RawData> _SendingQueue = new ConcurrentQueue<RawData>();

		public event EventHandler<DataReceivedEventArgs> DataReceived;


		public System.Net.EndPoint LocalEndPoint
		{
			get
			{
				return _Socket == null
					? (_LocalEndPoint ?? new IPEndPoint(IPAddress.IPv6Any, _Port))
					: _Socket.Socket.LocalEndPoint;
			}
		}

		public int ReceiveBufferSize
		{
			get { return _ReceiveBufferSize; }
			set { _ReceiveBufferSize = value; }
		}

		public int SendBufferSize
		{
			get { return _SendBufferSize; }
			set { _SendBufferSize = value; }
		}

		public int ReceivePacketSize
		{
			get { return _ReceivePacketSize; }
			set { _ReceivePacketSize = value; }
		}

		public FlowChannel()
			: this(0)
		{

		}

		public FlowChannel(int port)
		{
			_Port = port;
		}

		public FlowChannel(EndPoint localEP)
		{
			_LocalEndPoint = localEP;
		}

		public void Start()
		{
			if (System.Threading.Interlocked.CompareExchange(ref _Running, 1, 0) > 0)
				return;

			if (_LocalEndPoint == null)
			{
				try
				{
					_Socket = SetupUDPSocket(AddressFamily.InterNetworkV6, _ReceivePacketSize + 1); // +1 to check for > ReceivePacketSize
				}
				catch (SocketException e)
				{
					if (e.SocketErrorCode == SocketError.AddressFamilyNotSupported)
						_Socket = null;
					else
						throw e;
				}

				if (_Socket == null)
				{
					// IPv6 is not supported, use IPv4 instead
					_Socket = SetupUDPSocket(AddressFamily.InterNetwork, _ReceivePacketSize + 1);
					_Socket.Socket.Bind(new IPEndPoint(IPAddress.Any, _Port));
				}
				else
				{
					_SocketIPv4 = SetupUDPSocket(AddressFamily.InterNetwork, _ReceivePacketSize + 1);
					_Socket.Socket.Bind(new IPEndPoint(IPAddress.IPv6Any, _Port));
					if (_SocketIPv4 != null)
						_SocketIPv4.Socket.Bind(new IPEndPoint(IPAddress.Any, _Port));
				}
			}
			else
			{
				_Socket = SetupUDPSocket(_LocalEndPoint.AddressFamily, _ReceivePacketSize + 1);
				_Socket.Socket.Bind(_LocalEndPoint);
			}

			if (_ReceiveBufferSize > 0)
			{
				_Socket.Socket.ReceiveBufferSize = _ReceiveBufferSize;
				if (_SocketIPv4 != null)
					_SocketIPv4.Socket.ReceiveBufferSize = _ReceiveBufferSize;
			}
			if (_SendBufferSize > 0)
			{
				_Socket.Socket.SendBufferSize = _SendBufferSize;
				if (_SocketIPv4 != null)
					_SocketIPv4.Socket.SendBufferSize = _SendBufferSize;
			}

			BeginReceive();
		}

		public void Stop()
		{
			if (System.Threading.Interlocked.Exchange(ref _Running, 0) == 0)
				return;

			if (_Socket != null)
			{
				_Socket.Dispose();
				_Socket = null;
			}
			if (_SocketIPv4 != null)
			{
				_SocketIPv4.Dispose();
				_SocketIPv4 = null;
			}
		}

		public void Send(byte[] data, System.Net.EndPoint ep)
		{
			RawData raw = new RawData();
			raw.Data = data;
			raw.EndPoint = ep;
			_SendingQueue.Enqueue(raw);
			if (System.Threading.Interlocked.CompareExchange(ref _Writing, 1, 0) > 0)
				return;
			BeginSend();
		}

		public void Dispose()
		{
			Stop();
		}

		private void BeginReceive()
		{
			if (_Running > 0)
			{
				BeginReceive(_Socket);

				if (_SocketIPv4 != null)
					BeginReceive(_SocketIPv4);
			}
		}

		private void EndReceive(UDPSocket socket, byte[] buffer, int offset, int count, System.Net.EndPoint ep)
		{
			if (count > 0)
			{
				byte[] bytes = new byte[count];
				Buffer.BlockCopy(buffer, 0, bytes, 0, count);

				if (ep.AddressFamily == AddressFamily.InterNetworkV6)
				{
					IPEndPoint ipep = (IPEndPoint)ep;
					if (IPAddressExtensions.IsIPv4MappedToIPv6(ipep.Address))
						ipep.Address = IPAddressExtensions.MapToIPv4(ipep.Address);
				}

				FireDataReceived(bytes, ep);
			}

			BeginReceive(socket);
		}

		private void EndReceive(UDPSocket socket, Exception ex)
		{
			BeginReceive(socket);
		}

		private void FireDataReceived(byte[] data, System.Net.EndPoint ep)
		{
			EventHandler<DataReceivedEventArgs> h = DataReceived;
			if (h != null)
				h(this, new DataReceivedEventArgs(data, ep));
		}

		private void BeginSend()
		{
			if (_Running == 0)
				return;

			RawData raw;
			if (!_SendingQueue.TryDequeue(out raw))
			{
				System.Threading.Interlocked.Exchange(ref _Writing, 0);
				return;
			}

			UDPSocket socket = _Socket;
			IPEndPoint remoteEP = (IPEndPoint)raw.EndPoint;

			if (remoteEP.AddressFamily == AddressFamily.InterNetwork)
			{
				if (_SocketIPv4 != null)
				{
					// use the separated socket of IPv4 to deal with IPv4 conversions.
					socket = _SocketIPv4;
				}
				else if (_Socket.Socket.AddressFamily == AddressFamily.InterNetworkV6)
				{
					remoteEP = new IPEndPoint(IPAddressExtensions.MapToIPv6(remoteEP.Address), remoteEP.Port);
				}
			}

			BeginSend(socket, raw.Data, remoteEP);
		}

		private void EndSend(UDPSocket socket, int bytesTransferred)
		{
			BeginSend();
		}

		private void EndSend(UDPSocket socket, Exception ex)
		{
			BeginSend();
		}

		private UDPSocket SetupUDPSocket(AddressFamily addressFamily, int bufferSize)
		{
			UDPSocket socket = NewUDPSocket(addressFamily, bufferSize);
			if (Environment.OSVersion.Platform != PlatformID.Unix)
			{
				// do not throw SocketError.ConnectionReset by ignoring ICMP Port Unreachable
				const int SIO_UDP_CONNRESET = -1744830452;
				socket.Socket.IOControl(SIO_UDP_CONNRESET, new byte[] { 0 }, null);
			}
			return socket;
		}

		private UDPSocket NewUDPSocket(AddressFamily addressFamily, int bufferSize)
		{
			return new UDPSocket(addressFamily, bufferSize);
		}

		private void BeginReceive(UDPSocket socket)
		{
			if (_Running == 0)
				return;

			System.Net.EndPoint remoteEP = new IPEndPoint(
					socket.Socket.AddressFamily == AddressFamily.InterNetwork ?
					IPAddress.Any : IPAddress.IPv6Any, 0);

			try
			{
				socket.Socket.BeginReceiveFrom(socket.Buffer, 0, socket.Buffer.Length,
					SocketFlags.None, ref remoteEP, ReceiveCallback, socket);
			}
			catch (ObjectDisposedException)
			{
				// do nothing
			}
			catch (Exception ex)
			{
				EndReceive(socket, ex);
			}
		}

		private void ReceiveCallback(IAsyncResult ar)
		{
			UDPSocket socket = (UDPSocket)ar.AsyncState;
			System.Net.EndPoint remoteEP = new IPEndPoint(socket.Socket.AddressFamily == AddressFamily.InterNetwork ? IPAddress.Any : IPAddress.IPv6Any, 0);

			int count = 0;
			try
			{
				count = socket.Socket.EndReceiveFrom(ar, ref remoteEP);
			}
			catch (ObjectDisposedException)
			{
				// do nothing
				return;
			}
			catch (Exception ex)
			{
				EndReceive(socket, ex);
				return;
			}

			EndReceive(socket, socket.Buffer, 0, count, remoteEP);
		}

		private void BeginSend(UDPSocket socket, byte[] data, System.Net.EndPoint destination)
		{
			try
			{
				socket.Socket.BeginSendTo(data, 0, data.Length, SocketFlags.None, destination, SendCallback, socket);
			}
			catch (ObjectDisposedException)
			{
				// do nothing
			}
			catch (Exception ex)
			{
				EndSend(socket, ex);
			}
		}

		private void SendCallback(IAsyncResult ar)
		{
			UDPSocket socket = (UDPSocket)ar.AsyncState;

			int written;
			try
			{
				written = socket.Socket.EndSendTo(ar);
			}
			catch (ObjectDisposedException)
			{
				// do nothing
				return;
			}
			catch (Exception ex)
			{
				EndSend(socket, ex);
				return;
			}

			EndSend(socket, written);
		}
	}
}
