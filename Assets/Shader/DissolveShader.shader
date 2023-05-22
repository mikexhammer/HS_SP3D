Shader "Custom/DissolveShader"
{
    Properties
    {
       _Color ("Color", Color) = (1, 1, 1, 1)
    _MainTex ("Albedo (RGB)", 2D) = "white" {}

    _NoiseTex ("Noise Texture", 2D) = "white" {}
    _Vanishing ("Vanishing", Range(0,1)) = 0.5
    _LightBandColor ("Light Band Color", Color) = (1, 1, 1, 1)
    _LightBandThreshold ("Light Band Threshold", Range(0,1)) = 0.1
        
    _Smoothness ("Smoothness", Range(0,1)) = 0.5
    _Metallic ("Metallic", Range(0,1)) = 0.0
    }
   SubShader {
        Tags { "RenderType"="Transparent" "IgnoreProjector"="True" "Queue"="Transparent" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        #pragma alpha:fade

        struct Input {
            float2 uv_MainTex;
        };

        sampler2D _MainTex;
        float _Smoothness;
        float _Metallic;
        sampler2D _NoiseTex;
        float _Vanishing;
        float4 _LightBandColor;
        float _LightBandThreshold;
        float4 _Color; // _Color-Property

        void surf(Input IN, inout SurfaceOutputStandard o) {
            float4 albedo = tex2D(_MainTex, IN.uv_MainTex) * _Color; // Verwenden von _Color
            o.Albedo = albedo.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Alpha = 1.0;
 
            // Calculate dissolve factor based on noise texture
            float dissolveFactor = tex2D(_NoiseTex, IN.uv_MainTex).r;
 
            // Apply dissolve effect
            if (dissolveFactor <= _Vanishing) {
                float dissolveBlend = saturate((_Vanishing - dissolveFactor) / _Vanishing);
                o.Alpha *= dissolveBlend;
            }
 
            // Apply light band effect
            float lightBandBlend = smoothstep(_LightBandThreshold, _LightBandThreshold + 0.1, dissolveFactor);
            o.Emission = lerp(o.Emission, _LightBandColor.rgb, lightBandBlend);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
