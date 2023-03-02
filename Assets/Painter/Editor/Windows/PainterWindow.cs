using UnityEditor;
using UnityEngine;

public class PainterWindow : EditorWindow
{
    #region Variables
    public bool allowPainting = false;
    public bool changingBrushValue = false;
    GUIStyle boxStyle;

    public Vector2 mousPos = Vector2.zero;
    public RaycastHit curHit;

    // Brush
    public float brushSize = 1.0f;
    public float brushOpacity = 1.0f;
    public float brushFallOff = 1.0f;

    public GameObject currGameObject;
    public Mesh currMesh;
    public GameObject lastGameObject;

    //Color
    public Color ForegroundColor;


    #endregion

    #region Main Method
    public static void LaunchVertexPainter()
    {
        var win = EditorWindow.GetWindow<PainterWindow>(false, "Painter", true);
        win.GenerateStyles();
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui -= this.OnSceneGUI;
        SceneView.duringSceneGui += this.OnSceneGUI;
    }

    private void OnDestroy()
    {
        SceneView.duringSceneGui -= this.OnSceneGUI;
    }

    private void Update()
    {
        //Debug.Log("We are updating in the Update Method!");
        if (allowPainting)
        {
            Selection.activeGameObject = null;

          
        }
        else
        {
            //create methods for this :) 
            currGameObject = null;
            currMesh = null;
            lastGameObject = null;
        }

        if(curHit.transform != null) // object detected
        {
            if (curHit.transform.gameObject != lastGameObject)
            {
                currGameObject = curHit.transform.gameObject;
                currMesh = VTXPainterUtility.GetMesh(currGameObject);
                lastGameObject = currGameObject;

                if (currGameObject != null && currMesh != null)
                {
                    Debug.Log(currGameObject.name + " : " + currMesh.name);
                }
            }
        }
    }
    #endregion

    #region GUI Methods
    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Box("Title", boxStyle, GUILayout.Height(60), GUILayout.ExpandWidth(true));
        GUILayout.EndHorizontal();

        GUILayout.BeginVertical(boxStyle); //default : GUI.skin.box
        
        GUILayout.Space(10);

        allowPainting = GUILayout.Toggle(allowPainting, "Allow Painting", GUI.skin.button, GUILayout.Height(60));

        //GUILayout.BeginHorizontal();
        //GUILayout.Button("Button", GUILayout.Height(30));
        //GUILayout.Button("Button", GUILayout.Height(30));
        //GUILayout.EndHorizontal();

        if(GUILayout.Button("Button", GUILayout.Height(30)))
        {
            GenerateStyles();
        }

        GUILayout.Space(10);
        ForegroundColor = EditorGUILayout.ColorField("Foreground Color:", ForegroundColor);

        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();

        GUILayout.Box("Title", boxStyle, GUILayout.Height(60), GUILayout.ExpandWidth(true));

        //Updates and Repaints the UI in real time
        Repaint();
    }
    void OnSceneGUI(SceneView sceneView)
    {
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(10, 10, 200, 150));
        GUILayout.Button("Buttton", GUILayout.Height(25));
        GUILayout.Button("Buttton", GUILayout.Height(25));
        GUILayout.EndArea();
        Handles.EndGUI();

        //Debug.Log("Bingoo !");
        if (allowPainting)
        {
            if (curHit.transform != null)
            {
                Handles.color = new Color(1f, 0f, 0f, brushOpacity);
                Handles.DrawSolidDisc(curHit.point, curHit.normal, brushSize);

                Handles.color = Color.red;
                Handles.DrawWireDisc(curHit.point, curHit.normal, brushSize);
                Handles.DrawWireDisc(curHit.point, curHit.normal, brushFallOff);
            }

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            Ray ray = HandleUtility.GUIPointToWorldRay(mousPos);
            if (!changingBrushValue)
            {
                if (Physics.Raycast(ray, out curHit, 500f))
                {
                    //Begin Vertex Painting
                    PaintVertexColor();
                }
            }
            
        }
        

        //Get user inputs
        ProcessInputs();

        //Update and repaint our scene view GUI
        sceneView.Repaint();
    }

    #endregion


    #region TempPainter Method
    void PaintVertexColor()
    {
        if (currMesh)
        {
            Vector3[] vertices = currMesh.vertices;
            Color[] colors = new Color[0];

            if(currMesh.colors.Length > 0)
            {
                colors = currMesh.colors;
            }
            else
            {
                colors = new Color[vertices.Length];
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertPos = currGameObject.transform.TransformPoint(vertices[i]);
                float sqrMag = (vertPos - curHit.point).sqrMagnitude;
                if(sqrMag > brushSize)
                {
                    continue;
                }

                float fallof = VTXPainterUtility.LinearFalloff(sqrMag, brushSize);
                fallof = Mathf.Pow(fallof, brushFallOff * 3f) * brushOpacity;
                colors[i] = VTXPainterUtility.VtxColorLerp(colors[i], ForegroundColor, fallof);
            }
            

            currMesh.colors = colors;
        }
        else
        {
            Debug.Log("Can't paint vertex color because there is now mesh available...");
        }
    }
    #endregion


    #region Utiliy Methods

    void ProcessInputs()
    {
        Event e = Event.current;

        mousPos = e.mousePosition;

        //Key Pressed down
        if(e.type == EventType.KeyDown)
        {
            if(e.isKey && e.keyCode == KeyCode.T)
            {
                //Debug.Log("T");
                allowPainting = !allowPainting;
                if (!allowPainting)
                {
                    Tools.current = Tool.View;
                }
                else
                {
                    Tools.current = Tool.None;
                }
            }
        }

        if (e.type == EventType.MouseUp)
        {
            changingBrushValue = false;
        }

        //Mous button
        if(e.type == EventType.MouseDown)
        {
            if(e.button == 0)
            {
                //Left
            }
            else if(e.button == 1)
            {
                //Right
            }else if(e.button == 2)
            {
                //Middle
            }
        }

        // Brush key combinations
        if (allowPainting)
        {
            if(e.type == EventType.MouseDrag && e.control && e.button == 0  && !e.shift)
            {
                brushSize += e.delta.x * 0.005f;
                brushSize = Mathf.Clamp(brushSize, 0.1f, 10f); 
                if(brushFallOff > brushSize)
                {
                    brushFallOff = brushSize;
                }
                changingBrushValue = true;
            }

            if (e.type == EventType.MouseDrag && !e.control && e.button == 0 && e.shift)
            {
                brushOpacity += e.delta.x * 0.005f;
                brushOpacity = Mathf.Clamp01(brushOpacity);
                changingBrushValue = true;
            }

            if (e.type == EventType.MouseDrag && e.control && e.button == 0 && e.shift)
            {
                brushFallOff += e.delta.x * 0.005f;
                brushFallOff = Mathf.Clamp(brushFallOff, 0.1f, brushSize);
                changingBrushValue = true;
            }
        }
    }

    void GenerateStyles()
    {
        boxStyle = new GUIStyle();
        boxStyle.normal.background = (Texture2D) Resources.Load("Textures/default_box_bg");
        boxStyle.normal.textColor = Color.white;
        boxStyle.border = new RectOffset(3, 3, 3, 3);
        boxStyle.margin = new RectOffset(2, 2, 2, 2);
        boxStyle.fontSize = 25;
        boxStyle.fontStyle = FontStyle.Bold;
        boxStyle.alignment = TextAnchor.MiddleCenter;

    }
    #endregion
}
