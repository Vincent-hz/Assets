using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using System;
using System.Net;
 // 功能：该脚本用于模型的加载与替换
 
public  class loadMoudle : MonoBehaviour {
    public Text text;//参数状态显示
    public static loadMoudle instance;
    private string objname;//构件名
    public GameObject LoadedModel;//当前进入协程里要加载的模型
    public Transform target;//坐标目标点
    List<ModelInfo> lisMdInf;//加载ab信息存储容器
   int assetIndex=0;//要加载的ab包个数
   public bool empty = true;//当前ab数据下载完标识符
    public WWW bundle;//要在下载的ab包
    public Vector3 ModelCenter;//模型中心点
    public Slider slider;//进度条
    public Text tex;//进度条百分比
    private float count;//ab包个数
    // Use this for initialization
    public string URL;
   public Transform tt;
    Material mat;
    Material mat2;
    public GameObject go;
    public string token;
    public GameObject hint;//模型提示
    void Awake()
    {
       
        instance = this;
        //限制帧率
        //Application.targetFrameRate = 100;



    }
    void Start()
    {

        Overall.MdArray = new List<GameObject>();  //实例化一次     
                                                   // startLoad();
                                                   //ApplicationChrome.dimmed = false;
                                                   // ApplicationChrome.statusBarState = ApplicationChrome.navigationBarState = ApplicationChrome.States.Visible;
        Application.ExternalEval("loadRemove()");
       
    }


    public delegate void loadComplete(List<GameObject> text);
    public static event loadComplete loadOver;
    public delegate void loadCompletee(Transform text);   //定义这个事件主要是给手势事件传个目标
    public static event loadCompletee start_load;


