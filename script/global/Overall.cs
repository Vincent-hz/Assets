using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
public class Overall : MonoBehaviour {
    public static Overall instance;
    public   string   project_id="1";//项目id
    public string cache_pid="-1";//备份
    public string pro_name = "";//项目名称
    // public static GameObject Md;   //场景中的模型
    public static List<GameObject> MdArray;//存储assetbundle
    public List<CompentInfo> listt;//存储模型挂载的信息
    public Dictionary<string, GameObject> cache;  //模型具体的构件号-该模型对象  
    public string modell_id;//模型id




    void Awake()
    {
        instance = this;
        listt = new List<CompentInfo>();
        cache = new Dictionary<string, GameObject>();
        //Application.ExternalEval("GetId1()");
        //Debug.Log("调用文老师的GetId1()函数");
    }
    void OnEnable() {
        
    }
    

    public bool  HaveCompNamId( string pp)     //用于listt的工具遍历方法
    {
        foreach (CompentInfo kk in listt)
        {
            if (kk.compentNameId == pp)
            {
                
                return true;
               
            }
        }
        return false;
    }



    public CompentInfo compentNameIdObj(string pp)     //用于listt的工具遍历方法  
    {
        foreach (CompentInfo kk in listt)
        {
            if (kk.compentNameId == pp)
            {
                return kk;
            }
        }
        return null;
    }

    //public string SetPro_id() {              //项目id

    //    return project_id;
    //}
    //public string SetPro_name() {          //项目名称
    //    return pro_name;
    //}
    //public void MySet() {
    //    using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
    //    {
    //        jc.Call("MySet", project_id, pro_name);
    //    }

    //}
    [DllImport("__Internal")]
    private static extern void SetThings(string id,string name);
    public static void MySetting(string id,string name) {

        if (Application.platform != RuntimePlatform.OSXEditor)
        {
            SetThings(id,name);
        }
    }

}
    //自定义存储文件格式
    public class CompentInfo
    {
        public string compentNameId;
        public string textureId;
        public string offsetX;
        public string offsetY;
        public string scaleX;
        public string scaleY;
        public string luminance;
    }
