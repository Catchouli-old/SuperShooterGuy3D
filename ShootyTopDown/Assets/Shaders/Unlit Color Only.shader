Shader "Unlit Color Only"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
    }
    
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }

        LOD 100

        Pass
        {
            Lighting Off
            ZWrite On
            Cull Back
            Blend SrcAlpha OneMinusSrcAlpha
            SetTexture[_]
            {
                constantColor [_Color]
                Combine constant
            }
        }
    }
}