    // Update is called once per frame
    IEnumerator ie01;
    IEnumerator ie02;
    void Update()
    {
        if (slider.value > 0.95f) {
            slider.gameObject.SetActive(false);
        }
    }
    //再次下载
    public void startLoad()
    {
        Resources.UnloadUnusedAssets();
        GC.Collect();
        StopAllCoroutines();
        
        if (Overall.instance.project_id != Overall.instance.cache_pid)
        {
            slider.gameObject.SetActive(true);
            slider.value = 0;
            tex.text = "";
            Overall.instance.cache_pid = Overall.instance.project_id;
            // filter.instance.startLoad();
            if (lisMdInf != null)
            {
                lisMdInf.Clear();
            }

            if (ie01 != null)
            {
                StopCoroutine(ie01);

            }
            if (lisMdInf != null)
            {
                lisMdInf.Clear();
            }
            if (ie02 != null)
            {
                StopCoroutine(ie02);

            }

            foreach (GameObject gk in Overall.MdArray)
            {
                //Debug.Log(gk.name);
                Destroy(gk);
                
            }

            empty = true;
            assetIndex = 0;
            ie01 = LoadHouse2(Overall.instance.project_id);
            StartCoroutine(ie01);
        }
        else
        {
            Debug.Log("不重复下载当前模型");
        }
    }
    public void ReadUrl(string path)
    {
        URL = path;
        Debug.Log("域名：" + path);
        startLoad();
        SearchComp.instance.starLoad();
        ComponentWebGL.instance.starLoad();

    }
    public void GetToken(string token_h5) {
        token = token_h5;
    }
    public void HideHint() {
        hint.SetActive(false);
    }
    private IEnumerator LoadHouse2(string prjid)
    {
        
        //正式服务器
          WWW  MS = new WWW(URL+"index.php/Admin/GetModell/GetModell?PrjID="+ prjid + "&version_id=4"+"&token="+token);
        yield return MS;
        Debug.Log(MS.error);
        Debug.Log(MS.url);
        Debug.Log(MS.text);
       // Debug.Log(MS.text+"////"+MS.url);
        //测试服务器地址
        // WWW MS = new WWW("http://192.168.1.126:8081/index.php/Admin/GetModell/GetModell?PrjID=" + prjid + "&version_id=4");  //android的版本号是1，ios版本号是2，vr版本号是3，webgl版本号是4
        if (MS.error!=null) {
            slider.gameObject.SetActive(false);
            Debug.Log("错误信息："+MS.error);
            
        }

        if (MS.text == "")
        {
            objname = "获取版本信息失败！";


        }
        else if (MS.text == "[]")
        {

            Debug.Log(MS.text);
            slider.gameObject.SetActive(false);
            hint.SetActive(true);
            Invoke("HideHint", 2f);
        }
        else {
            objname = "获取版本信息成功！";
        }


        string ss = MS.text;
     
        lisMdInf = JsonMapper.ToObject<List<ModelInfo>>(ss);
         count = lisMdInf.Count;
        if (lisMdInf.Count != 0)  //开始分包下载
        {
            Overall.MdArray.Clear();
            string kkj = lisMdInf[0].path;
            //Debug.Log("assetbundle地址：" + kkj);
            Overall.instance.modell_id = lisMdInf[0].modell_id;
            ie02 = IE_Load(kkj);
            StartCoroutine(ie02);
            //EditorCoroutineLooper.StartLoop(this, ie02);
        }
    }
    private IEnumerator IE_Load(string path)
    {
        //Caching.CleanCache();
        //int version = int.Parse(lisMdInf[0].mqid);
        
        WWW bundle = WWW.LoadFromCacheOrDownload(path, 6);//有缓存，第二次加载速度明显加快
        //WWW bundle = new WWW(path);
        //Debug.Log("内容:"+bundle.text);
        Debug.Log(bundle.url);
        Debug.Log("缓存剩余的空间:" + Caching.spaceFree+"字节");
        Debug.Log("缓存所占的空间:" + Caching.spaceOccupied+"字节");
      //  Debug.Log("时间戳：" + Caching.MarkAsUsed(path + "?mqid=" + lisMdInf[0].mqid, version));
        yield return bundle;
        Debug.Log(bundle.error);
        Debug.Log(bundle.url);
        
        if (bundle.error == null&&bundle.isDone)
        {
            
            GameObject obj = bundle.assetBundle.mainAsset as GameObject; 
            LoadedModel = Instantiate(obj);
            Overall.MdArray.Add(LoadedModel);
           
            if (loadOver != null) //保证一边下一边可以搜
            {
                loadOver(Overall.MdArray);
            }
            //target= LoadedModel.transform;
            LoadedModel.transform.parent = go.transform;
            target = go.transform;
            ModelCenter = GetCenter(target);
            

            start_load(target);        

            //实时释放掉assetbundle里面的内存镜像（本身）
            bundle.assetBundle.Unload(false);
            //卸载未使用（过期）资源
            Resources.UnloadUnusedAssets();
            // yield return new WaitForSeconds(2f);
            assetIndex++;
            slider.value = assetIndex / count;
            tex.text = (assetIndex / count*100).ToString("f0") + "%";
           // Debug.Log("加载了一个ab包，进度条走了");
            if (assetIndex < lisMdInf.Count)  //说明还有分包需要下载
            {
                StartCoroutine("IE_Load", lisMdInf[assetIndex].path);
                GetMat();
            }
            else
            {
                //这行代码能够被执行，说明是所有的分包已经全部完成
                // Application.ExternalEval("GetId()");//回调html方法
                //给每个构件动态替换shader，以至于可以变色
                // GetMat();
                assetIndex = 0;
                empty = true;
                GetMat();
               //  ShaderMag.instance.ShaderCha();
                if (loadOver != null)
                {
                    loadOver(Overall.MdArray);
                }
            }
        }
        else
        {
            slider.gameObject.SetActive(false);
        }
    }
    private void OnDisable()
    {
        GC.Collect();

    }
   //public List<Material> material = new List<Material>();
    public void GetMat()
    {
        //int x = 0;
        foreach (GameObject gg in ComponentWebGL.instance.Dic.Values)
        {
            
            if (gg != null && gg.GetComponent<MeshRenderer>())
            {
                //material.Clear();
                Material mat= gg.GetComponent<MeshRenderer>().material;
                if (mat.shader.name.Contains("BV"))
                {
                    string str = mat.shader.name.ToString().Split('(')[0];
                    mat.shader = Shader.Find(str);
                }
            }
        }
    }
    //自定义数据类型
    public class ModelInfo
    {
        public string mqid;
        public string id;
        public string path;
        public string modell_id;
        public string order;
    }
    //计算模型的中心点
    public static Vector3 GetCenter(Transform text)
    {

        Transform parent = text;

        Vector3 postion = parent.position;

        Quaternion rotation = parent.rotation;

        Vector3 scale = parent.localScale;

        parent.position = Vector3.zero;

        parent.rotation = Quaternion.Euler(Vector3.zero);

        parent.localScale = Vector3.one;

        Vector3 center = Vector3.zero;

        Renderer[] renders = parent.GetComponentsInChildren<Renderer>();

        foreach (Renderer child in renders)
        {

            center += child.bounds.center;

        }

        center /= parent.GetComponentsInChildren<Renderer>().Length;

        Bounds bounds = new Bounds(center, Vector3.zero);

        foreach (Renderer child in renders)
        {

            bounds.Encapsulate(child.bounds);

        }

        parent.position = postion;

        parent.rotation = rotation;

        parent.localScale = scale;

        foreach (Transform t in parent)
        {

            t.position = t.position - bounds.center;

        }

        parent.transform.position = bounds.center + parent.position;
        return parent.transform.position;
    }

}
