Shader "Custom/CartoonShaderURP"
{
    Properties
    {
        [Toggle] _IsLit ("Use Lighting", int) = 1
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Color", Color) = (1,1,1,1)

        [Space(10)]
        _ShadowStep ("Shadow Step Threshold", Range(0, 1)) = 0.5
        _ShadowStepSmooth ("Shadow Step Smoothness", Range(0.01, 1)) = 0.1
        _ShadowColor ("Shadow Color", Color) = (0.2, 0.2, 0.2, 1)

        [Space(10)]
        _SpecularStep ("Specular Step Threshold", Range(0.0, 1)) = 0.8
        _SpecularStepSmooth ("Specular Step Smoothness", Range(0.01, 1)) = 0.1
        [HDR]_SpecularColor ("Specular Color", Color) = (0.8, 0.8, 0.8, 1)
        _SpecularPower ("Specular Power", Range(1, 128)) = 32

        [Space(10)]
        _RimStep ("Rim Threshold", Range(0.0, 1)) = 0.7
        _RimSmooth ("Rim Smoothness", Range(0.01, 1)) = 0.1
        _RimColor ("Rim Color", Color) = (0.8, 0.8, 0.8, 1)
        _RimPower ("Rim Power", Range(1, 10)) = 3

        [Space(10)]
        _OutlineWidth ("Outline Width", Range(0.0, 1.0)) = 0.05
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)

        [Space(10)]
        [Toggle] _DoubleSided ("Double Sided", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue"="Geometry" }
        LOD 100

        // --- PASS PRINCIPAL ---
        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ _DOUBLESIDED_ON
            // #pragma multi_compile_shadows // Deshabilitado para pruebas, ya que las sombras nos están dando problemas

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl" // Lo mantenemos por si alguna dependencia lo necesita

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _ShadowStep;
                float _ShadowStepSmooth;
                float4 _ShadowColor;
                float _SpecularStep;
                float _SpecularStepSmooth;
                float4 _SpecularColor;
                float _SpecularPower;
                float _RimStep;
                float _RimSmooth;
                float4 _RimColor;
                float _RimPower;
                int _IsLit;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
                // float4 shadowCoord : TEXCOORD4; // Comentado, ya que el cálculo de sombras se deshabilitó temporalmente
                float4 fogCoord : TEXCOORD5;
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                
                // Sin cálculo de sombras en el vert shader por ahora
                // output.shadowCoord = ...;

                output.fogCoord = ComputeFogFactor(output.positionCS.z);

                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float2 uv = input.uv;
                // === CAMBIO CRÍTICO AQUÍ: Usar float4 en lugar de fixed4 para baseColor ===
                float4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * _BaseColor;
                // ========================================================================
                
                float3 N = normalize(input.normalWS);
                float3 V = normalize(input.viewDirWS);

                #ifdef _DOUBLESIDED_ON
                    if (dot(N, V) < 0) N = -N;
                #endif

                float3 finalColor = baseColor.rgb;

                if (_IsLit)
                {
                    Light mainLight = GetMainLight(); // Debería funcionar en URP 17.x sin parámetros
                    float3 L = normalize(mainLight.direction);
                    float3 H = normalize(V + L);

                    float NL = dot(N, L);
                    float NH = dot(N, H);

                    // Temporalmente sin sombras, shadowAtten = 1.0 (sin atenuación)
                    float shadowAtten = 1.0; 
                    fixed3 blendedShadowColor = lerp(_ShadowColor.rgb, fixed3(1,1,1), smoothstep(_ShadowStep - _ShadowStepSmooth, _ShadowStep + _ShadowStepSmooth, shadowAtten));
                    
                    float diffuseRaw = max(0, NL);
                    float diffuseQuantized = smoothstep(_ShadowStep - _ShadowStepSmooth, _ShadowStep + _ShadowStepSmooth, diffuseRaw);
                    finalColor *= diffuseQuantized * blendedShadowColor;

                    float specularRaw = pow(max(0, NH), _SpecularPower);
                    float3 specular = _SpecularColor.rgb * smoothstep(_SpecularStep - _SpecularStepSmooth, _SpecularStep + _SpecularStepSmooth, specularRaw);
                    finalColor += specular;

                    float rimDot = 1 - saturate(dot(V, N));
                    float3 rim = _RimColor.rgb * smoothstep(_RimStep - _RimSmooth, _RimStep + _RimSmooth, pow(rimDot, _RimPower));
                    finalColor += rim;
                }
                
                finalColor += SampleSH(N) * baseColor.rgb;
                finalColor = MixFog(finalColor, input.fogCoord);

                return float4(finalColor , baseColor.a);
            }
            ENDHLSL
        }

        // --- PASS DEL CONTORNO (Warnings corregidoss) ---
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front
            ZWrite On
            ColorMask RGB

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _OutlineWidth;
                float4 _OutlineColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 fogCoord : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                float3 offsetPositionOS = input.positionOS.xyz + input.normalOS * _OutlineWidth * 0.1;
                output.positionCS = TransformObjectToHClip(float4(offsetPositionOS.xyz, 1.0)); 
                output.fogCoord = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            float4 frag(Varyings i) : SV_Target
            {
                float3 finalColorRGB = MixFog(_OutlineColor.rgb, i.fogCoord).rgb;
                return float4(finalColorRGB, _OutlineColor.a);
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
    Fallback "Hidden/Universal Render Pipeline/FallbackError"
}