Shader "Custom/DZMHToonShader"
{
    Properties
    {
        [Toggle] _IsLit ("Use Lighting", int) = 1
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)

        [Space(10)]
        _ShadowStepThreshold ("Shadow Step Threshold", Range(0, 1)) = 0.5
        _ShadowStepSmooth ("Shadow Step Smoothness", Range(0.01, 1)) = 0.1
        _ShadowColor ("Shadow Color", Color) = (0.2, 0.2, 0.2, 1)

        [Space(10)]
        _SpecularStepThreshold ("Specular Step Threshold", Range(0, 1)) = 0.8
        _SpecularStepSmooth ("Specular Step Smoothness", Range(0.01, 1)) = 0.1
        _SpecularColor ("Specular Color", Color) = (0.8, 0.8, 0.8, 1)
        _SpecularPower ("Specular Power", Range(1, 128)) = 32

        [Space(10)]
        _RimThreshold ("Rim Threshold", Range(0, 1)) = 0.7
        _RimSmooth ("Rim Smoothness", Range(0.01, 1)) = 0.1
        _RimColor ("Rim Color", Color) = (0.8, 0.8, 0.8, 1)
        _RimPower ("Rim Power", Range(1, 10)) = 3

        [Space(10)]
        _OutlineWidth ("Outline Width", Range(0.001, 0.1)) = 0.01
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)

        [Space(10)]
        [Toggle] _DoubleSided ("Double Sided", int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry"} // "Queue"="Geometry" para orden de renderizado
        LOD 100

        // Pass principal para el renderizado del modelo
        Pass
        {
            // Control de culling (doble cara)
            Cull Off // Empezamos con Off para permitir el renderizado en ambos lados por defecto
            // Si _DoubleSided es 0 (falso), entonces se comporta como "Back". Si es 1 (verdadero), se comporta como "Off".
            // Para controlar esto con un toggle, es mejor un script o modificar directamente con ifdef
            // Por simplicidad, en el pass principal usaremos un Cull condicional basado en una propiedad.
            // La opción del shader original era Cull [_CullMode], que es más flexible si se controla con un script.
            // Para el toggle _DoubleSided en el inspector, haremos el ajuste en el C# en un script.

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma multi_compile_local _ _DOUBLESIDED_ON // Para el toggle de doble cara

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
                SHADOW_COORDS(4)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            int _IsLit;

            float _ShadowStepThreshold;
            float _ShadowStepSmooth;
            fixed4 _ShadowColor;

            float _SpecularStepThreshold;
            float _SpecularStepSmooth;
            fixed4 _SpecularColor;
            float _SpecularPower;

            float _RimThreshold;
            float _RimSmooth;
            fixed4 _RimColor;
            float _RimPower;

            // Para el toggle de doble cara, lo manejamos en el script
            int _DoubleSided;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(UnityWorldSpaceViewDir(o.worldPos));
                TRANSFER_SHADOW(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 albedo = tex2D(_MainTex, i.uv) * _Color;
                fixed3 normalDir = normalize(i.worldNormal);
                fixed3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                fixed3 viewDir = normalize(i.viewDir);

                // Si es doble cara, invertimos la normal si estamos viendo la parte trasera
                #ifdef _DOUBLESIDED_ON
                    if (dot(normalDir, viewDir) < 0) normalDir = -normalDir;
                #endif

                fixed shadowFactor = 1;
                fixed diffuseColor = 1;
                fixed3 specular = fixed3(0,0,0);
                fixed3 rim = fixed3(0,0,0);

                if (_IsLit)
                {
                    // Sombra
                    #ifdef POINT
                        float shadow = SHADOW_ATTENUATION(i);
                        shadowFactor = lerp(_ShadowColor.rgb, fixed3(1,1,1), smoothstep(_ShadowStepThreshold - _ShadowStepSmooth * 0.5, _ShadowStepThreshold + _ShadowStepSmooth * 0.5, shadow));
                    #else
                        shadowFactor = lerp(_ShadowColor.rgb, fixed3(1,1,1), smoothstep(_ShadowStepThreshold - _ShadowStepSmooth * 0.5, _ShadowStepThreshold + _ShadowStepSmooth * 0.5, SHADOW_ATTENUATION(i)));
                    #endif

                    // Difuso
                    diffuseColor = max(0, dot(normalDir, lightDir));
                    diffuseColor = smoothstep(_ShadowStepThreshold - _ShadowStepSmooth * 0.5, _ShadowStepThreshold + _ShadowStepSmooth * 0.5, diffuseColor);

                    // Especular
                    fixed3 halfDir = normalize(lightDir + viewDir);
                    fixed specularRaw = pow(max(0, dot(normalDir, halfDir)), _SpecularPower);
                    specular = _SpecularColor.rgb * smoothstep(_SpecularStepThreshold - _SpecularStepSmooth * 0.5, _SpecularStepThreshold + _SpecularStepSmooth * 0.5, specularRaw);

                    // Rim Light
                    fixed rimDot = 1 - saturate(dot(viewDir, normalDir));
                    rim = _RimColor.rgb * smoothstep(_RimThreshold - _RimSmooth * 0.5, _RimThreshold + _RimSmooth * 0.5, pow(rimDot, _RimPower));
                }

                fixed3 finalColor = albedo.rgb * (diffuseColor * shadowFactor + specular + rim);

                return fixed4(finalColor, albedo.a);
            }
            ENDCG
        }

        // Pass para el contorno (Outline)
        Pass
        {
            Name "Outline"
            Cull Front // Esto asegura que solo se rendericen las caras traseras
            ZWrite On // Escribir en el depth buffer para que el contorno sea visible
            ColorMask RGB // Asegurarse de que el color se escriba

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog // Por si quieres niebla en el contorno también

            #include "UnityCG.cginc" // Contiene UnityObjectToClipPos y _ObjectToWorld

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            float _OutlineWidth;
            fixed4 _OutlineColor;

            v2f vert (appdata v)
            {
                v2f o;
                // Aplicamos el desplazamiento a lo largo de la normal del vértice
                // El multiplicador 0.1 hace que el _OutlineWidth sea más manejable en el Inspector
                float3 offsetVertex = v.vertex.xyz + v.normal * _OutlineWidth * 0.1;

                // Transformamos la posición desplazada de espacio de objeto a espacio de clip
                o.pos = UnityObjectToClipPos(float4(offsetVertex, 1.0));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDCG
        }
    }
    Fallback "Standard"

}