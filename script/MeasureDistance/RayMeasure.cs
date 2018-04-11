using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
public class RayMeasure : MonoBehaviour {

   /*
    * 功能：该脚本主要用于测量线段，多边形面积测量计算
    * 时间：2017/11/20
    */
    public Camera camera;//渲染画线相机
    static Material lineMaterial;//画线材质
    [SerializeField]
    private bool IsLines;//是否直线测量
    public bool IsArea;//是否面积测量
    //public GameObject MeasurCam;
    public Texture texture;
    public Texture texture1;//显示背景底色图片
    public Texture texture2;//显示背景底色图片
    public Texture texture3;//显示背景底色图片
    public Texture fontTexture;//显示中文字图片
    public Texture meterTexture;//长度图片
    //public Texture meterTexture1;
    //public Texture meterTexture2;
    //public Texture meterTexture3;
    public Texture limitTexture;//角度图片
    //public Texture limitTexture1;
    //public Texture limitTexture2;
    //public Texture limitTexture3;
    public Texture areaTexture;//面积图片
    //public Texture areaTexture1;
    //public Texture areaTexture2;
    //public Texture areaTexture3;
    public Texture Bigtexture;//面积底色
    public Texture Bigtexture1;
    public Texture Bigtexture2;
    public Texture Bigtexture3;
    static void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            //透视shader
            Shader shader = Shader.Find("Yogi2/ImageEffect/Occlusion");
            //色泽平滑shader
           // Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            //修改线段颜色
            lineMaterial.SetColor("_MainColor", new Color(0.53f, 0.062f, 0.945f, 1f));
            //// 打开alpha混合通道
            //lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            //lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            //// 关掉隐面消除通道
            //lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            //// 关掉深度写入通道
            //lineMaterial.SetInt("_ZWrite", 0);
        }
    }
    
   public void OnRenderObject()
    {
        CreateLineMaterial();
        lineMaterial.SetPass(0);
        GL.PushMatrix();
        GL.Begin(GL.LINES);  
       // GL.Color(Color.yellow);
        if (!IsArea)
        {
            for (int vertexNumbers = 0; vertexNumbers < lv.Count - 1; vertexNumbers++)
            {
                // GL.Vertex3(lv[vertexNumbers].x, lv[vertexNumbers].y, lv[vertexNumbers].z);
                GL.Vertex(lv[vertexNumbers]);
                GL.Vertex(lv[vertexNumbers + 1]);
            }

        }
        else
        {
            if (lv.Count <= 4)
            {
                for (int vertexNumbers = 0; vertexNumbers < lv.Count; vertexNumbers++)
                {

                    GL.Vertex(lv[vertexNumbers]);


                }
            }
            else
            {
                for (int vertexNumbers = 0; vertexNumbers < lv.Count - 2; vertexNumbers++)
                {
                    GL.Vertex(lv[vertexNumbers]);
                }
                //虚线逻辑
                Vector3 dir = -(lv[lv.Count - 2] - lv[lv.Count - 1]).normalized;
                GL.Vertex(lv[lv.Count - 2]);
                Vector3 next = lv[lv.Count - 2] + dir * 0.5f;
                while (Vector3.Distance(next, lv[lv.Count - 2]) < Vector3.Distance(lv[lv.Count - 1], lv[lv.Count - 2]))
                {
                    GL.Vertex(next);
                    next = next + dir * 0.5f;
                }
                GL.Vertex(lv[lv.Count - 1]);
            }
        }
            GL.End();
        GL.PopMatrix();
    }
    
   private bool IsSave = false;
    //圆点的预制体
    public GameObject aim;
    //单位的预制体
    public GameObject longs;
    //圆点的父节点 ，所有物体在子节点下
    // public Transform aimPar;
    //单位的预制体 ，所有的单位在子节点下
    //public Transform unit;
    //GL 绘制的顶点数组  顺序是  0->1  2->3 4->5    取法 0 1 3 5 7 9
    //参考UI界面
    public List<Vector3> lv;//存储点
    private  List<GameObject> aims;
    //public Vector3 V3;
    public static RayMeasure instance;
    //存取圆点的位置便于拖拽比较刷新数据
    //public Dictionary<string,Vector3> PointsDic;
    private int flag = 3;
    GUIStyle fontStyle;
    void Awake() {
        instance = this;
        lv = new List<Vector3>();
        aims = new List<GameObject>();
       // Debug.Log(Mathf.PI);
    }
    void Start() {
       
       // V3 = Vector3.zero;
       // PointsDic = new Dictionary<string, Vector3>();
        fontStyle = new GUIStyle();
        fontStyle.normal.textColor = Color.white;   //设置字体颜色  
    }
    //默认每次进入可以进行直线测量
    void OnEnable() {
        //模型居中
        //this.transform.position = new Vector3(loadMoudle.instance.ModelCenter.x, loadMoudle.instance.ModelCenter.y + 20f, loadMoudle.instance.ModelCenter.z);//设置漫游设置点相机的位置
        //IsLines = true;
       // IsArea = true;
        
    }
    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())//屏蔽UI点击穿透
        {
            if (Input.GetMouseButtonDown(0) && IsArea)//绘制多边形
            {
                Ray ray = camera.ScreenPointToRay(Input.mousePosition);

                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    if (!hit.collider.CompareTag("point"))
                    {
                        //创建圆点
                        GameObject go = Instantiate(aim, new Vector3(hit.point.x, hit.point.y, hit.point.z), Quaternion.Euler(90, 0, 0)) as GameObject;
                        //存储起来为了删除操作
                        aims.Add(go);
                        go.name = aims.Count.ToString();
                        if (lv.Count >= 2)
                        {

                            if (IsSave)
                            {
                                lv.RemoveAt(lv.Count - 1);
                                lv.RemoveAt(lv.Count - 1);
                                //lv.Remove(PointsDic[(aims.Count).ToString()]);
                                //lv.Remove(PointsDic[(aims.Count - 1).ToString()]);
                            }
                            // print("补线");
                            //Debug.Log("lv个数："+lv.Count);
                            lv.Add(lv[lv.Count - 1]);
                           // lv.Add(PointsDic[(aims.Count - 2).ToString()]);
                            lv.Add(hit.point);
                            //lv.Add(PointsDic[aims.Count.ToString()]);
                            lv.Add(hit.point);
                            //lv.Add(PointsDic[aims.Count.ToString()]);
                            lv.Add(lv[0]);
                           // lv.Add(PointsDic["1"]);
                            IsSave = true;

                        }
                        else
                        {
                            lv.Add(hit.point);
                          //  lv.Add(PointsDic[aims.Count.ToString()]);
                        }

                    }
                }


            }
            else if (Input.GetMouseButtonDown(0) && IsLines)//绘制线段
            {
                Ray ray = camera.ScreenPointToRay(Input.mousePosition);

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    if (!hit.collider.CompareTag("point"))
                    {
                        GameObject go = Instantiate(aim, new Vector3(hit.point.x, hit.point.y, hit.point.z), Quaternion.Euler(90, 0, 0)) as GameObject;
                        //存储起来为了删除操作
                        aims.Add(go);
                        go.name = aims.Count.ToString();
                        if (lv.Count >= 2)
                        {

                            lv.Add(lv[lv.Count - 1]);
                            lv.Add(hit.point);
                        }
                        else
                        {
                            lv.Add(hit.point);
                        }
                    }
                }
            }
        } 
    }
    
    void OnGUI() {
         
       // fontStyle.fontSize =26;       //字体大小  
       
      
        if (lv.Count >= 2)
        {
            for (int vertexNumbers = 0; vertexNumbers < lv.Count-1; vertexNumbers=vertexNumbers+2)
            {
                
                Vector3 s = new Vector3((lv[vertexNumbers].x + lv[vertexNumbers + 1].x) / 2, (lv[vertexNumbers].y + lv[vertexNumbers + 1].y) / 2,(lv[vertexNumbers].z+ lv[vertexNumbers + 1].z) / 2 );

                Vector3 a = camera.WorldToScreenPoint(s);
                //长度显示 
                GUI.Label(new Rect(a.x, Screen.height - a.y - 5, 70,29), texture);
                GUI.Label(new Rect(a.x+32, Screen.height - a.y-2, 30, 19), meterTexture);
               // GUI.Label(new Rect(a.x, Screen.height - a.y, 60, 40), "<color=yellow>" + Vector3.Distance(lv[vertexNumbers], lv[vertexNumbers + 1]).ToString("f3") + "</color>" + "<color=gay>" + " m" + "</color>");
                GUI.Label(new Rect(a.x, Screen.height - a.y, 60, 40), Vector3.Distance(lv[vertexNumbers], lv[vertexNumbers + 1]).ToString("f2"), fontStyle);
            }
        }
        if (lv.Count > 2&&IsArea)
        {
            //面积显示
            //if (GUI.Button(new Rect(Screen.width * 0.05f + 5, Screen.height * 0.02f -5, 150, 50),""))
            //{
            //    flag++;
            //    if (flag % 3 == 0)
            //    {
            //        lineMaterial.SetColor("_MainColor", Color.white);
            //       // camera.backgroundColor = Color.blue;
            //        fontStyle.normal.textColor = Color.white;
            //        texture = texture1;
            //        limitTexture = limitTexture1;
            //        meterTexture = meterTexture1;
            //    }
            //    else if (flag % 3 == 1)
            //    {
            //        lineMaterial.SetColor("_MainColor", Color.black);
            //        //camera.backgroundColor = Color.white;
            //        fontStyle.normal.textColor = Color.black;
            //        texture = texture2;
            //        limitTexture = limitTexture2;
            //        meterTexture = meterTexture2;
            //    }
            //    else if (flag % 3 == 2)
            //    {
            //        lineMaterial.SetColor("_MainColor", Color.yellow);
            //       // camera.backgroundColor = Color.red;
            //        fontStyle.normal.textColor= Color.yellow;
            //        texture = texture3;
            //        limitTexture = limitTexture3;
            //        meterTexture = meterTexture3;
            //    }
            //}
            GUI.Label(new Rect(Screen.width * 0.05f, Screen.height * 0.02f, 145, 50), Bigtexture);//底色
            GUI.Label(new Rect(Screen.width * 0.05f+95, Screen.height * 0.02f+15, 50, 50), areaTexture);//单位
            GUI.Label(new Rect(Screen.width * 0.05f + 5, Screen.height * 0.02f+5, 150, 30),"<size=26>"+ ComputePolygonArea_2(lv).ToString("f3")+"</size>",fontStyle);//数据
            
        }
        //两根线以上才能有夹角 4个点以上
        if (lv.Count >= 4) {
            for (int vertexNumbers = 0; vertexNumbers < lv.Count-3; vertexNumbers = vertexNumbers + 2)
            {
                //向量计算 两点确定向量，两三维向量转成二维向量求夹角
               // Vector2 vec1 = new Vector2(lv[vertexNumbers].x, lv[vertexNumbers].y) - new Vector2(lv[vertexNumbers + 1].x, lv[vertexNumbers + 1].y);
              //  Vector2 vec2 = new Vector2(lv[vertexNumbers + 2].x, lv[vertexNumbers + 2].y) - new Vector2(lv[vertexNumbers + 3].x, lv[vertexNumbers + 3].y);
                Vector3 vec1 = lv[vertexNumbers] - lv[vertexNumbers + 1];
                Vector3 vec2 = lv[vertexNumbers + 2] - lv[vertexNumbers + 3];
                Vector3 pos=camera.WorldToScreenPoint(lv[vertexNumbers+1]);
                //Vector3 c = Vector3.Cross(vec1, vec2);  
                // Debug.Log(vec1);
                // 通过反正弦函数获取向量 a、b 夹角（默认为弧度）  
                //float radians = Mathf.Asin(Vector3.Distance(Vector3.zero, Vector3.Cross(vec1.normalized, vec2.normalized)));  
                //向量点积
               // float vec12 = Vector3.Dot(vec1,vec2);
                //角度计算
                float angle =180-Mathf.Acos(Vector3.Dot(vec1.normalized,vec2.normalized))*Mathf.Rad2Deg;
               // float angle =180-Vector3.Angle(vec1,vec2);
                //角度显示
                GUI.Label(new Rect(pos.x - 21, Screen.height - pos.y+10, 95, 30), texture);
                GUI.Label(new Rect(pos.x+12, Screen.height - pos.y + 15, 25, 18),limitTexture);
                GUI.Label(new Rect(pos.x-21, Screen.height - pos.y+18, 50, 30),(angle).ToString("f1"),fontStyle);
                
            }
            //显示多边形缝合的虚线与第一根线的夹角
            if (IsArea&&lv.Count>=6) {

                Vector3 po1 = camera.WorldToScreenPoint(lv[0]);
                Vector2 vec1 = new Vector2(lv[0].x, lv[0].z) - new Vector2((lv[1]).x, lv[1].z);
                Vector2 vec2 = new Vector2(lv[lv.Count - 2].x, lv[lv.Count - 2].z) - new Vector2(lv[lv.Count - 1].x, lv[lv.Count - 1].z);
                //float vec12 = Vector3.Dot(vec1, vec2);
                float angle = Mathf.Acos(Vector3.Dot(vec1.normalized, vec2.normalized)) * Mathf.Rad2Deg;
                //float angle = Vector3.Angle(vec1,vec2);
                //float angle = Vector3.Angle((new Vector2(lv[0].x,lv[0].y)-new Vector2 ((lv[1]).x,lv[1].y)),(new Vector2(lv[lv.Count-2].x,lv[lv.Count-2].y)-new Vector2(lv[lv.Count-1].x,lv[lv.Count-1].y)));
                GUI.Label(new Rect(po1.x - 21, Screen.height - po1.y+2, 95, 30), texture);
                GUI.Label(new Rect(po1.x+12, Screen.height - po1.y +10, 25, 18),limitTexture);
                GUI.Label(new Rect(po1.x -21, Screen.height - po1.y+10, 50, 30),(180-angle).ToString("f1"),fontStyle);
                }
        }
    }
    //变色
    public void TurnColors() {

        flag++;
        if (flag % 3 == 0)
        {
            lineMaterial.SetColor("_MainColor", Color.green);
            // camera.backgroundColor = Color.blue;
          //  fontStyle.normal.textColor = Color.white;
            texture = texture1;
            Bigtexture = Bigtexture1;
            //limitTexture = limitTexture1;
            //meterTexture = meterTexture1;
        }
        else if (flag % 3 == 1)
        {
            lineMaterial.SetColor("_MainColor", new Color(0.098f,0.607f,1f,1f));
            //camera.backgroundColor = Color.white;
           // fontStyle.normal.textColor = Color.white;
            texture = texture2;
            Bigtexture = Bigtexture2;
            //limitTexture = limitTexture2;
            //meterTexture = meterTexture2;
        }
        else if (flag % 3 == 2)
        {
            lineMaterial.SetColor("_MainColor", new Color(0.53f,0.062f,0.945f,1f));
            // camera.backgroundColor = Color.red;
           // fontStyle.normal.textColor = Color.white;
            texture = texture3;
            Bigtexture = Bigtexture3;
            //limitTexture = limitTexture3;
            //meterTexture = meterTexture3;
        }
    
    }

    //进入测量
    public void Measure() {
        
           // MeasurCam.SetActive(true);
           // Comprehensive.instance.mianCam.SetActive(false);
            IsArea = true;
            RegionDelete.instance.IsRegion = false;//屏蔽框选
           // Application.ExternalEval("HideMeasuUI()");//隐藏HTML UI
        
    }
    //退出测量
    public void BackMeasure() {
       // this.gameObject.SetActive(false);
        //Comprehensive.instance.mianCam.SetActive(true);
        IsArea = false;
        RegionDelete.instance.IsRegion = true;
        ClearLines();
      //  Application.ExternalEval("ShowMeasuUI()");//显示HTML UI
    }
    //多点测量
    public void LinsDistance_2() {
        if (!IsLines)
        {

            IsLines = true;
            IsArea = false;
            ClearLines();

        }
    }
    //面积测量
    public void AreaMeasure_2() {
       
        if (!IsArea)
        {
            IsLines = false;
            IsArea = true;
            ClearLines();
        }
    }
    //清除线段和测量数据
    public void ClearLines() {

        IsSave = false;
        
        for (int vertexNumbers = 0; vertexNumbers < aims.Count; vertexNumbers++) {
            GameObject.Destroy(aims[vertexNumbers]);

        }
        lv.Clear();
        //lv = new List<Vector3>();
        aims.Clear();
    }
    //计算任意多边形的面积，顶点按照顺时针或者逆时针方向排列
    double ComputePolygonArea(List<Vector3> points)
    {
        int point_num = points.Count;
        if (point_num < 3) return 0.0;
        float area = points[0].z * (points[point_num - 1].x - points[1].x);
        for (int vertexNumbers = 1; vertexNumbers < point_num; ++vertexNumbers)
        area += points[vertexNumbers].z * (points[vertexNumbers - 1].x - points[(vertexNumbers + 1) % point_num].x);
        return Mathf.Abs(area / 2.0f);
    }
    double ComputePolygonArea_2(List<Vector3> points)
    {
        int point_num = points.Count;
        if (point_num < 3) return 0.0;
        float area = points[0].z * (points[point_num - 1].x - points[1].x) + points[0].z * (points[point_num - 1].y - points[1].y) + points[0].x * (points[point_num - 1].y - points[1].y);
        for (int vertexNumbers = 1; vertexNumbers < point_num; ++vertexNumbers)
            area += points[vertexNumbers].z * (points[vertexNumbers - 1].x - points[(vertexNumbers + 1) % point_num].x) + points[vertexNumbers].z * (points[vertexNumbers - 1].y - points[(vertexNumbers + 1) % point_num].y) + points[vertexNumbers].x * (points[vertexNumbers - 1].y - points[(vertexNumbers + 1) % point_num].y);
        return Mathf.Abs(area / 2.0f);
    }
   //三点确定一平面
    float a,b,c,d;
    Vector3 normalVec;
    void GetPanel() {
        a = ((lv[1].y - lv[0].y) * (lv[3].z - lv[0].z) - (lv[1].z - lv[0].z) * (lv[3].y - lv[0].y));

        b = ((lv[1].z - lv[0].z) * (lv[3].x - lv[0].x) - (lv[1].x - lv[0].x) * (lv[3].z - lv[0].z));

        c = ((lv[1].x - lv[0].x) * (lv[3].y - lv[0].y) - (lv[1].y - lv[0].y) * (lv[3].x - lv[0].x));

        d = (0 - (a * lv[0].x + b * lv[0].y + c * lv[0].z));
        Vector3 ve1 = lv[1] - lv[0];
        Vector3 ve2 = lv[3] - lv[1];

        normalVec = Vector3.Cross(ve1, ve2).normalized;
    
    }
    //  判断点是不是在平面上
    double ComputePolygonArea_3D()
    {
        double area = 0;
        float an, ax, ay, az;
        int coord;
        int ii, jj, kk;
        for (int i = 5; i <lv.Count; i+=2)
        {
            double gg = a * lv[i].x + b * lv[i].y + c * lv[i].z + d;
            if (gg > -1.2f && gg < 1.2f)
            {
                ax = (normalVec.x > 0 ? normalVec.x : -normalVec.x);
                ay = (normalVec.y > 0 ? normalVec.y : -normalVec.y);
                az = (normalVec.z > 0 ? normalVec.z : -normalVec.z);
                coord = 3;
                if (ax > ay)
                {
                    if (ax > az)
                        coord = 1;
                }
                else if (ay > az)
                {
                    coord = 2;
                }
                for (ii = 1, jj = 2, kk = 0; ii <= i; ++ii, ++jj, ++kk)
                {
                    switch (coord)
                    {

                        case 1:

                            area += (lv[ii].y * (lv[jj].z - lv[kk].z));

                            continue;

                        case 2:

                            area += (lv[ii].x * (lv[jj].z - lv[kk].z));

                            continue;

                        case 3:

                            area += (lv[ii].x * (lv[jj].y - lv[kk].y));

                            continue;

                    }
                    an = Mathf.Sqrt(ax * ax + ay * ay + az * az);
                    switch (coord)
                    {
                        case 1:
                            area *= (an / (2 * ax));
                            break;
                        case 2:
                            area *= (an / (2 * ay));
                            break;
                        case 3:
                            area *= (an / (2 * az));
                            break;
                    }
                    return area;
                }
            }

        }
        return 0;
    }
    
}
