using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class GridLine : MonoBehaviour {
    public Color color = Color.red;
    public Material mat;
    public bool IsLine = false;
    List<Vector3> Vecs;
    void Start() {
        mat.hideFlags = HideFlags.HideAndDontSave;
        mat.shader.hideFlags = HideFlags.HideAndDontSave;
        Vecs = new List<Vector3>();
    }

    void Update() {
        if (Input.GetMouseButtonUp(0)) {
            
            Debug.Log("鼠标位置："+Input.mousePosition);
            Vecs.Add(Input.mousePosition);
            IsLine = true;
        }
        
    }
    void OnPostRender()
    {
        //Vector3 pos = Input.mousePosition;
        
            GL.PushMatrix();
            mat.SetPass(0);
            GL.LoadOrtho();
            GL.Begin(GL.QUADS);
        //foreach (Vector3 zz in Vecs)
        //{
        //    GL.Vertex(zz);
        //    Debug.Log("zz:" + zz);
        //}
        GL.Vertex3(0,0,0);
        GL.Vertex3(1,2,0);
        GL.Vertex3(0, 6, 0);
        GL.Vertex3(1, 6, 0);
        GL.End();
            GL.PopMatrix();
           
        
    }
}
