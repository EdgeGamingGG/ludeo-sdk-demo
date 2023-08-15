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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using renoir;

namespace cohtml
{
	class CBOComparer : IEqualityComparer<ConstantBufferObject>
	{
		public bool Equals(ConstantBufferObject obj, ConstantBufferObject obj2)
		{
			return obj.Id == obj2.Id;
		}

		public int GetHashCode(ConstantBufferObject obj)
		{
			return (int)obj.Id;
		}
	}

	class ConstantBuffers
	{
		public ConstantBuffers()
		{
			m_CBs = new Dictionary<ConstantBufferObject, CBType>(new CBOComparer());
			m_PerFrameCBMap = new Dictionary<ConstantBufferObject, IntPtr>(new CBOComparer());
		}

		public bool CreateCB(CBType type, ConstantBufferObject obj, uint size)
		{
			m_CBs.Add(obj, type);
			return true;
		}

		public void DestroyCB(ConstantBufferObject obj)
		{
			m_CBs.Remove(obj);
		}

		public void BuildCBMappingToData(IntPtr CBOUpdates, uint CBOUpdatesCount)
		{
			m_PerFrameCBMap.Clear();
			unsafe
			{
				ConstantBufferUpdateData* updates = (ConstantBufferUpdateData*)CBOUpdates.ToPointer();
				for (int i = 0; i < CBOUpdatesCount; i++)
				{
					ConstantBufferUpdateData update = updates[i];
					if (!m_CBs.ContainsKey(update.Object))
					{
						Debug.LogAssertion("Coulnd't find constant buffer!");
						continue;
					}

					m_PerFrameCBMap.Add(update.Object, update.Data);
				}
			}
		}

