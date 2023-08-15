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

using UnityEngine;
using UnityEngine.Rendering;
using renoir;

namespace cohtml
{
	public static class BackendUtilities
	{
		public static UnityEngine.Experimental.Rendering.GraphicsFormat RenoirToUnityGraphicsFormat(PixelFormat format)
		{
			switch (format)
			{
				case PixelFormat.PF_R8G8B8A8:
				{
					return UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
				}
				case PixelFormat.PF_R16G16B16A16:
				{
					return UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat;
				}
				case PixelFormat.PF_R32G32B32A32:
				{
					return UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat;
				}
				case PixelFormat.PF_R8:
				{
					return UnityEngine.Experimental.Rendering.GraphicsFormat.R8_UNorm;
				}
				case PixelFormat.PF_R16:
				{
					#if UNITY_PS4 || UNITY_PS5
					// PS4/5 have problems with blending win R16 Unorm texutres so till the problem is
					// resolve we'll use 16 bit *float* textures
					return UnityEngine.Experimental.Rendering.GraphicsFormat.R16_SFloat;
					#else
					return UnityEngine.Experimental.Rendering.GraphicsFormat.R16_UNorm;
					#endif
				}
				case PixelFormat.PF_BC1:
				{
					return UnityEngine.Experimental.Rendering.GraphicsFormat.RGBA_DXT1_UNorm;
				}
				case PixelFormat.PF_BC2:
				{
					return UnityEngine.Experimental.Rendering.GraphicsFormat.RGBA_DXT3_UNorm;
				}
				case PixelFormat.PF_BC3:
				{
					return UnityEngine.Experimental.Rendering.GraphicsFormat.RGBA_DXT5_UNorm;
				}
				case PixelFormat.PF_BC4:
				{
					return UnityEngine.Experimental.Rendering.GraphicsFormat.R_BC4_UNorm;
				}
				case PixelFormat.PF_BC5:
				{
					return UnityEngine.Experimental.Rendering.GraphicsFormat.RG_BC5_UNorm;
				}
				case PixelFormat.PF_BC6:
				{
					return UnityEngine.Experimental.Rendering.GraphicsFormat.RGB_BC6H_UFloat;
				}
				case PixelFormat.PF_BC7:
				{
					return UnityEngine.Experimental.Rendering.GraphicsFormat.RGBA_BC7_UNorm;
				}
				default:
				{
					Debug.LogAssertion($"Unsupported texture format {format}!");
					break;
				}
			}

			return UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
		}

		public static TextureFormat RenoirToUnityTextureFormat(PixelFormat format)
		{
			switch (format)
			{
				case PixelFormat.PF_R8G8B8A8:
				{
					return TextureFormat.RGBA32;
				}
				case PixelFormat.PF_R16G16B16A16:
				{
					return TextureFormat.RGBAHalf;
				}
				case PixelFormat.PF_R32G32B32A32:
				{
					return TextureFormat.RGBAFloat;
				}
				default:
				{
					Debug.LogAssertion($"Unsupported texture format {format}!");
					break;
				}
			}
			return TextureFormat.RGBA32;
		}

