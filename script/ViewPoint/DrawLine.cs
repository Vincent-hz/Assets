using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine.EventSystems;
public class DrawLine : MonoBehaviour {
    /// <summary>  
    ///功能:鼠标涂鸦信息上传服务端，服务端下载恢复视点涂鸦信息
    ///时间:2017/10/24
    ///Author:胡哲
    /// </summary>  
    private GameObject clone;//挂在线段组件物体
    private LineRenderer line;
    public bool IsDrawLines;//是否涂鸦标识符
    private int vertexNumbers;//绘制时顶点计数
    public int lineNumbers;//当前绘制的线段条数
    public GameObject linRenPerfabs;//组件预制体
    public List<GameObject> Lines;//保存屏幕上当前的线段
    //public List<Vector3> LoadLines;//还原标记一条线段容器
    //public Dictionary<int , List<Vector3>> SignLines;//还原标记所有线段
    public Dictionary<int, List<string>> linePoints;//保存当前要上传服务端线段点的信息 
    public List<string> points;//保存当前线段的所有点
    //public GameObject boult;//箭头图标
    public static DrawLine instance;
    void Start() {
        IsDrawLines = false;
       
        instance = this;
        Lines = new List<GameObject>();
        linePoints = new Dictionary<int, List<string>>();
         //points = new List<string>();
        
    }

