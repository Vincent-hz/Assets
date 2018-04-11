using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class RegionDelete : MonoBehaviour {

/*
Function：实现鼠标框选批量删除模型
Time：2017/10/16
*/

	public Color rectColor ;//框选背景默认颜色
	private Vector3 start;//记录鼠标起始位置
	public Material rectMat;//框选边框线的材质
	private bool drawRectangle;//是否开始画线标记
    public List<string> Members;//构件id记录容器
    public bool IsRegion;//是否触发框选功能
   // public List<string> BefMebers;
    public static RegionDelete instance;
    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        rectMat.hideFlags = HideFlags.HideAndDontSave;
        rectMat.shader.hideFlags = HideFlags.HideAndDontSave;
       // IsRegion = true;
        start = Vector3.zero;
        //rectMat = null;
        drawRectangle = false;
        // rectColor = Color.yellow;

    }

    // Update is called once per frame
    void Update()
    {
        if (IsRegion)
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())//屏蔽点击UI穿透
            {
                start = Input.mousePosition;//记录按下的位置
                drawRectangle = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {

                drawRectangle = false;
                checkSelet(start, Input.mousePosition);
            }
        }
    }
    //绘制框选的框
    void OnPostRender()
    {
        if (drawRectangle)
        {
            Vector3 end = Input.mousePosition;//鼠标当前位置
            GL.PushMatrix();//保存相机变换矩阵，将摄影视图矩阵和模型视图矩阵压入堆栈进行保存
            if (!rectMat)
            {
                return;
            }
            rectMat.SetPass(0);//渲染激活指定的pass
            GL.LoadPixelMatrix();//设置屏幕坐标进行绘图
            GL.Begin(GL.QUADS);//开始绘制矩形
            GL.Color(new Color(rectColor.r, rectColor.g, rectColor.b, 0.1f));//框选框的颜色和透明度
                                                                             //开始绘制顶点
            GL.Vertex3(start.x, start.y, 0);
            GL.Vertex3(end.x, start.y, 0);
            GL.Vertex3(end.x, end.y, 0);
            GL.Vertex3(start.x, end.y, 0);
            GL.End();
            GL.Begin(GL.LINES);//开始绘制线
            GL.Color(rectColor);
            GL.Vertex3(start.x, start.y, 0);
            GL.Vertex3(end.x, start.y, 0);
            GL.Vertex3(end.x, start.y, 0);
            GL.Vertex3(end.x, end.y, 0);
            GL.Vertex3(end.x, end.y, 0);
            GL.Vertex3(start.x, end.y, 0);
            GL.Vertex3(start.x, end.y, 0);
            GL.Vertex3(start.x, start.y, 0);
            GL.End();
            GL.PopMatrix();//恢复摄像机投影矩阵

        }
    }
    //框选中的构件进行处理操作
    void checkSelet(Vector3 start, Vector3 end)
    {
        Members = new List<string>();
        Vector3 p1 = Vector3.zero;
        Vector3 p2 = Vector3.zero;
        if (start.x > end.x)
        {
            p1.x = end.x;
            p2.x = start.x;
        }
        else {
            p1.x = start.x;
            p2.x = end.x;
        }
        if (start.y > end.y)
        {
            p1.y = end.y;
            p2.y = start.y;
        }
        else {
            p1.y = start.y;
            p2.y = end.y;
        }
        foreach (string ww in ComponentWebGL.instance.Dic.Keys)
        {
            Vector3 location = Camera.main.WorldToScreenPoint(ComponentWebGL.instance.Dic[ww].transform.position);//模型在主相机下模型世界坐标转换成屏幕坐标进行与框选框进行坐标处理
            if (location.x < p1.x || location.x > p2.x || location.y < p1.y || location.y > p2.y || location.z < Camera.main.nearClipPlane || location.z > Camera.main.farClipPlane)
            {

            }
            else {
                //必须得判断框选的构件是否被隐藏了否则会记录已经隐藏的而重复存储
                if (ComponentWebGL.instance.Dic[ww].activeInHierarchy)
                {
                    Members.Add(ww);
                    if (Comprehensive.instance.IsHide)
                    {
                        Comprehensive.instance.HiddenArtifacts_2();
                    }
                }
            }
        }

    }
}