		public static PixelFormat UnityToRenoirPixelFormat(UnityEngine.Experimental.Rendering.GraphicsFormat format)
		{
			switch (format)
			{
				case UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm:
				{
					return PixelFormat.PF_R8G8B8A8;
				}
				case UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat:
				{
					return PixelFormat.PF_R16G16B16A16;
				}
				case UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat:
				{
					return PixelFormat.PF_R32G32B32A32;
				}
				case UnityEngine.Experimental.Rendering.GraphicsFormat.R8_UNorm:
				{
					return PixelFormat.PF_R8;
				}
				case UnityEngine.Experimental.Rendering.GraphicsFormat.R16_SFloat:
				{
					return PixelFormat.PF_R16;
				}
				case UnityEngine.Experimental.Rendering.GraphicsFormat.RGBA_DXT1_UNorm:
				{
					return PixelFormat.PF_BC1;
				}
				case UnityEngine.Experimental.Rendering.GraphicsFormat.RGBA_DXT3_UNorm:
				{
					return PixelFormat.PF_BC2;
				}
				case UnityEngine.Experimental.Rendering.GraphicsFormat.RGBA_DXT5_UNorm:
				{
					return PixelFormat.PF_BC3;
				}
				case UnityEngine.Experimental.Rendering.GraphicsFormat.R_BC4_UNorm:
				{
					return PixelFormat.PF_BC4;
				}
				case UnityEngine.Experimental.Rendering.GraphicsFormat.RG_BC5_UNorm:
				{
					return PixelFormat.PF_BC5;
				}
				case UnityEngine.Experimental.Rendering.GraphicsFormat.RGB_BC6H_UFloat:
				{
					return PixelFormat.PF_BC6;
				}
				case UnityEngine.Experimental.Rendering.GraphicsFormat.RGBA_BC7_UNorm:
				{
					return PixelFormat.PF_BC7;
				}
				default:
				{
					Debug.LogAssertion($"Unsupported texture format {format}!");
					return PixelFormat.PF_R8G8B8A8;
				}
			}
		}

		public static BlendOp Renoir2UnityBlendingOp(BlendingOp op)
		{
			switch (op)
			{
				case BlendingOp.BLOP_Add:
					return BlendOp.Add;
				case BlendingOp.BLOP_Subtract:
					return BlendOp.Subtract;
				case BlendingOp.BLOP_ReverseSubtract:
					return BlendOp.ReverseSubtract;
				case BlendingOp.BLOP_Min:
					return BlendOp.Min;
				case BlendingOp.BLOP_Max:
					return BlendOp.Max;
				default:
				{
					Debug.LogAssertion("Unsupported blending operation!");
					break;
				}
			}

			return BlendOp.Add;
		}

		public static BlendMode Renoir2UnityBlendMode(BlendingCoeff coeff)
		{
			switch (coeff)
			{
				case BlendingCoeff.BC_Zero:
					return BlendMode.Zero;
				case BlendingCoeff.BC_One:
					return BlendMode.One;
				case BlendingCoeff.BC_SrcColor:
					return BlendMode.SrcColor;
				case BlendingCoeff.BC_InvSrcColor:
					return BlendMode.OneMinusSrcColor;
				case BlendingCoeff.BC_SrcAlpha:
					return BlendMode.SrcAlpha;
				case BlendingCoeff.BC_InvSrcAlpha:
					return BlendMode.OneMinusSrcAlpha;
				case BlendingCoeff.BC_DestAlpha:
					return BlendMode.DstAlpha;
				case BlendingCoeff.BC_InvDestAlpha:
					return BlendMode.OneMinusDstAlpha;
				case BlendingCoeff.BC_DestColor:
					return BlendMode.DstColor;
				case BlendingCoeff.BC_InvDestColor:
					return BlendMode.OneMinusDstColor;
				case BlendingCoeff.BC_SrcAlphaSat:
					return BlendMode.SrcAlphaSaturate;
				case BlendingCoeff.BC_BlendFactor:
				case BlendingCoeff.BC_InvBlendFactor:
				default:
				{
					Debug.LogAssertion("Unsupported blending mode!");
					break;
				}
			}

			return BlendMode.One;
		}


