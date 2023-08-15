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
using renoir;

namespace cohtml
{
	class PipelineStateInfo
	{
		public PipelineStateInfo(
			PipelineStateObject pso = new PipelineStateObject(),
			uint stencilRef = 0
			)
		{
			PSO = pso;
			StencilRef = stencilRef;
		}

		public override string ToString()
		{
			return String.Format("Pipeline State Tuple(PSO: {0}, StencilRef: {1})", PSO.Id, StencilRef);
		}

		public PipelineStateObject PSO;
		public uint StencilRef;
	}

	class PSOInfoComparer : System.Collections.Generic.IEqualityComparer<PipelineStateInfo>
	{
		public bool Equals(PipelineStateInfo obj, PipelineStateInfo obj2)
		{
			return obj.PSO.Id == obj2.PSO.Id &&
					obj.StencilRef == obj2.StencilRef;
		}

		public int GetHashCode(PipelineStateInfo obj)
		{
			uint hash = 23;
			hash = hash * 31 + obj.PSO.Id;
			hash = hash * 31 + obj.StencilRef;
			return (int)(hash);
		}
	}
}