		public void UpdateConstantBuffer(UpdateConstantBufferCmd cmd, ref MaterialPropertyBlock propertyBlock)
		{
			CBType type;
			m_CBs.TryGetValue(cmd.Object, out type);

			IntPtr data;
			m_PerFrameCBMap.TryGetValue(cmd.Object, out data);
			data = new IntPtr(data.ToInt64() + (int)cmd.Offset);
			unsafe
			{
				switch (type)
				{
					case CBType.CB_TransformVS:
					{
						CBTransforms* cB = (CBTransforms*)data.ToPointer();
						Matrix4x4 matrix = new Matrix4x4();

						matrix.m00 = cB->Transform._11;
						matrix.m01 = cB->Transform._12;
						matrix.m02 = cB->Transform._13;
						matrix.m03 = cB->Transform._14;
						matrix.m10 = cB->Transform._21;
						matrix.m11 = cB->Transform._22;
						matrix.m12 = cB->Transform._23;
						matrix.m13 = cB->Transform._24;
						matrix.m20 = cB->Transform._31;
						matrix.m21 = cB->Transform._32;
						matrix.m22 = cB->Transform._33;
						matrix.m23 = cB->Transform._34;
						matrix.m30 = cB->Transform._41;
						matrix.m31 = cB->Transform._42;
						matrix.m32 = cB->Transform._43;
						matrix.m33 = cB->Transform._44;

						propertyBlock.SetMatrix("Transform", matrix.transpose);

						break;
					}
					case CBType.CB_GlobalDataPS:
					{
						GlobalPixelCB* cb = (GlobalPixelCB*)data.ToPointer();
						Vector2 v1 = new Vector2(cb->ViewportSize.x, cb->ViewportSize.y);
						propertyBlock.SetVector("ViewportSize", v1);
						Vector2 v2 = new Vector2(cb->ClipMaskViewportSize.x, cb->ClipMaskViewportSize.y);
						propertyBlock.SetVector("ClipMaskViewportSize", v2);

						break;
					}
					case CBType.CB_StandardPrimitivePS:
					{
						StandardPrimitivePixel* cb = (StandardPrimitivePixel*)data.ToPointer();
						propertyBlock.SetInt("ShaderType", cb->ShaderType);
						propertyBlock.SetInt("ShouldClip", cb->ShouldClip);
						Vector2 v1 = new Vector2(cb->ClipMaskOrigin.x, cb->ClipMaskOrigin.y);
						propertyBlock.SetVector("ClipMaskOrigin", v1);
						break;
					}
					case CBType.CB_StandardPrimitiveAdditionalPS:
					{
						StandardPrimitiveAdditionalPixel* cb = (StandardPrimitiveAdditionalPixel*)data.ToPointer();
						Vector4 v1 = new Vector4(cb->PrimProps0.x, cb->PrimProps0.y, cb->PrimProps0.z, cb->PrimProps0.w);
						Vector4 v2 = new Vector4(cb->PrimProps1.x, cb->PrimProps1.y, cb->PrimProps1.z, cb->PrimProps1.w);
						propertyBlock.SetVector("PrimProps0", v1);
						propertyBlock.SetVector("PrimProps1", v2);
						break;
					}
					case CBType.CB_RenoirShaderParamsVS:
					{
						RenoirShaderVS* cb = (RenoirShaderVS*)data.ToPointer();
						Matrix4x4 matrix = new Matrix4x4();

						matrix.m00 = cb->Matrix0._11;
						matrix.m01 = cb->Matrix0._12;
						matrix.m02 = cb->Matrix0._13;
						matrix.m03 = cb->Matrix0._14;
						matrix.m10 = cb->Matrix0._21;
						matrix.m11 = cb->Matrix0._22;
						matrix.m12 = cb->Matrix0._23;
						matrix.m13 = cb->Matrix0._24;
						matrix.m20 = cb->Matrix0._31;
						matrix.m21 = cb->Matrix0._32;
						matrix.m22 = cb->Matrix0._33;
						matrix.m23 = cb->Matrix0._34;
						matrix.m30 = cb->Matrix0._41;
						matrix.m31 = cb->Matrix0._42;
						matrix.m32 = cb->Matrix0._43;
						matrix.m33 = cb->Matrix0._44;

						propertyBlock.SetMatrix("CoordTransformVS", matrix.transpose);
						Vector4 v = new Vector4(cb->Prop0.x, cb->Prop0.y, cb->Prop0.z, cb->Prop0.w);
						propertyBlock.SetVector("MaskScaleAndOffset", v);
						break;
					}
					case CBType.CB_RenoirShaderParamsPS:
					{
						RenoirShaderPS* cb = (RenoirShaderPS*)data.ToPointer();
						Vector4 v0 = new Vector4(cb->Prop0.x, cb->Prop0.y, cb->Prop0.z, cb->Prop0.w);
						Vector4 v1 = new Vector4(cb->Prop1.x, cb->Prop1.y, cb->Prop1.z, cb->Prop1.w);
						Vector4 v2 = new Vector4(cb->Prop2.x, cb->Prop2.y, cb->Prop2.z, cb->Prop2.w);

						propertyBlock.SetVector("GradientStartColor", v0);
						propertyBlock.SetVector("GradientMidColor", v1);
						propertyBlock.SetVector("GradientEndColor", v2);
						propertyBlock.SetFloat("GradientYCoord", cb->Prop3.x);
						break;
					}
					case CBType.CB_EffectsPS:
					{
						//EffectsPixelCB* cb = (EffectsPixelCB*)data.ToPointer(); We can't use it, because the struct Layout is not Explicit.
						// It must be Sequential otherwise we can't pass fixed arrays in the struct.
						EffectsPixelCB cb = (EffectsPixelCB)Marshal.PtrToStructure(data, typeof(EffectsPixelCB));

						Vector4[] coeff = new Vector4[3];
						coeff[0] = new Vector4(cb.Coefficients[0], cb.Coefficients[1], cb.Coefficients[2], cb.Coefficients[3]);
						coeff[1] = new Vector4(cb.Coefficients[4], cb.Coefficients[5], cb.Coefficients[6], cb.Coefficients[7]);
						coeff[2] = new Vector4(cb.Coefficients[8], cb.Coefficients[9], cb.Coefficients[10], cb.Coefficients[11]);
						propertyBlock.SetVectorArray("Coefficients", coeff);

						Vector4[] offfests = new Vector4[6];
						offfests[0] = new Vector4(cb.PixelOffsets[0].x, cb.PixelOffsets[0].y, cb.PixelOffsets[1].x, cb.PixelOffsets[1].y);
						offfests[1] = new Vector4(cb.PixelOffsets[2].x, cb.PixelOffsets[2].y, cb.PixelOffsets[3].x, cb.PixelOffsets[3].y);
						offfests[2] = new Vector4(cb.PixelOffsets[4].x, cb.PixelOffsets[4].y, cb.PixelOffsets[5].x, cb.PixelOffsets[5].y);
						offfests[3] = new Vector4(cb.PixelOffsets[6].x, cb.PixelOffsets[6].y, cb.PixelOffsets[7].x, cb.PixelOffsets[7].y);
						offfests[4] = new Vector4(cb.PixelOffsets[8].x, cb.PixelOffsets[8].y, cb.PixelOffsets[9].x, cb.PixelOffsets[9].y);
						offfests[5] = new Vector4(cb.PixelOffsets[10].x, cb.PixelOffsets[10].y, cb.PixelOffsets[11].x, cb.PixelOffsets[11].y);
						propertyBlock.SetVectorArray("PixelOffsets", offfests);
						break;
					}
					default:
					{
						Debug.Assert(false);
						Debug.LogAssertion("Couldn't find constatnt buffer object!");
						return;
					}
				}
			}
		}

		Dictionary<ConstantBufferObject, CBType> m_CBs;
		Dictionary<ConstantBufferObject, IntPtr> m_PerFrameCBMap;
	}

}
