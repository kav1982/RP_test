﻿Shader "Custom RP/Unlit"
{
    Properties
    {
        _BaseMap("Texture", 2D) = "white"{ }
        [HDR]_BaseColor("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        [Toggle(_CLIPPING)] _Clipping("Alpha Clipping", Float) = 0
        //[HDR] _BaseColor("Color", Color) = (1.0, 1.0, 1.0,1.0)

        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("Dst Blend", Float) = 0
        [Enum(Off, 0, On, 1)] _ZWrite("Z Write", Float) = 1

    }
    SubShader
    {
        HLSLINCLUDE
        #include "../ShaderLibrary/Common.hlsl"
        #include "../ShaderLibrary/UnlitInput.hlsl"
        ENDHLSL

        Blend[_SrcBlend][_DstBlend]
        ZWrite[_ZWrite]

         Pass
         {
                Tags 
                {
                    "LightMode" = "CustomLit"
                }

                HLSLPROGRAM
                #pragma target 3.5
                #pragma shader_feature _CLIPPING                
                #pragma shader_feature _PREMULTIPLY_ALPHA                
                #pragma multi_compile_instancing
                #pragma vertex UnlitPassVertex
                #pragma fragment UnlitPassFragment
                #include "../ShaderLibrary/UnlitPass.hlsl"
                ENDHLSL
         }

         Pass
         {
                Tags {"LightMode" = "Meta"}

                Cull Off

                HLSLPROGRAM
                #pragma target 3.5           			
                #pragma vertex MetaPassVertex
                #pragma fragment MetaPassFragment
                #include "../ShaderLibrary/MetaPass.hlsl"
                ENDHLSL
         }
          
    }
    CustomEditor "CustomShaderGUI"
}
