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

#ifdef COH_DX12_RS
[RootSignature(COH_DX12_RS)]
#endif
float4 PathPS(PS_INPUT_PATH input) : SV_Target
{
	float4 outColor;
	if (ShaderType == 14)
	{
		// Hairline quads
		float2 px = ddx(input.ExtraParams.xy);
		float2 py = ddy(input.ExtraParams.xy);

		float fx = (2 * input.ExtraParams.x) * px.x - px.y;
		float fy = (2 * input.ExtraParams.x) * py.x - py.y;

		float edgeAlpha = (input.ExtraParams.x * input.ExtraParams.x - input.ExtraParams.y);
		float sd = sqrt((edgeAlpha * edgeAlpha) / (fx * fx + fy * fy));

		float alpha = 1.0 - sd;

		// @HDR: if the alpha is not clamp here, there are artifacts in HDR mode
		// when 16/32 bit textures are used because thee will be values over 1.0 in
		// the areas around the path; normally those are clippend (with 8bit textures)
		// but with HDR this is not longet the case
		outColor = PrimProps0 * PrimProps1.x * saturate(alpha);
	}
	else if (ShaderType == 11)
	{
		// Hairline lines:
		//	PrimProps1.x is used for the stroke width
		//	PrimProps1.y = 1; PrimProps1.z = 0, effectively removing them from the equation
		// Also reused for drawing the AA border when using the GPU algorithm for filling.
		// In the latter case, PrimProps1.x = 1 as the width of the AA is set up in code.
		// PrimProps1.y = 2; PrimProps1.z = 1, creating a pyramid output in the [0,1] input range,
		// resulting in 0 at both ends and a peak of 1 at 0.5.

		outColor = PrimProps0 * min(1.0f, (1.0f - abs(input.ExtraParams.y * PrimProps1.y - PrimProps1.z)) * PrimProps1.x);
	}
	else
	{
		// non-hairline paths
		outColor = PrimProps0 * input.ExtraParams.y;
	}

	return ClipWithMaskTexture(input, outColor);
}
