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

/*
This file is part of Cohtml, Gameface and Prysm -
modern user interface technologies.

Copyright (c) 2012-2020 Coherent Labs AD and/or its licensors. All
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

#if UNITY_2019_3_OR_NEWER

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using renoir;

namespace cohtml
{
	public class UnityBackend
	{
		private static UnityBackend s_UnityBackendInstance = null;

		private Dictionary<Texture2DObject, RTImpl> m_UserRTs;
		private Dictionary<Texture2DObject, Texture> m_Textures;
		private UnityEngine.Texture2D m_DummyTexture;
		private HashSet<Texture2DObject> m_UserTextures;
		private Dictionary<DepthStencilTextureObject, RenderTexture> m_DepthStencilTextures;
		private Dictionary<Sampler2DObject, Sampler2D> m_Samplers;
		private Dictionary<PipelineStateObject, PipelineState> m_PipelineStates;
		private Dictionary<VertexBufferObject, StandardVertexBuffer> m_StandardVertexBuffers;
		private Dictionary<VertexBufferObject, SlimVertexBuffer> m_SlimVertexBuffers;
		private Dictionary<IndexBufferObject, ByteIndexBuffer> m_ByteIndexBuffers;
		private Dictionary<IndexBufferObject, ShortIndexBuffer> m_ShortIndexBuffers;
		private Dictionary<IndexBufferObject, IntegerIndexBuffer> m_IntegerIndexBuffers;

		private ConstantBuffers m_CBs;

		private MeshBuffersInfo m_CurrentMeshBuffers;
		private Dictionary<MeshBuffersInfo, MeshInfo> m_Meshes;

		private PipelineStateInfo m_CurrentPipelineState;
		private Dictionary<PipelineStateInfo, Material> m_Materials;
		private Material m_MaterialRenoir;
		private Shader m_RenoirShader;
		private MaterialPropertyBlock m_PropertyBlock;
		private CommandBuffer m_CommandBuffer;

		private ushort m_ShaderPass = 0;

		private Texture2DObject[] m_BoundPSTextures;
		private Sampler2DObject[] m_BoundPSTexturesSamplers;

		private const uint MaxTexturesCount = 5u;
		private const uint RenoirInvalidId = uint.MaxValue;

		private VertexAttributeDescriptor[] m_StandartVertexLayout = new[] {
			new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 4),
			new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 4),
			new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 4),
		};

		private VertexAttributeDescriptor[] m_SlimVertexLayout = new[] {
			new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 4),
		};

		public Dictionary<Texture2DObject, Texture> Textures => m_Textures;

		public UnityBackend()
		{}

		public void Initialize()
		{
			#if COHERENT_CPU_32BIT
			bool arch86Defined = true;
			#else
			bool arch86Defined = false;
			#endif

			if (Environment.Is64BitProcess == false && arch86Defined)
			{
				Debug.LogError("The script define 'COHERENT_CPU_32BIT' is not set properly!");
			}

			m_UserRTs = new Dictionary<Texture2DObject, RTImpl>(new TextureObjComparer());
			m_Textures = new Dictionary<Texture2DObject, Texture>(new TextureObjComparer());
			m_DummyTexture = new UnityEngine.Texture2D(1, 1);
			m_DummyTexture.name = "DummyTexture";
			m_UserTextures = new HashSet<Texture2DObject>(new TextureObjComparer());
			m_DepthStencilTextures = new Dictionary<DepthStencilTextureObject, RenderTexture>(new DSTextureObjComparer());
			m_Samplers = new Dictionary<Sampler2DObject, Sampler2D>(new SamplerObjComparer());
			m_PipelineStates = new Dictionary<PipelineStateObject, PipelineState>(new PSOComparer());
			m_StandardVertexBuffers = new Dictionary<VertexBufferObject, StandardVertexBuffer>(new VBOComparer());
			m_SlimVertexBuffers = new Dictionary<VertexBufferObject, SlimVertexBuffer>(new VBOComparer());
			m_ByteIndexBuffers = new Dictionary<IndexBufferObject, ByteIndexBuffer>(new IBOComparer());
			m_ShortIndexBuffers = new Dictionary<IndexBufferObject, ShortIndexBuffer>(new IBOComparer());
			m_IntegerIndexBuffers = new Dictionary<IndexBufferObject, IntegerIndexBuffer>(new IBOComparer());
			m_CBs = new ConstantBuffers();
			m_BoundPSTextures = new Texture2DObject[MaxTexturesCount];
			m_BoundPSTexturesSamplers = new Sampler2DObject[MaxTexturesCount];

			m_CurrentMeshBuffers = new MeshBuffersInfo();
			m_Meshes = new Dictionary<MeshBuffersInfo, MeshInfo>(new MeshBufferInfoComparer());

			m_CurrentPipelineState = new PipelineStateInfo();
			m_Materials = new Dictionary<PipelineStateInfo, Material>(new PSOInfoComparer());

			m_RenoirShader = Shader.Find("Coherent/RenoirShader");
			m_MaterialRenoir = new Material(m_RenoirShader);
			CompileAllShaderPasses(m_MaterialRenoir);

			m_PropertyBlock = new MaterialPropertyBlock();
			m_CommandBuffer = new CommandBuffer();
			m_CommandBuffer.name = "RenoirUnityBackend";

			SendManagedFunctionsToNative();
		}

		void CompileAllShaderPasses(Material material)
		{
#if UNITY_EDITOR
			for (int i = 0; i < BackendUtilities.ALL_SHADER_PASSES_COUNT; i++)
			{
				UnityEditor.ShaderUtil.CompilePass(material, i);
			}
#endif
		}

		#if UNITY_IOS && !UNITY_EDITOR
        private static void SetUnityBackendBuffersFunctions(
            CreateVertexBufferCallback createVB,
            CreateIndexBufferCallback createIB,
            MapVertexBufferCallback mapVP,
            MapIndexBufferCallback mapIB,
            UnmapVertexBufferCallback unmapVP,
            UnmapIndexBufferCallback unmapIB,
            DestroyVertexBufferCallback destroyVP,
            DestroyIndexBufferCallback destroyIB)
        {
            Debug.LogError("Symbols are missing from the library! Don't use C# backend, use the native instead!");
        }

        private static void SetUnityBackendTextureFunctions(
            WrapUserRenderTargetCallback wrap,
            WrapUserTextureCallback wrapUserTexture,
            CreateTextureCallback createTexture,
            DestroyTextureCallback destroyTexture,
            UpdateTextureCallback updateTexture,
            CreateDepthStencilTextureCallback createDS,
            DestroyDepthStencilTextureCallback destroyDS,
            SetDebugNameCallback setDebugName,
            SetDebugNameDSCallback setDebugNameDS
            )
        {
            Debug.LogError("Symbols are missing from the library! Don't use C# backend, use the native instead!");
        }

        private static void SetUnityBackendFunctions(
            FillCapsCallback fillCaps,
            CreateSampler2DCallback createSampler,
            DestroySampler2DCallback destroySampler,
            CreatePipelineStateCallback createPSO,
            DestroyPipelineStateCallback destroyPSO,
            CreateConstantBufferCallback createCB,
            DestroyConstantBufferCallback destroyCB,
            ExecuteRenderingCallback execute,
            ExecuteResourceCommandsCallback executeResourceCommands,
            BeginCommandsCallback beginCommands,
            EndCommandsCallback endCommands
            )
        {
            Debug.LogError("Symbols are missing from the library! Don't use C# backend, use the native instead!");
        }

        private static void SetUnityRendererCaps(RendererCaps caps)
        {
            Debug.LogError("Symbols are missing from the library! Don't use C# backend, use the native instead!");
        }
		#else
		[DllImport(CohtmlDllImport.DLL, CallingConvention = CallingConvention.Cdecl)]
		private static extern void SetUnityBackendBuffersFunctions(
			CreateVertexBufferCallback createVB,
			CreateIndexBufferCallback createIB,
			MapVertexBufferCallback mapVP,
			MapIndexBufferCallback mapIB,
			UnmapVertexBufferCallback unmapVP,
			UnmapIndexBufferCallback unmapIB,
			DestroyVertexBufferCallback destroyVP,
			DestroyIndexBufferCallback destroyIB
			);

		[DllImport(CohtmlDllImport.DLL, CallingConvention = CallingConvention.Cdecl)]
		private static extern void SetUnityBackendTextureFunctions(
			WrapUserRenderTargetCallback wrap,
			WrapUserTextureCallback wrapUserTexture,
			CreateTextureCallback createTexture,
			DestroyTextureCallback destroyTexture,
			UpdateTextureCallback updateTexture,
			CreateDepthStencilTextureCallback createDS,
			DestroyDepthStencilTextureCallback destroyDS,
			SetDebugNameCallback setDebugName,
			SetDebugNameDSCallback setDebugNameDS
			);

		[DllImport(CohtmlDllImport.DLL, CallingConvention = CallingConvention.Cdecl)]
		private static extern void SetUnityBackendFunctions(
			FillCapsCallback fillCaps,
			CreateSampler2DCallback createSampler,
			DestroySampler2DCallback destroySampler,
			CreatePipelineStateCallback createPSO,
			DestroyPipelineStateCallback destroyPSO,
			CreateConstantBufferCallback createCB,
			DestroyConstantBufferCallback destroyCB,
			ExecuteRenderingCallback execute,
			ExecuteResourceCommandsCallback executeResourceCommands,
			BeginCommandsCallback beginCommands,
			EndCommandsCallback endCommands
			);

		[DllImport(CohtmlDllImport.DLL, CallingConvention = CallingConvention.Cdecl)]
		private static extern void SetUnityRendererCaps(RendererCaps caps);
		#endif

		private void SendManagedFunctionsToNative()
		{
			s_UnityBackendInstance = this;

			SetUnityBackendBuffersFunctions(
				CreateVertexBufferWrapper,
				CreateIndexBufferWrapper,
				MapVertexBufferWrapper,
				MapIndexBufferWrapper,
				UnmapVertexBufferWrapper,
				UnmapIndexBufferWrapper,
				DestroyVertexBufferWrapper,
				DestroyIndexBufferWrapper);

			SetUnityBackendTextureFunctions(
				WrapUserRenderTargetWrapper,
				WrapUserTextureWrapper,
				CreateTextureWrapper,
				DestroyTextureWrapper,
				UpdateTextureWrapper,
				CreateDepthStencilTextureWrapper,
				DestroyDepthStencilTextureWrapper,
				SetDebugNameWrapper,
				SetDebugNameDSWrapper);

			SetUnityBackendFunctions(
				FillCapsWrapper,
				CreateSampler2DWrapper,
				DestroySampler2DWrapper,
				CreatePipelineStateWrapper,
				DestroyPipelineStateWrapper,
				CreateConstantBufferWrapper,
				DestroyConstantBufferWrapper,
				ExecuteRenderingWrapper,
				ExecuteResourceCommandsWrapper,
				BeginCommandsWrapper,
				EndCommandsWrapper);
		}

		[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
		private static void FillCapsWrapper()
		{
			s_UnityBackendInstance.FillCaps();
		}

		private void FillCaps()
		{
			RendererCaps outCaps = new RendererCaps();

			outCaps.SupportsMSAATextures = true;
			outCaps.RequiresMSAAResolve = true;
			outCaps.SupportsA8RenderTarget = true;
			outCaps.RequiresRBSwapForImages = false;
			outCaps.RequiresDrawFences = false;
			outCaps.RequiresShaderTypeInShader = true;
			outCaps.RequiresYFlipForLayers = false;
			outCaps.SupportsNPOTTextureOps = true;
			outCaps.ShaderChangeRequiresResourcesRebind = false;
			outCaps.SupportsOnly16bitIndices = false;
			outCaps.PreferCPUWorkload = false;
			outCaps.ShouldUseRenderPasses = false;
			outCaps.ConstantBufferRingSize = 1;
			outCaps.ConstantBufferBlockAlignment = 1;
			outCaps.ConstantBufferBlocksCount = 1;
			outCaps.ShouldClearRTWithClearQuad = true;
			outCaps.CanOutputDepthInPixelShader = true;
			outCaps.SupportsTwoSidedStencilOperations = true;
			outCaps.EndingRenderPassRequiresStateUpdate = false;
			outCaps.SupportsResourcesStateTransitions = false;

			// There is warning that Unity outpus when you try to share some IB across different
			// "submeshes"; without this set to false, a big portion of the cohtml's rendering
			// flow will use a shared index buffer for the differnet geometries
			outCaps.SupportsSharedIndexBuffers = false;

			outCaps.MaxTextureWidth = (uint)SystemInfo.maxTextureSize;
			outCaps.MaxTextureHeight = (uint)SystemInfo.maxTextureSize;

			outCaps.ShaderMapping = new ShaderType[(int)ShaderType.ST_Count];
			outCaps.ShaderMapping[(int)ShaderType.ST_Standard] = ShaderType.ST_Standard;
			outCaps.ShaderMapping[(int)ShaderType.ST_StandardCircle] = ShaderType.ST_StandardRare;
			outCaps.ShaderMapping[(int)ShaderType.ST_StandardStrokeCircle] = ShaderType.ST_StandardRare;
			outCaps.ShaderMapping[(int)ShaderType.ST_StandardEllipse] = ShaderType.ST_StandardRare;
			outCaps.ShaderMapping[(int)ShaderType.ST_StandardStrokeEllipse] = ShaderType.ST_StandardRare;
			outCaps.ShaderMapping[(int)ShaderType.ST_StandardRRect] = ShaderType.ST_StandardRare;
			outCaps.ShaderMapping[(int)ShaderType.ST_StandardStrokeRRect] = ShaderType.ST_StandardRare;
			outCaps.ShaderMapping[(int)ShaderType.ST_StandardTexture] = ShaderType.ST_Standard;
			outCaps.ShaderMapping[(int)ShaderType.ST_Path] = ShaderType.ST_Path;
			outCaps.ShaderMapping[(int)ShaderType.ST_HairlinePath] = ShaderType.ST_Path;
			outCaps.ShaderMapping[(int)ShaderType.ST_Hairline] = ShaderType.ST_StandardRare;
			outCaps.ShaderMapping[(int)ShaderType.ST_Stencil] = ShaderType.ST_Stencil;
			outCaps.ShaderMapping[(int)ShaderType.ST_StencilRRect] = ShaderType.ST_StencilRare;
			outCaps.ShaderMapping[(int)ShaderType.ST_StencilCircle] = ShaderType.ST_StencilRare;
			outCaps.ShaderMapping[(int)ShaderType.ST_StencilTexture] = ShaderType.ST_Stencil;
			outCaps.ShaderMapping[(int)ShaderType.ST_StencilPath] = ShaderType.ST_StencilPath;
			outCaps.ShaderMapping[(int)ShaderType.ST_StencilAnimatedPathBezier] = ShaderType.ST_StencilPath;
			outCaps.ShaderMapping[(int)ShaderType.ST_StencilAnimatedPathTriangle] = ShaderType.ST_StencilPath;
			outCaps.ShaderMapping[(int)ShaderType.ST_Text] = ShaderType.ST_Standard;
			outCaps.ShaderMapping[(int)ShaderType.ST_TextSDF] = ShaderType.ST_Standard;
			outCaps.ShaderMapping[(int)ShaderType.ST_TextStrokeSDF] = ShaderType.ST_StandardRare;
			outCaps.ShaderMapping[(int)ShaderType.ST_StandardBatched] = ShaderType.ST_StandardBatched;
			outCaps.ShaderMapping[(int)ShaderType.ST_StandardBatchedTexture] = ShaderType.ST_StandardBatched;
			outCaps.ShaderMapping[(int)ShaderType.ST_BatchedText] = ShaderType.ST_StandardBatched;
			outCaps.ShaderMapping[(int)ShaderType.ST_BatchedTextSDF] = ShaderType.ST_StandardBatched;

			outCaps.ShaderMapping[(int)ShaderType.ST_ClipMask] = ShaderType.ST_ClipMask;
			outCaps.ShaderMapping[(int)ShaderType.ST_ClippingRect] = ShaderType.ST_ClipMask;
			outCaps.ShaderMapping[(int)ShaderType.ST_ClippingCircle] = ShaderType.ST_ClipMask;
			outCaps.ShaderMapping[(int)ShaderType.ST_ClippingTexture] = ShaderType.ST_ClipMask;
			outCaps.ShaderMapping[(int)ShaderType.ST_ClippingPath] = ShaderType.ST_ClippingPath;

			for (int blur = (int)ShaderType.ST_Blur_1; blur <= (int)ShaderType.ST_Blur_12; ++blur)
			{
				outCaps.ShaderMapping[blur] = ShaderType.ST_StandardRare;
			}

			outCaps.ShaderMapping[(int)ShaderType.ST_StandardRare] = ShaderType.ST_StandardRare;
			outCaps.ShaderMapping[(int)ShaderType.ST_StencilRare] = ShaderType.ST_StencilRare;
			outCaps.ShaderMapping[(int)ShaderType.ST_ClearQuad] = ShaderType.ST_ClearQuad;

			outCaps.ShaderMapping[(int)ShaderType.ST_RenoirShader] = ShaderType.ST_RenoirShader;
			outCaps.ShaderMapping[(int)ShaderType.ST_LinearGradient2Point] = ShaderType.ST_RenoirShader;
			outCaps.ShaderMapping[(int)ShaderType.ST_LinearGradient3PointSymmetrical] = ShaderType.ST_RenoirShader;
			outCaps.ShaderMapping[(int)ShaderType.ST_LinearGradientFromTexture] = ShaderType.ST_RenoirShader;
			outCaps.ShaderMapping[(int)ShaderType.ST_RadialGradient2Point] = ShaderType.ST_RenoirShader;
			outCaps.ShaderMapping[(int)ShaderType.ST_RadialGradient3PointSymmetrical] = ShaderType.ST_RenoirShader;
			outCaps.ShaderMapping[(int)ShaderType.ST_RadialGradientFromTexture] = ShaderType.ST_RenoirShader;
			outCaps.ShaderMapping[(int)ShaderType.ST_LinearGradientMasked2Point] = ShaderType.ST_RenoirShader;
			outCaps.ShaderMapping[(int)ShaderType.ST_LinearGradientMasked3PointSymmetrical] = ShaderType.ST_RenoirShader;
			outCaps.ShaderMapping[(int)ShaderType.ST_LinearGradientMaskedFromTexture] = ShaderType.ST_RenoirShader;
			outCaps.ShaderMapping[(int)ShaderType.ST_RadialGradientMasked2Point] = ShaderType.ST_RenoirShader;
			outCaps.ShaderMapping[(int)ShaderType.ST_RadialGradientMasked3PointSymmetrical] = ShaderType.ST_RenoirShader;
			outCaps.ShaderMapping[(int)ShaderType.ST_RadialGradientMaskedFromTexture] = ShaderType.ST_RenoirShader;

			outCaps.ShaderMapping[(int)ShaderType.ST_SimpleTexture] = ShaderType.ST_RenoirShader;
			outCaps.ShaderMapping[(int)ShaderType.ST_SimpleTextureMasked] = ShaderType.ST_RenoirShader;

			outCaps.ShaderMapping[(int)ShaderType.ST_StandardTextureWithColorMatrix] = ShaderType.ST_StandardRare;

			outCaps.ShaderMapping[(int)ShaderType.ST_ColorMixing] = ShaderType.ST_ColorMixing;

			outCaps.ShaderMapping[(int)ShaderType.ST_YUV2RGB] = ShaderType.ST_StandardRare;
			outCaps.ShaderMapping[(int)ShaderType.ST_YUVA2RGB] = ShaderType.ST_StandardRare;

			outCaps.ShaderMapping[(int)ShaderType.ST_GenerateSDF] = ShaderType.ST_GenerateSDF;
			outCaps.ShaderMapping[(int)ShaderType.ST_GenerateSDFSolid] = ShaderType.ST_StandardRare;

			outCaps.ShaderMapping[(int)ShaderType.ST_TextSDFGPU] = ShaderType.ST_Standard;
			outCaps.ShaderMapping[(int)ShaderType.ST_TextStrokeSDFGPU] = ShaderType.ST_StandardRare;
			outCaps.ShaderMapping[(int)ShaderType.ST_BatchedTextSDFGPU] = ShaderType.ST_StandardBatched;

			outCaps.ShaderMapping[(int)ShaderType.ST_TextMSDF] = ShaderType.ST_StandardRare;
			outCaps.ShaderMapping[(int)ShaderType.ST_TextStrokeMSDF] = ShaderType.ST_StandardRare;
			outCaps.ShaderMapping[(int)ShaderType.ST_BatchedTextMSDF] = ShaderType.ST_StandardBatched;

			SetUnityRendererCaps(outCaps);
		}

		[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
		private static void BeginCommandsWrapper()
		{
			s_UnityBackendInstance.BeginCommands();
		}

		/// Called when a new list of commands will be executed. You can put state-setting code here.
		private void BeginCommands()
		{}

		[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
		private static void EndCommandsWrapper()
		{
			s_UnityBackendInstance.EndCommands();
		}

		/// Called when a new list of commands will be executed. You can put state-setting code here.
		private void EndCommands()
		{}

		[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
		private static void WrapUserRenderTargetWrapper(IntPtr userObject,
			renoir.Texture2D description,
			Texture2DObject texture,
			IntPtr depthStencil,
			DepthStencilTexture dsDescription,
			DepthStencilTextureObject dsObject)
		{
			s_UnityBackendInstance.WrapUserRenderTarget(userObject, description, texture, depthStencil, dsDescription, dsObject);
		}

		private void WrapUserRenderTarget(IntPtr userObject,
			renoir.Texture2D description,
			Texture2DObject texture,
			IntPtr depthStencil,
			DepthStencilTexture dsDescription,
			DepthStencilTextureObject dsObject)
		{
			GCHandle userRT = GCHandle.FromIntPtr(userObject);
			GCHandle userDS = GCHandle.FromIntPtr(depthStencil);
			m_UserRTs[texture] = new RTImpl((RenderTexture)userRT.Target, (RenderTexture)userDS.Target, description);
			userRT.Free();
			userDS.Free();
		}

		[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
		private static void WrapUserTextureWrapper(IntPtr userObject, renoir.Texture2D description, Texture2DObject obj)
		{
			s_UnityBackendInstance.WrapUserTexture(userObject, description, obj);
		}

		/// Called to wrap a user texture in a Renoir handle
		/// @param userObject opaque object that contains the user-supplied Texture
		/// @param description Description of the Texture
		/// @param object Renoir handle to the Texture
		private void WrapUserTexture(IntPtr userObject, renoir.Texture2D description, Texture2DObject obj)
		{
			GCHandle gch = GCHandle.FromIntPtr(userObject);
			Texture tex = (Texture)gch.Target;

			m_UserTextures.Add(obj);
			m_Textures.Add(obj, tex);
			gch.Free();
		}


		[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
		private static bool CreatePipelineStateWrapper(PipelineState state, PipelineStateObject obj)
		{
			return s_UnityBackendInstance.CreatePipelineState(state, obj);
		}

		/// Requests the creation of a Pipeline state. The PSO contains shaders and blend, ds states
		/// @param state description of the required state
		/// @param object Renoir handle to the PSO
		private bool CreatePipelineState(PipelineState state, PipelineStateObject obj)
		{
			m_PipelineStates.Add(obj, state);
			return true;
		}

		[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
		private static void DestroyPipelineStateWrapper(PipelineStateObject obj)
		{
			s_UnityBackendInstance.DestroyPipelineState(obj);
		}

		/// Requests the destruction of a PSO
		private void DestroyPipelineState(PipelineStateObject obj)
		{
			if (m_PipelineStates.ContainsKey(obj))
			{
				m_PipelineStates.Remove(obj);
			}
		}

		[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
		private static bool CreateVertexBufferWrapper(VertexType type, uint count, VertexBufferObject obj, bool changesOften)
		{
			return s_UnityBackendInstance.CreateVertexBuffer(type, count, obj, changesOften);
		}

		private bool CreateVertexBuffer(VertexType type, uint count, VertexBufferObject obj, bool changesOften)
		{
			switch (type)
			{
				case VertexType.VT_Standard:
				{
					StandardVertexBuffer vb = new StandardVertexBuffer();
					vb.m_Buffer = new NativeArray<StandardVertex>((int)count, Allocator.Persistent);
					m_StandardVertexBuffers.Add(obj, vb);
					break;
				}
				case VertexType.VT_Slim:
				{
					SlimVertexBuffer vb = new SlimVertexBuffer();
					vb.m_Buffer = new NativeArray<SlimVertex>((int)count, Allocator.Persistent);
					m_SlimVertexBuffers.Add(obj, vb);
					break;
				}
				default:
				{
					Debug.LogError("Unknown vertex type!" );
					return false;
				}
			}

			return true;
		}

		[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
		private static void DestroyVertexBufferWrapper(VertexBufferObject obj)
		{
			s_UnityBackendInstance.DestroyVertexBuffer(obj);
		}

		/// Requests the destruction of a VB
		private void DestroyVertexBuffer(VertexBufferObject obj)
		{
			StandardVertexBuffer standardVB;
			SlimVertexBuffer slimVB;
			if (m_StandardVertexBuffers.TryGetValue(obj, out standardVB))
			{
				standardVB.m_Buffer.Dispose();
				m_StandardVertexBuffers.Remove(obj);
			}
			else if (m_SlimVertexBuffers.TryGetValue(obj, out slimVB))
			{
				slimVB.m_Buffer.Dispose();
				m_SlimVertexBuffers.Remove(obj);
			}
			else
			{
				Debug.Assert(false);
				Debug.LogAssertion("Couldn't find vertex buffer to destroy");
			}
		}

		[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
		private static IntPtr MapVertexBufferWrapper(VertexBufferObject obj)
		{
			return s_UnityBackendInstance.MapVertexBuffer(obj);
		}

		/// Map VB to RAM for update
		private IntPtr MapVertexBuffer(VertexBufferObject obj)
		{
			StandardVertexBuffer standardVB;
			SlimVertexBuffer slimVB;
			if (m_StandardVertexBuffers.TryGetValue(obj, out standardVB))
			{
				unsafe
				{
					void* ptr = Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(standardVB.m_Buffer);
					return new IntPtr(ptr);
				}
			}
			else if (m_SlimVertexBuffers.TryGetValue(obj, out slimVB))
			{
				unsafe
				{
					void* ptr = Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(slimVB.m_Buffer);
					return new IntPtr(ptr);
				}
			}
			else
			{
				Debug.Assert(false);
				Debug.LogAssertion("Couldn't find vertex buffer to map");
			}

			return IntPtr.Zero;
		}

		[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
		private static void UnmapVertexBufferWrapper(VertexBufferObject obj, uint elementCount)
		{
			s_UnityBackendInstance.UnmapVertexBuffer(obj, elementCount);
		}

		/// Unmap vertex buffer
		/// @param object Renoir handle
		/// @param count of elements that were actually "touched" during the update
		private void UnmapVertexBuffer(VertexBufferObject obj, uint elementCount)
		{
			StandardVertexBuffer standardVB;
			SlimVertexBuffer slimVB;
			if (m_StandardVertexBuffers.TryGetValue(obj, out standardVB))
			{
				standardVB.m_UsedElementsCount = elementCount;
			}
			else if (m_SlimVertexBuffers.TryGetValue(obj, out slimVB))
			{
				slimVB.m_UsedElementsCount = elementCount;
			}
			else
			{
				Debug.Assert(false);
				Debug.LogAssertion("Couldn't find vertex buffer to map");
			}
		}

		[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
		private static bool CreateIndexBufferWrapper(IndexBufferType format, uint count, IndexBufferObject obj, bool changesOften)
		{
			return s_UnityBackendInstance.CreateIndexBuffer(format, count, obj, changesOften);
		}

		/// Create an index buffer on the GPU
		/// @param count count of indices in the buffer
		/// @param object Renoir handle
		/// @param changesOften Indicates that the buffer will be mapped/unmapped often
		private bool CreateIndexBuffer(IndexBufferType format, uint count, IndexBufferObject obj, bool changesOften)
		{
			switch (format)
			{
				case IndexBufferType.IBT_U8:
				{
					ByteIndexBuffer ib = new ByteIndexBuffer();
					ib.m_Buffer = new NativeArray<byte>((int)count, Allocator.Persistent);
					m_ByteIndexBuffers.Add(obj, ib);
					break;
				}
				case IndexBufferType.IBT_U16:
				{
					ShortIndexBuffer ib = new ShortIndexBuffer();
					ib.m_Buffer = new NativeArray<ushort>((int)count, Allocator.Persistent);
					m_ShortIndexBuffers.Add(obj, ib);
					break;
				}
				case IndexBufferType.IBT_U32:
				{
					IntegerIndexBuffer ib = new IntegerIndexBuffer();
					ib.m_Buffer = new NativeArray<uint>((int)count, Allocator.Persistent);
					m_IntegerIndexBuffers.Add(obj, ib);
					break;
				}
				default:
				{
					Debug.LogAssertion("Unsupported index buffer type~");
					return false;
				}
			}

			return true;
		}

		[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
		private static void DestroyIndexBufferWrapper(IndexBufferObject obj)
		{
			s_UnityBackendInstance.DestroyIndexBuffer(obj);
		}

		/// Destroy index buffer
		private void DestroyIndexBuffer(IndexBufferObject obj)
		{
			ByteIndexBuffer byteIB;
			ShortIndexBuffer shortIB;
			IntegerIndexBuffer integerIB;
			if (m_ByteIndexBuffers.TryGetValue(obj, out byteIB))
			{
				byteIB.m_Buffer.Dispose();
				m_ByteIndexBuffers.Remove(obj);
			}
			else if (m_ShortIndexBuffers.TryGetValue(obj, out shortIB))
			{
				shortIB.m_Buffer.Dispose();
				m_ShortIndexBuffers.Remove(obj);
			}
			else if (m_IntegerIndexBuffers.TryGetValue(obj, out integerIB))
			{
				integerIB.m_Buffer.Dispose();
				m_IntegerIndexBuffers.Remove(obj);
			}
			else
			{
				Debug.LogAssertion("Couldn't find index buffer to destroy!");
			}
		}

		[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
		private static IntPtr MapIndexBufferWrapper(IndexBufferObject obj)
		{
			return s_UnityBackendInstance.MapIndexBuffer(obj);
		}

		/// Maps an index buffer for update from the CPU
		private IntPtr MapIndexBuffer(IndexBufferObject obj)
		{
			ByteIndexBuffer byteIB;
			ShortIndexBuffer shortIB;
			IntegerIndexBuffer integerIB;
			if (m_ByteIndexBuffers.TryGetValue(obj, out byteIB))
			{
				unsafe
				{
					void* ptr = Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(byteIB.m_Buffer);
					return new IntPtr(ptr);
				}
			}
			else if (m_ShortIndexBuffers.TryGetValue(obj, out shortIB))
			{
				unsafe
				{
					void* ptr = Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(shortIB.m_Buffer);
					return new IntPtr(ptr);
				}
			}
			else if (m_IntegerIndexBuffers.TryGetValue(obj, out integerIB))
			{
				unsafe
				{
					void* ptr = Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(integerIB.m_Buffer);
					return new IntPtr(ptr);
				}
			}
			else
			{
				Debug.LogAssertion("Couldn't find index buffer to map!");
			}

			return IntPtr.Zero;
		}

		[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
		private static void UnmapIndexBufferWrapper(IndexBufferObject obj, uint elementCount)
		{
			s_UnityBackendInstance.UnmapIndexBuffer(obj, elementCount);
		}

		/// Unmaps an index buffer
		/// @param object Renoir handle
		/// @param elementCount count of elements that were "touched" during the update
		private void UnmapIndexBuffer(IndexBufferObject obj, uint elementCount)
		{
			ByteIndexBuffer byteIB;
			ShortIndexBuffer shortIB;
			IntegerIndexBuffer integerIB;
			if (m_ByteIndexBuffers.TryGetValue(obj, out byteIB))
			{
				byteIB.m_UsedElementsCount = elementCount;
			}
			else if (m_ShortIndexBuffers.TryGetValue(obj, out shortIB))
			{
				shortIB.m_UsedElementsCount = elementCount;
			}
			else if (m_IntegerIndexBuffers.TryGetValue(obj, out integerIB))
			{
				integerIB.m_UsedElementsCount = elementCount;
			}
			else
			{
				Debug.LogAssertion("Couldn't find index buffer to unmap!");
			}
		}

		[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
		private static bool CreateConstantBufferWrapper(CBType type, ConstantBufferObject obj, uint size)
		{
			return s_UnityBackendInstance.CreateConstantBuffer(type, obj, size);
		}

		/// Creates a constant (uniform) buffer object
		/// @param type Indicates the type of the constant buffer object
		/// @param object Renoir handle
		/// @param size requested byte size of the constant buffer
		private bool CreateConstantBuffer(CBType type, ConstantBufferObject obj, uint size)
		{
			return m_CBs.CreateCB(type, obj, size);
		}

		[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
		private static void DestroyConstantBufferWrapper(ConstantBufferObject obj)
		{
			s_UnityBackendInstance.DestroyConstantBuffer(obj);
		}

		/// Requests the destruction of a constant buffer
		private void DestroyConstantBuffer(ConstantBufferObject obj)
		{
			m_CBs.DestroyCB(obj);
		}

		[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
		private static bool CreateTextureWrapper(Texture2DObject texture, renoir.Texture2D description, IntPtr data, uint dataLen)
		{
			return s_UnityBackendInstance.CreateTexture(texture, description, data, dataLen);
		}

		private bool CreateTexture(Texture2DObject texture, renoir.Texture2D description, IntPtr data, uint dataLen)
		{
			UnityEngine.Experimental.Rendering.FormatUsage formatUsage = description.IsRenderTarget ?
				UnityEngine.Experimental.Rendering.FormatUsage.Render :
				UnityEngine.Experimental.Rendering.FormatUsage.Sample;

			if (!SystemInfo.IsFormatSupported(BackendUtilities.RenoirToUnityGraphicsFormat(description.Format), formatUsage))
			{
				Debug.LogError("The texture format (" + BackendUtilities.RenoirToUnityGraphicsFormat(description.Format).ToString() + ") is not supported!");
				return false;
			}

			UnityEngine.Texture2D tex = null;
			RenderTexture rt = null;
			bool shouldCopyToRT = false;
			if (dataLen != 0 || (description.Props & (uint)ImageProperties.IMP_ClearOnInit) != 0)
			{
				tex = new UnityEngine.Texture2D(
					(int)description.Width,
					(int)description.Height,
					BackendUtilities.RenoirToUnityGraphicsFormat(description.Format),
					UnityEngine.Experimental.Rendering.TextureCreationFlags.None
					);

				if ((description.Props & (uint)ImageProperties.IMP_ClearOnInit) != 0)
				{
					// The texture's data format can use pixels in different n-bit formats, so we can't store the data in a structure like Color32 for all textures.
					// Storing it in byte format and iterating based on the length of the array would cover all cases since it sets the color byte by byte
					var pixelData = tex.GetRawTextureData<byte>();

					for (int i = 0; i < pixelData.Length; i++)
					{
						pixelData[i] = 0;
					}
					tex.Apply();
				}

				if (dataLen != 0)
				{
					tex.LoadRawTextureData(data, (int)dataLen);
					tex.Apply();
				}

				if (!description.IsRenderTarget)
				{
					m_Textures.Add(texture, tex);
					return true;
				}
				else
				{
					shouldCopyToRT = true;
				}
			}

			rt = new RenderTexture(
				(int)description.Width,
				(int)description.Height,
				0,
				BackendUtilities.RenoirToUnityGraphicsFormat(description.Format)
				);
			if (!rt.Create())
			{
				Debug.LogError("Unable to create render depth texture with ID: " + texture.Id);
			}

			if (shouldCopyToRT)
			{
				CopyTextureToRenderTexture(tex, rt);
			}

			m_Textures.Add(texture, rt);
			return true;
		}

		[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
		private static void DestroyTextureWrapper(Texture2DObject texture)
		{
			s_UnityBackendInstance.DestroyTexture(texture);
		}

		/// Destroy a texture
		private void DestroyTexture(Texture2DObject texture)
		{
			Texture tex;
			if (m_Textures.TryGetValue(texture, out tex))
			{
				// If the resource is user texture we don't need to release it
				// here, because user has the responsibility of
				// keeping track of the lifetime
				if (!m_UserTextures.Contains(texture))
				{
					if (tex is RenderTexture)
					{
						RenderTexture renderTexture = (RenderTexture)tex;
						renderTexture.Release();
					}

					UnityEngine.Object.Destroy(tex);
				}
				else
				{
					m_UserTextures.Remove(texture);
				}

				m_Textures.Remove(texture);
			}
		}

		[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
		private static bool CreateDepthStencilTextureWrapper(DepthStencilTextureObject texture, DepthStencilTexture description)
		{
			return s_UnityBackendInstance.CreateDepthStencilTexture(texture, description);
		}

		private bool CreateDepthStencilTexture(DepthStencilTextureObject texture, DepthStencilTexture description)
		{
			RenderTexture tex = new RenderTexture(
				(int)description.Width,
				(int)description.Height,
				24,
				RenderTextureFormat.Depth,
				RenderTextureReadWrite.Default
				);

			if (!tex.Create())
			{
				Debug.LogError("Unable to create render depth texture with ID: " + texture.Id);
			}

			m_DepthStencilTextures.Add(texture, tex);
			return true;
		}

		[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
		private static void DestroyDepthStencilTextureWrapper(DepthStencilTextureObject texture)
		{
			s_UnityBackendInstance.DestroyDepthStencilTexture(texture);
		}

		// Destroy a DS texture
		private void DestroyDepthStencilTexture(DepthStencilTextureObject texture)
		{
			RenderTexture ds = null;
			if (m_DepthStencilTextures.TryGetValue(texture, out ds))
			{
				ds.Release();
				UnityEngine.Object.Destroy(ds);
				m_DepthStencilTextures.Remove(texture);
				return;
			}
		}

		[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
		private static void UpdateTextureWrapper(Texture2DObject texture, renoir.Texture2D description, IntPtr boxes, IntPtr newBytes, uint count, bool willOverwrite)
		{
			s_UnityBackendInstance.UpdateTexture(texture, description, boxes, newBytes, count, willOverwrite);
		}

		private void UpdateTexture(Texture2DObject texture, renoir.Texture2D description, IntPtr boxes, IntPtr newBytes, uint count, bool willOverwrite)
		{
			UnityEngine.Texture2D tex = null;
			RenderTexture rt = null;
			Texture unityTex = null;
			bool shouldCopyBackToRT = false;

			if (!m_Textures.TryGetValue(texture, out unityTex))
			{
					Debug.LogAssertion("Texture doesn't exist!");
					return;
			}

			if (unityTex is UnityEngine.Texture2D)
			{
				tex = (UnityEngine.Texture2D)unityTex;
			}
			else
			{
				if (!(unityTex is RenderTexture))
				{
					Debug.LogAssertion("Texture is not correct type!");
					return;
				}

				rt = (RenderTexture)unityTex;
				UnityEngine.Texture2D tempTex = new UnityEngine.Texture2D(rt.width, rt.height);
				CopyRenderTextureToTexture2D(rt, tempTex);
				tex = tempTex;
				shouldCopyBackToRT = true;
			}

			unsafe
			{
				UpdateBox* boxesPtr = (UpdateBox*)boxes.ToPointer();
				void** bytes = (void**)newBytes.ToPointer();
				for (int i = 0; i < count; i++)
				{
					UpdateBox box = boxesPtr[i];
					byte* data = (byte*)bytes[i];
					uint cnt = 0;
					for (uint j = box.Top; j < box.Bottom; j++)
					{
						if (description.Format == PixelFormat.PF_R8)
						{
							for (uint k = box.Left; k < box.Right; k++)
							{
								UnityEngine.Color clr = new UnityEngine.Color(((float)data[cnt++]) / 255, 1, 1, 1);
								tex.SetPixel((int)k, (int)j, clr);
							}
						}
						else if (description.Format == PixelFormat.PF_R8G8B8A8)
						{
							for (uint k = box.Left; k < box.Right; k++)
							{
								UnityEngine.Color clr = new UnityEngine.Color(
									((float)data[cnt++]) / 255,
									((float)data[cnt++]) / 255,
									((float)data[cnt++]) / 255,
									((float)data[cnt++]) / 255
									);
								tex.SetPixel((int)k, (int)j, clr);
							}
						}
						else
						{
							Debug.LogAssertion("UpdateTexture for texture with format" + description.Format.ToString() + " is not implemented!");
						}
					}
				}
			}

			tex.Apply();

			if (shouldCopyBackToRT)
			{
				CopyTextureToRenderTexture(tex, rt);
			}
		}

		private void CopyTextureToRenderTexture(Texture src, RenderTexture dst)
		{
			RenderTexture original = RenderTexture.active;

			RenderTexture.active = dst;
			Graphics.Blit(src, dst);

			RenderTexture.active = original;
		}

		private void CopyRenderTextureToTexture2D(RenderTexture src, UnityEngine.Texture2D dst)
		{
			RenderTexture original = RenderTexture.active;

			RenderTexture.active = src;
			dst.ReadPixels(new Rect(0, 0, src.width, src.height), 0, 0);
			dst.Apply();

			RenderTexture.active = original;
		}

		[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
		private static bool CreateSampler2DWrapper(Sampler2DObject sampler, Sampler2D description)
		{
			return s_UnityBackendInstance.CreateSampler2D(sampler, description);
		}

		/// Create a sampler object for 2D textures
		public bool CreateSampler2D(Sampler2DObject sampler, Sampler2D description)
		{
			m_Samplers.Add(sampler, description);
			return true;
		}


		[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
		private static void DestroySampler2DWrapper(Sampler2DObject sampler)
		{
			s_UnityBackendInstance.DestroySampler2D(sampler);
		}

		/// Destroy sampler
		public void DestroySampler2D(Sampler2DObject sampler)
		{
			if (m_Samplers.ContainsKey(sampler))
			{
				m_Samplers.Remove(sampler);
			}
		}

		[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
		private static void SetDebugNameWrapper(Texture2DObject texture, string name)
		{
			s_UnityBackendInstance.SetDebugName(texture, name);
		}

		private void SetDebugName(Texture2DObject texture, string name)
		{
			Texture tex;
			if (m_Textures.TryGetValue(texture, out tex))
			{
				tex.name = name;
			}
			else
			{
				Debug.LogAssertion("Couldn't find texture to set name!");
			}
		}

		[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
		private static void SetDebugNameDSWrapper(DepthStencilTextureObject texture, string name)
		{
			s_UnityBackendInstance.SetDebugNameDS(texture, name);
		}

		private void SetDebugNameDS(DepthStencilTextureObject texture, string name)
		{
			RenderTexture renderTexture;
			if (m_DepthStencilTextures.TryGetValue(texture, out renderTexture))
			{
				renderTexture.name = name;
			}
			else
			{
				Debug.LogAssertion("Couldn't find texture to set name!");
			}
		}

		[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
		private static void ExecuteRenderingWrapper(IntPtr buffers, uint buffersCount, IntPtr CBOUpdates, uint CBOUpdatesCount)
		{
			s_UnityBackendInstance.ExecuteRendering(buffers, buffersCount, CBOUpdates, CBOUpdatesCount);
		}

		[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
		private static void ExecuteResourceCommandsWrapper(IntPtr buffers, uint buffersCount, ResourcesCommandsStage stage)
		{
			s_UnityBackendInstance.ExecuteResourceCommands(buffers, buffersCount, stage);
		}

		private void ExecuteRendering(IntPtr buffers, uint buffersCount, IntPtr CBOUpdates, uint CBOUpdatesCount)
		{
			m_CBs.BuildCBMappingToData(CBOUpdates, CBOUpdatesCount);

			for (int buffId = 0; buffId < buffersCount; buffId++)
			{
				unsafe
				{
					BackendCommandsBuffer* buff = (BackendCommandsBuffer*)buffers.ToPointer();
					BackendCommandOffset* offsetsPtr = (BackendCommandOffset*)buff[buffId].Offsets.ToPointer();
					for (int cmdId = 0; cmdId < buff[buffId].Count; cmdId++)
					{
						BackendCommandOffset offset = offsetsPtr[cmdId];
						IntPtr cmdPtr = new IntPtr(buff[buffId].Commands.ToInt64() + (int)offset.Offset);
						switch (offset.Command)
						{
							case BackendCommands.BC_SetPipelineState:
							{
								SetPipelineStateCmd cmd = (SetPipelineStateCmd)Marshal.PtrToStructure(cmdPtr, typeof(SetPipelineStateCmd));
								SetPipelineState(cmd);
								break;
							}
							case BackendCommands.BC_SetVSConstantBuffers:
							{
								SetVSConstantBuffersCmd cmd = (SetVSConstantBuffersCmd)Marshal.PtrToStructure(cmdPtr, typeof(SetVSConstantBuffersCmd));
								SetVSConstantBuffer(cmd);
								break;
							}
							case BackendCommands.BC_SetPSConstantBuffers:
							{
								SetPSConstantBuffersCmd cmd = (SetPSConstantBuffersCmd)Marshal.PtrToStructure(cmdPtr, typeof(SetPSConstantBuffersCmd));
								SetPSConstantBuffers(cmd);
								break;
							}
							case BackendCommands.BC_UpdateConstantBuffer:
							{
								UpdateConstantBufferCmd cmd = (UpdateConstantBufferCmd)Marshal.PtrToStructure(cmdPtr, typeof(UpdateConstantBufferCmd));
								UpdateConstantBuffer(cmd);
								break;
							}
							case BackendCommands.BC_SetPSSamplers:
							{
								SetPSSamplersCmd cmd = (SetPSSamplersCmd)Marshal.PtrToStructure(cmdPtr, typeof(SetPSSamplersCmd));
								IntPtr data = new IntPtr(cmdPtr.ToInt64() + Marshal.SizeOf(typeof(SetPSSamplersCmd)));
								SetPSSamplers(cmd, data);
								break;
							}
							case BackendCommands.BC_SetPSTextures:
							{
								SetPSTexturesCmd cmd = (SetPSTexturesCmd)Marshal.PtrToStructure(cmdPtr, typeof(SetPSTexturesCmd));
								IntPtr data = new IntPtr(cmdPtr.ToInt64() + Marshal.SizeOf(typeof(SetPSTexturesCmd)));
								SetPSTextures(cmd, data);
								break;
							}
							case BackendCommands.BC_SetVertexBuffer:
							{
								SetVertexBufferCmd cmd = (SetVertexBufferCmd)Marshal.PtrToStructure(cmdPtr, typeof(SetVertexBufferCmd));
								SetVertexBuffer(cmd);
								break;
							}
							case BackendCommands.BC_SetIndexBuffer:
							{
								SetIndexBufferCmd cmd = (SetIndexBufferCmd)Marshal.PtrToStructure(cmdPtr, typeof(SetIndexBufferCmd));
								SetIndexBuffer(cmd);
								break;
							}
							case BackendCommands.BC_DrawIndexed:
							{
								DrawIndexedCmd cmd = (DrawIndexedCmd)Marshal.PtrToStructure(cmdPtr, typeof(DrawIndexedCmd));
								DrawIndexed(cmd);
								break;
							}
							case BackendCommands.BC_DrawCustomEffect:
							{
								DrawCustomEffectCmd cmd = (DrawCustomEffectCmd)Marshal.PtrToStructure(cmdPtr, typeof(DrawCustomEffectCmd));
								DrawCustomEffect(cmd);
								break;
							}
							case BackendCommands.BC_SetRenderTarget:
							{
								SetRenderTargetCmd cmd = (SetRenderTargetCmd)Marshal.PtrToStructure(cmdPtr, typeof(SetRenderTargetCmd));
								SetRenderTarget(cmd);
								break;
							}
							case BackendCommands.BC_BeginRenderPass:
							{
								BeginRenderPassCmd cmd = (BeginRenderPassCmd)Marshal.PtrToStructure(cmdPtr, typeof(BeginRenderPassCmd));
								BeginRenderPass(cmd);
								break;
							}
							case BackendCommands.BC_EndRenderPass:
							{
								EndRenderPassCmd cmd = (EndRenderPassCmd)Marshal.PtrToStructure(cmdPtr, typeof(EndRenderPassCmd));
								EndRenderPass(cmd);
								break;
							}
							case BackendCommands.BC_ClearRenderTarget:
							{
								ClearRenderTargetCmd cmd = (ClearRenderTargetCmd)Marshal.PtrToStructure(cmdPtr, typeof(ClearRenderTargetCmd));
								ClearRenderTarget(cmd);
								break;
							}
							case BackendCommands.BC_SetScissorRect:
							{
								SetScissorRectCmd cmd = (SetScissorRectCmd)Marshal.PtrToStructure(cmdPtr, typeof(SetScissorRectCmd));
								SetScissorRect(cmd);
								break;
							}
							case BackendCommands.BC_SetViewport:
							{
								SetViewportCmd cmd = (SetViewportCmd)Marshal.PtrToStructure(cmdPtr, typeof(SetViewportCmd));
								SetViewport(cmd);
								break;
							}
							case BackendCommands.BC_SetStencilReference:
							{
								SetStencilReferenceCmd cmd = (SetStencilReferenceCmd)Marshal.PtrToStructure(cmdPtr, typeof(SetStencilReferenceCmd));
								SetStencilReference(cmd);
								break;
							}
							case BackendCommands.BC_ResolveRenderTarget:
							{
								ResolveRenderTargetCmd cmd = (ResolveRenderTargetCmd)Marshal.PtrToStructure(cmdPtr, typeof(ResolveRenderTargetCmd));
								ResolveRenderTarget(cmd);
								break;
							}
							case BackendCommands.BC_PushMetadata:
							{
								PushMetadataCmd cmd = (PushMetadataCmd)Marshal.PtrToStructure(cmdPtr, typeof(PushMetadataCmd));
								PushMarker(cmd);
								break;
							}
							case BackendCommands.BC_PopMetadata:
							{
								PopMetadataCmd cmd = (PopMetadataCmd)Marshal.PtrToStructure(cmdPtr, typeof(PopMetadataCmd));
								PopMarker(cmd);
								break;
							}
							default:
							{
								Debug.Assert(false);
								Debug.LogAssertion("Unimplemented command detected!");
								break;
							}
						}
					}
				}
			}
		}

		private void ExecuteResourceCommands(IntPtr buffers, uint buffersCount, ResourcesCommandsStage stage)
		{
			for (int buffId = 0; buffId < buffersCount; buffId++)
			{
				unsafe
				{
					BackendResourceCommandsBuffer* buff = (BackendResourceCommandsBuffer*)buffers.ToPointer();
					BackendResourceCommandOffset* offsetsPtr = (BackendResourceCommandOffset*)buff[buffId].Offsets.ToPointer();

					for (int cmdId = 0; cmdId < buff[buffId].Count; cmdId++)
					{
						BackendResourceCommandOffset offset = offsetsPtr[cmdId];
						IntPtr cmdPtr = new IntPtr(buff[buffId].Commands.ToInt64() + (int)offset.Offset);
						switch (offset.Command)
						{
							case BackendResourceCommands.BRC_WrapUserRT:
							{
								WrapUserRTCmd cmd = (WrapUserRTCmd)Marshal.PtrToStructure(cmdPtr, typeof(WrapUserRTCmd));
								WrapUserRenderTarget(cmd.UserObject, cmd.Description, cmd.Object, cmd.DepthStencil, cmd.DSDescription, cmd.DSObject);
								break;
							}
							case BackendResourceCommands.BRC_CreateTexture:
							{
								CreateTextureCmd cmd = (CreateTextureCmd)Marshal.PtrToStructure(cmdPtr, typeof(CreateTextureCmd));
								IntPtr data = new IntPtr(buff[buffId].Commands.ToInt64() + (int)offset.Offset + Marshal.SizeOf(typeof(CreateTextureCmd)));
								bool created = CreateTexture(cmd.Texture, cmd.TextureDesc, cmd.DataLength > 0 ? data : new IntPtr(null), cmd.DataLength);
								Debug.Assert(created);
								break;
							}
							case BackendResourceCommands.BRC_CreateTextureWithDataPtr:
							{
								CreateTextureWithDataPtrCmd cmd = (CreateTextureWithDataPtrCmd)Marshal.PtrToStructure(cmdPtr, typeof(CreateTextureWithDataPtrCmd));
								bool created = CreateTexture(cmd.Texture, cmd.TextureDesc, cmd.DataLength > 0 ? cmd.Data : new IntPtr(null), cmd.DataLength);
								Debug.Assert(created);
								break;
							}
							case BackendResourceCommands.BRC_CreateDSTexture:
							{
								CreateDSTextureCmd cmd = (CreateDSTextureCmd)Marshal.PtrToStructure(cmdPtr, typeof(CreateDSTextureCmd));
								bool created = CreateDepthStencilTexture(cmd.Texture, cmd.TextureDesc);
								Debug.Assert(created);
								break;
							}
							case BackendResourceCommands.BRC_DestroyTexture:
							{
								DestroyTextureCmd cmd = (DestroyTextureCmd)Marshal.PtrToStructure(cmdPtr, typeof(DestroyTextureCmd));
								DestroyTexture(cmd.Texture);
								break;
							}
							case BackendResourceCommands.BRC_DestroyDSTexture:
							{
								DestroyDSTextureCmd cmd = (DestroyDSTextureCmd)Marshal.PtrToStructure(cmdPtr, typeof(DestroyDSTextureCmd));
								DestroyDepthStencilTexture(cmd.DSTexture);
								break;
							}
							case BackendResourceCommands.BRC_CreateVertexBuffer:
							{
								CreateVertexBufferCmd cmd = (CreateVertexBufferCmd)Marshal.PtrToStructure(cmdPtr, typeof(CreateVertexBufferCmd));
								bool created = CreateVertexBuffer(cmd.VertexType, cmd.Size, cmd.Object, cmd.ChangesOften);
								Debug.Assert(created);
								break;
							}
							case BackendResourceCommands.BRC_DestroyVertexBuffer:
							{
								DestroyVertexBufferCmd cmd = (DestroyVertexBufferCmd)Marshal.PtrToStructure(cmdPtr, typeof(DestroyVertexBufferCmd));
								DestroyVertexBuffer(cmd.Object);
								break;
							}
							case BackendResourceCommands.BRC_CreateIndexBuffer:
							{
								CreateIndexBufferCmd cmd = (CreateIndexBufferCmd)Marshal.PtrToStructure(cmdPtr, typeof(CreateIndexBufferCmd));
								bool created = CreateIndexBuffer(cmd.IndexType, cmd.Size, cmd.Object, cmd.ChangesOften);
								Debug.Assert(created);
								break;
							}
							case BackendResourceCommands.BRC_DestroyIndexBuffer:
							{
								DestroyIndexBufferCmd cmd = (DestroyIndexBufferCmd)Marshal.PtrToStructure(cmdPtr, typeof(DestroyIndexBufferCmd));
								DestroyIndexBuffer(cmd.Object);
								break;
							}
							case BackendResourceCommands.BRC_WrapUserTexture:
							{
								WrapUserTextureCmd cmd = (WrapUserTextureCmd)Marshal.PtrToStructure(cmdPtr, typeof(WrapUserTextureCmd));
								WrapUserTexture(cmd.UserTextureData, cmd.TextureDesc, cmd.Texture);
								break;
							}
							case BackendResourceCommands.BRC_UpdateTexture:
							{
								UpdateTextureCmd cmd = (UpdateTextureCmd)Marshal.PtrToStructure(cmdPtr, typeof(UpdateTextureCmd));
								UpdateTextureCmdInfo info = (UpdateTextureCmdInfo)Marshal.PtrToStructure(cmd.UpdateTextureInfo, typeof(UpdateTextureCmdInfo));
								UpdateTexture(cmd.Texture, cmd.TextureDesc, info.UpdateBoxes, info.PixelData, info.BoxesCount, info.WillOverwrite);
								break;
							}
							case BackendResourceCommands.BRC_CreateConstantBuffer:
							{
								CreateConstantBufferCmd cmd = (CreateConstantBufferCmd)Marshal.PtrToStructure(cmdPtr, typeof(CreateConstantBufferCmd));
								bool created = CreateConstantBuffer(cmd.ConstantBufferType, cmd.Object, cmd.Size);
								Debug.Assert(created);
								break;
							}
							case BackendResourceCommands.BRC_DestroyConstantBuffer:
							{
								DestroyConstantBufferCmd cmd = (DestroyConstantBufferCmd)Marshal.PtrToStructure(cmdPtr, typeof(DestroyConstantBufferCmd));
								DestroyConstantBuffer(cmd.Object);
								break;
							}
							case BackendResourceCommands.BRC_CreatePSO:
							{
								CreatePSOCmd cmd = (CreatePSOCmd)Marshal.PtrToStructure(cmdPtr, typeof(CreatePSOCmd));
								bool created = CreatePipelineState(cmd.PSO, cmd.Object);
								Debug.Assert(created);
								break;
							}
							case BackendResourceCommands.BRC_DestroyPSO:
							{
								DestroyPSOCmd cmd = (DestroyPSOCmd)Marshal.PtrToStructure(cmdPtr, typeof(DestroyPSOCmd));
								DestroyPipelineState(cmd.Object);
								break;
							}
							case BackendResourceCommands.BRC_CreateSampler:
							{
								CreateSamplerCmd cmd = (CreateSamplerCmd)Marshal.PtrToStructure(cmdPtr, typeof(CreateSamplerCmd));
								bool created = CreateSampler2D(cmd.Object, cmd.SamplerDesc);
								Debug.Assert(created);
								break;
							}
							case BackendResourceCommands.BRC_DestroySampler:
							{
								DestroySamplerCmd cmd = (DestroySamplerCmd)Marshal.PtrToStructure(cmdPtr, typeof(DestroySamplerCmd));
								DestroySampler2D(cmd.Object);
								break;
							}
							case BackendResourceCommands.BRC_SetDebugName:
							{
								SetDebugNameCmd cmd = (SetDebugNameCmd)Marshal.PtrToStructure(cmdPtr, typeof(SetDebugNameCmd));
								IntPtr textureNamePtr = new IntPtr(buff[buffId].Commands.ToInt64() + (int)offset.Offset + Marshal.SizeOf(typeof(SetDebugNameCmd)));
								string textureName = Marshal.PtrToStringAnsi(textureNamePtr);
								SetDebugName(cmd.Texture, textureName);
								break;
							}
							case BackendResourceCommands.BRC_SetDSDebugName:
							{
								SetDSDebugNameCmd cmd = (SetDSDebugNameCmd)Marshal.PtrToStructure(cmdPtr, typeof(SetDSDebugNameCmd));
								IntPtr textureNamePtr = new IntPtr(buff[buffId].Commands.ToInt64() + (int)offset.Offset + Marshal.SizeOf(typeof(SetDSDebugNameCmd)));
								string textureName = Marshal.PtrToStringAnsi(textureNamePtr);
								SetDebugNameDS(cmd.Texture, textureName);
								break;
							}
							default:
							{
								break;
							}
						}
					}
				}
			}
		}

		public void ExecuteBuffers()
		{
			if (m_CommandBuffer.sizeInBytes == 0)
			{
				return;
			}

			Graphics.ExecuteCommandBuffer(m_CommandBuffer);
			m_CommandBuffer.Clear();

			foreach (var item in m_Meshes.Values)
			{
				item.Reset();
			}
		}

		private void SetMaterialProperties(ref Material material, ref PipelineState pso)
		{
			material.SetInt("_SrcBlend", (int)BackendUtilities.Renoir2UnityBlendMode(pso.Blending.SrcBlend));
			material.SetInt("_DstBlend", (int)BackendUtilities.Renoir2UnityBlendMode(pso.Blending.DestBlend));

			material.SetInt("_SrcBlendA", (int)BackendUtilities.Renoir2UnityBlendMode(pso.Blending.SrcBlendAlpha));
			material.SetInt("_DstBlendA", (int)BackendUtilities.Renoir2UnityBlendMode(pso.Blending.DestBlendAlpha));

			material.SetInt("_OpColor", (int)BackendUtilities.Renoir2UnityBlendingOp(pso.Blending.BlendOp));
			material.SetInt("_OpAlpha", (int)BackendUtilities.Renoir2UnityBlendingOp(pso.Blending.BlendOpAlpha));

			material.SetInt("_ColorWriteMask", (int)BackendUtilities.Renoir2UnityColorWriteMask(pso.ColorMask));
			material.SetInt("_Cull", (int)CullMode.Off);

			if (pso.DepthStencil.StencilEnable)
			{
				material.SetInt("_StencilComp", (int)BackendUtilities.Renoir2UnityCompareFunction(pso.DepthStencil.Function));
			}
			else
			{
				material.SetInt("_StencilComp", (int)CompareFunction.Disabled);
			}

			if (pso.DepthStencil.DepthEnable)
			{
				material.SetInt("_ZWrite", pso.DepthStencil.DepthWrites ? 1 : 0);

				ComparisonFunction depthComparisonFunc = pso.DepthStencil.DepthFunction;
#if UNITY_PS4 || UNITY_PS5 || UNITY_SWITCH
				// There is a bug in Unity (PS4/PS5 & Nintendo) where if the Depth is enabled,
				// Depth write is true and the DepthFunction is Always, the final GPU state will be:
				// - Depth: Disabled, Depth Write: Enabled, Depth Func: CMP_Always
				// and we wouldn't clear correctly the depth.
				// But if the depth function is CMP_GreaterEqual. The final GPU state will be:
				// - Depth: enable, Depth Write: Enabled, Depth Func: CMP_GreaterEqual
				// so we use this hack to clear the depth properly.
				depthComparisonFunc = depthComparisonFunc == ComparisonFunction.CMP_Always ? ComparisonFunction.CMP_GreaterEqual : depthComparisonFunc;
#endif
				material.SetInt("_ZTest", BackendUtilities.Renoir2UnityCompareDepthFunction(depthComparisonFunc));
			}
			else
			{
				material.SetInt("_ZWrite", 0);
				material.SetInt("_ZTest", (int)CompareFunction.Always);
			}

			material.SetInt("_Stencil", (int)m_CurrentPipelineState.StencilRef);

			material.SetInt("_StencilReadMask", (int)pso.DepthStencil.StencilReadMask);
			material.SetInt("_StencilWriteMask", (int)pso.DepthStencil.StencilWriteMask);

			material.SetInt("_CompFront", (int)BackendUtilities.Renoir2UnityCompareFunction(pso.DepthStencil.Function));
			material.SetInt("_PassFront", (int)BackendUtilities.Renoir2UnityStencilOp(pso.DepthStencil.PassOp));
			material.SetInt("_FailFront", (int)BackendUtilities.Renoir2UnityStencilOp(pso.DepthStencil.FailOp));
			material.SetInt("_ZFailFront", (int)BackendUtilities.Renoir2UnityStencilOp(pso.DepthStencil.ZFailOp));

			material.SetInt("_CompBack", (int)BackendUtilities.Renoir2UnityCompareFunction(pso.DepthStencil.BackFunction));
			material.SetInt("_PassBack", (int)BackendUtilities.Renoir2UnityStencilOp(pso.DepthStencil.BackPassOp));
			material.SetInt("_FailBack", (int)BackendUtilities.Renoir2UnityStencilOp(pso.DepthStencil.BackFailOp));
			material.SetInt("_ZFailBack", (int)BackendUtilities.Renoir2UnityStencilOp(pso.DepthStencil.BackZFailOp));
		}

		private void ApplyPSO()
		{
			PipelineState pso;
			if (!m_PipelineStates.TryGetValue(m_CurrentPipelineState.PSO, out pso))
			{
				Debug.Assert(false);
			}

			Material material = null;
			bool success = m_Materials.TryGetValue(m_CurrentPipelineState, out material);
			if (!success)
			{
				material = new Material(m_RenoirShader);
				CompileAllShaderPasses(material);

				SetMaterialProperties(ref material, ref pso);
				m_Materials.Add(m_CurrentPipelineState, material);
			}

			if (material != m_MaterialRenoir)
			{
				m_MaterialRenoir = material;
			}

			m_ShaderPass = BackendUtilities.Renoir2UnityShader(pso.VertexShader, pso.PixelShader);
		}

		private void SetPipelineState(SetPipelineStateCmd cmd)
		{
			m_CurrentPipelineState.PSO = cmd.Object;
			ApplyPSO();
		}

		private void SetVSConstantBuffer(SetVSConstantBuffersCmd cmd)
		{
		}

		private void SetPSTextures(SetPSTexturesCmd cmd, IntPtr after)
		{
			unsafe
			{
				Sampler2D sampler;
				bool foundSampler = m_Samplers.TryGetValue(m_BoundPSTexturesSamplers[0], out sampler);

				Texture2DObject* ids = (Texture2DObject*)after.ToPointer();
				for (int i = 0; i < cmd.Count; i++)
				{
					Texture tex;
					long psTexIndex = cmd.StartSlot + i;
					string texName = psTexIndex == 0 ? "txBuffer" : "txBuffer" + (cmd.StartSlot + i);

					if (ids[i].Id == RenoirInvalidId)
					{
						// Renoir is clearing the texture bindings by using invalid ids
						m_BoundPSTextures[psTexIndex].Id = RenoirInvalidId;
						m_PropertyBlock.SetTexture(texName, m_DummyTexture);
						continue;
					}

					if (!m_Textures.TryGetValue(ids[i], out tex))
					{
						RTImpl rt;
						// all other backends support using user RT as shader resources and there is no reason
						// to not be able to support this here; this is also needed in order for the backdrop
						// feature to work properly
						if (!m_UserRTs.TryGetValue(ids[i], out rt))
						{
							Debug.LogAssertion("Couldn't find texture with ID:" + ids[i].Id + " to be bound as input!");
							continue;
						}
						tex = (Texture)rt.m_RT;
					}

					if (foundSampler)
					{
						tex.filterMode = BackendUtilities.Renoir2UnityFilterMode(sampler.Filtering);
						tex.wrapModeU = BackendUtilities.Renoir2UnityWrapMode(sampler.UAddress);
						tex.wrapModeV = BackendUtilities.Renoir2UnityWrapMode(sampler.VAddress);
						tex.anisoLevel = 0;
					}

					m_PropertyBlock.SetTexture(texName, tex);
					m_BoundPSTextures[cmd.StartSlot + i] = ids[i];
				}
			}
		}

		private void SetRenderTarget(SetRenderTargetCmd cmd)
		{
			RenderTexture tex = null;
			RenderTexture depth = null;

			RTImpl rt;
			if (m_UserRTs.TryGetValue(cmd.Object, out rt))
			{
				tex = rt.m_RT;
				depth = rt.m_DS;
			}
			else
			{
				Texture unityTex;
				bool success = m_Textures.TryGetValue(cmd.Object, out unityTex);
				Debug.Assert(success && unityTex is RenderTexture);
				tex = (RenderTexture)unityTex;
				m_DepthStencilTextures.TryGetValue(cmd.DSObject, out depth);
			}

			if (depth != null)
			{
				m_CommandBuffer.SetRenderTarget(tex, depth);
			}
			else
			{
				m_CommandBuffer.SetRenderTarget(tex);
			}
		}

		private void SetPSConstantBuffers(SetPSConstantBuffersCmd cmd)
		{
		}

		private void UpdateConstantBuffer(UpdateConstantBufferCmd cmd)
		{
			m_CBs.UpdateConstantBuffer(cmd, ref m_PropertyBlock);
		}

		private void SetPSSamplers(SetPSSamplersCmd cmd, IntPtr samplersData)
		{
			unsafe
			{
				Sampler2DObject* ids = (Sampler2DObject*)samplersData.ToPointer();
				Sampler2D sampler;
				for (uint i = 0; i < cmd.Count; i++)
				{
					uint samplerIndex = cmd.StartSlot + i;
					if (!m_Samplers.TryGetValue(ids[samplerIndex], out sampler))
					{
						Debug.LogError("Sampler object was not found!");
						continue;
					}

					Debug.Assert(samplerIndex == 0, "Currently we are using only the first sampler!");

					m_BoundPSTexturesSamplers[samplerIndex] = ids[samplerIndex];
				}
			}
		}

		private void SetVertexBuffer(SetVertexBufferCmd cmd)
		{
			m_CurrentMeshBuffers.VBO = cmd.Object;
		}

		private void SetIndexBuffer(SetIndexBufferCmd cmd)
		{
			m_CurrentMeshBuffers.IBO = cmd.Object;
		}

		SubMeshDescriptor m_SubMeshDescriptor = new SubMeshDescriptor();
		private void DrawIndexed(DrawIndexedCmd cmd)
		{
			MeshInfo meshInfo;
			bool foundMesh = m_Meshes.TryGetValue(m_CurrentMeshBuffers, out meshInfo);
			if (!foundMesh)
			{
				meshInfo = new MeshInfo();
			}

			if (meshInfo.m_ShouldBeUpdated)
			{
				UpdateMeshBuffers(ref meshInfo.m_Mesh);
				meshInfo.m_ShouldBeUpdated = false;
			}

			m_SubMeshDescriptor.indexStart = (int)cmd.StartIndex;
			m_SubMeshDescriptor.indexCount = (int)cmd.IndexCount;
			m_SubMeshDescriptor.topology = MeshTopology.Triangles;

			int currentSubMeshIndex = -1;
			meshInfo.GetSubMeshId(m_SubMeshDescriptor, out currentSubMeshIndex);
			m_CommandBuffer.DrawMesh(meshInfo.m_Mesh, Matrix4x4.identity, m_MaterialRenoir, currentSubMeshIndex, m_ShaderPass, m_PropertyBlock);

			if (!foundMesh)
			{
				m_Meshes.Add(m_CurrentMeshBuffers, meshInfo);
			}
		}


		// DontValidateIndices - Renoir is generating geometry by reusing different
		// combinations of buffers for the different meshes,
		// so we expect all geometry to be correct and no checks are needed.
		// DontResetBoneBounds - we don't have skinned meshes, also we reset the meshes after buffer execution.
		// DontNotifyMeshUsers - we don't have components which should be notified.
		const MeshUpdateFlags m_BufferUpdateFlags = MeshUpdateFlags.DontValidateIndices |
													MeshUpdateFlags.DontResetBoneBounds |
													MeshUpdateFlags.DontNotifyMeshUsers;
		private void UpdateMeshBuffers(ref Mesh mesh)
		{
			StandardVertexBuffer vb;
			SlimVertexBuffer vbSlim;
			if (m_StandardVertexBuffers.TryGetValue(m_CurrentMeshBuffers.VBO, out vb))
			{
				mesh.SetVertexBufferParams((int)vb.m_UsedElementsCount, m_StandartVertexLayout);
				mesh.SetVertexBufferData(vb.m_Buffer, 0, 0, (int)vb.m_UsedElementsCount, 0, m_BufferUpdateFlags);
			}
			else if(m_SlimVertexBuffers.TryGetValue(m_CurrentMeshBuffers.VBO, out vbSlim))
			{
				mesh.SetVertexBufferParams((int)vbSlim.m_UsedElementsCount, m_SlimVertexLayout);
				mesh.SetVertexBufferData(vbSlim.m_Buffer, 0, 0, (int)vbSlim.m_UsedElementsCount, 0, m_BufferUpdateFlags);
			}
			else
			{
				Debug.LogAssertion("Couldn't find vertex buffer with ID: " + m_CurrentMeshBuffers.VBO.Id);
			}

			ByteIndexBuffer byteIB;
			ShortIndexBuffer shortIB;
			IntegerIndexBuffer integerIB;
			if (m_ByteIndexBuffers.TryGetValue(m_CurrentMeshBuffers.IBO, out byteIB))
			{
				Debug.LogAssertion("Indices with format UINT8 are not supported!");
			}
			else if (m_ShortIndexBuffers.TryGetValue(m_CurrentMeshBuffers.IBO, out shortIB))
			{
				mesh.SetIndexBufferParams((int)shortIB.m_UsedElementsCount, IndexFormat.UInt16);
				mesh.SetIndexBufferData(shortIB.m_Buffer, 0, 0, (int)shortIB.m_UsedElementsCount, m_BufferUpdateFlags);
			}
			else if (m_IntegerIndexBuffers.TryGetValue(m_CurrentMeshBuffers.IBO, out integerIB))
			{
				mesh.SetIndexBufferParams((int)integerIB.m_UsedElementsCount, IndexFormat.UInt32);
				mesh.SetIndexBufferData(integerIB.m_Buffer, 0, 0, (int)integerIB.m_UsedElementsCount, m_BufferUpdateFlags);
			}
			else
			{
				Debug.LogAssertion("Couldn't find index buffer with ID: " + m_CurrentMeshBuffers.IBO.Id);
			}
		}

		private void DrawCustomEffect(DrawCustomEffectCmd cmd)
		{
			// TODO: Implement
			Debug.Assert(false, "Draw Custom Effect command is not implemented yet!");
			//unsafe
			//{
			//	Debug.Log(Marshal.PtrToStringAnsi(cmd.Effect.Name));
			//	IntPtr floatIntPtr = new IntPtr(cmd.Effect.Params);
			//	float[] floatArray = new float[16];

			//	Marshal.Copy(floatIntPtr, floatArray, 0, 16);

			//	Debug.Log(floatArray);
			//	var a = cmd.Effect.StringParams?.Select(x => Marshal.PtrToStringAnsi(x)).ToArray();
			//	Debug.Log(a);
			//}
		}

		private void BeginRenderPass(BeginRenderPassCmd cmd)
		{}

		private void EndRenderPass(EndRenderPassCmd cmd)
		{}

		private void ClearRenderTarget(ClearRenderTargetCmd cmd)
		{
			// Here we are not supposed to enter because Unity's API for
			// clearing RT doesn't support clearing of the stencil value.
			// That's why we are using DrawClearQuad shader when we need to clear RT.
			Debug.Assert(false);
			UnityEngine.Color color = new UnityEngine.Color();
			color.r = cmd.ClearColor.r;
			color.g = cmd.ClearColor.g;
			color.b = cmd.ClearColor.b;
			color.a = cmd.ClearColor.a;
			m_CommandBuffer.ClearRenderTarget(cmd.ShouldClearDepth, cmd.ShouldClearColor, color, cmd.ClearDepthValue);
		}

		private void SetScissorRect(SetScissorRectCmd cmd)
		{
			if (cmd.Enabled)
			{
				Rect scissorRect = new Rect();
				scissorRect.height = cmd.Rect.w;
				scissorRect.width = cmd.Rect.z;
				scissorRect.y = cmd.Rect.y;
				scissorRect.x = cmd.Rect.x;
				m_CommandBuffer.EnableScissorRect(scissorRect);
			}
			else
			{
				m_CommandBuffer.DisableScissorRect();
			}
		}

		private void SetViewport(SetViewportCmd cmd)
		{
			Rect viewportRect = new Rect();
			viewportRect.height = cmd.Rect.w;
			viewportRect.width = cmd.Rect.z;
			viewportRect.y = cmd.Rect.y;
			viewportRect.x = cmd.Rect.x;
			m_CommandBuffer.SetViewport(viewportRect);
		}

		private void SetStencilReference(SetStencilReferenceCmd cmd)
		{
			m_CurrentPipelineState.StencilRef = cmd.Value;
			ApplyPSO();
		}

		private void ResolveRenderTarget(ResolveRenderTargetCmd cmd)
		{
			Texture temp;
			if (!(m_Textures.TryGetValue(cmd.Source, out temp)))
			{
				Debug.LogError("Couldn't find needed texture to resolve!");
				return;
			}
			RenderTexture srcTex = (RenderTexture)temp;

			temp = null;
			if (!(m_Textures.TryGetValue(cmd.Destination, out temp)))
			{
				Debug.LogError("Couldn't find needed texture to resolve into!");
				return;
			}
			RenderTexture desTex = (RenderTexture)temp;

			Debug.Assert(
				srcTex.width == desTex.width &&
				srcTex.width == desTex.width &&
				srcTex.graphicsFormat == desTex.graphicsFormat
				);
			//Target render texture must have the same dimensions and format as the source.
			srcTex.ResolveAntiAliasedSurface(desTex);
		}

		private void PushMarker(PushMetadataCmd cmd)
		{
			// TODO: Here we should push event marker to the GPU
		}

		private void PopMarker(PopMetadataCmd cmd)
		{
			// TODO: Here we should pop event marker
		}
	};
}

#endif
