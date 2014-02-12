// Shader "Color Space/YCrCbtoRGB Trans" 
// {
    // Properties 
    // {
        // _YTex ("Y (RGB)", 2D) = "white" {}
        // _CrTex ("Cr (RGB)", 2D) = "white" {}
        // _CbTex ("Cb (RGB)", 2D) = "white" {}
        // _TintColor ("Color", COLOR) = (1,1,1,1)
    // }
    // SubShader 
    // {
		// Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        // Pass 
        // {
			// Blend SrcAlpha OneMinusSrcAlpha
			// ColorMask RGB
			// Lighting Off Fog { Color (0,0,0,0) }
			// ZWrite Off

			// CGPROGRAM
			// #pragma vertex vert
			// #pragma fragment frag

			// #include "UnityCG.cginc"

			// sampler2D _YTex;
			// sampler2D _CbTex;
			// sampler2D _CrTex;
			// fixed4 _TintColor;

			// struct v2f 
			// {
				// float4  pos : SV_POSITION;
				// half2  uvY : TEXCOORD0;
				// half2  uvCbCr : TEXCOORD1;
			// };

			// float4 _YTex_ST;
			// float4 _CbTex_ST;

			// v2f vert (appdata_base v)
			// {
				// v2f o;
				// o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				// o.uvY = TRANSFORM_TEX (v.texcoord, _YTex);
				// o.uvCbCr = TRANSFORM_TEX (v.texcoord, _CbTex);
				// return o;
			// }

			// fixed4 frag (v2f i) : COLOR
			// {
				// fixed4 yuvVec = fixed4(tex2D (_YTex, i.uvY).r, tex2D (_CrTex, i.uvCbCr).g, tex2D (_CbTex, i.uvCbCr).b, 1.0);
				
				// fixed4 rgbVec;
				
				// rgbVec.x = dot(fixed4(1.1643828125, 0, 1.59602734375, -.87078515625), yuvVec);
				// rgbVec.y = dot(fixed4(1.1643828125, -.39176171875, -.81296875, .52959375), yuvVec);
				// rgbVec.z = dot(fixed4(1.1643828125, 2.017234375, 0, -1.081390625), yuvVec);
				// rgbVec.w = 1.0f;
				
				// rgbVec*=_TintColor;
				
				// return rgbVec;
			// }
			// ENDCG
		// }
	// }
// }


