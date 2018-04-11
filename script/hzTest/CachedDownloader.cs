using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Threading;
public class CachedDownloader : MonoBehaviour
{
    public string filePath;
    public bool web;
    public WWW www;
    public string pathforwww;
    private static CachedDownloader instance;
    private static readonly object SynObject = new object();
    public static CachedDownloader Instance
    {
        get
        {
            if (null == instance)
            {
                lock (SynObject)
                {
                    instance = new CachedDownloader();
                }
            }

            return instance;
        }
        set { }
    }

    // Use this for initialization
    void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {

    }

    static long GetInt64HashCode(string strtext)
    {
        var s1 = strtext.Substring(0, strtext.Length / 2);
        var s2 = strtext.Substring(strtext.Length / 2);
        var a = ((long)s1.GetHashCode()) << 0x20 | s2.GetHashCode();
        return a;
    }
    static bool CheckFileOutOfDate(string path)
    {
        System.DateTime write = File.GetLastAccessTimeUtc(path);
        System.DateTime now = System.DateTime.UtcNow;
        double totalhours = now.Subtract(write).TotalHours;
        return (totalhours > 300);
    }
    public IEnumerator DoLoad(WWW www, string path, bool web)
    {
        Debug.Log("缓存下载开始");
        yield return www;
        if (www.error == null)
        {
            //throw new Exception(www.error);
            if (web)
            {
                Debug.Log("正在下载保存" + www.url + "到" + path);
                File.WriteAllBytes(path, www.bytes);
                Debug.Log("已经下载保存" + www.url + "到" + path);
            }
            else
            {

                Debug.Log("从本地缓存加载.." + www.url);
            }
        }
        else
        {
            if (!web)
            {
                File.Delete(path);
            }
            Debug.Log("下载出错" + www.url);
            Debug.Log("错误"+www.error);
        }
    }
    public WWW GetCashWWW(string url)
    {
        if (url.EndsWith(".assetbundle"))
        {
            filePath += "/" + GetInt64HashCode(url) + ".assetbundle";
        }
        else
        {
            filePath += "/" + GetInt64HashCode(url);
        }


        // bool web = false;
        //WWW www;
        bool useCached = false;
        useCached = System.IO.File.Exists(filePath) && !CheckFileOutOfDate(filePath);
        if (useCached)
        {
            /*    pathforwww =
    #if UNITY_IPHONE
            filePath;
    #elif UNITY_STANDALONE_WIN || UNITY_EDITOR
            "file://" + filePath;
    #elif UNITY_ANDROID 
          "jar:file://" + filePath;
    #endif*/
            string pathforwww = "file://"+filePath;
            Debug.Log("TRYING FROM CACHE " + url + "  file " + pathforwww);
            www = new WWW(pathforwww);
        }
        else
        {
            web = true;
            www = new WWW(url);
        }

        //Debug.Log(filePath);
        loadMoudle.instance.StartCoroutine(DoLoad(www, filePath, web));
        //loadMoudle.instance.EditorCoroutineLooper.StartLoop(this, DoLoad(www, filePath, web));
        //Thread thread = new Thread(new ParameterizedThreadStart(DoLoad));
        return www;

    }
    public void Test()
    {
        Debug.Log("我去年买了个表");
    }
}
