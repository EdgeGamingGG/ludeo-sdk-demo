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
using System.Runtime.InteropServices;
using renoir;

namespace cohtml
{
	[AttributeUsage(AttributeTargets.Method)]
	public class MonoPInvokeCallbackAttribute : Attribute
	{
		public MonoPInvokeCallbackAttribute(Type t)
		{
		}
	}

	internal delegate void ManagedCallbackAOT();

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	delegate void FillCapsCallback();

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	delegate void WrapUserRenderTargetCallback(
		IntPtr userObject,
		renoir.Texture2D description,
		Texture2DObject texture,
		IntPtr depthStencil,
		DepthStencilTexture dsDescription,
		DepthStencilTextureObject dsObject);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	delegate void WrapUserTextureCallback(
		IntPtr userObject,
		renoir.Texture2D description,
		Texture2DObject obj);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	delegate bool CreateTextureCallback(
		Texture2DObject texture,
		renoir.Texture2D description,
		IntPtr data,
		uint dataLen);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	delegate void DestroyTextureCallback(Texture2DObject texture);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	delegate void UpdateTextureCallback(
		Texture2DObject texture,
		renoir.Texture2D description,
		IntPtr boxes,
		IntPtr newBytes,
		uint count,
		bool willOverwrite);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	delegate bool CreateDepthStencilTextureCallback(
		DepthStencilTextureObject texture,
		DepthStencilTexture description);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	delegate void DestroyDepthStencilTextureCallback(DepthStencilTextureObject texture);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	delegate bool CreateVertexBufferCallback(
		VertexType type,
		uint count,
		VertexBufferObject obj,
		bool changesOften);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	delegate bool CreateIndexBufferCallback(
		IndexBufferType format,
		uint count,
		IndexBufferObject obj,
		bool changesOften);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	delegate IntPtr MapVertexBufferCallback(VertexBufferObject obj);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	delegate IntPtr MapIndexBufferCallback(IndexBufferObject obj);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	delegate void UnmapVertexBufferCallback(VertexBufferObject obj, uint elementCount);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	delegate void UnmapIndexBufferCallback(IndexBufferObject obj, uint elementCount);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	delegate void DestroyVertexBufferCallback(VertexBufferObject obj);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	delegate void DestroyIndexBufferCallback(IndexBufferObject obj);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	delegate bool CreateSampler2DCallback(Sampler2DObject sampler, Sampler2D description);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	delegate void DestroySampler2DCallback(Sampler2DObject sampler);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	delegate bool CreatePipelineStateCallback(PipelineState state, PipelineStateObject obj);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	delegate void DestroyPipelineStateCallback(PipelineStateObject sampler);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	delegate bool CreateConstantBufferCallback(CBType type, ConstantBufferObject obj, uint size);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	delegate void DestroyConstantBufferCallback(ConstantBufferObject obj);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	delegate void ExecuteRenderingCallback(
		IntPtr buffers,
		uint buffersCount,
		IntPtr cbUpdates,
		uint cbUpdatesCount);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	delegate void ExecuteResourceCommandsCallback(
		IntPtr buffers,
		uint buffersCount,
		ResourcesCommandsStage stage);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	delegate void BeginCommandsCallback();

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	delegate void EndCommandsCallback();

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	delegate void SetDebugNameCallback(Texture2DObject texture, string name);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	delegate void SetDebugNameDSCallback(DepthStencilTextureObject texture, string name);
}
