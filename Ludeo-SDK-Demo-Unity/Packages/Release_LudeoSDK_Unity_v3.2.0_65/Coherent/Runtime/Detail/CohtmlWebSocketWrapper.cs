/*
This file is part of Cohtml, Gameface and Prysm - modern user interface technologies.

Copyright (c) 2012-2023 Coherent Labs AD and/or its licensors. All
rights reserved in all media.

The coded instructions, statements, computer programs, and/or related
material (collectively the "Data") in these files contain confidential
and unpublished information proprietary Coherent Labs and/or its
licensors, which is protected by United States of America federal
copyright law and by international treaties.

This software or source code is supplied under the terms of a license
agreement and nondisclosure agreement with Coherent Labs AD and may
not be copied, disclosed, or exploited except in accordance with the
terms of that agreement. The Data may not be disclosed or distributed to
third parties, in whole or in part, without the prior written consent of
Coherent Labs AD.

COHERENT LABS MAKES NO REPRESENTATION ABOUT THE SUITABILITY OF THIS
SOURCE CODE FOR ANY PURPOSE. THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT
HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
MERCHANTABILITY, NONINFRINGEMENT, AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER, ITS AFFILIATES,
PARENT COMPANIES, LICENSORS, SUPPLIERS, OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
ANY WAY OUT OF THE USE OR PERFORMANCE OF THIS SOFTWARE OR SOURCE CODE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using cohtml.Net;

namespace cohtml
{
public class CohtmlWebSocketWrapper : IClientSideSocket
{
	public struct RequestData
	{
		public RequestData(string message, ArraySegment<byte> bytes)
		{
			Message = message;
			Bytes = bytes;
		}

		public string Message { get; set; }
		public ArraySegment<byte> Bytes { get; set; }
	}

	private readonly ClientWebSocket m_WebSocket;
	private Thread m_WebSocketThread;
	private readonly Thread m_SendThread;
	private readonly Thread m_ReceiveThread;

	private BlockingCollection<RequestData> m_SendQueue;

	public ISocketListener Listener { get; }

	public Uri ServerUrl { get; }

	public string[] Protocols { get; }

	private bool IsSocketConnected => m_WebSocket.State == WebSocketState.Open;

		private event Action m_onOpened;

	public CohtmlWebSocketWrapper(ISocketListener listener, string serverUrl, string[] protocols, Action onOpened)
	{
			m_onOpened = onOpened;

		Listener = listener;
		ServerUrl = new Uri(serverUrl);
		Protocols = protocols;

		m_WebSocket = new ClientWebSocket();
		m_SendQueue = new BlockingCollection<RequestData>();

		m_WebSocketThread = new Thread(ConnectToServerAsync);
		m_WebSocketThread.Start();

		m_SendThread = new Thread(SendToServerAsync);
		m_SendThread.Start();

		m_ReceiveThread = new Thread(ReceiveFromServerAsync);
		m_ReceiveThread.Start();
	}

	public override void Send(string message, uint length)
	{
		RequestData data = new RequestData(message, new ArraySegment<byte>(Utils.EncodeString(message)));
		m_SendQueue.Add(data);
	}

	public override void Close(uint code, string reason, uint length)
	{
		CloseConnectionToServer();
	}

	public override void Terminate(uint code, string reason, uint length)
	{
        reason = null;
		m_WebSocketThread.Abort();
		m_WebSocketThread = new Thread(() => TerminateServerConnectionAsync(code, reason));
		m_WebSocketThread.Start();
	}

	public async void ConnectToServerAsync()
	{
		await m_WebSocket.ConnectAsync(ServerUrl, CancellationToken.None);
		while (m_WebSocket.State == WebSocketState.Connecting)
		{
			Task.Delay(50).Wait();
		}

		if (IsSocketConnected)
		{
			Listener.OnOpen();

				if (m_onOpened != null)
					m_onOpened();
		}

		m_WebSocketThread = new Thread(ObserveServerConnection);
		m_WebSocketThread.Start();
	}

	public void CloseConnectionToServer()
	{
		string message = "The connection has closed after the request was fulfilled.";
		Listener.OnClose((uint)WebSocketCloseStatus.NormalClosure, message, (uint)message.Length);
		m_WebSocketThread.Abort();
		m_WebSocket.Abort();
	}

	private async void TerminateServerConnectionAsync(uint code, string reason)
	{
		m_ReceiveThread.Abort();
		m_SendThread.Abort();

			try
			{
				await m_WebSocket.CloseAsync(WebSocketCloseStatus.Empty, reason, CancellationToken.None);
			}
			catch(Exception e)
            {
				// Need to print THIS (if not in main thread - may not be printed by Unity's Debug.Log!)
            }
			finally
            {
				m_WebSocket.Dispose();
			}		
	}

	private async void SendToServerAsync()
	{
		RequestData data;
		while (true)
		{
			if (IsSocketConnected)
			{
				while (!m_SendQueue.IsCompleted)
				{
					data = m_SendQueue.Take();
					await m_WebSocket.SendAsync(data.Bytes, WebSocketMessageType.Text, true, CancellationToken.None);
					Listener.OnMessage(data.Message, (uint)data.Message.Length);
				}
			}
			else
			{
				Task.Delay(50).Wait();
			}
		}
	}

	private async void ReceiveFromServerAsync()
	{
		string result;
		
		while (true)
		{
			if (IsSocketConnected)
			{
				result = await ReceiveStream();
				if (!string.IsNullOrEmpty(result))
				{
					Listener.OnMessage(result, (uint)result.Length);
				}
				else
				{
					Task.Delay(50).Wait();
				}
			}
		}
	}

	private void ObserveServerConnection()
	{
		while (true)
		{
			if (!IsSocketConnected)
			{
				//Shutdown();
				break;
			}

			Task.Delay(50).Wait();
		}
	}

	private async Task<string> ReceiveStream()
	{
		var ms = new MemoryStream();
		byte[] buf = new byte[4 * 1024];
		ArraySegment<byte> arrayBuf = new ArraySegment<byte>(buf);
		WebSocketReceiveResult chunkResult;

		if (IsSocketConnected)
		{
			do
			{
				chunkResult = await m_WebSocket.ReceiveAsync(arrayBuf, CancellationToken.None);
				ms.Write(arrayBuf.Array, arrayBuf.Offset, chunkResult.Count);
			} while (!chunkResult.EndOfMessage && IsSocketConnected);

			ms.Seek(0, SeekOrigin.Begin);
			if (chunkResult.MessageType == WebSocketMessageType.Text)
			{
				string readString = "";
				using (var reader = new System.IO.StreamReader(ms, Encoding.UTF8))
				{
					readString = reader.ReadToEnd();
				}

				return readString;
			}
			else if(chunkResult.MessageType == WebSocketMessageType.Binary)
            {
                string textData = System.Text.Encoding.UTF8.GetString(ms.ToArray());
				return textData;
            }
		}

		return string.Empty;
	}

		private void Shutdown()
        {
			uint closeStatus = m_WebSocket.CloseStatus != null ? (uint)m_WebSocket.CloseStatus : 0;
			string closeStatusDescription = m_WebSocket.CloseStatusDescription;
			uint closeStatusDescriptionLength = !string.IsNullOrEmpty(closeStatusDescription) ? (uint)closeStatusDescription.Length : 0;

			Close(closeStatus, closeStatusDescription, closeStatusDescriptionLength);
		}

		public void OnApplicationQuit()
		{
			Shutdown();
		}
	}
}