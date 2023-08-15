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
using UnityEngine;
using UnityEngine.Rendering;
using renoir;

namespace cohtml
{
	class MeshInfo
	{
		public MeshInfo()
		{
			m_Mesh = new Mesh();
			m_ShouldBeUpdated = true;
			m_CurrentSubMeshId = 0;
			m_SubMeshesToCheck = new SubMeshToCheckInfo[MAX_SUBMESHES_TO_CHECK];
			for (int i = 0; i < MAX_SUBMESHES_TO_CHECK; i++)
			{
				m_SubMeshesToCheck[i] = new SubMeshToCheckInfo();
			}
		}

		public void GetSubMeshId(SubMeshDescriptor desc, out int subMeshId)
		{
			for (int i = 1; i <= MAX_SUBMESHES_TO_CHECK; i++)
			{
				int index = (m_RingBufferIndex + i) % MAX_SUBMESHES_TO_CHECK;
				if (m_SubMeshesToCheck[index].m_Desc.indexCount == desc.indexCount &&
					m_SubMeshesToCheck[index].m_Desc.indexStart == desc.indexStart)
				{
					subMeshId = m_SubMeshesToCheck[(m_RingBufferIndex + i) % MAX_SUBMESHES_TO_CHECK].m_SubmeshId;
					return;
				}
			}

			if (m_CurrentSubMeshId >= m_Mesh.subMeshCount)
			{
				m_Mesh.subMeshCount *= 3;
			}

			m_SubMeshesToCheck[m_RingBufferIndex].m_Desc = desc;
			m_SubMeshesToCheck[m_RingBufferIndex].m_SubmeshId = m_CurrentSubMeshId;
			m_RingBufferIndex = (m_RingBufferIndex + 1) % MAX_SUBMESHES_TO_CHECK;

			m_Mesh.SetSubMesh(m_CurrentSubMeshId, desc);
			subMeshId = m_CurrentSubMeshId++;
		}

		public void Reset()
		{
			// We should mark meshes as dirty because in the next frame
			// the index/vertex data will be different and we should reupload them to the GPU
			m_Mesh.Clear();
			m_Mesh.subMeshCount = 1;
			m_ShouldBeUpdated = true;
			m_CurrentSubMeshId = 0;

			for (int i = 0; i < MAX_SUBMESHES_TO_CHECK; i++)
			{
				m_SubMeshesToCheck[i].Reset();
			}
		}

		public Mesh m_Mesh;
		public bool m_ShouldBeUpdated;
		public int m_CurrentSubMeshId;

		private const int MAX_SUBMESHES_TO_CHECK = 3;
		private int m_RingBufferIndex = 0;
		private SubMeshToCheckInfo[] m_SubMeshesToCheck;
	}

	class MeshBuffersInfo
	{
		public MeshBuffersInfo(
			VertexBufferObject vbo = new VertexBufferObject(),
			IndexBufferObject ibo = new IndexBufferObject()
			)
		{
			VBO = vbo;
			IBO = ibo;
		}

		public override string ToString()
		{
			return String.Format("Mesh Tuple(VBO: {0}, IBO: {1})", VBO.Id, IBO.Id);
		}

		public VertexBufferObject VBO;
		public IndexBufferObject IBO;
	}

	class MeshBufferInfoComparer : System.Collections.Generic.IEqualityComparer<MeshBuffersInfo>
	{
		public bool Equals(MeshBuffersInfo obj, MeshBuffersInfo obj2)
		{
			return	obj.VBO.Id == obj2.VBO.Id &&
					obj.IBO.Id == obj2.IBO.Id;
		}

		public int GetHashCode(MeshBuffersInfo obj)
		{
			uint hash = 23;
			hash = hash * 31 + obj.VBO.Id;
			hash = hash * 31 + obj.IBO.Id;
			return (int)(hash);
		}
	}
	
}