    // Update is called once per frame  
    void Update() {
        if (IsDrawLines&&!EventSystem.current.IsPointerOverGameObject()) {
            if (Input.GetMouseButtonDown(0)) {
                points = new List<string>();
                clone = (GameObject)Instantiate(linRenPerfabs, linRenPerfabs.transform.position, transform.rotation);//克隆一个带有LineRender的物体  
                                                                                               //clone.gameObject.GetComponent<LineRendersTest>().enabled=false;  
                                                                                               //clone.GetComponent<LineRenderer>().enabled=true;  
                clone.name = "线" + lineNumbers;
                ++lineNumbers;
                Lines.Add(clone);
                line = clone.GetComponent<LineRenderer>();//获得该物体上的LineRender组件  
                line.SetColors(new Color(1,0,0,1), new Color(1, 0, 0, 1));//设置颜色  
                line.SetWidth(0.008f, 0.006f);//设置宽度  
                vertexNumbers = 0;
            }
            if (Input.GetMouseButton(0)) {

                vertexNumbers++;
                line.SetVertexCount(vertexNumbers);//设置顶点数  
                Vector3 xyz = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1));
                //print("aadasdads::"+xyz.x.ToString());
                //确保点的精度
                points.Add(("("+xyz.x.ToString()+","+xyz.y.ToString()+","+xyz.z.ToString()+")"));
                 //Debug.Log("hua de dian"+xyz.ToString());
                line.SetPosition(vertexNumbers - 1, xyz);//设置顶点位置  
                                             //line.enabled=false; 

            }
            if (Input.GetMouseButtonUp(0)) {
              
                linePoints.Add(lineNumbers - 1, points);
                //foreach (int jj in linePoints.Keys)
                //{
                //    for (int vertexNumbers = 0; vertexNumbers < linePoints[jj].Count; ++vertexNumbers)
                //    {
                //        // Debug.Log(333);
                //        //  Debug.Log("2222" + linePoints[jj][vertexNumbers]);
                //    }
                //}
                //linePoints.Add(lineNumbers - 1, points);
                //points.Clear();
            }

        }

    }
    void OnEnable() {

       // FingerGestures.OnFingerLongPress += OnFingerLongPress;
    }
    void OnDisable() {
       // FingerGestures.OnFingerLongPress -= OnFingerLongPress;
    }
    //长按事件,鼠标左键长按点击地方出现箭头，再次就隐藏箭头
    void OnFingerLongPress(int fingerIndex, Vector2 fingerPos) {
        //if (Input.GetMouseButton(0)&&IsDrawLines)
        //{
        //    Debug.Log(1111111);
        //   // Instantiate(boult,Input.mousePosition, Quaternion.identity);
        //    boult.SetActive(true);
        //    boult.transform.position = new Vector3(Input.mousePosition.x+5,Input.mousePosition.y,2);
        //}
    }
    public void DeleteAllLinePoints() {
        if (lineNumbers > 0)
        {
            for (int vertexNumbers = 0; vertexNumbers < Lines.Count; ++vertexNumbers)
            {
                Destroy(Lines[vertexNumbers]);
            }
            lineNumbers = 0;
            Lines.Clear();
            linePoints.Clear();
            //points.Clear();
        }
    }
    //清除上一条线的点的存储
    public void DeleteLinePoints() {
        if (lineNumbers > 0)
        {
            Destroy(Lines[lineNumbers - 1]);
            Lines.Remove(Lines[lineNumbers - 1]);
            --lineNumbers;
            linePoints.Remove(lineNumbers);
        }
    }

    //视点信息上传服务器
    public void SaveLines(string str){
        if (lineNumbers > 0)//防止没涂鸦导致保存没必要的空信息
        {

            List<string> listV3 = new List<string>();
            //listV3.Clear ();
            //print("KKKKK" + linePoints.Keys.Count);
            foreach (int vertexNumbers in linePoints.Keys)
            {
                //print("BBBBBB" + linePoints[vertexNumbers].ToString());
                for (int kk = 0; kk < linePoints[vertexNumbers].Count; ++kk)
                {
                    listV3.Add(linePoints[vertexNumbers][kk]);
                }
                listV3.Add(";");
            }
            StartCoroutine(ViewPut(listV3, str));
        }
    }
    IEnumerator ViewPut(List<string> listVec,string str){
        //Debug.Log("上传的线段："+listVec.ToString());
        string ss = "";
        //Debug.Log(listVec);
        for (int index = 0; index < listVec.Count; ++index) {
            ss += listVec[index].ToString();
           // Debug.Log(listVec[index]);
        }
        Vector3 pos = GameObject.FindWithTag("MainCamera").transform.position;//当前相机坐标点
        Vector3 rot = GameObject.FindWithTag("MainCamera").transform.eulerAngles;//当前相机角度
        //存取箭头坐标位置，得判断是否存在箭头
        
        
        // Debug.Log("ss:"+ss);
        //分割HTML字符串
        string[] Arystr = str.Split(',');
		WWWForm form = new WWWForm ();
        //分割后上传h5的参数
        for (int index = 0; index < Arystr.Length; index++) {
            form.AddField(Arystr[index].Split(';')[0], Arystr[index].Split(';')[1]);
        }
        //if (boult.activeInHierarchy)
        //{
        //  Vector3  bou = boult.transform.position;
        //    form.AddField("arrow", bou.ToString());
        //}
		form.AddField ("project_id",Overall.instance.project_id);
		form.AddField ("line",ss);
		form.AddField("pos",pos.ToString());
		form.AddField ("rot",rot.ToString());
        form.AddField("token",loadMoudle.instance.token);
       // form.AddField("title","测试数据3");
		WWW www=new WWW(loadMoudle.instance.URL+"index.php/family/angle/set",form);
        
		yield return www;
        if (www.isDone && www.error == null)
        {
            Debug.Log("返回值：" + www.text.ToString());
            Debug.Log("上传成功");
            DeleteAllLinePoints();//保存成功就清除当前涂鸦标记
            Application.ExternalCall("viewpoint_add", "true");//交互h5使其界面隐藏
        }
        else {
            Debug.Log("上传失败");
            Application.ExternalCall("viewpoint_add", "false");
        }
    }
    //从服务端下载视点信息
    public void LoadViewMesg(string id) {
       this.GetComponent<GestureMag>().enabled = false;
        DeleteAllLinePoints();
        //if (boult.activeInHierarchy) {
        //    boult.SetActive(false);
        //}
        StartCoroutine(ViewGet(id));
       // 
    }
    public IEnumerator ViewGet(string id) {
        WWW www = new WWW(loadMoudle.instance.URL+"index.php/family/angle/get?project_id=" +Overall.instance.project_id+"&id="+id+"&token="+loadMoudle.instance.token);
        
        yield return www;
        if (www.error == null&&www.isDone) {
            // Debug.Log("服务端信息:"+www.text);
            JsonData jd = JsonMapper.ToObject(www.text);
            //Debug.Log(jd["result"].ToString());
            string[] Points_2;
            if (jd["status"].ToString().Equals("ok")) {
                //箭头坐标
                //if (jd["result"][0]["arrow"].ToString() != "") {
                //    string arrowx = jd["result"][0]["arrow"].ToString().Split('(')[1].Split(',')[0];
                //    string arrowy = jd["result"][0]["arrow"].ToString().Split('(')[1].Split(',')[1];
                //    string arrowz = jd["result"][0]["arrow"].ToString().Split('(')[1].Split(',')[2].Split(')')[0];
                //    //boult.SetActive(true);
                //   // boult.transform.position = new Vector3(float.Parse(arrowx), float.Parse(arrowy), float.Parse(arrowz));
                //}
               
                if (jd["result"][0]["pos"].ToString() != "")
                {
                    //相机坐标
                    string posx = jd["result"][0]["pos"].ToString().Split('(')[1].Split(',')[0];
                    string posy = jd["result"][0]["pos"].ToString().Split('(')[1].Split(',')[1];
                    string posz = jd["result"][0]["pos"].ToString().Split('(')[1].Split(',')[2].Split(')')[0];
                    GameObject.FindWithTag("MainCamera").transform.position = new Vector3(float.Parse(posx), float.Parse(posy), float.Parse(posz));
                    //Debug.Log("pos:"+jd["result"][0]["pos"].ToString());
                    //  Debug.Log("111:" + GameObject.FindWithTag("MainCamera").transform.position);
                    //相机角度
                    string rotx = jd["result"][0]["rot"].ToString().Split('(')[1].Split(',')[0];
                    string roty = jd["result"][0]["rot"].ToString().Split('(')[1].Split(',')[1];
                    string rotz = jd["result"][0]["rot"].ToString().Split('(')[1].Split(',')[2].Split(')')[0];
                    GameObject.FindWithTag("MainCamera").transform.eulerAngles = new Vector3(float.Parse(rotx), float.Parse(roty), float.Parse(rotz));
                }
                string []Points_1 = jd["result"][0]["line"].ToString().Split(';');
                //print(":::——————" + Points_1.Length);
                for (int vertexNumbers = 0; vertexNumbers < Points_1.Length - 1; vertexNumbers++) {//分离出几条线段
                    //Debug.Log("每条线段构成的点："+Points_1[vertexNumbers]);
                    
                     Points_2 = Points_1[vertexNumbers].Split(')');
                    clone = (GameObject)Instantiate(linRenPerfabs, linRenPerfabs.transform.position, transform.rotation);
                    ++lineNumbers;
                    Lines.Add(clone);
                    line = clone.GetComponent<LineRenderer>();//获得该物体上的LineRender组件  
                    line.SetColors(Color.red, Color.red);//设置颜色  
                    line.SetWidth(0.008f, 0.006f);//设置宽度
                    //line.SetVertexCount(Points_2.Length+1);//设置顶点个数
                    //print(":::" + Points_2.Length);
                    for (int z = 0; z < Points_2.Length-1; z++) {//分离出线段的点
                        //Debug.Log("点:"+Points_2[lineNumbers]);
                       string[] pos = (Points_2[z].Split('(')[1].Split(','));
                        //Debug.Log(pos[0] + "," + pos[1] + "," + pos[2]);
                        //new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]) 点
                        line.SetVertexCount(z+1);
                        line.SetPosition(z, new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2])));
                    }
                }
                
               
            }
        }
       
    }
    //开始涂鸦画线
    public void StarDrawLins() {
        IsDrawLines = true;
        RegionDelete.instance.IsRegion = false;//屏蔽框选功能
       
        GetComponent<GestureMag>().enabled = true;
    }
    //退出涂鸦画线
    public void BackDrawLins() {
        IsDrawLines = false;
        RegionDelete.instance.IsRegion = true;
        DeleteAllLinePoints();
        //if (boult.activeInHierarchy) {
        //    boult.SetActive(false);
        //}
        GetComponent<GestureMag>().enabled = true;
    }
}
