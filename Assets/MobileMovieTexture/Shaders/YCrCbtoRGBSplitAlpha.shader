// Shader "Color Space/YCrCbtoRGB Split Alpha" 
// {
    // Properties 
    // {
        // _YTex ("Y (RGB)", 2D) = "white" {}
        // _CrTex ("Cr (RGB)", 2D) = "white" {}
        // _CbTex ("Cb (RGB)", 2D) = "white" {}
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
				// half2  uv : TEXCOORD0;
				// half2  uvAlpha : TEXCOORD1;
				// half2  uvCbCr : TEXCOORD2;
			// };

			// float4 _YTex_ST;
			// float4 _CbTex_ST;

			// v2f vert (appdata_base v)
			// {
				// v2f o;
				// o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				
				// float4 texcoordBottom = v.texcoord;
				// texcoordBottom.y = ( v.texcoord.y / 2.0f ) ;
				
				// float4 texcoordTop = v.texcoord;
				// texcoordTop.y = texcoordBottom.y + 0.5f;
				
				// o.uv = TRANSFORM_TEX (texcoordTop, _YTex);
				// o.uvAlpha = TRANSFORM_TEX (texcoordBottom, _YTex);
				// o.uvCbCr = TRANSFORM_TEX (texcoordTop, _CbTex);
				// return o;
			// }

			// fixed4 frag (v2f i) : COLOR
			// {
				// fixed4 yuvVec = fixed4(tex2D (_YTex, i.uv).r, tex2D (_CrTex, i.uvCbCr).g, tex2D (_CbTex, i.uvCbCr).b, 1.0);
				
				// fixed4 rgbVec;
				
				// rgbVec.x = dot(fixed4(1.1643828125, 0, 1.59602734375, -.87078515625), yuvVec);
				// rgbVec.y = dot(fixed4(1.1643828125, -.39176171875, -.81296875, .52959375), yuvVec);
				// rgbVec.z = dot(fixed4(1.1643828125, 2.017234375, 0, -1.081390625), yuvVec);
				// rgbVec.w = (tex2D(_YTex, i.uvAlpha).g - (16.0f/255.0f)) * (255.0f/(235.0f-16.0f));
								
				// return rgbVec;
			// }
			// ENDCG
		// }
	// }
// }

