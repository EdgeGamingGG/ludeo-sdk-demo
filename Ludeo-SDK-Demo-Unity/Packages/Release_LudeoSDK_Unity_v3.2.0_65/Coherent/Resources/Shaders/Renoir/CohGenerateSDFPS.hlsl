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


float CalculateParabolaDistance(float2 pcoord, int iter, float2 limits){
    float sigx = pcoord.x > 0.0 ? 1.0 : -1.0;  
    float px = max(abs(pcoord.x), 1.0e-12); // Fixes a division by zero problem
    float py = pcoord.y;
    float h = 0.5 * px;
    float g = 0.5 - py;
    float xr = sqrt(0.5 * px);
    float x0 = g < -h ? sqrt(abs(g)):
               g > xr ? h / abs(g):
               xr;

    for (int i = 0; i < iter; ++i)
	{
        float rcx0 = 1.0 / x0;
        float pb = h * rcx0 * rcx0;
        float pc = -px * rcx0 + g;
        x0 = 2.0 * pc / (-pb - sqrt(abs(pb*pb - 4.0*pc)));
    }
    
    x0 = sigx * x0;
    float dx = sigx * sqrt(max(0, -0.75 * x0*x0 - g));
    float x1 = -0.5 * x0 - dx;

    x0 = clamp(x0, limits.x, limits.y);
    x1 = clamp(x1, limits.x, limits.y);
    
    float d0 = length(float2(x0, x0 * x0) - pcoord);
    float d1 = length(float2(x1, x1 * x1) - pcoord);
    
    float dist = min(d0, d1);
    return dist;
}

struct SDF_PS_OUT
{
    float4 ColorValue : SV_Target; // TODO: check whether this can be optimized with float color : SV_Target
    float DepthValue : SV_Depth;
};

#ifdef COH_DX12_RS
[RootSignature(COH_DX12_RS)]
#endif
SDF_PS_OUT GenerateSDFPS(PS_INPUT input)
{
	SDF_PS_OUT output = (SDF_PS_OUT)0;
	if (ShaderType == 20)
	{
		// Generate SDF Outline
		float2 par = input.Additional.xy;
		float2 limits = input.Additional.zw;
		float distanceScale = input.Color.x / input.Color.y;
		
		float dist = CalculateParabolaDistance(par, 3, limits);
		float pdist = min(dist * distanceScale, 1.0);
		
		if (pdist == 1.0)
		{
			discard;
		}
		
		float color = 0.5 - 0.5 * pdist;
		
		output.ColorValue = float4(color, color, color, color);
		output.DepthValue = pdist;
	}
	
	return output;
}
