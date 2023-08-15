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
This file is part of Renoir, a modern graphics library.

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

#if COHERENT_CPU_32BIT
using System;
using System.Runtime.InteropServices;

namespace renoir
{

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct BackendCommandOffset
{
	[FieldOffset(0)] public BackendCommands Command;
	[FieldOffset(4)] public uint Offset;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct SetPipelineStateCmd
{
	[FieldOffset(0)] public PipelineStateObject Object;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct SetVSConstantBuffersCmd
{
	[FieldOffset(0)] public uint StartSlot;
	[FieldOffset(4)] public uint Count;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct SetPSConstantBuffersCmd
{
	[FieldOffset(0)] public uint StartSlot;
	[FieldOffset(4)] public uint Count;
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct UpdateConstantBufferCmd
{
	[FieldOffset(0)] public ConstantBufferObject Object;
	[FieldOffset(4)] public uint Offset;
	[FieldOffset(8)] public uint DataSize;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct SetVertexBufferCmd
{
	[FieldOffset(0)] public VertexBufferObject Object;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct SetMultipleVertexBuffersCmd
{
	[FieldOffset(0)] public uint StartSlot;
	[FieldOffset(4)] public uint Count;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct SetIndexBufferCmd
{
	[FieldOffset(0)] public IndexBufferObject Object;
	[FieldOffset(4)] public IndexBufferType Type;
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct DrawIndexedCmd
{
	[FieldOffset(0)] public uint IndexCount;
	[FieldOffset(4)] public uint StartIndex;
	[FieldOffset(8)] public uint BaseVertex;
	[FieldOffset(12)] public PrimitiveTopology Topology;
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct DrawCmd
{
	[FieldOffset(0)] public uint Count;
	[FieldOffset(4)] public uint StartVertex;
	[FieldOffset(8)] public PrimitiveTopology Topology;
}

[StructLayout(LayoutKind.Explicit, Size = 24)]
public struct DrawIndexedInstancedCmd
{
	[FieldOffset(0)] public uint IndexCount;
	[FieldOffset(4)] public uint StartIndex;
	[FieldOffset(8)] public uint BaseVertex;
	[FieldOffset(12)] public uint InstanceCount;
	[FieldOffset(16)] public uint StartInstance;
	[FieldOffset(20)] public PrimitiveTopology Topology;
}

[StructLayout(LayoutKind.Explicit, Size = 24)]
public struct DrawInstancedCmd
{
	[FieldOffset(0)] public uint VertexCount;
	[FieldOffset(4)] public uint StartVertex;
	[FieldOffset(8)] public uint InstanceCount;
	[FieldOffset(12)] public uint StartInstance;
	[FieldOffset(16)] public PrimitiveTopology Topology;
}

[StructLayout(LayoutKind.Explicit, Size = 288)]
public struct DrawCustomEffectCmd
{
	[FieldOffset(0)] public uint IndexCount;
	[FieldOffset(4)] public uint StartIndex;
	[FieldOffset(8)] public uint BaseVertex;
	[FieldOffset(12)] public PrimitiveTopology Topology;
	[FieldOffset(16)] public Texture2DObject Texture;
	[FieldOffset(20)] public Texture2D TextureInfo;
	[FieldOffset(72)] public float4 UVScaleBias;
	[FieldOffset(88)] public float4 TargetGeometryPositionSize;
	[FieldOffset(104)] public float4 Viewport;
	[FieldOffset(120)] public float2 RenderTargetSize;
	[FieldOffset(128)] public unaligned_float4x4 TransformMatrix;
	[FieldOffset(192)] public IntPtr UserData;
	[FieldOffset(196)] public UserEffect Effect;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct SetRenderTargetCmd
{
	[FieldOffset(0)] public Texture2DObject Object;
	[FieldOffset(4)] public DepthStencilTextureObject DSObject;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct SetMultipleRenderTargetsCmd
{
	[FieldOffset(0)] public DepthStencilTextureObject DSObject;
	[FieldOffset(4)] public uint Count;
}

[StructLayout(LayoutKind.Explicit, Size = 40)]
public struct ClearRenderTargetCmd
{
	[FieldOffset(0)] public TextureObject Object;
	[FieldOffset(8)] public DepthStencilTextureObject DSObject;
	[FieldOffset(12)] public Color ClearColor;
	[FieldOffset(28)] public float ClearDepthValue;
	[FieldOffset(32)] public byte ClearClipValue;
	[FieldOffset(33)][MarshalAs(UnmanagedType.I1)] public bool ShouldClearColor;
	[FieldOffset(34)][MarshalAs(UnmanagedType.I1)] public bool ShouldClearDepth;
	[FieldOffset(35)][MarshalAs(UnmanagedType.I1)] public bool ClearClipping;
}

[StructLayout(LayoutKind.Explicit, Size = 24)]
public struct SetScissorRectCmd
{
	[FieldOffset(0)][MarshalAs(UnmanagedType.I1)] public bool Enabled;
	[FieldOffset(4)] public float4 Rect;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct SetPSTexturesCmd
{
	[FieldOffset(0)] public uint StartSlot;
	[FieldOffset(4)] public uint Count;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct SetPSSamplersCmd
{
	[FieldOffset(0)] public uint StartSlot;
	[FieldOffset(4)] public uint Count;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct SetStencilReferenceCmd
{
	[FieldOffset(0)] public byte Value;
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct SetViewportCmd
{
	[FieldOffset(0)] public float4 Rect;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct ResolveRenderTargetCmd
{
	[FieldOffset(0)] public Texture2DObject Source;
	[FieldOffset(4)] public Texture2DObject Destination;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct PushMetadataCmd
{
	[FieldOffset(0)] public uint Length;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct PopMetadataCmd
{
	[FieldOffset(0)] public byte _Dummy;
}

[StructLayout(LayoutKind.Explicit, Size = 48)]
public struct BeginRenderPassCmd
{
	[FieldOffset(0)] public Texture2DObject Object;
	[FieldOffset(4)] public PixelFormat RTFormat;
	[FieldOffset(8)] public Texture2DObject ResolveObject;
	[FieldOffset(12)] public DepthStencilTextureObject DSObject;
	[FieldOffset(16)] public uint MSAASamplesCount;
	[FieldOffset(20)] public Color ClearColor;
	[FieldOffset(36)] public byte ClearClipValue;
	[FieldOffset(40)] public float ClearDepthValue;
	[FieldOffset(44)][MarshalAs(UnmanagedType.I1)] public bool ShouldResolveRT;
	[FieldOffset(45)][MarshalAs(UnmanagedType.I1)] public bool ShouldClearColor;
	[FieldOffset(46)][MarshalAs(UnmanagedType.I1)] public bool ShouldClearStencil;
	[FieldOffset(47)][MarshalAs(UnmanagedType.I1)] public bool ShouldClearDepth;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct EndRenderPassCmd
{
	[FieldOffset(0)] public byte _Dummy;
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct TransitionTextureStateCmd
{
	[FieldOffset(0)] public Texture2DObject Tex;
	[FieldOffset(4)] public TextureState OldState;
	[FieldOffset(8)] public TextureState NewState;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct TransitionDSTextureStateCmd
{
	[FieldOffset(0)] public DepthStencilTextureObject DSTex;
	[FieldOffset(4)] public DSTextureState NewState;
}

[StructLayout(LayoutKind.Explicit, Size = 4)]
public struct PipelineStateObject
{
	[FieldOffset(0)] public uint Id;
}

[StructLayout(LayoutKind.Explicit, Size = 4)]
public struct ConstantBufferObject
{
	[FieldOffset(0)] public uint Id;
}

[StructLayout(LayoutKind.Explicit, Size = 4)]
public struct VertexBufferObject
{
	[FieldOffset(0)] public uint Id;
}

[StructLayout(LayoutKind.Explicit, Size = 4)]
public struct IndexBufferObject
{
	[FieldOffset(0)] public uint Id;
}

[StructLayout(LayoutKind.Explicit, Size = 4)]
public struct Texture2DObject
{
	[FieldOffset(0)] public uint Id;
}

[StructLayout(LayoutKind.Explicit, Size = 52)]
public struct Texture2D
{
	[FieldOffset(0)] public uint Width;
	[FieldOffset(4)] public uint Height;
	[FieldOffset(8)] public uint MipsCount;
	[FieldOffset(12)] public uint SamplesCount;
	[FieldOffset(16)] public uint Props;
	[FieldOffset(20)][MarshalAs(UnmanagedType.I1)] public bool IsRenderTarget;
	[FieldOffset(24)] public PixelFormat Format;
	[FieldOffset(28)] public ushort TileMode;
	[FieldOffset(30)] public ushort BaseAlignment;
	[FieldOffset(32)] public uint ContentRectX;
	[FieldOffset(36)] public uint ContentRectY;
	[FieldOffset(40)] public uint ContentRectWidth;
	[FieldOffset(44)] public uint ContentRectHeight;
	[FieldOffset(48)] public ImageOrigin Origin;
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public unsafe struct float4
{
	[FieldOffset(0)] public float x;
	[FieldOffset(0)] public fixed float _v[4];
	[FieldOffset(4)] public float y;
	[FieldOffset(8)] public float z;
	[FieldOffset(12)] public float w;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public unsafe struct float2
{
	[FieldOffset(0)] public float x;
	[FieldOffset(0)] public fixed float _v[2];
	[FieldOffset(4)] public float y;
}

[StructLayout(LayoutKind.Explicit, Size = 64)]
public unsafe struct unaligned_float4x4
{
	[FieldOffset(0)] public float _11;
	[FieldOffset(0)] public fixed float m[16];
	// [FieldOffset(0)] public fixed float mm[4][4];
	[FieldOffset(4)] public float _12;
	[FieldOffset(8)] public float _13;
	[FieldOffset(12)] public float _14;
	[FieldOffset(16)] public float _21;
	[FieldOffset(20)] public float _22;
	[FieldOffset(24)] public float _23;
	[FieldOffset(28)] public float _24;
	[FieldOffset(32)] public float _31;
	[FieldOffset(36)] public float _32;
	[FieldOffset(40)] public float _33;
	[FieldOffset(44)] public float _34;
	[FieldOffset(48)] public float _41;
	[FieldOffset(52)] public float _42;
	[FieldOffset(56)] public float _43;
	[FieldOffset(60)] public float _44;
}

[StructLayout(LayoutKind.Explicit, Size = 88)]
public unsafe struct UserEffect
{
	public const ushort NUMERIC_PARAM_COUNT = 16;
	public const ushort STRING_PARAM_COUNT = 4;
	[FieldOffset(0)] public uint Id;
	[FieldOffset(4)] public IntPtr Name;
	[FieldOffset(8)] public fixed float Params[16];
	[FieldOffset(72)][MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public IntPtr[] StringParams;
}

[StructLayout(LayoutKind.Explicit, Size = 4)]
public struct DepthStencilTextureObject
{
	[FieldOffset(0)] public uint Id;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct TextureObject
{
    public enum TextureType
    {
       TT_2D = 0,
       TT_2DArray = 1,
    }

	[FieldOffset(0)] public uint Id;
	[FieldOffset(4)] public sbyte Type;
	[FieldOffset(6)] public short Index;
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct Color
{
	[FieldOffset(0)] public float a;
	[FieldOffset(4)] public float b;
	[FieldOffset(8)] public float g;
	[FieldOffset(12)] public float r;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct BackendResourceCommandOffset
{
	[FieldOffset(0)] public BackendResourceCommands Command;
	[FieldOffset(4)] public uint Offset;
}

[StructLayout(LayoutKind.Explicit, Size = 80)]
public struct WrapUserRTCmd
{
	[FieldOffset(0)] public IntPtr UserObject;
	[FieldOffset(4)] public Texture2D Description;
	[FieldOffset(56)] public Texture2DObject Object;
	[FieldOffset(60)] public IntPtr DepthStencil;
	[FieldOffset(64)] public DepthStencilTexture DSDescription;
	[FieldOffset(76)] public DepthStencilTextureObject DSObject;
}

[StructLayout(LayoutKind.Explicit, Size = 64)]
public struct CreateTextureCmd
{
	[FieldOffset(0)] public Texture2DObject Texture;
	[FieldOffset(4)] public Texture2D TextureDesc;
	[FieldOffset(56)] public uint DataLength;
}

[StructLayout(LayoutKind.Explicit, Size = 64)]
public struct CreateTextureWithDataPtrCmd
{
	[FieldOffset(0)] public Texture2DObject Texture;
	[FieldOffset(4)] public Texture2D TextureDesc;
	[FieldOffset(56)] public IntPtr Data;
	[FieldOffset(60)] public uint DataLength;
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct CreateDSTextureCmd
{
	[FieldOffset(0)] public DepthStencilTextureObject Texture;
	[FieldOffset(4)] public DepthStencilTexture TextureDesc;
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct CreateVertexBufferCmd
{
	[FieldOffset(0)] public VertexType VertexType;
	[FieldOffset(4)] public VertexBufferObject Object;
	[FieldOffset(8)] public uint Size;
	[FieldOffset(12)][MarshalAs(UnmanagedType.I1)] public bool ChangesOften;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct DestroyVertexBufferCmd
{
	[FieldOffset(0)] public VertexBufferObject Object;
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct CreateIndexBufferCmd
{
	[FieldOffset(0)] public IndexBufferType IndexType;
	[FieldOffset(4)] public IndexBufferObject Object;
	[FieldOffset(8)] public uint Size;
	[FieldOffset(12)][MarshalAs(UnmanagedType.I1)] public bool ChangesOften;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct DestroyIndexBufferCmd
{
	[FieldOffset(0)] public IndexBufferObject Object;
}

[StructLayout(LayoutKind.Explicit, Size = 64)]
public struct WrapUserTextureCmd
{
	[FieldOffset(0)] public Texture2DObject Texture;
	[FieldOffset(4)] public Texture2D TextureDesc;
	[FieldOffset(56)] public IntPtr UserTextureData;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct DestroyTextureCmd
{
	[FieldOffset(0)] public Texture2DObject Texture;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct DestroyDSTextureCmd
{
	[FieldOffset(0)] public DepthStencilTextureObject DSTexture;
}

[StructLayout(LayoutKind.Explicit, Size = 64)]
public struct UpdateTextureCmd
{
	[FieldOffset(0)] public Texture2DObject Texture;
	[FieldOffset(4)] public Texture2D TextureDesc;
	[FieldOffset(56)] public IntPtr UpdateTextureInfo;
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct UpdateTextureCmdInfo
{
	[FieldOffset(0)] public IntPtr UpdateBoxes;
	[FieldOffset(4)] public IntPtr PixelData;
	[FieldOffset(8)] public uint BoxesCount;
	[FieldOffset(12)][MarshalAs(UnmanagedType.I1)] public bool WillOverwrite;
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct CreateConstantBufferCmd
{
	[FieldOffset(0)] public CBType ConstantBufferType;
	[FieldOffset(4)] public ConstantBufferObject Object;
	[FieldOffset(8)] public uint Size;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct DestroyConstantBufferCmd
{
	[FieldOffset(0)] public ConstantBufferObject Object;
}

[StructLayout(LayoutKind.Explicit, Size = 152)]
public struct CreatePSOCmd
{
	[FieldOffset(0)] public PipelineState PSO;
	[FieldOffset(148)] public PipelineStateObject Object;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct DestroyPSOCmd
{
	[FieldOffset(0)] public PipelineStateObject Object;
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct CreateSamplerCmd
{
	[FieldOffset(0)] public Sampler2DObject Object;
	[FieldOffset(4)] public Sampler2D SamplerDesc;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct DestroySamplerCmd
{
	[FieldOffset(0)] public Sampler2DObject Object;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct SetDebugNameCmd
{
	[FieldOffset(0)] public Texture2DObject Texture;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct SetDSDebugNameCmd
{
	[FieldOffset(0)] public DepthStencilTextureObject Texture;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct SetVBODebugNameCmd
{
	[FieldOffset(0)] public VertexBufferObject VBO;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct SetIBODebugNameCmd
{
	[FieldOffset(0)] public IndexBufferObject IBO;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct SetPSODebugNameCmd
{
	[FieldOffset(0)] public PipelineStateObject PSO;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct SetCBODebugNameCmd
{
	[FieldOffset(0)] public ConstantBufferObject CBO;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct SetSamplerDebugNameCmd
{
	[FieldOffset(0)] public Sampler2DObject SamplerObject;
}

[StructLayout(LayoutKind.Explicit, Size = 12)]
public struct DepthStencilTexture
{
	[FieldOffset(0)] public uint Width;
	[FieldOffset(4)] public uint Height;
	[FieldOffset(8)] public uint SamplesCount;
}

[StructLayout(LayoutKind.Explicit, Size = 148)]
public struct PipelineState
{
	[FieldOffset(0)] public ShaderType VertexShader;
	[FieldOffset(4)] public ShaderType PixelShader;
	[FieldOffset(8)] public BlendingState Blending;
	[FieldOffset(52)] public DepthStencilState DepthStencil;
	[FieldOffset(104)][MarshalAs(UnmanagedType.I1)] public bool CullEnable;
	[FieldOffset(105)][MarshalAs(UnmanagedType.I1)] public bool DepthBiasEnable;
	[FieldOffset(108)] public PixelFormat RTFormat;
	[FieldOffset(112)] public CullFaceType CullFace;
	[FieldOffset(116)] public FrontFaceType FrontFace;
	[FieldOffset(120)] public ColorWriteMask ColorMask;
	[FieldOffset(124)] public uint MSAASamples;
	[FieldOffset(128)] public float DepthBias;
	[FieldOffset(132)] public float SlopeScaledDepthBias;
	[FieldOffset(136)] public PipelineType Type;
	[FieldOffset(140)] public ShadingPipelineObject UserShading;
	[FieldOffset(144)] public CustomVertexDeclarationObject VertexDeclaration;
}

[StructLayout(LayoutKind.Explicit, Size = 44)]
public struct BlendingState
{
	[FieldOffset(0)][MarshalAs(UnmanagedType.I1)] public bool Enabled;
	[FieldOffset(4)] public BlendingCoeff SrcBlend;
	[FieldOffset(8)] public BlendingCoeff DestBlend;
	[FieldOffset(12)] public BlendingOp BlendOp;
	[FieldOffset(16)] public BlendingCoeff SrcBlendAlpha;
	[FieldOffset(20)] public BlendingCoeff DestBlendAlpha;
	[FieldOffset(24)] public BlendingOp BlendOpAlpha;
	[FieldOffset(28)] public float4 BlendFactor;
}

[StructLayout(LayoutKind.Explicit, Size = 52)]
public struct DepthStencilState
{
	[FieldOffset(0)] public ComparisonFunction DepthFunction;
	[FieldOffset(4)] public float ZNear;
	[FieldOffset(8)] public float ZFar;
	[FieldOffset(12)] public StencilOp FailOp;
	[FieldOffset(16)] public StencilOp ZFailOp;
	[FieldOffset(20)] public StencilOp PassOp;
	[FieldOffset(24)] public ComparisonFunction Function;
	[FieldOffset(28)] public StencilOp BackFailOp;
	[FieldOffset(32)] public StencilOp BackZFailOp;
	[FieldOffset(36)] public StencilOp BackPassOp;
	[FieldOffset(40)] public ComparisonFunction BackFunction;
	[FieldOffset(44)] public byte StencilWriteMask;
	[FieldOffset(45)] public byte StencilReadMask;
	[FieldOffset(46)][MarshalAs(UnmanagedType.I1)] public bool DepthEnable;
	[FieldOffset(47)][MarshalAs(UnmanagedType.I1)] public bool DepthWrites;
	[FieldOffset(48)][MarshalAs(UnmanagedType.I1)] public bool StencilEnable;
}

[StructLayout(LayoutKind.Explicit, Size = 4)]
public struct ShadingPipelineObject
{
	[FieldOffset(0)] public uint Id;
}

[StructLayout(LayoutKind.Explicit, Size = 4)]
public struct CustomVertexDeclarationObject
{
	[FieldOffset(0)] public uint Id;
}

[StructLayout(LayoutKind.Explicit, Size = 4)]
public struct Sampler2DObject
{
	[FieldOffset(0)] public uint Id;
}

[StructLayout(LayoutKind.Explicit, Size = 12)]
public struct Sampler2D
{
	[FieldOffset(0)] public SamplerFilter Filtering;
	[FieldOffset(4)] public SamplerAddressing UAddress;
	[FieldOffset(8)] public SamplerAddressing VAddress;
}

[StructLayout(LayoutKind.Explicit, Size = 12)]
public unsafe struct float3
{
	[FieldOffset(0)] public float x;
	[FieldOffset(0)] public fixed float _v[3];
	[FieldOffset(4)] public float y;
	[FieldOffset(8)] public float z;
}

[StructLayout(LayoutKind.Explicit, Size = 64)]
public unsafe struct float4x4
{
    public enum MatKeys
    {
       FL4_Tx = 3,
       FL4_Ty = 7,
       FL4_Tz = 11,
       FL4_Sx = 0,
       FL4_Sy = 5,
       FL4_Sz = 10,
       FL4_Skx = 1,
       FL4_Sky = 4,
       FL4_RX11 = 5,
       FL4_RX12 = 6,
       FL4_RX21 = 9,
       FL4_RX22 = 10,
       FL4_RY00 = 0,
       FL4_RY02 = 2,
       FL4_RY20 = 8,
       FL4_RY22 = 10,
       FL4_RZ00 = 0,
       FL4_RZ01 = 1,
       FL4_RZ10 = 4,
       FL4_RZ11 = 5,
       FL4_Sxx = 0,
       FL4_Sxy = 4,
       FL4_Sxz = 8,
       FL4_Syx = 1,
       FL4_Syy = 5,
       FL4_Syz = 9,
       FL4_Szx = 2,
       FL4_Szy = 6,
       FL4_Szz = 10,
    }

	[FieldOffset(0)] public float _11;
	[FieldOffset(0)] public fixed float m[16];
	// [FieldOffset(0)] public fixed float mm[4][4];
	[FieldOffset(4)] public float _12;
	[FieldOffset(8)] public float _13;
	[FieldOffset(12)] public float _14;
	[FieldOffset(16)] public float _21;
	[FieldOffset(20)] public float _22;
	[FieldOffset(24)] public float _23;
	[FieldOffset(28)] public float _24;
	[FieldOffset(32)] public float _31;
	[FieldOffset(36)] public float _32;
	[FieldOffset(40)] public float _33;
	[FieldOffset(44)] public float _34;
	[FieldOffset(48)] public float _41;
	[FieldOffset(52)] public float _42;
	[FieldOffset(56)] public float _43;
	[FieldOffset(60)] public float _44;
}

[StructLayout(LayoutKind.Explicit, Size = 36)]
public struct Matrix2DDecomposition
{
	[FieldOffset(0)] public float2 Translation;
	[FieldOffset(8)] public float2 Scale;
	[FieldOffset(16)] public float Angle;
	[FieldOffset(20)] public float m11;
	[FieldOffset(24)] public float m12;
	[FieldOffset(28)] public float m21;
	[FieldOffset(32)] public float m22;
}

[StructLayout(LayoutKind.Explicit, Size = 68)]
public struct Matrix3DDecomposition
{
	[FieldOffset(0)] public float3 Scale;
	[FieldOffset(12)] public float SkewXY;
	[FieldOffset(16)] public float SkewXZ;
	[FieldOffset(20)] public float SkewYZ;
	[FieldOffset(24)] public float4 QuaternionRotation;
	[FieldOffset(40)] public float3 Translation;
	[FieldOffset(52)] public float4 Perspective;
}

[StructLayout(LayoutKind.Explicit, Size = 80)]
public unsafe struct ColorMatrix
{
	[FieldOffset(0)] public fixed float m[20];
	// [FieldOffset(0)] public fixed float mm[5][4];
	[FieldOffset(0)] public float _11;
	[FieldOffset(4)] public float _12;
	[FieldOffset(8)] public float _13;
	[FieldOffset(12)] public float _14;
	[FieldOffset(16)] public float _21;
	[FieldOffset(20)] public float _22;
	[FieldOffset(24)] public float _23;
	[FieldOffset(28)] public float _24;
	[FieldOffset(32)] public float _31;
	[FieldOffset(36)] public float _32;
	[FieldOffset(40)] public float _33;
	[FieldOffset(44)] public float _34;
	[FieldOffset(48)] public float _41;
	[FieldOffset(52)] public float _42;
	[FieldOffset(56)] public float _43;
	[FieldOffset(60)] public float _44;
	[FieldOffset(64)] public float _51;
	[FieldOffset(68)] public float _52;
	[FieldOffset(72)] public float _53;
	[FieldOffset(76)] public float _54;
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct UpdateBox
{
	[FieldOffset(0)] public uint Top;
	[FieldOffset(4)] public uint Left;
	[FieldOffset(8)] public uint Right;
	[FieldOffset(12)] public uint Bottom;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct WordSpacing
{
	[FieldOffset(0)] public IntPtr Data;
	[FieldOffset(4)] public uint Count;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct GlyphOffsets
{
	[FieldOffset(0)] public IntPtr Data;
	[FieldOffset(4)] public uint Count;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct BufferDescription
{
	[FieldOffset(0)] public uint SizeInBytes;
	[FieldOffset(4)] public BufferUsageType Usage;
}

[StructLayout(LayoutKind.Explicit, Size = 32)]
public struct VertexElementDescription
{
    public enum VertexType
    {
       Byte = 0,
       UnsignedByte = 1,
       Short = 2,
       UnsignedShort = 3,
       Int = 4,
       UnsignedInt = 5,
       Fixed = 6,
       Float = 7,
    }

	[FieldOffset(0)] public uint ElementSize;
	[FieldOffset(4)] public VertexType Type;
	[FieldOffset(8)][MarshalAs(UnmanagedType.I1)] public bool Normalized;
	[FieldOffset(12)] public uint Stride;
	[FieldOffset(16)] public uint Offset;
	[FieldOffset(20)] public uint Slot;
	[FieldOffset(24)] public uint Location;
	[FieldOffset(28)] public uint InstanceDataStepRate;
}

[StructLayout(LayoutKind.Explicit, Size = 64)]
public struct CBTransforms
{
	[FieldOffset(0)] public float4x4 Transform;
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct GlobalPixelCB
{
	[FieldOffset(0)] public float2 ViewportSize;
	[FieldOffset(8)] public float2 ClipMaskViewportSize;
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct StandardPrimitivePixel
{
	[FieldOffset(0)] public int ShaderType;
	[FieldOffset(4)] public int ShouldClip;
	[FieldOffset(8)] public float2 ClipMaskOrigin;
}

[StructLayout(LayoutKind.Explicit, Size = 32)]
public struct StandardPrimitiveAdditionalPixel
{
	[FieldOffset(0)] public float4 PrimProps0;
	[FieldOffset(16)] public float4 PrimProps1;
}

[StructLayout(LayoutKind.Explicit, Size = 80)]
public struct RenoirShaderVS
{
	[FieldOffset(0)] public float4x4 Matrix0;
	[FieldOffset(64)] public float4 Prop0;
}

[StructLayout(LayoutKind.Explicit, Size = 64)]
public struct RenoirShaderPS
{
	[FieldOffset(0)] public float4 Prop0;
	[FieldOffset(16)] public float4 Prop1;
	[FieldOffset(32)] public float4 Prop2;
	[FieldOffset(48)] public float4 Prop3;
}

[StructLayout(LayoutKind.Sequential, Size = 144)]
public struct EffectsPixelCB
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public float[] Coefficients;
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public float2[] PixelOffsets;
}

[StructLayout(LayoutKind.Explicit, Size = 4)]
public struct UserShaderObject
{
	[FieldOffset(0)] public uint Id;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct ShadingPipeline
{
	[FieldOffset(0)] public UserShaderObject Vertex;
	[FieldOffset(4)] public UserShaderObject Fragment;
}

[StructLayout(LayoutKind.Explicit, Size = 48)]
public struct StandardVertex
{
	[FieldOffset(0)] public float4 Position;
	[FieldOffset(16)] public float4 Color;
	[FieldOffset(32)] public float4 Additional;
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct SlimVertex
{
	[FieldOffset(0)] public float4 Position;
}

[StructLayout(LayoutKind.Sequential, Size = 328)]
public struct RendererCaps
{
	[MarshalAs(UnmanagedType.I1)] public bool SupportsMSAATextures;
	[MarshalAs(UnmanagedType.I1)] public bool RequiresMSAAResolve;
	[MarshalAs(UnmanagedType.I1)] public bool RequiresDrawFences;
	[MarshalAs(UnmanagedType.I1)] public bool RequiresShaderTypeInShader;
	[MarshalAs(UnmanagedType.I1)] public bool RequiresYFlipForLayers;
	[MarshalAs(UnmanagedType.I1)] public bool SupportsNPOTTextureOps;
	[MarshalAs(UnmanagedType.I1)] public bool ShaderChangeRequiresResourcesRebind;
	[MarshalAs(UnmanagedType.I1)] public bool RequiresRBSwapForImages;
	[MarshalAs(UnmanagedType.I1)] public bool SupportsA8RenderTarget;
	[MarshalAs(UnmanagedType.I1)] public bool SupportsOnly16bitIndices;
	[MarshalAs(UnmanagedType.I1)] public bool PreferCPUWorkload;
	[MarshalAs(UnmanagedType.I1)] public bool ShouldUseRenderPasses;
	[MarshalAs(UnmanagedType.I1)] public bool ShouldClearRTWithClearQuad;
	[MarshalAs(UnmanagedType.I1)] public bool CanOutputDepthInPixelShader;
	[MarshalAs(UnmanagedType.I1)] public bool SupportsTwoSidedStencilOperations;
	[MarshalAs(UnmanagedType.I1)] public bool SupportsSharedIndexBuffers;
	[MarshalAs(UnmanagedType.I1)] public bool EndingRenderPassRequiresStateUpdate;
	[MarshalAs(UnmanagedType.I1)] public bool SupportsResourcesStateTransitions;
	 public int ConstantBufferRingSize;
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 72)] public ShaderType[] ShaderMapping;
	 public int ConstantBufferBlockAlignment;
	 public int ConstantBufferBlocksCount;
	 public uint MaxTextureWidth;
	 public uint MaxTextureHeight;
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct BackendCommandsBuffer
{
	[FieldOffset(0)] public IntPtr Offsets;
	[FieldOffset(4)] public IntPtr Commands;
	[FieldOffset(8)] public uint Count;
	[FieldOffset(12)] public uint CommandsDataSize;
}

[StructLayout(LayoutKind.Explicit, Size = 12)]
public struct BackendResourceCommandsBuffer
{
	[FieldOffset(0)] public IntPtr Offsets;
	[FieldOffset(4)] public IntPtr Commands;
	[FieldOffset(8)] public uint Count;
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct ConstantBufferUpdateData
{
	[FieldOffset(0)] public ConstantBufferObject Object;
	[FieldOffset(4)] public IntPtr Data;
	[FieldOffset(8)] public uint Start;
	[FieldOffset(12)] public uint DataSize;
}

[StructLayout(LayoutKind.Explicit, Size = 4)]
public struct Texture2DArrayObject
{
	[FieldOffset(0)] public uint Id;
}

[StructLayout(LayoutKind.Explicit, Size = 60)]
public struct Texture2DArray
{
	[FieldOffset(0)] public uint Width;
	[FieldOffset(4)] public uint Height;
	[FieldOffset(8)] public uint MipsCount;
	[FieldOffset(12)] public uint SamplesCount;
	[FieldOffset(16)] public uint Props;
	[FieldOffset(20)][MarshalAs(UnmanagedType.I1)] public bool IsRenderTarget;
	[FieldOffset(24)] public PixelFormat Format;
	[FieldOffset(28)] public ushort TileMode;
	[FieldOffset(30)] public ushort BaseAlignment;
	[FieldOffset(32)] public uint ContentRectX;
	[FieldOffset(36)] public uint ContentRectY;
	[FieldOffset(40)] public uint ContentRectWidth;
	[FieldOffset(44)] public uint ContentRectHeight;
	[FieldOffset(48)] public ImageOrigin Origin;
	[FieldOffset(52)] public uint ArraySize;
	[FieldOffset(56)][MarshalAs(UnmanagedType.I1)] public bool IsCubemap;
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct PixelFormatInfo
{
	[FieldOffset(0)] public PixelFormat RenoirFormat;
	[FieldOffset(4)] public int BlockSizeX;
	[FieldOffset(8)] public int BlockSizeY;
	[FieldOffset(12)] public int BytesPerBlock;
}

[StructLayout(LayoutKind.Explicit, Size = 192)]
public struct DrawData
{
	[FieldOffset(0)] public uint ViewId;
	[FieldOffset(4)] public IntPtr SubLayerCompositionId;
	[FieldOffset(8)] public IntPtr CustomSceneMetadata;
	[FieldOffset(12)] public Texture2DObject Texture;
	[FieldOffset(16)] public Texture2D TextureInfo;
	[FieldOffset(68)] public float2 UVScale;
	[FieldOffset(76)] public float2 UVOffset;
	[FieldOffset(96)] public float4x4 FinalTransform;
	[FieldOffset(160)] public Rectangle Untransformed2DTargetRect;
	[FieldOffset(180)] public ColorMixingModes MixingMode;
}

[StructLayout(LayoutKind.Explicit, Size = 20)]
public struct Rectangle
{
	[FieldOffset(0)] public float3 Position;
	[FieldOffset(12)] public float2 Size;
}


    public enum BackendCommands
    {
       BC_SetPipelineState = 0,
       BC_SetVSConstantBuffers = 1,
       BC_SetPSConstantBuffers = 2,
       BC_UpdateConstantBuffer = 3,
       BC_SetVertexBuffer = 4,
       BC_SetIndexBuffer = 5,
       BC_DrawIndexed = 6,
       BC_DrawCustomEffect = 7,
       BC_SetRenderTarget = 8,
       BC_ClearRenderTarget = 9,
       BC_SetScissorRect = 10,
       BC_SetViewport = 11,
       BC_ResolveRenderTarget = 12,
       BC_SetPSSamplers = 13,
       BC_SetPSTextures = 14,
       BC_SetStencilReference = 15,
       BC_Draw = 16,
       BC_DrawInstanced = 17,
       BC_DrawIndexedInstanced = 18,
       BC_SetMultipleVertexBuffers = 19,
       BC_SetMultipleRenderTargets = 20,
       BC_ClearRenderTargetWithState = 21,
       BC_PushMetadata = 22,
       BC_PopMetadata = 23,
       BC_BeginRenderPass = 24,
       BC_EndRenderPass = 25,
       BC_TransitionTextureState = 26,
       BC_TransitionDSTextureState = 27,
       BC_Count = 28,
    }

    public enum IndexBufferType
    {
       IBT_U8 = 0,
       IBT_U16 = 1,
       IBT_U32 = 2,
    }

    public enum PrimitiveTopology
    {
       PTO_TriangleList = 0,
       PTO_TriangleStrip = 1,
       PTO_Points = 2,
       PTO_Lines = 3,
       PTO_LineStrip = 4,
       PTO_Count = 5,
    }

    public enum PixelFormat
    {
       PF_R8G8B8A8 = 0,
       PF_R16G16B16A16 = 1,
       PF_R32G32B32A32 = 2,
       PF_R8 = 3,
       PF_R16 = 4,
       PF_BC1 = 5,
       PF_BC2 = 6,
       PF_BC3 = 7,
       PF_BC4 = 8,
       PF_BC5 = 9,
       PF_BC6 = 10,
       PF_BC7 = 11,
       PF_RGB8_ETC1 = 12,
       PF_RGB8_ETC2 = 13,
       PF_RGBA8_ETC2_EAC = 14,
       PF_RGB8_PUNCHTHROUGH_ALPHA1_ETC2 = 15,
       PF_R11_EAC = 16,
       PF_RG11_EAC = 17,
       PF_SIGNED_R11_EAC = 18,
       PF_SIGNED_RG11_EAC = 19,
       PF_SRGB_ETC2 = 20,
       PF_SRGB8_PUNCHTHROUGH_ALPHA1_ETC2 = 21,
       PF_SRGB_ALPHA8_ETC2_EAC = 22,
       PF_ASTC_RGBA_4x4 = 23,
       PF_ASTC_RGBA_5x4 = 24,
       PF_ASTC_RGBA_5x5 = 25,
       PF_ASTC_RGBA_6x5 = 26,
       PF_ASTC_RGBA_6x6 = 27,
       PF_ASTC_RGBA_8x5 = 28,
       PF_ASTC_RGBA_8x6 = 29,
       PF_ASTC_RGBA_8x8 = 30,
       PF_ASTC_RGBA_10x5 = 31,
       PF_ASTC_RGBA_10x6 = 32,
       PF_ASTC_RGBA_10x8 = 33,
       PF_ASTC_RGBA_10x10 = 34,
       PF_ASTC_RGBA_12x10 = 35,
       PF_ASTC_RGBA_12x12 = 36,
       PF_ASTC_SRGB_4x4 = 37,
       PF_ASTC_SRGB_5x4 = 38,
       PF_ASTC_SRGB_5x5 = 39,
       PF_ASTC_SRGB_6x5 = 40,
       PF_ASTC_SRGB_6x6 = 41,
       PF_ASTC_SRGB_8x5 = 42,
       PF_ASTC_SRGB_8x6 = 43,
       PF_ASTC_SRGB_8x8 = 44,
       PF_ASTC_SRGB_10x5 = 45,
       PF_ASTC_SRGB_10x6 = 46,
       PF_ASTC_SRGB_10x8 = 47,
       PF_ASTC_SRGB_10x10 = 48,
       PF_ASTC_SRGB_12x10 = 49,
       PF_ASTC_SRGB_12x12 = 50,
       PF_R8G8B8 = 51,
       PF_R8G8B8A8_SRGB = 52,
       PF_USER_FORMAT = 53,
    }

    public enum ImageOrigin
    {
       TopLeft = 0,
       BottomLeft = 1,
    }

    public enum TextureState
    {
       TS_RenderTarget = 0,
       TS_ShaderResource = 1,
       TS_TransferTarget = 2,
       TS_ResolveTarget = 3,
       TS_ResolveSource = 4,
    }

    public enum DSTextureState
    {
       DSTS_DepthStencilReadWrite = 0,
       DSTS_DepthStencilRead = 1,
    }

    public enum BackendResourceCommands
    {
       BRC_WrapUserRT = 0,
       BRC_CreateTexture = 1,
       BRC_CreateTextureWithDataPtr = 2,
       BRC_CreateDSTexture = 3,
       BRC_UpdateTexture = 4,
       BRC_WrapUserTexture = 5,
       BRC_CreateVertexBuffer = 6,
       BRC_DestroyVertexBuffer = 7,
       BRC_CreateIndexBuffer = 8,
       BRC_DestroyIndexBuffer = 9,
       BRC_DestroyTexture = 10,
       BRC_DestroyDSTexture = 11,
       BRC_CreateConstantBuffer = 12,
       BRC_DestroyConstantBuffer = 13,
       BRC_CreatePSO = 14,
       BRC_DestroyPSO = 15,
       BRC_CreateSampler = 16,
       BRC_DestroySampler = 17,
       BRC_SetDebugName = 18,
       BRC_SetDSDebugName = 19,
       BRC_SetVBODebugName = 20,
       BRC_SetIBODebugName = 21,
       BRC_SetPSODebugName = 22,
       BRC_SetCBODebugName = 23,
       BRC_SetSamplerDebugName = 24,
       BRC_TransitionTextureState = 25,
       BRC_Count = 26,
    }

    public enum VertexType
    {
       VT_Standard = 0,
       VT_Slim = 1,
       VT_Custom = 2,
       VT_Count = 3,
    }

    public enum CBType
    {
       CB_TransformVS = 0,
       CB_RenoirShaderParamsVS = 1,
       CB_GlobalDataPS = 2,
       CB_StandardPrimitivePS = 3,
       CB_StandardPrimitiveAdditionalPS = 4,
       CB_EffectsPS = 5,
       CB_RenoirShaderParamsPS = 6,
       CB_Count = 7,
    }

    public enum ShaderType
    {
       ST_Standard = 0,
       ST_StandardCircle = 1,
       ST_StandardStrokeCircle = 2,
       ST_StandardRRect = 3,
       ST_StandardStrokeRRect = 4,
       ST_StandardEllipse = 5,
       ST_StandardStrokeEllipse = 6,
       ST_StandardTexture = 7,
       ST_StandardTextureWithColorMatrix = 8,
       ST_Stencil = 9,
       ST_StencilRRect = 10,
       ST_StencilCircle = 11,
       ST_StencilTexture = 12,
       ST_StencilPath = 13,
       ST_Text = 14,
       ST_TextSDF = 15,
       ST_TextStrokeSDF = 16,
       ST_Blur_1 = 17,
       ST_Blur_2 = 18,
       ST_Blur_3 = 19,
       ST_Blur_4 = 20,
       ST_Blur_5 = 21,
       ST_Blur_6 = 22,
       ST_Blur_7 = 23,
       ST_Blur_8 = 24,
       ST_Blur_9 = 25,
       ST_Blur_10 = 26,
       ST_Blur_11 = 27,
       ST_Blur_12 = 28,
       ST_StandardBatched = 29,
       ST_StandardBatchedTexture = 30,
       ST_BatchedText = 31,
       ST_BatchedTextSDF = 32,
       ST_StandardRare = 33,
       ST_StencilRare = 34,
       ST_ClearQuad = 35,
       ST_RenoirShader = 36,
       ST_LinearGradient2Point = 37,
       ST_LinearGradient3PointSymmetrical = 38,
       ST_LinearGradientFromTexture = 39,
       ST_RadialGradient2Point = 40,
       ST_RadialGradient3PointSymmetrical = 41,
       ST_RadialGradientFromTexture = 42,
       ST_LinearGradientMasked2Point = 43,
       ST_LinearGradientMasked3PointSymmetrical = 44,
       ST_LinearGradientMaskedFromTexture = 45,
       ST_RadialGradientMasked2Point = 46,
       ST_RadialGradientMasked3PointSymmetrical = 47,
       ST_RadialGradientMaskedFromTexture = 48,
       ST_SimpleTexture = 49,
       ST_SimpleTextureMasked = 50,
       ST_Path = 51,
       ST_HairlinePath = 52,
       ST_StencilAnimatedPathBezier = 53,
       ST_StencilAnimatedPathTriangle = 54,
       ST_YUV2RGB = 55,
       ST_YUVA2RGB = 56,
       ST_Hairline = 57,
       ST_ColorMixing = 58,
       ST_GenerateSDF = 59,
       ST_GenerateSDFSolid = 60,
       ST_TextSDFGPU = 61,
       ST_TextStrokeSDFGPU = 62,
       ST_BatchedTextSDFGPU = 63,
       ST_TextMSDF = 64,
       ST_TextStrokeMSDF = 65,
       ST_BatchedTextMSDF = 66,
       ST_ClipMask = 67,
       ST_ClippingRect = 68,
       ST_ClippingCircle = 69,
       ST_ClippingTexture = 70,
       ST_ClippingPath = 71,
       ST_Count = 72,
    }

    public enum BlendingCoeff
    {
       BC_Zero = 1,
       BC_One = 2,
       BC_SrcColor = 3,
       BC_InvSrcColor = 4,
       BC_SrcAlpha = 5,
       BC_InvSrcAlpha = 6,
       BC_DestAlpha = 7,
       BC_InvDestAlpha = 8,
       BC_DestColor = 9,
       BC_InvDestColor = 10,
       BC_SrcAlphaSat = 11,
       BC_BlendFactor = 14,
       BC_InvBlendFactor = 15,
    }

    public enum BlendingOp
    {
       BLOP_Add = 1,
       BLOP_Subtract = 2,
       BLOP_ReverseSubtract = 3,
       BLOP_Min = 4,
       BLOP_Max = 5,
    }

    public enum ComparisonFunction
    {
       CMP_Never = 0,
       CMP_Less = 1,
       CMP_LessEqual = 2,
       CMP_Greater = 3,
       CMP_GreaterEqual = 4,
       CMP_Equal = 5,
       CMP_NotEqual = 6,
       CMP_Always = 7,
    }

    public enum StencilOp
    {
       STEO_Keep = 0,
       STEO_Zero = 1,
       STEO_Replace = 2,
       STEO_Invert = 3,
       STEO_Increment = 4,
       STEO_Decrement = 5,
       STEO_IncrementWrap = 6,
       STEO_DecrementWrap = 7,
    }

    public enum CullFaceType
    {
       CF_Back = 0,
       CF_Front = 1,
       CF_FrontAndBack = 2,
    }

    public enum FrontFaceType
    {
       FF_CW = 0,
       FF_CCW = 1,
    }

    public enum ColorWriteMask
    {
       CWM_None = 0,
       CWM_Red = 1,
       CWM_Green = 2,
       CWM_Blue = 4,
       CWM_Alpha = 8,
       CWM_All = 15,
    }

    public enum PipelineType
    {
       PT_Standard = 0,
       PT_Custom = 1,
    }

    public enum SamplerFilter
    {
       SAMF_Point = 0,
       SAMF_Linear = 1,
       SAMF_Trilinear = 2,
    }

    public enum SamplerAddressing
    {
       SAMA_Wrap = 0,
       SAMA_Mirror = 1,
       SAMA_Clamp = 2,
    }

    public enum BufferUsageType
    {
       BUSG_Static = 0,
       BUSG_Dynamic = 1,
       BUSG_Stream = 2,
    }

    public enum ImageProperties
    {
       IMP_None = 0,
       IMP_HasMips = 2,
       IMP_Premultiplied = 4,
       IMP_Dynamic = 8,
       IMP_XBDDS = 16,
       IMP_FilteringPoint = 32,
       IMP_InvertAlpha = 64,
       IMP_ClearOnInit = 128,
    }

    public enum ShaderFamily
    {
       SF_Unknown = 0,
       SF_VertexShader = 1,
       SF_PixelShader = 2,
    }

    public enum ResourcesCommandsStage
    {
       RCS_PreExecuteRendering = 0,
       RCS_PostExecuteRendering = 1,
       RCS_DestoryAllRenderingResources = 2,
       RCS_CreateGlyphResources = 3,
    }

    public enum TextureType
    {
       Image = 0,
       Surface = 1,
    }

    public enum ColorMixingModes
    {
       CMM_Normal = 0,
       CMM_Multiply = 1,
       CMM_Screen = 2,
       CMM_Overlay = 3,
       CMM_Darken = 4,
       CMM_Lighten = 5,
       CMM_ColorDodge = 6,
       CMM_ColorBurn = 7,
       CMM_HardLight = 8,
       CMM_SoftLight = 9,
       CMM_Difference = 10,
       CMM_Exclusion = 11,
       CMM_Hue = 12,
       CMM_Saturation = 13,
       CMM_Color = 14,
       CMM_Luminosity = 15,
       CMM_Additive = 16,
       CMM_Count = 17,
    }
}
#endif
