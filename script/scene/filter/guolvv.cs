using UnityEngine;
using System.Collections;
using LitJson;

public class guolvv : MonoBehaviour {
    private string objname;
    public GameObject LoadedModel;
    public Transform target;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


    public void dianji01()   //点击水电
    {
        //if (Overall.Md != null)
        //{
        //    Overall.Md.SetActive(false);
        //}

       StartCoroutine("LoadHouse2");
    }
    private IEnumerator LoadHouse2()
    {
        print("jia zai shui dian");
        string filename = "";
        string phpfilename = "";

#if UNITY_ANDROID
        phpfilename = "getFileName.php";
#elif UNITY_IPHONE
            phpfilename = "getFileNameios.php";
#endif

    WWW MS = new WWW("http://101.201.150.102:8081/index.php/Admin/GetModell/GetModell?PrjID=14&version_id=1");
    yield return MS;
        if (MS.text == "")
        {
            objname = "获取版本信息失败！";
        }
        else
        {
            objname = "获取版本信息成功！";
        }

        string ss = MS.text;
        print(ss);
        JsonData jd = JsonMapper.ToObject(ss);
        if (jd["item"].Count == 0)
        {

        }
        else if (jd["item"].Count > 0)
        {
            filename = "http://101.201.150.102:8081" + jd["item"][0]["path"].ToString().Substring(1);
            //WWW bundle = WWW.LoadFromCacheOrDownload(filename, 8);
            WWW bundle = CachedDownloader.Instance.GetCashWWW(filename);
            yield return bundle;
            if (bundle.error == null)
            {
                GameObject obj = bundle.assetBundle.mainAsset as GameObject;
                LoadedModel = Instantiate(obj);
                yield return LoadedModel;
                target = LoadedModel.transform;
        

                bundle.assetBundle.Unload(false);
                Resources.UnloadUnusedAssets();

                ////初始化模型状态
                //StartCoroutine(InitModelState());     //根据物体所处的状态 赋予材质

                GetCenter(target);

                // SetListData(target);
            }
            else
            {
                //objname = "error:" + bundle.error.ToString();
            }
        }

    }

    //计算模型的中心点
    static Vector3 GetCenter(Transform text)
    {

        //Transform parent = text;

        //Vector3 postion = parent.position;

        //Quaternion rotation = parent.rotation;

        //Vector3 scale = parent.localScale;

        //parent.position = Vector3.zero;

        //parent.rotation = Quaternion.Euler(Vector3.zero);

        //parent.localScale = Vector3.one;

        //Vector3 center = Vector3.zero;

        //Renderer[] renders = parent.GetComponentsInChildren<Renderer>();

        //foreach (Renderer child in renders)
        //{
        //    center += child.bounds.center;
        //}

        //center /= parent.GetComponentsInChildren<Renderer>().Length;

        //Bounds bounds = new Bounds(center, Vector3.zero);

        //foreach (Renderer child in renders)
        //{

        //    bounds.Encapsulate(child.bounds);

        //}

        //parent.position = postion;

        //parent.rotation = rotation;

        //parent.localScale = scale;

        //foreach (Transform t in parent)
        //{

        //    t.position = t.position - bounds.center;

        //}

        //parent.transform.position = bounds.center + parent.position;
        //return parent.transform.position;
        return Vector3.zero;
    }
}

