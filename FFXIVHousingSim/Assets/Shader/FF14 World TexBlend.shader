// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Silent/FF14 World TexBlend"
{
	Properties
	{
		[NoScaleOffset]_Albedo0("Albedo 0", 2D) = "white" {}
		[NoScaleOffset]_Albedo1("Albedo 1", 2D) = "white" {}
		[NoScaleOffset][Normal]_NormalMap0("Normal Map 0", 2D) = "bump" {}
		[NoScaleOffset][Normal]_NormalMap1("Normal Map 1", 2D) = "bump" {}
		[NoScaleOffset]_Metallic0("Metallic 0", 2D) = "black" {}
		[NoScaleOffset]_Metallic1("Metallic 1", 2D) = "black" {}
		_EmissionPow("Emission Pow", Range( 0 , 10)) = 0
		[Toggle(_APPLYVERTEXCOLOURING_ON)] _ApplyVertexColouring("Apply Vertex Colouring", Float) = 0
		[KeywordEnum(XYZW,UV1UV2,None)] _SecondUVSource("Second UV Source", Float) = 0
		[ToggleOff(_DISABLEPARALLAXEFFECT_OFF)] _DisableParallaxEffect("Disable Parallax Effect", Float) = 0
		[HideInInspector] _tex4coord( "", 2D ) = "white" {}
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 5.0
		#pragma multi_compile_instancing
		#pragma shader_feature _DISABLEPARALLAXEFFECT_OFF
		#pragma shader_feature _SECONDUVSOURCE_XYZW _SECONDUVSOURCE_UV1UV2 _SECONDUVSOURCE_NONE
		#pragma shader_feature _APPLYVERTEXCOLOURING_ON
		#pragma shader_feature _DISABLEBLENDING_OFF
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		#undef TRANSFORM_TEX
		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
		struct Input
		{
			float2 uv_texcoord;
			float3 viewDir;
			INTERNAL_DATA
			float4 uv_tex4coord;
			float2 uv2_texcoord2;
			float4 vertexColor : COLOR;
		};

		uniform sampler2D _NormalMap0;
		uniform sampler2D _NormalMap1;
		uniform sampler2D _Albedo0;
		uniform sampler2D _Albedo1;
		uniform sampler2D _Metallic0;
		uniform sampler2D _Metallic1;
		uniform float _EmissionPow;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_NormalMap0113 = i.uv_texcoord;
			float4 tex2DNode113 = tex2D( _NormalMap0, uv_NormalMap0113 );
			float2 paralaxOffset110 = ParallaxOffset( ( tex2DNode113.b + 0.5 ) , 0.005 , i.viewDir );
			#ifdef _DISABLEPARALLAXEFFECT_OFF
				float2 staticSwitch118 = paralaxOffset110;
			#else
				float2 staticSwitch118 = float2( 0,0 );
			#endif
			float4 uv_TexCoord50 = i.uv_tex4coord;
			uv_TexCoord50.xy = i.uv_tex4coord.xy + staticSwitch118;
			float2 appendResult68 = (float2(uv_TexCoord50.x , uv_TexCoord50.y));
			float2 appendResult69 = (float2(uv_TexCoord50.z , uv_TexCoord50.w));
			float2 uv2_TexCoord109 = i.uv2_texcoord2 + staticSwitch118;
			#if defined(_SECONDUVSOURCE_XYZW)
				float2 staticSwitch108 = appendResult69;
			#elif defined(_SECONDUVSOURCE_UV1UV2)
				float2 staticSwitch108 = uv2_TexCoord109;
			#elif defined(_SECONDUVSOURCE_NONE)
				float2 staticSwitch108 = appendResult68;
			#else
				float2 staticSwitch108 = appendResult69;
			#endif
			
            float staticSwitch119 = i.vertexColor.a;
            
			#ifdef _APPLYVERTEXCOLOURING_ON
				float staticSwitch107 = saturate( ( staticSwitch119 * tex2D( _NormalMap0, appendResult68 ).a ) );
			#else
				float staticSwitch107 = staticSwitch119;
			#endif
			float BlendValue60 = staticSwitch107;
			float3 lerpResult58 = lerp( UnpackNormal( tex2D( _NormalMap0, appendResult68 ) ) , UnpackNormal( tex2D( _NormalMap1, staticSwitch108 ) ) , BlendValue60);
			o.Normal = lerpResult58;
			float4 lerpResult84 = lerp( tex2D( _Albedo0, appendResult68 ) , tex2D( _Albedo1, staticSwitch108 ) , BlendValue60);
			#ifdef _APPLYVERTEXCOLOURING_ON
				float4 staticSwitch74 = ( lerpResult84 + -0.5 + i.vertexColor );
			#else
				float4 staticSwitch74 = lerpResult84;
			#endif
			float4 AlbedoFinal71 = staticSwitch74;
			o.Albedo = AlbedoFinal71.rgb;
			float4 lerpResult59 = lerp( tex2D( _Metallic0, appendResult68 ) , tex2D( _Metallic1, staticSwitch108 ) , BlendValue60);
			float4 break64 = lerpResult59;
			o.Emission = ( break64.a * AlbedoFinal71 * _EmissionPow ).rgb;
			o.Metallic = break64.b;
			float lerpResult40 = lerp( 0.18 , 0.81 , break64.r);
			o.Smoothness = lerpResult40;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float4 customPack1 : TEXCOORD1;
				float4 customPack2 : TEXCOORD2;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				fixed4 color : COLOR0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				fixed3 worldNormal = UnityObjectToWorldNormal( v.normal );
				fixed3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				fixed3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.customPack2.xyzw = customInputData.uv_tex4coord;
				o.customPack2.xyzw = v.texcoord;
				o.customPack1.zw = customInputData.uv2_texcoord2;
				o.customPack1.zw = v.texcoord1;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.color = v.color;
				return o;
			}
			fixed4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				surfIN.uv_tex4coord = IN.customPack2.xyzw;
				surfIN.uv2_texcoord2 = IN.customPack1.zw;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				fixed3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.viewDir = IN.tSpace0.xyz * worldViewDir.x + IN.tSpace1.xyz * worldViewDir.y + IN.tSpace2.xyz * worldViewDir.z;
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				surfIN.vertexColor = IN.color;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=15301
2146;515;1687;1598;3954.585;780.7387;1.3;True;False
Node;AmplifyShaderEditor.SamplerNode;113;-3395.698,-202.194;Float;True;Property;_TextureSample1;Texture Sample 1;2;1;[NoScaleOffset];Create;True;0;0;False;0;None;1a3cdec6fb04a0a4aa32fb62469335ae;True;0;False;black;Auto;False;Instance;3;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;111;-3009.134,99.53765;Float;False;Tangent;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;115;-2996.584,-114.2033;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.ParallaxOffsetHlpNode;110;-2760.792,107.3555;Float;False;3;0;FLOAT;0;False;1;FLOAT;0.005;False;2;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;118;-2562.365,83.41164;Float;False;Property;_DisableParallaxEffect;Disable Parallax Effect;9;0;Create;True;0;0;False;0;0;0;0;True;;ToggleOff;2;Key0;Key1;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;50;-2273.639,-21.31805;Float;False;0;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;88;-1284,-1097.265;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;68;-1919.885,-274.5865;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;119;-1028.304,-925.4226;Float;False;Property;_DisableBlending;Disable Blending;10;0;Create;True;0;0;False;0;0;0;1;True;;ToggleOff;2;Key0;Key1;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;86;-1281.134,-757.3568;Float;True;Property;_TextureSample0;Texture Sample 0;2;1;[NoScaleOffset];Create;True;0;0;False;0;None;b28045cdce9535548bc595a44521ccf3;True;0;True;bump;Auto;False;Instance;3;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;121;-862.3467,-748.5497;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;109;-2272.998,173.0495;Float;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;101;-688.2937,-742.3562;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;69;-1958.715,89.06883;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;108;-1696.54,566.7291;Float;False;Property;_SecondUVSource;Second UV Source;8;0;Create;True;0;0;False;0;0;0;0;True;;KeywordEnum;3;XYZW;UV1UV2;None;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;107;-614.1003,-935.02;Float;False;Property;_ApplyVertexColouring;Apply Vertex Colouring;7;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;87;-446.6017,-312.7636;Float;False;60;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-827.4937,-500.5341;Float;True;Property;_Albedo0;Albedo 0;0;1;[NoScaleOffset];Create;True;0;0;False;0;None;d77f0eb95cf118f469b30066ee76c0e6;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;49;-832.2064,-297.8654;Float;True;Property;_Albedo1;Albedo 1;1;1;[NoScaleOffset];Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;60;-282.9399,-972.7994;Float;False;BlendValue;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;84;-373.9803,-550.6643;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;54;-445.2009,-224.7999;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;95;-417.7131,-63.52338;Float;False;Constant;_Float0;Float 0;8;0;Create;True;0;0;False;0;-0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;4;-1147.296,685.766;Float;True;Property;_Metallic0;Metallic 0;4;1;[NoScaleOffset];Create;True;0;0;False;0;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;57;-1142.16,895.0431;Float;True;Property;_Metallic1;Metallic 1;5;1;[NoScaleOffset];Create;True;0;0;False;0;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;83;-160.9803,-235.6643;Float;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;62;-1019.136,1096.063;Float;False;60;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;74;-77.07983,-515.3254;Float;False;Property;_ApplyVertexColouring;Apply Vertex Colouring;8;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;59;-687.0394,843.3876;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.BreakToComponentsNode;64;-475.6379,795.5853;Float;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RegisterLocalVarNode;71;302.3309,-520.1566;Float;False;AlbedoFinal;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;56;-855.7323,312.0049;Float;True;Property;_NormalMap1;Normal Map 1;3;2;[NoScaleOffset];[Normal];Create;True;0;0;False;0;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;61;-568.9885,387.5907;Float;False;60;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-533.3942,1125.066;Float;False;Property;_EmissionPow;Emission Pow;6;0;Create;True;0;0;False;0;0;0;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;73;-486.852,1019.953;Float;False;71;0;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;3;-851.8496,104.7894;Float;True;Property;_NormalMap0;Normal Map 0;2;2;[NoScaleOffset];[Normal];Create;True;0;0;False;0;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ParallaxOcclusionMappingNode;117;-2778.836,-125.3574;Float;False;0;8;16;2;0.02;0.5;False;1,1;False;0,0;False;7;0;FLOAT2;0,0;False;1;SAMPLER2D;;False;2;FLOAT;0.02;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT2;0,0;False;6;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;40;-317.2414,495.426;Float;False;3;0;FLOAT;0.18;False;1;FLOAT;0.81;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;58;-499.4165,206.7442;Float;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-105.1217,934.5651;Float;False;3;3;0;FLOAT;1;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;120;-2998.946,-9.053104;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;72;-478.5316,17.48549;Float;False;71;0;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;33,15;Float;False;True;7;Float;ASEMaterialInspector;0;0;Standard;Silent/FF14 World TexBlend;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;Back;0;False;-1;0;False;-1;False;0;0;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;-1;False;-1;-1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;0;0;False;0;0;0;False;-1;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;115;0;113;3
WireConnection;110;0;115;0
WireConnection;110;2;111;0
WireConnection;118;0;110;0
WireConnection;50;1;118;0
WireConnection;68;0;50;1
WireConnection;68;1;50;2
WireConnection;119;0;88;4
WireConnection;86;1;68;0
WireConnection;121;0;119;0
WireConnection;121;1;86;4
WireConnection;109;1;118;0
WireConnection;101;0;121;0
WireConnection;69;0;50;3
WireConnection;69;1;50;4
WireConnection;108;1;69;0
WireConnection;108;0;109;0
WireConnection;108;2;68;0
WireConnection;107;1;119;0
WireConnection;107;0;101;0
WireConnection;2;1;68;0
WireConnection;49;1;108;0
WireConnection;60;0;107;0
WireConnection;84;0;2;0
WireConnection;84;1;49;0
WireConnection;84;2;87;0
WireConnection;4;1;68;0
WireConnection;57;1;108;0
WireConnection;83;0;84;0
WireConnection;83;1;95;0
WireConnection;83;2;54;0
WireConnection;74;1;84;0
WireConnection;74;0;83;0
WireConnection;59;0;4;0
WireConnection;59;1;57;0
WireConnection;59;2;62;0
WireConnection;64;0;59;0
WireConnection;71;0;74;0
WireConnection;56;1;108;0
WireConnection;3;1;68;0
WireConnection;117;3;111;0
WireConnection;40;2;64;0
WireConnection;58;0;3;0
WireConnection;58;1;56;0
WireConnection;58;2;61;0
WireConnection;5;0;64;3
WireConnection;5;1;73;0
WireConnection;5;2;6;0
WireConnection;120;0;113;3
WireConnection;0;0;72;0
WireConnection;0;1;58;0
WireConnection;0;2;5;0
WireConnection;0;3;64;2
WireConnection;0;4;40;0
ASEEND*/
//CHKSM=DD96AC3F97C5DA51E1AF1E39B4224F0F7130217A