		public const uint VERTEX_SHADERS_COUNT = 5;
		public const uint PIXEL_SHADERS_COUNT = 12;
		public const uint ALL_SHADER_PASSES_COUNT = VERTEX_SHADERS_COUNT * PIXEL_SHADERS_COUNT;
		public static ushort Renoir2UnityShader(ShaderType vs, ShaderType ps)
		{
			ushort vsValue = 0;
			switch (vs)
			{

				case ShaderType.ST_Path:
				case ShaderType.ST_StencilPath:
				case ShaderType.ST_ClippingPath:
					{
						vsValue = 1;
						break;
					}
				case ShaderType.ST_ClearQuad:
					{
						vsValue = 0;
						break;
					}
				case ShaderType.ST_RenoirShader:
					{
						vsValue = 2;
						break;
					}
				case ShaderType.ST_StandardBatched:
					{
						vsValue = 3;
						break;
					}
				case ShaderType.ST_Standard:
				case ShaderType.ST_StandardRare:
				case ShaderType.ST_Stencil:
				case ShaderType.ST_StencilRare:
				case ShaderType.ST_ColorMixing:
				case ShaderType.ST_GenerateSDF:
				case ShaderType.ST_ClipMask:
					{
						vsValue = 4;
						break;
					}

				default:
					{
						Debug.LogAssertion("Unsupported vertex shader!");
						break;
					}
			}

			ushort psValue = 0;
			switch (ps)
			{

				case ShaderType.ST_StandardRare:
					{
						psValue = 8;
						break;
					}
				case ShaderType.ST_ClearQuad:
					{
						psValue = 0;
						break;
					}
				case ShaderType.ST_ColorMixing:
					{
						psValue = 2;
						break;
					}
				case ShaderType.ST_StandardBatched:
					{
						psValue = 6;
						break;
					}
				case ShaderType.ST_Stencil:
					{
						psValue = 10;
						break;
					}
				case ShaderType.ST_ClippingPath:
				case ShaderType.ST_ClipMask:
					{
						psValue = 1;
						break;
					}
				case ShaderType.ST_StencilPath:
					{
						psValue = 9;
						break;
					}
				case ShaderType.ST_StencilRare:
					{
						psValue = 11;
						break;
					}
				case ShaderType.ST_GenerateSDF:
					{
						psValue = 3;
						break;
					}
				case ShaderType.ST_Standard:
					{
						psValue = 7;
						break;
					}
				case ShaderType.ST_RenoirShader:
					{
						psValue = 5;
						break;
					}
				case ShaderType.ST_Path:
					{
						psValue = 4;
						break;
					}

				default:
					{
						Debug.LogAssertion("Unsupported pixel shader!");
						break;
					}
			}

			return (ushort)(vsValue * PIXEL_SHADERS_COUNT + psValue);
		}



		public static UnityEngine.Rendering.ColorWriteMask Renoir2UnityColorWriteMask(renoir.ColorWriteMask cwm)
		{
			switch (cwm)
			{
				case renoir.ColorWriteMask.CWM_None:
				{
					return 0; // Value to turn off the rendering to all color channels
				}
				case renoir.ColorWriteMask.CWM_Red:
				{
					return UnityEngine.Rendering.ColorWriteMask.Red;
				}
				case renoir.ColorWriteMask.CWM_Green:
				{
					return UnityEngine.Rendering.ColorWriteMask.Green;
				}
				case renoir.ColorWriteMask.CWM_Blue:
				{
					return UnityEngine.Rendering.ColorWriteMask.Blue;
				}
				case renoir.ColorWriteMask.CWM_Alpha:
				{
					return UnityEngine.Rendering.ColorWriteMask.Alpha;
				}
				case renoir.ColorWriteMask.CWM_All:
				{
					return UnityEngine.Rendering.ColorWriteMask.All;
				}
				default:
				{
					Debug.LogAssertion("Unsupported color mask value!");
					break;
				}
			}

			return UnityEngine.Rendering.ColorWriteMask.All;
		}

		public static CompareFunction Renoir2UnityCompareFunction(ComparisonFunction func)
		{
			switch (func)
			{
				case ComparisonFunction.CMP_Always:
				{
					return CompareFunction.Always;
				}
				case ComparisonFunction.CMP_Equal:
				{
					return CompareFunction.Equal;
				}
				case ComparisonFunction.CMP_Greater:
				{
					return CompareFunction.Greater;
				}
				case ComparisonFunction.CMP_GreaterEqual:
				{
					return CompareFunction.GreaterEqual;
				}
				case ComparisonFunction.CMP_Less:
				{
					return CompareFunction.Less;
				}
				case ComparisonFunction.CMP_LessEqual:
				{
					return CompareFunction.LessEqual;
				}
				case ComparisonFunction.CMP_Never:
				{
					return CompareFunction.Never;
				}
				case ComparisonFunction.CMP_NotEqual:
				{
					return CompareFunction.NotEqual;
				}
				default:
				{
					Debug.LogAssertion("Unsupported stencil compare function!");
					break;
				}
			}

			return CompareFunction.Disabled;
		}

