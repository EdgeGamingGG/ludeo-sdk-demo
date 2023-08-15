
Shader "Coherent/RenoirShader"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Blend [_SrcBlend] [_DstBlend], [_SrcBlendA] [_DstBlendA]
        BlendOp [_OpColor], [_OpAlpha]
        ZTest [_ZTest]
        ZWrite [_ZWrite]
        Cull [_Cull]
        ColorMask [_ColorWriteMask]
        Stencil
        {
            Ref [_Stencil]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
            CompFront [_CompFront]
            PassFront [_PassFront]
            FailFront [_FailFront]
            ZFailFront [_ZFailFront]
            CompBack [_CompBack]
            PassBack [_PassBack]
            FailBack [_FailBack]
            ZFailBack [_ZFailBack]
        }

//===================================================================================
         // 0
        Pass
        {
            HLSLPROGRAM

            #pragma vertex ClearQuadVS
            #pragma fragment ClearQuadPS

            #define __UNITY__

            #include "CohClearQuadVS.hlsl"
            #include "CohClearQuadPS.hlsl"

            ENDHLSL
        }

        // 1
        Pass
        {
            HLSLPROGRAM

            #pragma vertex ClearQuadVS
            #pragma fragment ClipMaskPS

            #define __UNITY__

            #include "CohClearQuadVS.hlsl"
            #include "CohClipMaskPS.hlsl"

            ENDHLSL
        }

        // 2
        Pass
        {
            HLSLPROGRAM

            #pragma vertex ClearQuadVS
            #pragma fragment ColorMixingPS

            #define __UNITY__

            #include "CohClearQuadVS.hlsl"
            #include "CohColorMixingPS.hlsl"

            ENDHLSL
        }

        // 3
        Pass
        {
            HLSLPROGRAM

            #pragma vertex ClearQuadVS
            #pragma fragment GenerateSDFPS

            #define __UNITY__

            #include "CohClearQuadVS.hlsl"
            #include "CohGenerateSDFPS.hlsl"

            ENDHLSL
        }

        // 4
        Pass
        {
            HLSLPROGRAM

            #pragma vertex ClearQuadVS
            #pragma fragment PathPS

            #define __UNITY__

            #include "CohClearQuadVS.hlsl"
            #include "CohPathPS.hlsl"

            ENDHLSL
        }

        // 5
        Pass
        {
            HLSLPROGRAM

            #if defined(SHADER_API_GLES)
            #define COH_DISABLE_BITWISE_OPERATIONS
            #pragma target 2.0
            #endif

            #pragma vertex ClearQuadVS
            #pragma fragment RenoirShaderPS

            #define __UNITY__

            #include "CohClearQuadVS.hlsl"
            #include "CohRenoirShaderPS.hlsl"

            ENDHLSL
        }

        // 6
        Pass
        {
            HLSLPROGRAM

            #pragma vertex ClearQuadVS
            #pragma fragment StandardBatchedPS

            #define __UNITY__

            #include "CohClearQuadVS.hlsl"
            #include "CohStandardBatchedPS.hlsl"

            ENDHLSL
        }

        // 7
        Pass
        {
            HLSLPROGRAM

            #pragma vertex ClearQuadVS
            #pragma fragment StandardPS

            #define __UNITY__

            #include "CohClearQuadVS.hlsl"
            #include "CohStandardPS.hlsl"

            ENDHLSL
        }

        // 8
        Pass
        {
            HLSLPROGRAM

            #pragma vertex ClearQuadVS
            #pragma fragment StandardRarePS

            #define __UNITY__

            #include "CohClearQuadVS.hlsl"
            #include "CohStandardRarePS.hlsl"

            ENDHLSL
        }

        // 9
        Pass
        {
            HLSLPROGRAM

            #if defined(SHADER_API_SWITCH)
            #pragma enable_d3d11_debug_symbols
            #endif

            #pragma vertex ClearQuadVS
            #pragma fragment StencilPathPS

            #define __UNITY__

            #include "CohClearQuadVS.hlsl"
            #include "CohStencilPathPS.hlsl"

            ENDHLSL
        }

        // 10
        Pass
        {
            HLSLPROGRAM

            #pragma vertex ClearQuadVS
            #pragma fragment StencilPS

            #define __UNITY__

            #include "CohClearQuadVS.hlsl"
            #include "CohStencilPS.hlsl"

            ENDHLSL
        }

        // 11
        Pass
        {
            HLSLPROGRAM

            #pragma vertex ClearQuadVS
            #pragma fragment StencilRarePS

            #define __UNITY__

            #include "CohClearQuadVS.hlsl"
            #include "CohStencilRarePS.hlsl"

            ENDHLSL
        }

        // 12
        Pass
        {
            HLSLPROGRAM

            #pragma vertex PathVS
            #pragma fragment ClearQuadPS

            #define __UNITY__

            #include "CohPathVS.hlsl"
            #include "CohClearQuadPS.hlsl"

            ENDHLSL
        }

        // 13
        Pass
        {
            HLSLPROGRAM

            #pragma vertex PathVS
            #pragma fragment ClipMaskPS

            #define __UNITY__

            #include "CohPathVS.hlsl"
            #include "CohClipMaskPS.hlsl"

            ENDHLSL
        }

        // 14
        Pass
        {
            HLSLPROGRAM

            #pragma vertex PathVS
            #pragma fragment ColorMixingPS

            #define __UNITY__

            #include "CohPathVS.hlsl"
            #include "CohColorMixingPS.hlsl"

            ENDHLSL
        }

        // 15
        Pass
        {
            HLSLPROGRAM

            #pragma vertex PathVS
            #pragma fragment GenerateSDFPS

            #define __UNITY__

            #include "CohPathVS.hlsl"
            #include "CohGenerateSDFPS.hlsl"

            ENDHLSL
        }

        // 16
        Pass
        {
            HLSLPROGRAM

            #pragma vertex PathVS
            #pragma fragment PathPS

            #define __UNITY__

            #include "CohPathVS.hlsl"
            #include "CohPathPS.hlsl"

            ENDHLSL
        }

        // 17
        Pass
        {
            HLSLPROGRAM

            #if defined(SHADER_API_GLES)
            #define COH_DISABLE_BITWISE_OPERATIONS
            #pragma target 2.0
            #endif

            #pragma vertex PathVS
            #pragma fragment RenoirShaderPS

            #define __UNITY__

            #include "CohPathVS.hlsl"
            #include "CohRenoirShaderPS.hlsl"

            ENDHLSL
        }

        // 18
        Pass
        {
            HLSLPROGRAM

            #pragma vertex PathVS
            #pragma fragment StandardBatchedPS

            #define __UNITY__

            #include "CohPathVS.hlsl"
            #include "CohStandardBatchedPS.hlsl"

            ENDHLSL
        }

        // 19
        Pass
        {
            HLSLPROGRAM

            #pragma vertex PathVS
            #pragma fragment StandardPS

            #define __UNITY__

            #include "CohPathVS.hlsl"
            #include "CohStandardPS.hlsl"

            ENDHLSL
        }

        // 20
        Pass
        {
            HLSLPROGRAM

            #pragma vertex PathVS
            #pragma fragment StandardRarePS

            #define __UNITY__

            #include "CohPathVS.hlsl"
            #include "CohStandardRarePS.hlsl"

            ENDHLSL
        }

        // 21
        Pass
        {
            HLSLPROGRAM

            #if defined(SHADER_API_SWITCH)
            #pragma enable_d3d11_debug_symbols
            #endif

            #pragma vertex PathVS
            #pragma fragment StencilPathPS

            #define __UNITY__

            #include "CohPathVS.hlsl"
            #include "CohStencilPathPS.hlsl"

            ENDHLSL
        }

        // 22
        Pass
        {
            HLSLPROGRAM

            #pragma vertex PathVS
            #pragma fragment StencilPS

            #define __UNITY__

            #include "CohPathVS.hlsl"
            #include "CohStencilPS.hlsl"

            ENDHLSL
        }

        // 23
        Pass
        {
            HLSLPROGRAM

            #pragma vertex PathVS
            #pragma fragment StencilRarePS

            #define __UNITY__

            #include "CohPathVS.hlsl"
            #include "CohStencilRarePS.hlsl"

            ENDHLSL
        }

        // 24
        Pass
        {
            HLSLPROGRAM

            #pragma vertex RenoirShaderVS
            #pragma fragment ClearQuadPS

            #define __UNITY__

            #include "CohRenoirShaderVS.hlsl"
            #include "CohClearQuadPS.hlsl"

            ENDHLSL
        }

        // 25
        Pass
        {
            HLSLPROGRAM

            #pragma vertex RenoirShaderVS
            #pragma fragment ClipMaskPS

            #define __UNITY__

            #include "CohRenoirShaderVS.hlsl"
            #include "CohClipMaskPS.hlsl"

            ENDHLSL
        }

        // 26
        Pass
        {
            HLSLPROGRAM

            #pragma vertex RenoirShaderVS
            #pragma fragment ColorMixingPS

            #define __UNITY__

            #include "CohRenoirShaderVS.hlsl"
            #include "CohColorMixingPS.hlsl"

            ENDHLSL
        }

        // 27
        Pass
        {
            HLSLPROGRAM

            #pragma vertex RenoirShaderVS
            #pragma fragment GenerateSDFPS

            #define __UNITY__

            #include "CohRenoirShaderVS.hlsl"
            #include "CohGenerateSDFPS.hlsl"

            ENDHLSL
        }

        // 28
        Pass
        {
            HLSLPROGRAM

            #pragma vertex RenoirShaderVS
            #pragma fragment PathPS

            #define __UNITY__

            #include "CohRenoirShaderVS.hlsl"
            #include "CohPathPS.hlsl"

            ENDHLSL
        }

        // 29
        Pass
        {
            HLSLPROGRAM

            #if defined(SHADER_API_GLES)
            #define COH_DISABLE_BITWISE_OPERATIONS
            #pragma target 2.0
            #endif

            #pragma vertex RenoirShaderVS
            #pragma fragment RenoirShaderPS

            #define __UNITY__

            #include "CohRenoirShaderVS.hlsl"
            #include "CohRenoirShaderPS.hlsl"

            ENDHLSL
        }

        // 30
        Pass
        {
            HLSLPROGRAM

            #pragma vertex RenoirShaderVS
            #pragma fragment StandardBatchedPS

            #define __UNITY__

            #include "CohRenoirShaderVS.hlsl"
            #include "CohStandardBatchedPS.hlsl"

            ENDHLSL
        }

        // 31
        Pass
        {
            HLSLPROGRAM

            #pragma vertex RenoirShaderVS
            #pragma fragment StandardPS

            #define __UNITY__

            #include "CohRenoirShaderVS.hlsl"
            #include "CohStandardPS.hlsl"

            ENDHLSL
        }

        // 32
        Pass
        {
            HLSLPROGRAM

            #pragma vertex RenoirShaderVS
            #pragma fragment StandardRarePS

            #define __UNITY__

            #include "CohRenoirShaderVS.hlsl"
            #include "CohStandardRarePS.hlsl"

            ENDHLSL
        }

        // 33
        Pass
        {
            HLSLPROGRAM

            #if defined(SHADER_API_SWITCH)
            #pragma enable_d3d11_debug_symbols
            #endif

            #pragma vertex RenoirShaderVS
            #pragma fragment StencilPathPS

            #define __UNITY__

            #include "CohRenoirShaderVS.hlsl"
            #include "CohStencilPathPS.hlsl"

            ENDHLSL
        }

        // 34
        Pass
        {
            HLSLPROGRAM

            #pragma vertex RenoirShaderVS
            #pragma fragment StencilPS

            #define __UNITY__

            #include "CohRenoirShaderVS.hlsl"
            #include "CohStencilPS.hlsl"

            ENDHLSL
        }

        // 35
        Pass
        {
            HLSLPROGRAM

            #pragma vertex RenoirShaderVS
            #pragma fragment StencilRarePS

            #define __UNITY__

            #include "CohRenoirShaderVS.hlsl"
            #include "CohStencilRarePS.hlsl"

            ENDHLSL
        }

        // 36
        Pass
        {
            HLSLPROGRAM

            #pragma vertex StandardBatchedVS
            #pragma fragment ClearQuadPS

            #define __UNITY__

            #include "CohStandardBatchedVS.hlsl"
            #include "CohClearQuadPS.hlsl"

            ENDHLSL
        }

        // 37
        Pass
        {
            HLSLPROGRAM

            #pragma vertex StandardBatchedVS
            #pragma fragment ClipMaskPS

            #define __UNITY__

            #include "CohStandardBatchedVS.hlsl"
            #include "CohClipMaskPS.hlsl"

            ENDHLSL
        }

        // 38
        Pass
        {
            HLSLPROGRAM

            #pragma vertex StandardBatchedVS
            #pragma fragment ColorMixingPS

            #define __UNITY__

            #include "CohStandardBatchedVS.hlsl"
            #include "CohColorMixingPS.hlsl"

            ENDHLSL
        }

        // 39
        Pass
        {
            HLSLPROGRAM

            #pragma vertex StandardBatchedVS
            #pragma fragment GenerateSDFPS

            #define __UNITY__

            #include "CohStandardBatchedVS.hlsl"
            #include "CohGenerateSDFPS.hlsl"

            ENDHLSL
        }

        // 40
        Pass
        {
            HLSLPROGRAM

            #pragma vertex StandardBatchedVS
            #pragma fragment PathPS

            #define __UNITY__

            #include "CohStandardBatchedVS.hlsl"
            #include "CohPathPS.hlsl"

            ENDHLSL
        }

        // 41
        Pass
        {
            HLSLPROGRAM

            #if defined(SHADER_API_GLES)
            #define COH_DISABLE_BITWISE_OPERATIONS
            #pragma target 2.0
            #endif

            #pragma vertex StandardBatchedVS
            #pragma fragment RenoirShaderPS

            #define __UNITY__

            #include "CohStandardBatchedVS.hlsl"
            #include "CohRenoirShaderPS.hlsl"

            ENDHLSL
        }

        // 42
        Pass
        {
            HLSLPROGRAM

            #pragma vertex StandardBatchedVS
            #pragma fragment StandardBatchedPS

            #define __UNITY__

            #include "CohStandardBatchedVS.hlsl"
            #include "CohStandardBatchedPS.hlsl"

            ENDHLSL
        }

        // 43
        Pass
        {
            HLSLPROGRAM

            #pragma vertex StandardBatchedVS
            #pragma fragment StandardPS

            #define __UNITY__

            #include "CohStandardBatchedVS.hlsl"
            #include "CohStandardPS.hlsl"

            ENDHLSL
        }

        // 44
        Pass
        {
            HLSLPROGRAM

            #pragma vertex StandardBatchedVS
            #pragma fragment StandardRarePS

            #define __UNITY__

            #include "CohStandardBatchedVS.hlsl"
            #include "CohStandardRarePS.hlsl"

            ENDHLSL
        }

        // 45
        Pass
        {
            HLSLPROGRAM

            #if defined(SHADER_API_SWITCH)
            #pragma enable_d3d11_debug_symbols
            #endif

            #pragma vertex StandardBatchedVS
            #pragma fragment StencilPathPS

            #define __UNITY__

            #include "CohStandardBatchedVS.hlsl"
            #include "CohStencilPathPS.hlsl"

            ENDHLSL
        }

        // 46
        Pass
        {
            HLSLPROGRAM

            #pragma vertex StandardBatchedVS
            #pragma fragment StencilPS

            #define __UNITY__

            #include "CohStandardBatchedVS.hlsl"
            #include "CohStencilPS.hlsl"

            ENDHLSL
        }

        // 47
        Pass
        {
            HLSLPROGRAM

            #pragma vertex StandardBatchedVS
            #pragma fragment StencilRarePS

            #define __UNITY__

            #include "CohStandardBatchedVS.hlsl"
            #include "CohStencilRarePS.hlsl"

            ENDHLSL
        }

        // 48
        Pass
        {
            HLSLPROGRAM

            #pragma vertex StandardVS
            #pragma fragment ClearQuadPS

            #define __UNITY__

            #include "CohStandardVS.hlsl"
            #include "CohClearQuadPS.hlsl"

            ENDHLSL
        }

        // 49
        Pass
        {
            HLSLPROGRAM

            #pragma vertex StandardVS
            #pragma fragment ClipMaskPS

            #define __UNITY__

            #include "CohStandardVS.hlsl"
            #include "CohClipMaskPS.hlsl"

            ENDHLSL
        }

        // 50
        Pass
        {
            HLSLPROGRAM

            #pragma vertex StandardVS
            #pragma fragment ColorMixingPS

            #define __UNITY__

            #include "CohStandardVS.hlsl"
            #include "CohColorMixingPS.hlsl"

            ENDHLSL
        }

        // 51
        Pass
        {
            HLSLPROGRAM

            #pragma vertex StandardVS
            #pragma fragment GenerateSDFPS

            #define __UNITY__

            #include "CohStandardVS.hlsl"
            #include "CohGenerateSDFPS.hlsl"

            ENDHLSL
        }

        // 52
        Pass
        {
            HLSLPROGRAM

            #pragma vertex StandardVS
            #pragma fragment PathPS

            #define __UNITY__

            #include "CohStandardVS.hlsl"
            #include "CohPathPS.hlsl"

            ENDHLSL
        }

        // 53
        Pass
        {
            HLSLPROGRAM

            #if defined(SHADER_API_GLES)
            #define COH_DISABLE_BITWISE_OPERATIONS
            #pragma target 2.0
            #endif

            #pragma vertex StandardVS
            #pragma fragment RenoirShaderPS

            #define __UNITY__

            #include "CohStandardVS.hlsl"
            #include "CohRenoirShaderPS.hlsl"

            ENDHLSL
        }

        // 54
        Pass
        {
            HLSLPROGRAM

            #pragma vertex StandardVS
            #pragma fragment StandardBatchedPS

            #define __UNITY__

            #include "CohStandardVS.hlsl"
            #include "CohStandardBatchedPS.hlsl"

            ENDHLSL
        }

        // 55
        Pass
        {
            HLSLPROGRAM

            #pragma vertex StandardVS
            #pragma fragment StandardPS

            #define __UNITY__

            #include "CohStandardVS.hlsl"
            #include "CohStandardPS.hlsl"

            ENDHLSL
        }

        // 56
        Pass
        {
            HLSLPROGRAM

            #pragma vertex StandardVS
            #pragma fragment StandardRarePS

            #define __UNITY__

            #include "CohStandardVS.hlsl"
            #include "CohStandardRarePS.hlsl"

            ENDHLSL
        }

        // 57
        Pass
        {
            HLSLPROGRAM

            #if defined(SHADER_API_SWITCH)
            #pragma enable_d3d11_debug_symbols
            #endif

            #pragma vertex StandardVS
            #pragma fragment StencilPathPS

            #define __UNITY__

            #include "CohStandardVS.hlsl"
            #include "CohStencilPathPS.hlsl"

            ENDHLSL
        }

        // 58
        Pass
        {
            HLSLPROGRAM

            #pragma vertex StandardVS
            #pragma fragment StencilPS

            #define __UNITY__

            #include "CohStandardVS.hlsl"
            #include "CohStencilPS.hlsl"

            ENDHLSL
        }

        // 59
        Pass
        {
            HLSLPROGRAM

            #pragma vertex StandardVS
            #pragma fragment StencilRarePS

            #define __UNITY__

            #include "CohStandardVS.hlsl"
            #include "CohStencilRarePS.hlsl"

            ENDHLSL
        }


//===================================================================================

    }
}


