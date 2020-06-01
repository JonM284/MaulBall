// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom_Hex/Ghostly_Aura"
{
	Properties
	{
		_Noise("Noise", 2D) = "white" {}
		_Noise_Scale_1("Noise_Scale_1", Range( 1 , 2.5)) = 0
		_Noise_Scale_2("Noise_Scale_2", Range( 0.5 , 2.5)) = 0
		_Noise_Scale_3("Noise_Scale_3", Range( 1 , 2.5)) = 0
		_Speed("Speed", Range( 0 , 2)) = 0
		_Fresnel_Color("Fresnel_Color", Color) = (0,0,0,0)
		_Fresnel_Power("Fresnel_Power", Range( 0.1 , 10)) = 0
		_Opacity("Opacity", Range( 0 , 1)) = 0
		_Albedo_Color("Albedo_Color", Color) = (0,0,0,0)
		_Distance_Num("Distance_Num", Range( 0 , 5)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
			float4 screenPos;
		};

		uniform float4 _Albedo_Color;
		uniform float4 _Fresnel_Color;
		uniform sampler2D _Noise;
		uniform float _Speed;
		uniform float _Noise_Scale_1;
		uniform float _Noise_Scale_2;
		uniform float _Noise_Scale_3;
		uniform float _Fresnel_Power;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _Distance_Num;
		uniform float _Opacity;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Albedo = _Albedo_Color.rgb;
			float4 appendResult19 = (float4(0.0 , _Speed , 0.0 , 0.0));
			float2 panner15 = ( 1.0 * _Time.y * appendResult19.xy + ( i.uv_texcoord * _Noise_Scale_1 ));
			float2 panner16 = ( 1.0 * _Time.y * appendResult19.xy + ( i.uv_texcoord * _Noise_Scale_2 ));
			float2 panner17 = ( 1.0 * _Time.y * appendResult19.xy + ( i.uv_texcoord * _Noise_Scale_3 ));
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV22 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode22 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV22, _Fresnel_Power ) );
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth30 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture,UNITY_PROJ_COORD( ase_screenPos )));
			float distanceDepth30 = abs( ( screenDepth30 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _Distance_Num ) );
			o.Emission = ( ( _Fresnel_Color * ( tex2D( _Noise, panner15 ) * tex2D( _Noise, panner16 ) * tex2D( _Noise, panner17 ) ) ) * fresnelNode22 * distanceDepth30 ).rgb;
			o.Alpha = _Opacity;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows exclude_path:deferred 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
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
				float3 worldNormal : TEXCOORD4;
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
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.screenPos = ComputeScreenPos( o.pos );
				return o;
			}
			half4 frag( v2f IN
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
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
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
Version=16900
0;92;927;414;4136.049;-440.4305;5.18832;True;False
Node;AmplifyShaderEditor.RangedFloatNode;7;-2116.983,507.4427;Float;False;Property;_Noise_Scale_3;Noise_Scale_3;3;0;Create;True;0;0;False;0;0;1.83;1;2.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;11;-2625.785,285.2813;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;5;-2097.233,90.23263;Float;False;Property;_Noise_Scale_1;Noise_Scale_1;1;0;Create;True;0;0;False;0;0;1.37;1;2.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-2183.339,684.2903;Float;False;Property;_Speed;Speed;4;0;Create;True;0;0;False;0;0;0.21;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-2112.046,300.0722;Float;False;Property;_Noise_Scale_2;Noise_Scale_2;2;0;Create;True;0;0;False;0;0;0.68;0.5;2.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-1761.487,487.6931;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-1783.706,87.76393;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;19;-1810.005,685.8535;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-1778.768,300.072;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;1;-1479.101,-176.2865;Float;True;Property;_Noise;Noise;0;0;Create;True;0;0;False;0;None;d94dabd0c484d044bbbdc8a3cb4cfe8d;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.PannerNode;17;-1481.587,479.6698;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;16;-1481.587,275.2355;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;15;-1463.44,66.34479;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;3;-1195.478,528.526;Float;True;Property;_TextureSample1;Texture Sample 1;1;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;4;-1198.95,282.0361;Float;True;Property;_TextureSample2;Texture Sample 2;1;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;-1213.616,40.15378;Float;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-787.5168,301.9656;Float;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-1124.665,754.3616;Float;False;Property;_Distance_Num;Distance_Num;9;0;Create;True;0;0;False;0;0;1.47;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-1161.429,1155.994;Float;False;Property;_Fresnel_Power;Fresnel_Power;6;0;Create;True;0;0;False;0;0;2.57;0.1;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;21;-806.4453,84.33335;Float;False;Property;_Fresnel_Color;Fresnel_Color;5;0;Create;True;0;0;False;0;0,0,0,0;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-598.2796,308.2722;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FresnelNode;22;-785.7831,953.8553;Float;False;Standard;WorldNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;30;-803.481,739.4923;Float;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceCameraPos;24;-1319.281,982.5397;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-423.7897,336.988;Float;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;27;-383.9986,477.7174;Float;False;Property;_Opacity;Opacity;7;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;28;-439.3017,-126.4199;Float;False;Property;_Albedo_Color;Albedo_Color;8;0;Create;True;0;0;False;0;0,0,0,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalVertexDataNode;26;-1242.354,839.4753;Float;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;Custom_Hex/Ghostly_Aura;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Translucent;0.5;True;True;0;False;Opaque;;Transparent;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0.11;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;12;0;11;0
WireConnection;12;1;7;0
WireConnection;14;0;11;0
WireConnection;14;1;5;0
WireConnection;19;1;18;0
WireConnection;13;0;11;0
WireConnection;13;1;6;0
WireConnection;17;0;12;0
WireConnection;17;2;19;0
WireConnection;16;0;13;0
WireConnection;16;2;19;0
WireConnection;15;0;14;0
WireConnection;15;2;19;0
WireConnection;3;0;1;0
WireConnection;3;1;17;0
WireConnection;4;0;1;0
WireConnection;4;1;16;0
WireConnection;2;0;1;0
WireConnection;2;1;15;0
WireConnection;20;0;2;0
WireConnection;20;1;4;0
WireConnection;20;2;3;0
WireConnection;29;0;21;0
WireConnection;29;1;20;0
WireConnection;22;3;25;0
WireConnection;30;0;31;0
WireConnection;23;0;29;0
WireConnection;23;1;22;0
WireConnection;23;2;30;0
WireConnection;0;0;28;0
WireConnection;0;2;23;0
WireConnection;0;9;27;0
ASEEND*/
//CHKSM=11F0F3829EC315C903ECE43AAB54F34AE5241D3C