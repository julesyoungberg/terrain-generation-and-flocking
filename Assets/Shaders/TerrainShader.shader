// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/TerrainShader" {

    Properties {
        _WaterTexture ("Water Texture", 2D) = "blue" {}
        _SandTexture("Sand Texture", 2D) = "baige" {}
        _GrassTexture("Grass Texture", 2D) = "green" {}
        _SnowTexture("Snow Texture", 2D) = "white" {}
        
        _Thresholds("Height Thresholds", vector) = (0, 0, 0, 0)
        _SpecularPow("Specular Power", float) = 1
        _SpecularCoef("Specular Coefficient", float) = 1
        _AmbientLight("Ambient Lighting", vector) = (0, 0, 0, 0)
        
        _WaterColor("Water Color", Color) = (0, 0, 1, 1)
        _WaterTex ("WaterTexture", 2D) = "water" {}
        [NoScaleOffset] _FlowMap ("Flow (RG, A noise)", 2D) = "black" {}
        [NoScaleOffset] _NormalMap ("Normals", 2D) = "bump" {}
        _UJump ("U jump per phase", Range(-0.25, 0.25)) = 0.25
        _VJump ("V jump per phase", Range(-0.25, 0.25)) = 0.25
        _Tiling ("Tiling", Float) = 1
        _Speed ("Speed", Float) = 1
        _FlowStrength ("Flow Strength", Float) = 1
        _FlowOffset ("Flow Offset", Float) = 0
        
        _ReflectionCube("Reflection Map", CUBE) = " " {}
    }
    
    SubShader {
        
        Tags { 
            "RenderType"="Opaque"
            "lightMode"="ForwardBase" 
        }
        LOD 100
        
        Pass {
            CGPROGRAM 
            
            #pragma vertex vert 
            #pragma fragment frag 
            
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            
            #include "Flow.cginc"
            
            float waterLevel = 0.5;
            float timberLine = 10;
            
            float4 _WaterColor;
            sampler2D _WaterTexture;
            sampler2D _SandTexture;
            sampler2D _GrassTexture;
            sampler2D _SnowTexture;
            float4 _MainTex_ST;
            float4 _Thresholds;
            float _SpecularPow;
            float _SpecularCoef;
            float4 _AmbientLight;
            
            sampler2D _WaterTex, _FlowMap, _NormalMap;
            float _UJump, _VJump, _Tiling, _Speed, _FlowStrength, _FlowOffset;
            
            samplerCUBE _ReflectionCube;
            
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;  
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float height : TEXCOORD1;
                float3 normal : TEXCOORD2;
                float4 worldpos : TEXCOORD3;
                float4 vertex : SV_POSITION;
                half3 worldRefl : TEXCOORD4;
            };
            
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.height = v.vertex.y;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // world space normal
                o.normal = UnityObjectToWorldNormal(v.normal);
                // compute world space position of the vertex
                o.worldpos = mul(unity_ObjectToWorld, v.vertex);
                // compute world space view direction
                float3 worldViewDir = normalize(UnityWorldSpaceViewDir(o.worldpos));
                // world space reflection vector
                o.worldRefl = reflect(-worldViewDir, o.normal);
                return o;
            }
            
            float4 lighting (float4 albedo, v2f i) {
                //component 1 ambient
                float4 fragment_color = _AmbientLight * albedo;
                fragment_color.w = 1;
                
                //component 2 diffuse
                float3 L = normalize(_WorldSpaceLightPos0.xyz);
                float3 N = normalize(i.normal);
                float d = dot(L, N);
                if (d > 0) {
                    fragment_color.xyz += _LightColor0.xyz * d * albedo.xyz;
                }
                
                //component 3 Blinn-Phong specular
                float3 V = _WorldSpaceCameraPos.xyz - i.worldpos.xyz;
                V = normalize(V);
                float3 H = (V + L) / 2;
                d = dot(H, N);
                if (d > 0) {
                    float specular = pow(d, _SpecularPow) * _SpecularCoef;
                    fragment_color.xyz += _LightColor0.xyz * specular;
                }
                return fragment_color;
            } 
            
            float4 water (v2f i) {
                // sample the reflection cubemap, using the reflection vector
                half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, i.worldRefl);
                // decode cubemap data into actual color
                half3 skyColor = DecodeHDR (skyData, unity_SpecCube0_HDR);
                fixed4 c = 1.0;
                c.rgb = skyColor;
                return c;
                
                //float2 flowVector = tex2D(_FlowMap, i.uv).rg * 2 - 1;
                //flowVector *= _FlowStrength;
                //float noise = tex2D(_FlowMap, i.uv).a;
                //float time = _Time.y * _Speed + noise;
                //float2 jump = float2(_UJump, _VJump);
                
                //float3 uvwA = FlowUVW(i.uv, flowVector, jump, _FlowOffset, _Tiling, time, false);
                //float3 uvwB = FlowUVW(i.uv, flowVector, jump, _FlowOffset, _Tiling, time, true);
                
                //float3 normalA = UnpackNormal(tex2D(_NormalMap, uvwA.xy)) * uvwA.z;
                //float3 normalB = UnpackNormal(tex2D(_NormalMap, uvwB.xy)) * uvwB.z;
                ////i.normal = normalize(i.normal + normalize(normalA + normalB));
                
                //fixed4 texA = tex2D(_WaterTex, uvwA.xy) * uvwA.z;
                //fixed4 texB = tex2D(_WaterTex, uvwB.xy) * uvwB.z;
                
                //return (texA + texB);// * c;
            }
            
            float4 frag (v2f i) : SV_Target {
                float4 albedo = 0;

                if (i.height < _Thresholds.x) {
                    albedo = tex2D(_WaterTexture, i.uv) * 0.5 + water(i) * 0.5;
                    //return water(i);
                } else if (i.height < _Thresholds.y) albedo = tex2D(_SandTexture, i.uv);
                else if (i.height < _Thresholds.z) albedo = tex2D(_GrassTexture, i.uv);
                else albedo = tex2D(_SnowTexture, i.uv);

                return lighting(albedo, i);
            }
            
            ENDCG
        }
    }
}