		// Depth function mapping is different somehow (This should be cehcked in another versions),
		// These values are taken directly from render doc when we were testing
		public static  int Renoir2UnityCompareDepthFunction(ComparisonFunction func)
		{
			switch (func)
			{
				case ComparisonFunction.CMP_Always:
				{
					return 0;
				}
				case ComparisonFunction.CMP_Equal:
				{
					return 3;
				}
				case ComparisonFunction.CMP_Greater:
				{
					return 2;
				}
				case ComparisonFunction.CMP_GreaterEqual:
				{
					return 4;
				}
				case ComparisonFunction.CMP_Less:
				{
					return 5;
				}
				case ComparisonFunction.CMP_LessEqual:
				{
					return 7;
				}
				case ComparisonFunction.CMP_Never:
				{
					return 1;
				}
				case ComparisonFunction.CMP_NotEqual:
				{
					return 6;
				}
				default:
				{
					Debug.LogAssertion("Unsupported depth compare function!");
					break;
				}
			}

			return 0;
		}

		public static UnityEngine.Rendering.StencilOp Renoir2UnityStencilOp(renoir.StencilOp op)
		{
			switch (op)
			{
				case renoir.StencilOp.STEO_Decrement:
				{
					return UnityEngine.Rendering.StencilOp.DecrementSaturate;
				}
				case renoir.StencilOp.STEO_DecrementWrap:
				{
					return UnityEngine.Rendering.StencilOp.DecrementWrap;
				}
				case renoir.StencilOp.STEO_Increment:
				{
					return UnityEngine.Rendering.StencilOp.IncrementSaturate;
				}
				case renoir.StencilOp.STEO_IncrementWrap:
				{
					return UnityEngine.Rendering.StencilOp.IncrementWrap;
				}
				case renoir.StencilOp.STEO_Invert:
				{
					return UnityEngine.Rendering.StencilOp.Invert;
				}
				case renoir.StencilOp.STEO_Keep:
				{
					return UnityEngine.Rendering.StencilOp.Keep;
				}
				case renoir.StencilOp.STEO_Replace:
				{
					return UnityEngine.Rendering.StencilOp.Replace;
				}
				case renoir.StencilOp.STEO_Zero:
				{
					return UnityEngine.Rendering.StencilOp.Zero;
				}
				default:
				{
					Debug.LogAssertion("Unsupported value for stencil operation!");
					break;
				}
			}

			return UnityEngine.Rendering.StencilOp.Keep;
		}


		public static FilterMode Renoir2UnityFilterMode(SamplerFilter filtering)
		{
			switch (filtering)
			{
				case SamplerFilter.SAMF_Linear:
				{
					return FilterMode.Bilinear;
				}
				case SamplerFilter.SAMF_Point:
				{
					return FilterMode.Point;
				}
				case SamplerFilter.SAMF_Trilinear:
				{
					return FilterMode.Trilinear;
				}
				default:
				{
					Debug.LogAssertion("Unsupported sampler filter mode!");
					break;
				}
			}

			return FilterMode.Bilinear;
		}

		public static TextureWrapMode Renoir2UnityWrapMode(SamplerAddressing addressing)
		{
			switch (addressing)
			{
				case SamplerAddressing.SAMA_Clamp:
				{
					return TextureWrapMode.Clamp;
				}
				case SamplerAddressing.SAMA_Mirror:
				{
					return TextureWrapMode.Mirror;
				}
				case SamplerAddressing.SAMA_Wrap:
				{
					return TextureWrapMode.Repeat;
				}
				default:
				{
					Debug.LogAssertion("Unsupported texture wrap mode!");
					break;
				}
			}

			return TextureWrapMode.Clamp;
		}
	}
}
