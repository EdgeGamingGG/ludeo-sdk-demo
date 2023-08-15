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

#ifdef COH_DX12_RS
[RootSignature(COH_DX12_RS)]
#endif
STENCIL_OUTPUT_TYPE StencilPathPS(PS_INPUT_PATH input) STENCIL_OUTPUT_SEMANTIC
{
	if (ShaderType == 15)
	{
		// Loop Blinn beziers
		float2 uvs = abs(input.ExtraParams.xy);
		float2 px = ddx(uvs);
		float2 py = ddy(uvs);

		float fx = (2 * uvs.x) * px.x - px.y;
		float fy = (2 * uvs.x) * py.x - py.y;

		float edgeAlpha = (uvs.x * uvs.x - uvs.y);
		float sd = edgeAlpha / sqrt(fx * fx + fy * fy);

		float alpha = saturate(0.0 - sd);

		if (alpha < 0.00390625f)
			discard;
	}
	else if (ShaderType == 16)
	{
		// Normal triangles for Loop Blinn
		// No discards here
	}
	else if (input.ExtraParams.y < 0.00390625f)
	{
			discard;
	}

	STENCIL_OUTPUT_RETURN(PrimProps0);
}
