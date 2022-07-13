// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Sprites/Emission"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		[HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
		[HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
		[PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
		[PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0
        [MaterialToggle]  useTexture("Use Texture", Float) = 0
		[HDR] _EmissionColor("Emission Color", Color) = (0, 0, 0)
		[HDR] _EmissionMap("Emission Map", 2D) = "black" {}

	}

		SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex SpriteVert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_instancing
			#pragma multi_compile_local _ PIXELSNAP_ON
			#pragma multi_compile _ ETC1_EXTERNAL_ALPHA
			#include "UnitySprites.cginc"

			sampler2D _EmissionMap;
			half3 _EmissionColor;
			float useTexture;

			half4 frag(v2f IN) : SV_Target
			{
				return SpriteFrag(IN) + half4(useTexture == 0 ? half3(1,1,1) * _EmissionColor.rgb : tex2D(_EmissionMap, IN.texcoord).rgb * _EmissionColor.rgb, 0.0);
			}
		ENDCG
		}
	}
}