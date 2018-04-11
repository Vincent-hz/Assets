using UnityEngine;
using System.Collections;


public class joint
{
    public Vector3 org;
    public Vector3 end;
}
public class example : MonoBehaviour
{
    Event e;
    private Vector3 orgPos;
    private Vector3 endPos;
    private Vector3 firstPost;
    private bool canDrawLines = false;
    ArrayList posAL;
    ArrayList temppos;
    public Material lineMaterial;
    private int index = 1;
    void Start()
    {
        temppos = new ArrayList();
        posAL = new ArrayList();
    }
    void Update()
    {
        //if (Input.GetMouseButton(0))
        //{
        //    canDrawLines = true;
        //}
        if (e.type != null)
        {
            if (e.type == EventType.MouseDown)
            {
                if (index == 1)
                {

                    orgPos = Input.mousePosition;
                    firstPost = orgPos;
                    index++;
                }
                endPos = Input.mousePosition;

            }
            if (Input.GetKey(KeyCode.LeftShift)) {
                GLDrawLine(endPos,firstPost);
            }
              
                if (e.type == EventType.MouseDrag&& Input.GetMouseButtonDown(0))
        {
            
            endPos = Input.mousePosition;
                
                //鼠标位置信息存入数组  
                temppos.Add(Input.mousePosition);
                GLDrawLine(orgPos, endPos);
                orgPos = Input.mousePosition;
                //print(temppos.Count);
                Debug.Log(temppos.Count);
            }
            if (e.type == EventType.MouseUp)
            {
                 orgPos=Input.mousePosition;    
                //endPos = Input.mousePosition;
            }
        }

    }

    void GLDrawLine(Vector3 beg, Vector3 end)
    {
        //if (!canDrawLines)
        //    return;
        GL.PushMatrix();
        GL.LoadOrtho();

        beg.x = beg.x / Screen.width;
        end.x = end.x / Screen.width;
        beg.y = beg.y / Screen.height;
        end.y = end.y / Screen.height;
        joint tmpJoint = new joint();
        tmpJoint.org = beg;
        tmpJoint.end = end;

        posAL.Add(tmpJoint);

        lineMaterial.SetPass(0);
        GL.Begin(GL.LINES);
        GL.Color(new Color(1f, 0f, 0f, 1f));
        for (int vertexNumbers = 0; vertexNumbers < posAL.Count; vertexNumbers++)
        {
            joint tj = (joint)posAL[vertexNumbers];
            Vector3 tmpBeg = tj.org;
            Vector3 tmpEnd = tj.end;
            GL.Vertex3(tmpBeg.x, tmpBeg.y, tmpBeg.z);
            GL.Vertex3(tmpEnd.x, tmpEnd.y, tmpEnd.z);
        }
        GL.End();
        GL.PopMatrix();
    }


    void OnGUI()
    {
        e = Event.current;

        if (GUI.Button(new Rect(150, 0, 100, 50), "清除"))
        {
            ClearLines();
        }
    }
    void ClearLines()
    {
        canDrawLines = false;
        posAL.Clear();
        temppos.Clear();
        index = 0;
    }

    void OnPostRender()
    {
        GLDrawLine(orgPos, endPos);
    }
}