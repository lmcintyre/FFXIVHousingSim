// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Silent/FF14 World Cutout (Multisampling + Wind)"
{
	Properties
	{
		[NoScaleOffset]_Albedo0("Albedo 0", 2D) = "white" {}
		[NoScaleOffset][Normal]_NormalMap0("Normal Map 0", 2D) = "bump" {}
		[NoScaleOffset]_Metallic0("Metallic 0", 2D) = "black" {}
		[HideInInspector][NoScaleOffset]_Noise("Noise", 2D) = "white" {}
		_DitherStrength("Dither Strength", Range( 0 , 1)) = 1
		[ToggleOff(_DISABLEWINDSWAY_OFF)] _DisableWindSway("Disable Wind Sway", Float) = 0
		_WindSpeed("Wind Speed", Range( 0 , 20)) = 1
		_Offsetpositionby("Offset position by...", Vector) = (0,0,0,0)
		_WindSwayDirectionalPower("Wind Sway Directional Power", Vector) = (1,1,1,1)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
		[Header(Forward Rendering Options)]
		[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[ToggleOff] _GlossyReflections("Reflections", Float) = 1.0
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IgnoreProjector" = "True" }
		Cull Off
		AlphaToMask On
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 5.0
		#pragma multi_compile_instancing
		#pragma shader_feature _SPECULARHIGHLIGHTS_OFF
		#pragma shader_feature _GLOSSYREFLECTIONS_OFF
		#pragma shader_feature _DISABLEWINDSWAY_OFF
		struct Input
		{
			float2 uv_texcoord;
			float4 screenPos;
		};

		uniform float4 _Offsetpositionby;
		uniform float4 _WindSwayDirectionalPower;
		uniform float _WindSpeed;
		uniform sampler2D _NormalMap0;
		uniform sampler2D _Albedo0;
		uniform sampler2D _Metallic0;
		uniform sampler2D _Noise;
		uniform float4 _Noise_TexelSize;
		uniform float _DitherStrength;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_worldNormal = UnityObjectToWorldNormal( v.normal );
			float2 appendResult208 = (float2(_WindSwayDirectionalPower.x , _WindSwayDirectionalPower.y));
			float3 appendResult209 = (float3(( appendResult208 * v.color.g ) , _WindSwayDirectionalPower.z));
			float3 temp_cast_0 = (( _Time.y * _WindSpeed )).xxx;
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float dotResult143 = dot( temp_cast_0 , frac( ase_worldPos ) );
			float4 WindSwayOffset154 = ( _Offsetpositionby + float4( ( ( _WindSwayDirectionalPower.w * v.color.r * v.color.a ) * ( ase_worldNormal * appendResult209 ) * cos( dotResult143 ) * sin( dotResult143 ) ) , 0.0 ) );
			#ifdef _DISABLEWINDSWAY_OFF
				float4 staticSwitch202 = WindSwayOffset154;
			#else
				float4 staticSwitch202 = float4( 0,0,0,0 );
			#endif
			v.vertex.xyz += staticSwitch202.xyz;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Normal = tex2D( _NormalMap0, i.uv_texcoord ).rgb;
			float4 tex2DNode13 = tex2D( _Albedo0, i.uv_texcoord );
			o.Albedo = tex2DNode13.rgb;
			float4 tex2DNode16 = tex2D( _Metallic0, i.uv_texcoord );
			o.Metallic = tex2DNode16.b;
			o.Smoothness = tex2DNode16.r;
			float Alpha173 = tex2DNode13.a;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float4 tex2DNode64 = tex2D( _Noise, ( (ase_screenPosNorm).xy * (_ScreenParams).xy * _Noise_TexelSize.x ) );
			float FinalAlpha184 = saturate( ( Alpha173 + ( tex2DNode64.r * min( ( Alpha173 + 0.003921569 ) , _DitherStrength ) ) ) );
			o.Alpha = FinalAlpha184;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			AlphaToMask Off
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
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float4 screenPos : TEXCOORD3;
				float4 tSpace0 : TEXCOORD4;
				float4 tSpace1 : TEXCOORD5;
				float4 tSpace2 : TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
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
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.screenPos = ComputeScreenPos( o.pos );
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
				float3 worldPos = IN.worldPos;
				fixed3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.screenPos = IN.screenPos;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
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
2144;614;1687;1598;988.5828;-698.8988;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;157;-2178.47,-1039.099;Float;False;1322.051;1159.512;Wind Sway;15;149;148;144;145;146;143;139;147;141;140;138;205;197;208;209;;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector4Node;147;-2148.437,-65.60419;Float;False;Property;_WindSwayDirectionalPower;Wind Sway Directional Power;12;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;56;-400.1234,1025.695;Float;False;1184.963;795.2761;Screen Space Noise;9;72;64;49;35;53;62;63;34;40;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;11;-1055.341,403.231;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;208;-1724.042,-81.6466;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;63;-320.9136,1584.859;Float;True;Property;_Noise;Noise;7;2;[HideInInspector];[NoScaleOffset];Create;True;0;0;False;0;d64d980827f3b5849a76e117c205e181;a637c4b307514d748a81bcc1f2ff0a91;False;white;Auto;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SamplerNode;13;-710.7492,-269.4251;Float;True;Property;_Albedo0;Albedo 0;0;1;[NoScaleOffset];Create;True;0;0;False;0;None;4ca3ed10a8d63bb4988491ce56eb0d1c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;138;-2121.075,-650.0013;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;206;-1763.487,171.2896;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScreenParams;40;-301.3275,1379.021;Float;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScreenPosInputsNode;34;-304.4275,1115.223;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldPosInputsNode;141;-1822.684,-693.0068;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;140;-2114.404,-752.2164;Float;False;Property;_WindSpeed;Wind Speed;10;0;Create;True;0;0;False;0;1;1;0;20;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;174;560.4715,650.8492;Float;False;173;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;139;-1646.559,-812.9084;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;205;-1577.487,-90.71039;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;173;-208.1582,-208.1592;Float;False;Alpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;142;-1631.561,-695.2332;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexelSizeNode;62;-94.77382,1588.432;Float;False;-1;1;0;SAMPLER2D;;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;53;-122.6035,1351.31;Float;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;35;-48.42764,1115.223;Float;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;185;741.7198,767.3431;Float;False;2;2;0;FLOAT;0.25;False;1;FLOAT;0.003921569;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;140.4052,1566.17;Float;False;3;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WorldNormalVector;146;-1836.03,-416.5509;Float;False;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;122;640.8503,890.379;Float;False;Property;_DitherStrength;Dither Strength;8;0;Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;209;-1347.816,-176.3918;Float;False;FLOAT3;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DotProductOpNode;143;-1483.071,-824.3595;Float;False;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMinOpNode;186;991.2732,792.0591;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;197;-1195.535,-39.5298;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;10;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CosOpNode;145;-1313.539,-865.5465;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;144;-1308.187,-944.9564;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;64;429.3842,1329.946;Float;True;Property;_TextureSample0;Texture Sample 0;5;0;Create;True;0;0;False;0;a637c4b307514d748a81bcc1f2ff0a91;a637c4b307514d748a81bcc1f2ff0a91;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;151;-1387.76,-373.9916;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;148;-1051.664,-923.8982;Float;False;4;4;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector4Node;152;-1451.944,-619.0352;Float;False;Property;_Offsetpositionby;Offset position by...;11;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;188;1186.281,832.5026;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;153;-1053.544,-671.8355;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;187;1349.279,737.4202;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;155;-319.2865,201.6124;Float;False;154;0;1;FLOAT4;0
Node;AmplifyShaderEditor.SaturateNode;190;1553.685,948.4113;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;203;-2309.849,428.534;Float;False;1171.281;214.4;Red channel seems to indicate which receives less effect. Alpha channel is always 1 for checked trees, might be a switch on the effect. Green seems to indicate something like vertical sway and is multiplied by red.;3;200;201;82;FF14 Vertex Colour;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;154;-1123.301,-507.2482;Float;False;WindSwayOffset;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;89;846.7073,-30.009;Float;False;759.5698;471.7314;Combine Alpha with Noise;3;85;66;156;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;16;-713.4539,116.3746;Float;True;Property;_Metallic0;Metallic 0;3;1;[NoScaleOffset];Create;True;0;0;False;0;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;200;-1462.926,518.3129;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;202;-109.7675,167.1033;Float;False;Property;_DisableWindSway;Disable Wind Sway;9;0;Create;True;0;0;False;0;0;0;0;True;;ToggleOff;2;;Key1;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.VertexColorNode;82;-1857.65,477.9743;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalVertexDataNode;149;-1818.011,-558.0662;Float;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;201;-1626.926,501.313;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;10;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;72;519.7252,1129.564;Float;False;dither=dither * 2.0f - 1.0f@$            dither=sign(dither)*(1.0f-sqrt(1.0f-abs(dither)))@$$return dither@$;1;False;1;True;dither;FLOAT;0;In;Rescale;True;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;183;-352.4571,20.01333;Float;False;184;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;85;1328.3,80.03921;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;166;-722.9203,501.3516;Half;False;UVcoord;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.VertexColorNode;204;-445.4882,-426.1578;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;156;1030.931,12.80443;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;184;1541.657,761.6793;Float;False;FinalAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;105;531.0839,-625.6516;Float;False;return (alpha - 0.5) / max(fwidth(alpha), 0.0001) + 0.5@;1;False;1;True;alpha;FLOAT;0;In;Sharpen Alpha;True;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;168;-691.825,-862.3884;Float;False;Property;_EmissionColour;Emission Colour;5;1;[HDR];Create;True;0;0;False;0;0,0,0,0;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;92;-724.2315,-653.7998;Float;True;Property;_Emission;Emission;6;1;[NoScaleOffset];Create;True;0;0;False;0;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;91;-78.4823,315.8256;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;90;-369.4823,397.8256;Float;False;Property;_Smoothness;Smoothness;4;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;66;924.8906,186.9201;Float;False;3;3;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;14;-717.4539,-71.6254;Float;True;Property;_NormalMap0;Normal Map 0;2;2;[NoScaleOffset];[Normal];Create;True;0;0;False;0;None;None;True;0;False;bump;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;169;-438.9135,-815.2228;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;147,-108;Float;False;True;7;Float;ASEMaterialInspector;0;0;Standard;Silent/FF14 World Cutout (Multisampling + Wind);False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;True;False;True;True;Off;0;False;-1;0;False;-1;False;0;0;False;0;Custom;0.1;True;True;0;True;TransparentCutout;;AlphaTest;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;-1;False;-1;-1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;1;-1;-1;-1;0;0;0;True;0;0;0;False;-1;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;208;0;147;1
WireConnection;208;1;147;2
WireConnection;13;1;11;0
WireConnection;139;0;138;0
WireConnection;139;1;140;0
WireConnection;205;0;208;0
WireConnection;205;1;206;2
WireConnection;173;0;13;4
WireConnection;142;0;141;0
WireConnection;62;0;63;0
WireConnection;53;0;40;0
WireConnection;35;0;34;0
WireConnection;185;0;174;0
WireConnection;49;0;35;0
WireConnection;49;1;53;0
WireConnection;49;2;62;1
WireConnection;209;0;205;0
WireConnection;209;2;147;3
WireConnection;143;0;139;0
WireConnection;143;1;142;0
WireConnection;186;0;185;0
WireConnection;186;1;122;0
WireConnection;197;0;147;4
WireConnection;197;1;206;1
WireConnection;197;2;206;4
WireConnection;145;0;143;0
WireConnection;144;0;143;0
WireConnection;64;0;63;0
WireConnection;64;1;49;0
WireConnection;151;0;146;0
WireConnection;151;1;209;0
WireConnection;148;0;197;0
WireConnection;148;1;151;0
WireConnection;148;2;145;0
WireConnection;148;3;144;0
WireConnection;188;0;64;1
WireConnection;188;1;186;0
WireConnection;153;0;152;0
WireConnection;153;1;148;0
WireConnection;187;0;174;0
WireConnection;187;1;188;0
WireConnection;190;0;187;0
WireConnection;154;0;153;0
WireConnection;16;1;11;0
WireConnection;200;0;201;0
WireConnection;202;0;155;0
WireConnection;201;0;82;1
WireConnection;201;2;82;4
WireConnection;72;0;64;1
WireConnection;85;0;174;0
WireConnection;85;1;66;0
WireConnection;166;0;11;0
WireConnection;156;0;66;0
WireConnection;184;0;190;0
WireConnection;92;1;11;0
WireConnection;91;0;16;4
WireConnection;91;1;90;0
WireConnection;66;2;174;0
WireConnection;14;1;11;0
WireConnection;169;0;168;0
WireConnection;169;1;92;0
WireConnection;0;0;13;0
WireConnection;0;1;14;0
WireConnection;0;3;16;3
WireConnection;0;4;16;1
WireConnection;0;9;183;0
WireConnection;0;11;202;0
ASEEND*/
//CHKSM=A8E016CF232164C8FA9AD024AF494F695B6994B9