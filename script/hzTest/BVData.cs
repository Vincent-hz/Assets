using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BVData : MonoBehaviour {

    // Use this for initialization
    [HideInInspector]
    [SerializeField]
    private string mapdata = null;
    [HideInInspector]
    [SerializeField]
    public List<string> listdata = new List<string>();
    void Start()
    {

    }
    public void SetMapData(string datastr)
    {
        mapdata = datastr;
    }
    public void SetGameObjectData(List<string> templist)
    {
        listdata = templist;
    }
    public List<string> GetGameObjectData()
    {
        return listdata;
    }
    public string GetMapData()
    {
        return mapdata;
    }
    // Update is called once per frame
    void Update()
    {

    }
}