Shader "Color Space/YCrCbtoRGB Trans" 
{
    Properties 
    {
        _YTex ("Y (RGB)", 2D) = "white" {}
        _CrTex ("Cr (RGB)", 2D) = "white" {}
        _CbTex ("Cb (RGB)", 2D) = "white" {}
        _TintColor ("Color", COLOR) = (1,1,1,1)
    }
    SubShader 
    {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Pass 
        {
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			Lighting Off Fog { Color (0,0,0,0) }
			ZWrite Off

			Program "vp" 
			{
				// Vertex combos: 1
				//   opengl - ALU: 6 to 6
				//   d3d9 - ALU: 6 to 6
				SubProgram "opengl " 
				{
					Keywords { }
					Bind "vertex" Vertex
					Bind "texcoord" TexCoord0
					Vector 5 [_YTex_ST]
					Vector 6 [_CbTex_ST]
					"!!ARBvp1.0
					# 6 ALU
					PARAM c[7] = { program.local[0],
							state.matrix.mvp,
							program.local[5..6] };
					MAD result.texcoord[0].xy, vertex.texcoord[0], c[5], c[5].zwzw;
					MAD result.texcoord[1].xy, vertex.texcoord[0], c[6], c[6].zwzw;
					DP4 result.position.w, vertex.position, c[4];
					DP4 result.position.z, vertex.position, c[3];
					DP4 result.position.y, vertex.position, c[2];
					DP4 result.position.x, vertex.position, c[1];
					END
					# 6 instructions, 0 R-regs
					"
				}

				SubProgram "d3d9 " 
				{
					Keywords { }
					Bind "vertex" Vertex
					Bind "texcoord" TexCoord0
					Matrix 0 [glstate_matrix_mvp]
					Vector 4 [_YTex_ST]
					Vector 5 [_CbTex_ST]
					"vs_2_0
					; 6 ALU
					dcl_position0 v0
					dcl_texcoord0 v1
					mad oT0.xy, v1, c4, c4.zwzw
					mad oT1.xy, v1, c5, c5.zwzw
					dp4 oPos.w, v0, c3
					dp4 oPos.z, v0, c2
					dp4 oPos.y, v0, c1
					dp4 oPos.x, v0, c0
					"
				}

				SubProgram "gles " 
				{
					Keywords { }
					"!!GLES
					#define SHADER_API_GLES 1
					#define tex2D texture2D


					#ifdef VERTEX
					#define gl_ModelViewProjectionMatrix glstate_matrix_mvp
					uniform mat4 glstate_matrix_mvp;

					varying mediump vec2 xlv_TEXCOORD1;
					varying mediump vec2 xlv_TEXCOORD0;

					uniform highp vec4 _YTex_ST;
					uniform highp vec4 _CbTex_ST;
					attribute vec4 _glesMultiTexCoord0;
					attribute vec4 _glesVertex;
					void main ()
					{
						mediump vec2 tmpvar_1;
						mediump vec2 tmpvar_2;
						highp vec2 tmpvar_3;
						tmpvar_3 = ((_glesMultiTexCoord0.xy * _YTex_ST.xy) + _YTex_ST.zw);
						tmpvar_1 = tmpvar_3;
						highp vec2 tmpvar_4;
						tmpvar_4 = ((_glesMultiTexCoord0.xy * _CbTex_ST.xy) + _CbTex_ST.zw);
						tmpvar_2 = tmpvar_4;
						gl_Position = (gl_ModelViewProjectionMatrix * _glesVertex);
						xlv_TEXCOORD0 = tmpvar_1;
						xlv_TEXCOORD1 = tmpvar_2;
					}



					#endif
					#ifdef FRAGMENT

					varying mediump vec2 xlv_TEXCOORD1;
					varying mediump vec2 xlv_TEXCOORD0;
					uniform sampler2D _YTex;
					uniform lowp vec4 _TintColor;
					uniform sampler2D _CrTex;
					uniform sampler2D _CbTex;
					void main ()
					{
						lowp vec4 rgbVec;
						lowp vec4 tmpvar_1;
						tmpvar_1.w = 1.0;
						tmpvar_1.x = texture2D (_YTex, xlv_TEXCOORD0).x;
						tmpvar_1.y = texture2D (_CrTex, xlv_TEXCOORD1).y;
						tmpvar_1.z = texture2D (_CbTex, xlv_TEXCOORD1).z;
						rgbVec.x = dot (vec4(1.16438, 0.0, 1.59603, -0.870785), tmpvar_1);
						rgbVec.y = dot (vec4(1.16438, -0.391762, -0.812969, 0.529594), tmpvar_1);
						rgbVec.z = dot (vec4(1.16438, 2.01723, 0.0, -1.08139), tmpvar_1);
						rgbVec.w = 1.0;
						lowp vec4 tmpvar_2;
						tmpvar_2 = (rgbVec * _TintColor);
						rgbVec = tmpvar_2;
						gl_FragData[0] = tmpvar_2;
					}

					#endif"
				}

				SubProgram "glesdesktop " 
				{
					Keywords { }
					"!!GLES
					#define SHADER_API_GLES 1
					#define tex2D texture2D

					#ifdef VERTEX
					#define gl_ModelViewProjectionMatrix glstate_matrix_mvp
					uniform mat4 glstate_matrix_mvp;

					varying mediump vec2 xlv_TEXCOORD1;
					varying mediump vec2 xlv_TEXCOORD0;

					uniform highp vec4 _YTex_ST;
					uniform highp vec4 _CbTex_ST;
					attribute vec4 _glesMultiTexCoord0;
					attribute vec4 _glesVertex;
					void main ()
					{
						mediump vec2 tmpvar_1;
						mediump vec2 tmpvar_2;
						highp vec2 tmpvar_3;
						tmpvar_3 = ((_glesMultiTexCoord0.xy * _YTex_ST.xy) + _YTex_ST.zw);
						tmpvar_1 = tmpvar_3;
						highp vec2 tmpvar_4;
						tmpvar_4 = ((_glesMultiTexCoord0.xy * _CbTex_ST.xy) + _CbTex_ST.zw);
						tmpvar_2 = tmpvar_4;
						gl_Position = (gl_ModelViewProjectionMatrix * _glesVertex);
						xlv_TEXCOORD0 = tmpvar_1;
						xlv_TEXCOORD1 = tmpvar_2;
					}

					#endif
					#ifdef FRAGMENT

					varying mediump vec2 xlv_TEXCOORD1;
					varying mediump vec2 xlv_TEXCOORD0;
					uniform sampler2D _YTex;
					uniform lowp vec4 _TintColor;
					uniform sampler2D _CrTex;
					uniform sampler2D _CbTex;
					void main ()
					{
						lowp vec4 rgbVec;
						lowp vec4 tmpvar_1;
						tmpvar_1.w = 1.0;
						tmpvar_1.x = texture2D (_YTex, xlv_TEXCOORD0).x;
						tmpvar_1.y = texture2D (_CrTex, xlv_TEXCOORD1).y;
						tmpvar_1.z = texture2D (_CbTex, xlv_TEXCOORD1).z;
						rgbVec.x = dot (vec4(1.16438, 0.0, 1.59603, -0.870785), tmpvar_1);
						rgbVec.y = dot (vec4(1.16438, -0.391762, -0.812969, 0.529594), tmpvar_1);
						rgbVec.z = dot (vec4(1.16438, 2.01723, 0.0, -1.08139), tmpvar_1);
						rgbVec.w = 1.0;
						lowp vec4 tmpvar_2;
						tmpvar_2 = (rgbVec * _TintColor);
						rgbVec = tmpvar_2;
						gl_FragData[0] = tmpvar_2;
					}

					#endif"
				}
			}
				
			Program "fp" 
			{
				// Fragment combos: 1
				//   opengl - ALU: 9 to 9, TEX: 3 to 3
				//   d3d9 - ALU: 7 to 7, TEX: 3 to 3
				SubProgram "opengl " 
				{
					Keywords { }
					Vector 0 [_TintColor]
					SetTexture 0 [_YTex] 2D
					SetTexture 1 [_CrTex] 2D
					SetTexture 2 [_CbTex] 2D
					"!!ARBfp1.0
					# 9 ALU, 3 TEX
					PARAM c[5] = { program.local[0],
							{ 1 },
							{ 1.1640625, 2.0175781, 0, -1.0810547 },
							{ 1.1640625, -0.3918457, -0.81298828, 0.52978516 },
							{ 1.1640625, 0, 1.5957031, -0.87060547 } };
					TEMP R0;
					TEMP R1;
					TEX R0.x, fragment.texcoord[0], texture[0], 2D;
					TEX R0.y, fragment.texcoord[1], texture[1], 2D;
					TEX R0.z, fragment.texcoord[1], texture[2], 2D;
					MOV R0.w, c[1].x;
					DP4 R1.z, R0, c[2];
					DP4 R1.y, R0, c[3];
					DP4 R1.x, R0, c[4];
					MOV R1.w, c[1].x;
					MUL result.color, R1, c[0];
					END
					# 9 instructions, 2 R-regs
					"
				}

				SubProgram "d3d9 " 
				{
					Keywords { }
					Vector 0 [_TintColor]
					SetTexture 0 [_YTex] 2D
					SetTexture 1 [_CrTex] 2D
					SetTexture 2 [_CbTex] 2D
					"ps_2_0
					; 7 ALU, 3 TEX
					dcl_2d s0
					dcl_2d s1
					dcl_2d s2
					def c1, 1.00000000, 0, 0, 0
					def c2, 1.16406250, 2.01757813, 0.00000000, -1.08105469
					def c3, 1.16406250, -0.39184570, -0.81298828, 0.52978516
					def c4, 1.16406250, 0.00000000, 1.59570313, -0.87060547
					dcl t0.xy
					dcl t1.xy
					texld r1, t1, s2
					texld r0, t0, s0
					mov_pp r1.x, r0.x
					texld r0, t1, s1
					mov_pp r1.y, r0.y
					mov_pp r1.w, c1.x
					dp4_pp r0.z, r1, c2
					dp4_pp r0.y, r1, c3
					dp4_pp r0.x, r1, c4
					mov_pp r0.w, c1.x
					mul_pp r0, r0, c0
					mov_pp oC0, r0
					"
				}

				SubProgram "gles " 
				{
					Keywords { }
					"!!GLES"
				}

				SubProgram "glesdesktop " 
				{
					Keywords { }
					"!!GLES"
				}
			}
		}
	}
}
