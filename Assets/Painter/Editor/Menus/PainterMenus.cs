using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Menus : MonoBehaviour
{
    [MenuItem("Painter/Tools/Vertex Painter", false, 10)]
    public static void LaunchPainter()
    {
        PainterWindow.LaunchVertexPainter();
    }
}
