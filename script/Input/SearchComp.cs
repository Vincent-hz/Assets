using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using LitJson;
public class SearchComp : MonoBehaviour
{

    public Dictionary<string, GameObject> NameDic;
    public List<GameObject> target;
    public Dictionary<string, List<string>> Space;
    public Dictionary<string, List<string>> Type;
    public Dictionary<string, string> TypeKey;
    private int Nameindex;
    public static SearchComp instance;
    IEnumerator ie_space;
    IEnumerator ie_type;
    JsonData dd01;
    JsonData dd02;
    JsonData dd03;
    JsonData dd04;
    void Awake() {
        instance = this;
    }
    void Start()
    {
        NameDic = new Dictionary<string, GameObject>();
        target = new List<GameObject>();
        Space = new Dictionary<string, List<string>>();
        Type = new Dictionary<string, List<string>>();
        TypeKey = new Dictionary<string, string>();
       // starLoad();
    }

    // Update is called once per frame
    void Update()
    {

    }
    //重新下载
    public void starLoad() {
        if (ie_space != null) {
            StopCoroutine(ie_space);
        }
        if (ie_type != null) {
            StopCoroutine(ie_type);
        }
        if (Space!=null){
            Space.Clear();
        }
        if (Type != null) {
            Type.Clear();
        }
        if (NameDic != null) {
            NameDic.Clear();
        }
        if (TypeKey != null) {
            TypeKey.Clear();
        }
        ie_space = Load_Space();
        ie_type = Load_Type();
        StartCoroutine(ie_space);
        StartCoroutine(ie_type);
    }
    public void LoadComplete(List<GameObject> comlepte)
    {
        target = comlepte;
        Collect(target);
    }
    public void OnEnable()
    {
        loadMoudle.loadOver += LoadComplete;
    }
    public void OnDisable()
    {
        loadMoudle.loadOver -= LoadComplete;
    }
    void Collect(List<GameObject> complete)
    {
        NameDic.Clear();
        foreach (GameObject gameobj in complete)
        {
            foreach (Transform te in gameobj.transform)
            {
                try
                {
                    NameDic.Add(te.name, te.gameObject);
                }
                catch (Exception e)
                {
                  //  Debug.Log("字典键值相同");
                    ++Nameindex;
                    te.name = te.name + Nameindex;
                    NameDic.Add(te.name,te.gameObject);
                }
            }
        }
    }
    public void SearchComponentName(string name) {
        if (name != null)
        {
            Debug.Log("搜索内容:"+name);
            foreach (string str in NameDic.Keys)
            {
                if (str.Contains(name))
                {
                    NameDic[str].SetActive(true);
                }
                else
                {
                    NameDic[str].SetActive(false);
                }
            }
        }
        else {
            foreach (string str in NameDic.Keys) {
                NameDic[str].SetActive(true);
            }
        }
    }
    public void ComponentMatch(string name)
    {

       
        if (name != "")
        {
            Debug.Log("html参数:" + name);
            string[] nameArray = name.Split(',');
           // Debug.Log(nameArray.Length);
                List<string> webList = new List<string>();

                for (int i = 0; i < nameArray.Length-1; i++)
                {
                //Debug.Log(nameArray[i]);
                    if (nameArray[i] != "")
                    {
                        webList.Add(nameArray[i]);
                        //Debug.Log(nameArray[i]);
                    }
                }

                List<string> uu = new List<string>();//空间
            //foreach (string ss in Space.Keys) {
            //   //Debug.Log("kj"+ss);
            //}
                for (int k = 0; k < webList.Count; ++k)
                {
                    if (Space.ContainsKey(webList[k]))
                    {
                    Debug.Log("空间："+webList[k]);
                        foreach (string gg in Space[webList[k]])
                        {
                            uu.Add(gg);

                        }
                    }
                }
                List<string> uu2 = new List<string>();//专业
            
            for (int z = 0; z < webList.Count; ++z)
                {
                    if (Type.ContainsKey(webList[z]))
                    {
                    Debug.Log("专业："+webList[z]);
                        foreach (string gg in Type[webList[z]])
                        {
                            uu2.Add(gg);

                        }
                    }
                }
                List<string> jiaoji = new List<string>();
                if (uu.Count == 0)
                {
                    jiaoji = uu2;
                }
                else if (uu2.Count == 0)
                {
                    jiaoji = uu;
                }
                else
                {
                    for (int i = 0; i < uu.Count; ++i)
                    {
                        if (uu2.Contains(uu[i]))
                        {
                            jiaoji.Add(uu[i]);
                        }
                    }
                }
            //foreach (string ss in jiaoji) {
            //    Debug.Log("交集:"+ss);
            //}

                foreach (string hh in ComponentWebGL.instance.Dic.Keys)
                {
                    string id = hh;
                    if (id.Contains("A"))
                    {
                        id = hh.Substring(0, hh.IndexOf("A"));
                    }


                    if (jiaoji.Contains(hh))
                    {
                        ComponentWebGL.instance.Dic[hh].SetActive(true);
                   // Debug.Log("存在构件显示");
                    }
                    else
                    {
                        ComponentWebGL.instance.Dic[hh].SetActive(false);
                   // Debug.Log("不存在构件不显示");
                }

                }
        }
    }
    //下载类型数据
    IEnumerator Load_Type() {
        //interhouse.cn/doc/bim9d/#api-Schedule-GetBim9dScheduleGetmodeltype
        //index.php/Admin/GetModelObjList/GetModelType
        WWW www01 = new WWW(loadMoudle.instance.URL+ "index.php/bim9D/schedule/getmodeltype?PrjID="+Overall.instance.project_id +"&token="+loadMoudle.instance.token);  //获取类型属性名集合
        yield return www01;
        if (www01.isDone && www01.error == null)
        {
            dd03 = JsonMapper.ToObject(www01.text)["result"];
            for (int j = 0; j < dd03.Count; ++j)
            {
                TypeKey.Add(dd03[j]["name"].ToString(), dd03[j]["code"].ToString());
               // Debug.Log(dd03[j]["code"].ToString());
            }
            WWW www = new WWW(loadMoudle.instance.URL+"index.php/Admin/GetTypeInfo/GetTypeInfo?PrjID=" + Overall.instance.project_id+"&token="+loadMoudle.instance.token);  //按照空间
            yield return www;
            //Debug.Log("Load01:"+www.text);
            if (www.isDone && www.error == null)
            {
                dd04 = JsonMapper.ToObject(www.text);

                foreach (string typekey in TypeKey.Keys)
                {
                    
                    List<string> uu = new List<string>();
                    if (www.text.Contains(TypeKey[typekey]))
                    {
                        JsonData jd = dd04[TypeKey[typekey]];
                        for (int k = 0; k < jd.Count; ++k)
                        {
                            uu.Add(jd[k].ToString());
                        }
                        Type.Add(TypeKey[typekey], uu);
                    }
                }
            }
            else
            {
                Debug.Log("清单获取失败");
            }
        }
        else
        {
            Debug.Log("属性名数组获取失败");
        }
    }
    IEnumerator Load_Space() {
        WWW www02 = new WWW(loadMoudle.instance.URL+ "index.php/Admin/GetModelObjList/GetModelSpace?PrjID=" + Overall.instance.project_id+"&token="+loadMoudle.instance.token);   //空间属性数组
        yield return www02;
        if (www02.isDone && www02.error == null)
        {
            dd01 = JsonMapper.ToObject(www02.text)["spaceInfo"];
            //     Debug.Log("我曾握这把剑年复一年"+dd01.ToString());
            WWW www = new WWW(loadMoudle.instance.URL+ "index.php/Admin/GetSpaceInfo/GetSpaceInfo?PrjID=" + Overall.instance.project_id+"&token="+loadMoudle.instance.token);  //按照空间
            yield return www;
            //Debug.Log("Load2:"+www.text);
            if (www.isDone && www.error == null)
            {
                dd02 = JsonMapper.ToObject(www.text);
                for (int i = 0; i < dd01.Count; ++i)
                {
                    List<string> uu = new List<string>();
                    // Debug.Log(dd01[i].ToString());
                    JsonData jd = dd02[dd01[i].ToString()];

                    for (int j = 0; j < jd.Count; ++j)
                    {
                        uu.Add(jd[j].ToString());
                        //Debug.Log(jd[j].ToString());
                    }
                    Space.Add(dd01[i].ToString(), uu);
                }
            }
            else
            {
                Debug.Log("清单获取失败");
            }
        }
        else
        {
            Debug.Log("属性名数组获取失败");
        }
    }
}
