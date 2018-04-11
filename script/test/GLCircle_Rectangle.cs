using UnityEngine;
using System.Collections;

public class GLCircle_Rectangle : MonoBehaviour
{
    //public Camera camera;
    private static Material lineMaterial;
    static void CreateLineMaterial()
    {
        if (!lineMaterial)
        {

            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }
    void OnRenderObject()
    {
        CreateLineMaterial();
        GL.LoadOrtho();//设置绘制2D图像          



        //画圆  
        GL.Begin(GL.LINES);//如果是填充圆则用GL.Begin(GL.QUADS);  
        GL.Color(Color.red);
        //float circle_r_x = 0.1f;
        //float circle_r_y = circle_r_x * Screen.width / Screen.height;  
        //if (RayMeasure.instance.lv.Count >= 4)
        //{
        //    for (int vertexNumbers = 0; vertexNumbers < RayMeasure.instance.lv.Count - 3; vertexNumbers = vertexNumbers + 2)
        //    {
        //        Vector3 vec1 = RayMeasure.instance.lv[vertexNumbers] - RayMeasure.instance.lv[vertexNumbers + 1];
        //        Vector3 vec2 = RayMeasure.instance.lv[vertexNumbers + 2] - RayMeasure.instance.lv[vertexNumbers + 3];
        //        Vector3 pos = GetComponent<Camera>().WorldToScreenPoint(RayMeasure.instance.lv[vertexNumbers + 1]);
        //        float angle = Vector3.Angle(vec1, vec2);
        //        for (int n = 0; n < 666; ++n)
        //        {
        //            GL.Vertex(new Vector2(RayMeasure.instance.lv[vertexNumbers + 1].x + circle_r_x * Mathf.Cos((2 * Mathf.PI) / 666 * n), RayMeasure.instance.lv[vertexNumbers + 1].y + circle_r_y * Mathf.Sin((2 * Mathf.PI) / 666 * n)));
        //            //GL.Vertex(new Vector2(lv[vertexNumbers + 1].x + circle_r_x * Mathf.Cos(angle / (2 * Mathf.PI) / 666 *vertexNumbers), lv[vertexNumbers + 1].y + circle_r_y * Mathf.Sin(angle / (2 * Mathf.PI) /666*vertexNumbers)));
        //        }

        //    }
        //    //显示多边形缝合的虚线与第一根线的夹角
        //    if (RayMeasure.instance.IsArea && RayMeasure.instance.lv.Count >= 6)
        //    {

        //        Vector3 po1 = GetComponent<Camera>().WorldToScreenPoint(RayMeasure.instance.lv[0]);
        //        float angle = Vector3.Angle((RayMeasure.instance.lv[0] - RayMeasure.instance.lv[1]), (RayMeasure.instance.lv[RayMeasure.instance.lv.Count - 2]) - RayMeasure.instance.lv[RayMeasure.instance.lv.Count - 1]);

        //    }
        //}
       // Vector2 Circle_center_point = new Vector2(0.75f, 0.75f);
        float Circle_r_x = 0.02f;
        float Circle_r_y = Circle_r_x * Screen.width / Screen.height;
        if (RayMeasure.instance.lv.Count >= 4)
        {
            for (int kk = 0; kk < RayMeasure.instance.lv.Count-3; kk=kk+2)
            {
                Vector3 ve1 = RayMeasure.instance.lv[kk] - RayMeasure.instance.lv[kk + 1];
                Vector3 ve2 = RayMeasure.instance.lv[kk + 2] - RayMeasure.instance.lv[kk + 3];
                float angle = Vector3.Angle(ve1, ve2)/360;
               
                Vector3 pos = GetComponent<Camera>().WorldToScreenPoint(RayMeasure.instance.lv[kk+1]);
                int n = 1000;//实质是绘制一个正1000边形  
                for (int vertexNumbers = 0; vertexNumbers < n; ++vertexNumbers)//割圆术画圆  
                {
                    // Debug.Log("我在画圆");
                    GL.Vertex(new Vector2(pos.x / Screen.width -Circle_r_x * Mathf.Cos(0.5f*Mathf.PI/ n * vertexNumbers), pos.y / Screen.height - Circle_r_y * Mathf.Sin(0.5f*Mathf.PI / n * vertexNumbers)));
                }
            }
        }
        if (RayMeasure.instance.IsArea && RayMeasure.instance.lv.Count >= 6) {
            Vector3 po1 = GetComponent<Camera>().WorldToScreenPoint(RayMeasure.instance.lv[0]);
            float angle = Vector3.Angle((RayMeasure.instance.lv[0] - RayMeasure.instance.lv[1]), (RayMeasure.instance.lv[RayMeasure.instance.lv.Count - 2]) - RayMeasure.instance.lv[RayMeasure.instance.lv.Count - 1]);
            int n = 1000;
            for (int vertexNumbers = 0; vertexNumbers < n; ++vertexNumbers)//割圆术画圆  
            {
                // Debug.Log("我在画圆");
                GL.Vertex(new Vector2(po1.x / Screen.width + Circle_r_x * Mathf.Cos(2 * Mathf.PI / n * vertexNumbers), po1.y / Screen.height + Circle_r_y * Mathf.Sin(2 * Mathf.PI / n * vertexNumbers)));
            }
        }
        GL.End();
    }
   
}