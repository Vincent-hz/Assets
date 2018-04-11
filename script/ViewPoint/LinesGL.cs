using UnityEngine;
using System.Collections;
public class LinesGL : MonoBehaviour
{
    public Shader shader;
    private static Material m;
    private GameObject g;
    private float speed = 100.0f;
    public Vector3[] lp;
    private Vector3[] sp;
    private Vector3 s;
    public GameObject JT;
    private GameObject tri_gameobject;
    void Start()
    {
       // shader = Shader.Find("Yogi/Occlusion");
        m = new Material(shader);
        g = new GameObject("g");
        m.hideFlags = HideFlags.HideAndDontSave;
        lp = new Vector3[0];
        sp = new Vector3[0];
    }

    void Update()
    {
        // processInput();

        Vector3 e;
        if (Input.GetMouseButtonDown(0))
        {
            s = GetNewPoint();
            tri_gameobject = Instantiate(JT, Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 2f)), Quaternion.identity) as GameObject;

        }

        if (Input.GetMouseButton(0))
        {
            e = GetNewPoint();
            lp = AddLine(lp, s, e, true);
           // go.transform.position = Input.mousePosition;
            tri_gameobject.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 2f));
            float distance=Vector3.Distance(tri_gameobject.transform.position,Camera.main.transform.position);
            tri_gameobject.transform.localScale = new Vector3(distance/40,distance/40,distance/40);
            Vector3 Vangle = lp[lp.Length - 1] - lp[lp.Length - 2];


            //if (Vangle.x > 0 || Vangle.y > 0 || Vangle.z > 0)
            //{
            //    tri_gameobject.transform.eulerAngles = new Vector3(Camera.main.transform.eulerAngles.x, Camera.main.transform.eulerAngles.y, Camera.main.transform.eulerAngles.z + 210);
            //}
            //else
            //{
            //    tri_gameobject.transform.eulerAngles = new Vector3(Camera.main.transform.eulerAngles.x, Camera.main.transform.eulerAngles.y, Camera.main.transform.eulerAngles.z + 210);
            //}
            //tri_gameobject.transform.LookAt(Camera.main.transform, Vector3.up);

        }
        if (Input.GetMouseButtonUp(0))
        {
            e = GetNewPoint();
            lp = AddLine(lp, s, e, false);

        }
    }

    Vector3[] AddLine(Vector3[] l, Vector3 s, Vector3 e, bool tmp)
    {
        int vl = l.Length;
        if (!tmp || vl == 0) l = resizeVertices(l, 2);
        else vl -= 2;

        l[vl] = s;
        l[vl + 1] = e;
        return l;
    }

    Vector3[] resizeVertices(Vector3[] ovs, int ns)
    {
        Vector3[] nvs = new Vector3[ovs.Length + ns];
        for (int i = 0; i < ovs.Length; i++) nvs[i] = ovs[i];
        return nvs;
    }

    Vector3 GetNewPoint()
    {
        return g.transform.InverseTransformPoint(
            Camera.main.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, 2f)
            )
        );
       // return Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z * 0.1f));
    }
    void OnPostRender()
    {
        m.SetPass(0);
        GL.PushMatrix();
        GL.MultMatrix(g.transform.transform.localToWorldMatrix);
        GL.Begin(GL.LINES);
        //GL.Color(new Color(1, 1, 1, 0.9f));
        m.SetColor("_MainColor", Color.red);
        for (int i = 0; i < lp.Length; i++)
        {
            GL.Vertex3(lp[i].x, lp[i].y, lp[i].z);
        }
        GL.End();
        GL.PopMatrix();
    }

}