Shader "Color Space/YCrCbtoRGB Split Alpha" 
{
    Properties 
    {
        _YTex ("Y (RGB)", 2D) = "white" {}
        _CrTex ("Cr (RGB)", 2D) = "white" {}
        _CbTex ("Cb (RGB)", 2D) = "white" {}
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
				//   opengl - ALU: 11 to 11
				//   d3d9 - ALU: 11 to 11
				SubProgram "opengl " 
				{
					Keywords { }
					Bind "vertex" Vertex
					Bind "texcoord" TexCoord0
					Vector 5 [_YTex_ST]
					Vector 6 [_CbTex_ST]
					"!!ARBvp1.0
					# 11 ALU
					PARAM c[7] = { { 0.5 },
							state.matrix.mvp,
							program.local[5..6] };
					TEMP R0;
					MUL R0.w, vertex.texcoord[0].y, c[0].x;
					MOV R0.z, vertex.texcoord[0].x;
					MOV R0.x, vertex.texcoord[0];
					ADD R0.y, R0.w, c[0].x;
					MAD result.texcoord[0].xy, R0, c[5], c[5].zwzw;
					MAD result.texcoord[1].xy, R0.zwzw, c[5], c[5].zwzw;
					MAD result.texcoord[2].xy, R0, c[6], c[6].zwzw;
					DP4 result.position.w, vertex.position, c[4];
					DP4 result.position.z, vertex.position, c[3];
					DP4 result.position.y, vertex.position, c[2];
					DP4 result.position.x, vertex.position, c[1];
					END
					# 11 instructions, 1 R-regs
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
					; 11 ALU
					def c6, 0.50000000, 0, 0, 0
					dcl_position0 v0
					dcl_texcoord0 v1
					mul r0.w, v1.y, c6.x
					mov r0.z, v1.x
					mov r0.x, v1
					add r0.y, r0.w, c6.x
					mad oT0.xy, r0, c4, c4.zwzw
					mad oT1.xy, r0.zwzw, c4, c4.zwzw
					mad oT2.xy, r0, c5, c5.zwzw
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

					varying mediump vec2 xlv_TEXCOORD2;
					varying mediump vec2 xlv_TEXCOORD1;
					varying mediump vec2 xlv_TEXCOORD0;

					uniform highp vec4 _YTex_ST;
					uniform highp vec4 _CbTex_ST;
					attribute vec4 _glesMultiTexCoord0;
					attribute vec4 _glesVertex;
					void main ()
					{
						highp vec4 texcoordTop;
						highp vec4 texcoordBottom;
						mediump vec2 tmpvar_1;
						mediump vec2 tmpvar_2;
						mediump vec2 tmpvar_3;
						texcoordBottom = _glesMultiTexCoord0;
						texcoordBottom.y = (_glesMultiTexCoord0.y / 2.0);
						texcoordTop = _glesMultiTexCoord0;
						texcoordTop.y = (texcoordBottom.y + 0.5);
						highp vec2 tmpvar_4;
						tmpvar_4 = ((texcoordTop.xy * _YTex_ST.xy) + _YTex_ST.zw);
						tmpvar_1 = tmpvar_4;
						highp vec2 tmpvar_5;
						tmpvar_5 = ((texcoordBottom.xy * _YTex_ST.xy) + _YTex_ST.zw);
						tmpvar_2 = tmpvar_5;
						highp vec2 tmpvar_6;
						tmpvar_6 = ((texcoordTop.xy * _CbTex_ST.xy) + _CbTex_ST.zw);
						tmpvar_3 = tmpvar_6;
						gl_Position = (gl_ModelViewProjectionMatrix * _glesVertex);
						xlv_TEXCOORD0 = tmpvar_1;
						xlv_TEXCOORD1 = tmpvar_2;
						xlv_TEXCOORD2 = tmpvar_3;
					}



					#endif
					#ifdef FRAGMENT

					varying mediump vec2 xlv_TEXCOORD2;
					varying mediump vec2 xlv_TEXCOORD1;
					varying mediump vec2 xlv_TEXCOORD0;
					uniform sampler2D _YTex;
					uniform sampler2D _CrTex;
					uniform sampler2D _CbTex;
					void main ()
					{
						lowp vec4 rgbVec;
						lowp vec4 tmpvar_1;
						tmpvar_1.w = 1.0;
						tmpvar_1.x = texture2D (_YTex, xlv_TEXCOORD0).x;
						tmpvar_1.y = texture2D (_CrTex, xlv_TEXCOORD2).y;
						tmpvar_1.z = texture2D (_CbTex, xlv_TEXCOORD2).z;
						rgbVec.x = dot (vec4(1.16438, 0.0, 1.59603, -0.870785), tmpvar_1);
						rgbVec.y = dot (vec4(1.16438, -0.391762, -0.812969, 0.529594), tmpvar_1);
						rgbVec.z = dot (vec4(1.16438, 2.01723, 0.0, -1.08139), tmpvar_1);
						rgbVec.w = ((texture2D (_YTex, xlv_TEXCOORD1).y - 0.0627451) * 1.16438);
						gl_FragData[0] = rgbVec;
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

					varying mediump vec2 xlv_TEXCOORD2;
					varying mediump vec2 xlv_TEXCOORD1;
					varying mediump vec2 xlv_TEXCOORD0;

					uniform highp vec4 _YTex_ST;
					uniform highp vec4 _CbTex_ST;
					attribute vec4 _glesMultiTexCoord0;
					attribute vec4 _glesVertex;
					void main ()
					{
						highp vec4 texcoordTop;
						highp vec4 texcoordBottom;
						mediump vec2 tmpvar_1;
						mediump vec2 tmpvar_2;
						mediump vec2 tmpvar_3;
						texcoordBottom = _glesMultiTexCoord0;
						texcoordBottom.y = (_glesMultiTexCoord0.y / 2.0);
						texcoordTop = _glesMultiTexCoord0;
						texcoordTop.y = (texcoordBottom.y + 0.5);
						highp vec2 tmpvar_4;
						tmpvar_4 = ((texcoordTop.xy * _YTex_ST.xy) + _YTex_ST.zw);
						tmpvar_1 = tmpvar_4;
						highp vec2 tmpvar_5;
						tmpvar_5 = ((texcoordBottom.xy * _YTex_ST.xy) + _YTex_ST.zw);
						tmpvar_2 = tmpvar_5;
						highp vec2 tmpvar_6;
						tmpvar_6 = ((texcoordTop.xy * _CbTex_ST.xy) + _CbTex_ST.zw);
						tmpvar_3 = tmpvar_6;
						gl_Position = (gl_ModelViewProjectionMatrix * _glesVertex);
						xlv_TEXCOORD0 = tmpvar_1;
						xlv_TEXCOORD1 = tmpvar_2;
						xlv_TEXCOORD2 = tmpvar_3;
					}

					#endif
					#ifdef FRAGMENT

					varying mediump vec2 xlv_TEXCOORD2;
					varying mediump vec2 xlv_TEXCOORD1;
					varying mediump vec2 xlv_TEXCOORD0;
					uniform sampler2D _YTex;
					uniform sampler2D _CrTex;
					uniform sampler2D _CbTex;
					void main ()
					{
						lowp vec4 rgbVec;
						lowp vec4 tmpvar_1;
						tmpvar_1.w = 1.0;
						tmpvar_1.x = texture2D (_YTex, xlv_TEXCOORD0).x;
						tmpvar_1.y = texture2D (_CrTex, xlv_TEXCOORD2).y;
						tmpvar_1.z = texture2D (_CbTex, xlv_TEXCOORD2).z;
						rgbVec.x = dot (vec4(1.16438, 0.0, 1.59603, -0.870785), tmpvar_1);
						rgbVec.y = dot (vec4(1.16438, -0.391762, -0.812969, 0.529594), tmpvar_1);
						rgbVec.z = dot (vec4(1.16438, 2.01723, 0.0, -1.08139), tmpvar_1);
						rgbVec.w = ((texture2D (_YTex, xlv_TEXCOORD1).y - 0.0627451) * 1.16438);
						gl_FragData[0] = rgbVec;
					}

					#endif"
				}

				

			}
			
			Program "fp" 
			{
				// Fragment combos: 1
				//   opengl - ALU: 10 to 10, TEX: 4 to 4
				//   d3d9 - ALU: 7 to 7, TEX: 4 to 4
				SubProgram "opengl " 
				{
					Keywords { }
					SetTexture 0 [_YTex] 2D
					SetTexture 1 [_CrTex] 2D
					SetTexture 2 [_CbTex] 2D
					"!!ARBfp1.0
					# 10 ALU, 4 TEX
					PARAM c[4] = { { 0.062745102, 1.1643835, 1 },
							{ 1.1640625, 2.0175781, 0, -1.0810547 },
							{ 1.1640625, -0.3918457, -0.81298828, 0.52978516 },
							{ 1.1640625, 0, 1.5957031, -0.87060547 } };
					TEMP R0;
					TEMP R1;
					TEX R1.y, fragment.texcoord[1], texture[0], 2D;
					TEX R0.x, fragment.texcoord[0], texture[0], 2D;
					TEX R0.y, fragment.texcoord[2], texture[1], 2D;
					TEX R0.z, fragment.texcoord[2], texture[2], 2D;
					MOV R0.w, c[0].z;
					ADD R1.x, R1.y, -c[0];
					DP4 result.color.z, R0, c[1];
					DP4 result.color.y, R0, c[2];
					DP4 result.color.x, R0, c[3];
					MUL result.color.w, R1.x, c[0].y;
					END
					# 10 instructions, 2 R-regs
					"
				}

				SubProgram "d3d9 " 
				{
					Keywords { }
					SetTexture 0 [_YTex] 2D
					SetTexture 1 [_CrTex] 2D
					SetTexture 2 [_CbTex] 2D
					"ps_2_0
					; 7 ALU, 4 TEX
					dcl_2d s0
					dcl_2d s1
					dcl_2d s2
					def c0, 1.00000000, -0.06274510, 1.16438353, 0
					def c1, 1.16406250, 2.01757813, 0.00000000, -1.08105469
					def c2, 1.16406250, -0.39184570, -0.81298828, 0.52978516
					def c3, 1.16406250, 0.00000000, 1.59570313, -0.87060547
					dcl t0.xy
					dcl t1.xy
					dcl t2.xy
					texld r2, t1, s0
					;texld r1, t2, s2
					;texld r1, t0, s0
					;texld r1, t2, s1
					texld r1, t2, s2
					texld r0, t0, s0
					mov_pp r1.x, r0.x
					texld r0, t2, s1
					mov_pp r1.y, r0.y
					mov_pp r1.w, c0.x
					add r2.x, r2.y, c0.y
					dp4_pp r0.z, r1, c1
					dp4_pp r0.y, r1, c2
					dp4_pp r0.x, r1, c3
					mul r0.w, r2.x, c0.z
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




