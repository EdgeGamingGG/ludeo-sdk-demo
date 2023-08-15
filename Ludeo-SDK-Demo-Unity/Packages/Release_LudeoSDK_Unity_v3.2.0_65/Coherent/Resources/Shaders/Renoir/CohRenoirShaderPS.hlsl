/*
This file is part of Renoir, a modern graphics library.

Copyright (c) 2012-2016 Coherent Labs AD and/or its licensors. All
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

#include "CohPlatform.ihlsl"
#include "CohStandardCommon.ihlsl"
#include "CohCommonPS.ihlsl"
#include "CohClipMasking.ihlsl"

#define txMask txBuffer1
#define txGradient txBuffer2

// Keep in sync with PSTFlags enum in CommandProcessor.cpp
static const int PSTF_ColorFromTexture = 0x1;
static const int PSTF_GradientLinear = 0x2;
static const int PSTF_GradientRadial = 0x4;
static const int PSTF_Gradient2Point = 0x8;
static const int PSTF_Gradient3PointSymmetrical = 0x10;
static const int PSTF_GradientFromTexture = 0x20;
static const int PSTF_HasMask = 0x40;
static const int PSTF_GradientRepeat = 0x80;
static const int PSTF_GradientReflect = 0x100;

#if defined(COH_DISABLE_BITWISE_OPERATIONS)
	#define IS_SET(value, flag) (value % (flag * 2) >= flag)
#else
	#define IS_SET(value, flag) value & flag
#endif

//#define APPLY_DITHER
#if defined(APPLY_DITHER)
static const float NOISE_GRANULARITY = 0.5 / 255.0;
float Random(float2 coords)
{
	return frac(sin(dot(coords.xy, float2(12.9898, 78.233))) * 43758.5453);
}
#endif // APPLY_DITHER

float Mirror(float u)
{
    const float t = 2.0 * frac(u * 0.5);
    return t < 1.0 ? t : 2.0 - t;
}

#ifdef COH_DX12_RS
[RootSignature(COH_DX12_RS)]
#endif
float4 RenoirShaderPS(PS_INPUT_RENOIR_SHADER input) : SV_Target
{
	float tVal = 0.f;
	if (IS_SET(ShaderType, PSTF_GradientLinear))
	{
		tVal = input.VaryingParam0.x;
	}
	else if (IS_SET(ShaderType, PSTF_GradientRadial))
	{
		tVal = length(input.VaryingParam0.xy);
	}

	if (IS_SET(ShaderType, PSTF_GradientRepeat))
	{
		tVal = frac(tVal);
	}
	else if (IS_SET(ShaderType, PSTF_GradientReflect))
	{
		tVal = Mirror(tVal);
	}

	float4 colorTemp = float4(0, 0, 0, 0);
	if (IS_SET(ShaderType, PSTF_Gradient2Point))
	{
		colorTemp = lerp(GradientStartColor, GradientEndColor, saturate(tVal));
	}
	else if (IS_SET(ShaderType, PSTF_Gradient3PointSymmetrical))
	{
		float oneMinus2t = 1.0 - (2.0 * tVal);
		colorTemp = clamp(oneMinus2t, 0.0, 1.0) * GradientStartColor;
		colorTemp += (1.0 - min(abs(oneMinus2t), 1.0)) * GradientMidColor;
		colorTemp += clamp(-oneMinus2t, 0.0, 1.0) * GradientEndColor;
	}
	else if (IS_SET(ShaderType, PSTF_GradientFromTexture))
	{
		float2 coord = float2(tVal, GradientYCoord);
		colorTemp = SAMPLE2D(txGradient, coord);
	}
	else if (IS_SET(ShaderType, PSTF_ColorFromTexture))
	{
		colorTemp = SAMPLE2D(txBuffer, input.Additional.xy);
	}

#if defined(APPLY_DITHER)
	colorTemp += lerp(-NOISE_GRANULARITY, NOISE_GRANULARITY, Random(input.VaryingParam0.xy));
#endif // APPLY_DITHER

	if (IS_SET(ShaderType, PSTF_HasMask))
	{
		float mask = SAMPLE2D(txMask, input.VaryingParam1.xy).a;
		colorTemp *= mask;
	}

	float4 outColor = colorTemp * saturate(input.Additional.z);
	outColor = ClipWithMaskTexture(input, outColor);

	return outColor;
}

#undef IS_SET
