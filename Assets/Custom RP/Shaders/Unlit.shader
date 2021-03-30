Shader "Custom/Unlit"
{
    Properties
    {
        
    }
    SubShader
    {        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #include "../ShaderLibrary/UnlitPass.hlsl"
            ENDHLSL
        }
    }
}
