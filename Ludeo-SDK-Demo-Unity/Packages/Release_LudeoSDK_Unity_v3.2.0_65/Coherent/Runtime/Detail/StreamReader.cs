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

using System.Runtime.InteropServices;
using System;
using System.IO;
using System.Collections.Generic;
using cohtml.Net;

namespace cohtml
{
public class StreamReader : UnitySyncStreamReader
{
	//Prevents GC to collect stream readers in use
	static List<StreamReader> s_StreamReaders;

	FileStream m_FileStream;
	byte[] m_FileBytes;

	static StreamReader()
	{
		s_StreamReaders = new List<StreamReader>();
	}

	public StreamReader(string path)
	{
		s_StreamReaders.Add(this);
		m_FileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
	}

	public StreamReader(byte[] bytes)
	{
		s_StreamReaders.Add(this);
		m_FileBytes = bytes;
	}

	public override void Close()
	{
		if (m_FileStream != null)
		{
			m_FileStream.Close();
		}

		s_StreamReaders.Remove(this);
		Dispose();
	}

	public override uint GetSize()
	{
		if (m_FileStream != null)
		{
			return (uint)m_FileStream.Length;
		}

		return (uint)m_FileBytes.Length;
	}

	public override uint Read(uint offset, IntPtr buffer, uint count)
	{
		if (m_FileStream != null)
		{
			if (count == 0)
			{
				m_FileStream.Seek(offset, SeekOrigin.Begin);
				return 0;
			}

			byte[] managedBuffer = new byte[count];
			int bytesRead = m_FileStream.Read(managedBuffer, 0, (int)count);

			Marshal.Copy(managedBuffer, 0, buffer, bytesRead);

			if (bytesRead == 0)
			{
				return (uint)m_FileStream.Length;
			}

			return (uint)bytesRead;
		}
		else
		{
			if (count != 0)
			{
				Marshal.Copy(m_FileBytes, (int)offset, buffer, (int)count);
			}

			return count;
		}
	}
}
}
