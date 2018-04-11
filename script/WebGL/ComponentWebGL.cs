using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine.UI;
using System;
using System.Text;

public class ComponentWebGL : MonoBehaviour
{
   /*
    * 功能:处理所需构件id配合的所有操作
    * 时间:2017/11/25
    * */
    
    public Dictionary<string,GameObject> Dic;//存储当前项目id的模型所有构件id对应的构件对象
    public Dictionary<string, List<string>> TimeAr ;//存储当前接口下构件id对应的构件id
    //public List<string> xianshiId = new List<string>();//存储已经显示的构件id
    // public List<string> BeforeId = new List<string>();//上次显示的ID
	public static ComponentWebGL instance;
    // public List<GameObject> AllGameObject;
    //下载时间进度对应构件id接口
    public void starLoad()
    {
        if (TimeAr != null)
        {
            TimeAr.Clear();
        }
        StartCoroutine(LoadComponent());
    }
    void Start()
    {
       // starLoad();
        Dic = new Dictionary<string, GameObject>();
        TimeAr = new Dictionary<string, List<string>>();
       
    }
    void Awake()
    {
        instance = this;

    }
   
    private void OnEnable()
    {
        // starLoad();
        loadMoudle.loadOver += Gather;//注册当模型构件id收集事件
        
    }
    private void OnDisable()
    {
        loadMoudle.loadOver -= Gather;//注销当模型构件id收集事件
    }
    //获取模型所有构件id 并存入字典里
    void Gather(List<GameObject> entering)
    {
        Dic.Clear();
        foreach (GameObject text in entering)
        {
            foreach (Transform dd in text.transform)
            {
                if (dd.GetChildCount() > 0 && !dd.name.Contains("["))
                {
                    foreach (Transform kk in dd.GetComponentInChildren<Transform>())
                        if (kk.name.Contains("["))
                        {
                            int startIndex = kk.name.IndexOf('[');
                            int endIndex = kk.name.LastIndexOf(']');
                            string ww = kk.name.Substring(startIndex + 1, endIndex - startIndex - 1);
                            while (Dic.ContainsKey(ww))
                            {
                                ww += "A";
                            }
                            Dic.Add(ww, kk.gameObject);
                        }
                }
                else if (dd.name.Contains("["))
                {
                    int startIndex = dd.name.IndexOf('[');
                    int endIndex = dd.name.LastIndexOf(']');
                    string ww = dd.name.Substring(startIndex + 1, endIndex - startIndex - 1);
                    while (Dic.ContainsKey(ww))
                    {
                        ww += "A";
                    }
                    Dic.Add(ww, dd.gameObject);
                }
                else {
                    string ss = dd.name;
                    if (!Dic.ContainsKey(ss))
                    {
                        Dic.Add(ss, dd.gameObject);
                    }
                }
            }
        }
        //Debug.Log("构件个数:"+Dic.Keys.Count);
    }
    //下载时间进度对应的构件号
    private IEnumerator LoadComponent()
    {
        WWW www = new WWW(loadMoudle.instance.URL+"index.php/Home/ModelManage/ModelObjList?PrjID=" + Overall.instance.project_id+"&token="+loadMoudle.instance.token);
        yield return www;
        // Debug.Log(www.text);
        if (www.isDone && www.error == null)
        {
            JsonData jd = JsonMapper.ToObject(www.text);
            // List<string> date = new List<string>();

            for (int i = 0; i < jd.Count; ++i)
            {
                List<string> obJid = new List<string>();
                if (jd[i]["datename"].ToString() != null)
                {

                    string ss = jd[i]["objid"].ToString();

                    if (ss.Contains(","))
                    {
                        // Debug.Log(jd[i]["objid"].ToString());

                        string[] objid = ss.Split(',');

                        for (int j = 0; j < objid.Length; ++j)
                        {
                            obJid.Add(objid[j]);
                            // Debug.Log(jd[i]["datename"] + ":"+objid[j]);
                        }
                    }
                    else
                    {
                        obJid.Add(ss);

                    }
                    TimeAr.Add(jd[i]["datename"].ToString(), obJid);
                    //    Debug.Log(ss);
                }
            }
        }
    }
    //隐藏所有模型构件并且移除发光组件
    public void HideAllComponent_h5()
    {

        foreach (string id in Dic.Keys)
        {
            MouseHighlight.instance.RemoveComponent(Dic[id]);
            Dic[id].SetActive(false);
        }
    }
    //隐藏对应模型防止多次操作移除发光组件
    public void HideAllComponent_h5_2()
    {

        foreach (string id in Dic.Keys)
        {
            // MouseHighlight.instance.RemoveComponent(Dic[id]);
            Dic[id].SetActive(false);

        }
    }
    //展示某一天构件
    public void GetDay_h5(string ss)
    {
        foreach (string kk in TimeAr.Keys)
        {

            if (ss.Equals(kk))
            {

                foreach (string pp in Dic.Keys)
                {
                    string id = pp;
                    if (id.Contains("A"))
                    {
                        id = pp.Substring(0, pp.IndexOf("A"));
                    }
                    if (TimeAr[kk].Contains(id))
                    {
                        Dic[pp].SetActive(true);
                        MouseHighlight.instance.SetObjectsHighlight(Dic[pp]);
                    }
                }
            }
        }

    }
    //交互h5控制播放速度
    public IEnumerator PlayModel(string ss)
    {
        string date = ss.Split(',')[0];
        string time = ss.Split(',')[1];
        // Debug.Log("日期："+date);
        // Debug.Log("时间："+time);

        if (time.Equals("404"))
        {
            //暂停
        }
        else
        {
            yield return new WaitForSeconds(float.Parse(time));
            StartCoroutine(ShowWays(date));
            Application.ExternalCall("HelloH5", "");//调用h5函数
        }
    }
    //html5调用控制播放速度和变色的接口
    public void PlayModel_h5(string ss)
    {

        StartCoroutine(PlayModel(ss));
    }
    //模型展示方式（施工模拟）
    public IEnumerator ShowWays(string ss)
    {

        foreach (string mk in TimeAr.Keys)
        {

            if (ss.Equals(mk))
            {
                // Debug.Log("ss:" + ss);
                foreach (string zz in Dic.Keys)
                {
                    string id = zz;
                    if (id.Contains("A"))
                    {
                        id = zz.Substring(0, zz.IndexOf("A"));
                    }
                    if (TimeAr[mk].Contains(id) && Dic[zz].activeInHierarchy)
                    {
                        if (Dic[zz].GetComponent<MeshRenderer>() != null)
                        {
                            //Debug.Log("ss:" + ss);
                            //实现构件变色的方向和百分比的接口                           
                            WWW www = new WWW(loadMoudle.instance.URL+"index.php/Unity/WebglModelManage/objDetails?PrjID=" + Overall.instance.project_id + "&objID=" + id + "&date=" + ss+"&token="+loadMoudle.instance.token);
                            //WWW www = new WWW("http://192.168.1.126:8081/index.php/Unity/WebglModelManage/objProfession?PrjID="+Overall.instance.project_id+"&date="+"2017-09-23");
                            yield return www;
                             Debug.Log("施工模拟构件信息："+www.text);
                            if (www.isDone && www.error == null)
                            {
                                //  Debug.Log(3333333);
                                //UTF8Encoding utf8 = new UTF8Encoding();
                                // string cc = utf8.GetString(utf8.GetBytes(www.text));
                                JsonData jd = JsonMapper.ToObject(www.text);


                                //Debug.Log(dd01["status"].ToString());
                                if (jd["status"].ToString().Equals("ok"))
                                {
                                    Debug.Log("开始变色");
                                    //string dire = jd["result"]["message"][0]["direction"].ToString();//变色方向
                                    string per = jd["result"]["message"][0]["percent"].ToString();//变色百分比
                                     Debug.Log("百分比:" + per);

                                    // 1 left到right 2 up到down 3 forward到back

                                    MouseHighlight.instance.RemoveComponent(Dic[zz]);

                                    Dic[zz].GetComponent<MeshRenderer>().material.SetFloat("_Mode", 1.0f);
                                    //-0.01->-0.001
                                    if (float.Parse(per) >= 0.9f)
                                    {
                                        Dic[zz].GetComponent<MeshRenderer>().material.SetFloat("_Clip", 1.0f);
                                    }
                                    else
                                    {
                                        Dic[zz].GetComponent<MeshRenderer>().material.SetFloat("_Clip", 1- float.Parse(per));
                                        // Debug.Log(per);
                                    }
                                    // MouseHighlight.instance.SetObjectsHighlight(Dic[zz]);


                                }

                            }
                        }
                    }

                }
            }
            // Application.ExternalCall("HelloH5", "");//调用h5函数
        }
    }
    //模型展示方式接口
    public void showWays(string time)
    {
        //ShaderMag.instance.ShaderCha();
        //  HideAllcomponentNameJian_h5();
        StartCoroutine(ShowWays(time));
    }
    //根据时间所对应的构件id串显示构件
    public void ShowComponent_h5(string h5str)
    {
        string[] str = h5str.Split(',');
        List<string> list = new List<string>();
        for (int i = 0; i < str.Length; ++i)
        {
            list.Add(str[i]);
        }
        foreach (string dd in Dic.Keys)
        {
            string id = dd;
            if (id.Contains("A"))
            {
                id = dd.Substring(0, dd.IndexOf("A"));
            }
            if (list.Contains(id))
            {
                Dic[dd].SetActive(true);
            }
        }

    }
    //接收h5传递的id，换模型
    public void GetId_h5(string id)
    {
        Debug.Log("id:"+id);
        Overall.instance.project_id = id;
        transform.GetComponent<loadMoudle>().startLoad();//模型更新
        //loadMoudle.instance.bundle.assetBundle.Unload(false);//卸载场景中未被引用ab包文件
        loadMoudle.instance.empty = true;
        GetComponent<GestureMag>().enabled = false;
        GetComponent<GestureMag>().enabled = true;
        starLoad();//时间进度构件信息更新
        SearchComp.instance.starLoad();//空间信息更新
    }
    public void FullSvreen() {
        Screen.fullScreen = true;
    }
    
}
