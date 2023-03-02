using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class VTXPainterUtility
{
    public static Mesh GetMesh(GameObject gameObject)
    {
        Mesh currMesh = null;
        if (gameObject)
        {
            MeshFilter currFilter = gameObject.GetComponent<MeshFilter>();
            SkinnedMeshRenderer currSkinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

            if(currFilter && !currSkinnedMeshRenderer)
            {
                currMesh = currFilter.sharedMesh;
            }
            
            if(!currFilter && currSkinnedMeshRenderer)
            {
                currMesh = currSkinnedMeshRenderer.sharedMesh;
            }
        }

        return currMesh;
    }

    //Fallof Methods
    public static float LinearFalloff(float distance ,float brushRadius)
    {
        return Mathf.Clamp01(1-distance/brushRadius);
    }

    //Lerp Methods
    public static Color VtxColorLerp(Color colorA, Color colorB, float value)
    {
        if(value > 1)
        {
            return colorB;
        }
        else if(value < 0)
        {
            return colorA;
        }

        return new Color(colorA.r + (colorB.r - colorA.r) * value,
                         colorA.g + (colorB.g - colorA.g) * value,
                         colorA.b + (colorB.b - colorA.b) * value,
                         colorA.a + (colorB.a - colorA.a) * value);
           
    }
}
