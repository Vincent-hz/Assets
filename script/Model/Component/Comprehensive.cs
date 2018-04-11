using UnityEngine;
using System.Collections;
using LitJson;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
public class Comprehensive : MonoBehaviour {

  
    /*
     * 功能：该脚步管理工程中所有点击综合事件
     */

    private string componentName; //缓存被点击的构件名
	// Use this for initialization
    public static Comprehensive instance;
    public string componentNameIndex;//当前点击的构件id
    public string befIndex;//记录上一次的构件id
   // public List<string> OnceDowns;
   // public List<string> BefOncDowns;
    public List<string> BefMebers;//备份上一次隐藏的构件
    public GameObject wanderCam;//漫游相机
    public GameObject BackButton;//退出漫游按钮
    public GameObject mianCam;//主相机
   public GameObject MeasurCam;//测量时正交相机
    public bool IsHide;//是否隐藏标识符
    private bool IsShow;//构件信息展示标识
  
    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        //Id = GetComponent<Text>();
        // OnceDowns=new List<string>();
        BefMebers = new List<string>();
        //BefOncDowns = new List<string>();
        IsHide = false;
    }
    void OnGUI()
    {
        if (mianCam.activeInHierarchy)
        {
            Event Mouse = Event.current;
            //单击构件发光
            if (Mouse.isMouse && Mouse.type == EventType.MouseDown && Mouse.clickCount == 1)
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);//鼠标的屏幕坐标转化为一条射线
                RaycastHit hit;

                //距离为5
                //if(Physics.Raycast(ray, out hit, 5)) {
                //    var hitObj = hit.collider.gameObject;
                //    Debug.Log(hitObj);
                //}
                //点击发光和隐藏事件
                if (Physics.Raycast(ray, out hit))
                {
                    var hitObj = hit.collider.gameObject;

                    string name = hit.transform.name;
                    if (name.Contains("["))
                    {
                        int startIndex = name.IndexOf("[");
                        int endIndex = name.IndexOf("]");
                        string ww = name.Substring(startIndex + 1, endIndex - startIndex - 1);
                        componentNameIndex = name.Substring(startIndex + 1, endIndex - startIndex - 1);
                    }
                    if (IsHide&&Input.GetMouseButton(0))
                    {
                        HiddenArtifacts_2();
                    }
                    else
                    {
                        if (!hitObj.CompareTag("point")&&Input.GetMouseButton(0))
                        {
                         
                            MouseHighlight.instance.SetObjectHighlight(hitObj);
                           // Debug.Log(hitObj.transform.position);
                           // Debug.Log(11111111);
                            if (IsShow) {//展示构件信息
                                Application.ExternalCall("popPropEditor", componentNameIndex);
                            }
                        }
                    }
                }

            }
            //双击展示构件工法信息和构件进度计划信息
            if (Mouse.isMouse && Mouse.type == EventType.MouseDown && Mouse.clickCount == 2)
            {
                if (!EventSystem.current.IsPointerOverGameObject())//判断点击UI穿透
                {


                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        string namee = hit.transform.name;
                        if (namee.Contains("["))
                        {
                            int startIndex = namee.IndexOf('[');
                            int endIndex = namee.LastIndexOf(']');
                            string subName = namee.Substring(startIndex + 1, endIndex - startIndex - 1);
                            //Debug.Log("构件ID："+ww);
                            componentName = namee.Substring(0, startIndex);  //截取构件名信息
                                                                             // Debug.Log("构件信息" + componentName);
                                                                             // StartCoroutine("kaishi", ww);
                                                                             // componentNamejian.SetActive(true);
                                                                             //Application.ExternalCall("DoubleShowCon", subName);//调用h5函数，弹出HTML界面
                        }
                    }

                }

            }
        }
    }
    //显示构件信息触发有用
    public void ShowCompInfo() {
        IsShow = true;
    }
    //显示构件信息触发失效
    public void BackComInfo() {
        IsShow = false;
    }
    //触发隐藏功能
    public void HiddenArtifacts()
    {
        IsHide = true;
    }
    //退出隐藏
    public void BackHIdden()
    {
        IsHide = false;
        RegionDelete.instance.IsRegion = false;
    }
    //隐藏单击构件或者框选构件
    public void HiddenArtifacts_2()
    {
        RegionDelete.instance.IsRegion = true;
        if (componentNameIndex != null)
        {
            foreach (string ss in ComponentWebGL.instance.Dic.Keys)
            {
                string id = ss;
                if (id.Contains("A"))
                {
                    id = ss.Substring(0, ss.IndexOf("A"));
                }
                if (componentNameIndex.Equals(id))
                {
                    //Debug.Log("单个隐藏构件:"+ss);
                    ComponentWebGL.instance.Dic[ss].SetActive(false);
                }
            }
            befIndex = componentNameIndex;
            // Debug.Log("222:"+befIndex);
            componentNameIndex = null;
            BefMebers.Clear();
            // BefOncDowns.Clear();
        }
        //if (OnceDowns.Count > 1) {
        //    foreach (string kk in ComponentWebGL.instance.Dic.Keys) {
        //        if (OnceDowns.Contains(kk)) {
        //            ComponentWebGL.instance.Dic[kk].SetActive(false);
        //        }
        //    }
        //    for (int vertexNumbers = 0; vertexNumbers <OnceDowns.Count; ++vertexNumbers)
        //    {
        //        BefOncDowns.Add(OnceDowns[vertexNumbers]);
        //    }
        //    OnceDowns.Clear();
        //    BefMebers.Clear();
        //    befIndex = null;
        //}
        if (RegionDelete.instance.Members.Count > 1)
        {
            // Debug.Log("框选隐藏");
            foreach (string ww in ComponentWebGL.instance.Dic.Keys)
            {
                if (RegionDelete.instance.Members.Contains(ww))
                {
                    ComponentWebGL.instance.Dic[ww].SetActive(false);
                }
            }

            for (int vertexNumbers = 0; vertexNumbers < RegionDelete.instance.Members.Count; ++vertexNumbers)
            {
                BefMebers.Add(RegionDelete.instance.Members[vertexNumbers]);
            }
            RegionDelete.instance.Members.Clear();
            befIndex = null;
            //  BefOncDowns.Clear();
            // Debug.Log(BefMebers.Count);
        }
    }
    //显示所有构件
    public void ShownArtifacts()
    {
        Debug.Log("显示构件");
        foreach (GameObject hh in ComponentWebGL.instance.Dic.Values)
        {
            hh.SetActive(true);
        }
        //  IsHide = false;
    }
    //显示上一次的构件
    public void ShowBefArtifacts()
    {
        foreach (string ww in ComponentWebGL.instance.Dic.Keys)
        {
            if (befIndex != null && befIndex.Equals(ww))
            {
                ComponentWebGL.instance.Dic[ww].SetActive(true);
                // Debug.Log("上一次单个：" + ww);
            }
            if (BefMebers.Count > 1 && BefMebers.Contains(ww))
            {
                ComponentWebGL.instance.Dic[ww].SetActive(true);
            }
            //if (BefOncDowns.Count > 1 && BefOncDowns.Contains(ww)) {
            //    ComponentWebGL.instance.Dic[ww].SetActive(true);
            //}

        }
    }
    //开始漫游
    public void StarWander()
    {
        Debug.Log("开始进入漫游");
        wanderCam.SetActive(true);
        BackButton.SetActive(true);
        mianCam.SetActive(false);
        // Application.ExternalEval("HideAllH5_UI");//点击看小地图隐藏h5UI
    }
    //退出漫游
    public void BackWander()
    {
        mianCam.SetActive(true);
        wanderCam.SetActive(false);
        BackButton.SetActive(false);
        Application.ExternalEval("ShowAllH5_UI()");//退出漫游显示h5UI
    }
    //直线测量
    public void LinsDistance()
    {
        if (!MeasurCam.activeInHierarchy)
        {
            //MeasurCam.SetActive(true);
            mianCam.SetActive(true);
            Debug.Log("开始进入测量mmp");
            //Application.ExternalEval("HideMeasuUI()");//隐藏HTML UI
        }
        RayMeasure.instance.LinsDistance_2();

    }
    //面积测量
    public void AreaMeasure()
    {
        if (!MeasurCam.activeInHierarchy)
        {
            //MeasurCam.SetActive(true);
            mianCam.SetActive(true);
            //Application.ExternalEval("HideMeasuUI()");//隐藏HTML UI
        }
        RayMeasure.instance.AreaMeasure_2();
    }

    void Update()
    {

    }
    
}
