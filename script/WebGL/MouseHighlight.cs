using UnityEngine;
using System.Collections;


public class MouseHighlight : MonoBehaviour {
    /*
     * 功能：处理构架高亮发光
     * 时间：2017/10/18
     * */


    public GameObject gameCheck;//标识对象（当前选中的构件）
	public static MouseHighlight instance;


	void Awake(){
		instance = this;
	}

    void Update () {

      
    }

    /// <summary>
    /// 设置物体高亮
    /// </summary>
    /// <param name="obj"></param>
    public void SetObjectHighlight(GameObject obj)
    {
        if(gameCheck == null) {
            //RemoveComponent(obj);
           
            AddComponent(obj);
        }
        else if(gameCheck == obj) {
           RemoveComponent(obj);
        }
        else {
            RemoveComponent(gameCheck);
            AddComponent(obj);
        }
    } 

    /// <summary>
    /// 批量发光
    /// </summary>
    /// <param name="obj"></param>
    public void SetObjectsHighlight(GameObject obj) {
       
            AddComponent(obj);
           // Debug.Log(11111111);
       
    }

    /// <summary>
    /// 移出组件
    /// </summary>
    /// <param name="obj"></param>
    public void RemoveComponent(GameObject obj)
    {
        if(obj.GetComponent<SpectrumController>() != null) {
            Destroy(obj.GetComponent<SpectrumController>());
        }

        if(obj.GetComponent<HighlightableObject>() != null) {
            Destroy(obj.GetComponent<HighlightableObject>());
        }

        gameCheck = null;
    }

    /// <summary>
    /// 添加高亮组件
    /// </summary>
    /// <param name="obj"></param>
    public void AddComponent(GameObject obj)
    {
        if(obj.GetComponent<SpectrumController>() == null) {
            obj.AddComponent<SpectrumController>();
        }
        gameCheck = obj;
    }

}
