using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ItemMessages : MonoBehaviour {
    public static ItemMessages instance;
    public List<GameObject> items = new List<GameObject>();
    public GameObject contect;
   // public Vector2 contectSize;//显示信息的高度;
    public GameObject item;//列表项
   // public float itemHeight;
    public Vector3 itemLocalPos;
    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        else {
            Destroy(this);
        }
    }
    // Use this for initialization
    void Start () {
        contect = GameObject.Find("Contect");
       // contectSize = contect.GetComponent<RectTransform>().sizeDelta;
        item = Resources.Load("MyItem") as GameObject;
        //itemHeight = item.GetComponent<RectTransform>().rect.height;
        itemLocalPos = item.transform.localPosition;
        for (int vertexNumbers = 0; vertexNumbers < 8; ++vertexNumbers) {
            AddItem();
        }
	}
    public void AddItem() {//添加表项
        GameObject obj = Instantiate(item) as GameObject;
        obj.transform.parent = contect.transform;
        obj.transform.localPosition = new Vector3(itemLocalPos.x, itemLocalPos.y - 5f, 0);
        items.Add(obj);
       /* if(contectSize.y<=items.Count*itemHeight)//加高度
        {
            contect.GetComponent<RectTransform>().sizeDelta = new Vector2(contectSize.x, items.Count * itemHeight);
        }*/

    }
    //移除列表项
   /* public void RemoveItem(GameObject t)
    {
        int index = items.IndexOf(t);
        items.Remove(t);
        Destroy(t);

        for (int vertexNumbers = index; vertexNumbers < items.Count; vertexNumbers++)//移除的列表项后的每一项都向前移动
        {
            items[vertexNumbers].transform.localPosition += new Vector3(0, itemHeight, 0);
        }

        if (contectSize.y <= items.Count * itemHeight)//调整内容的高度
            contect.GetComponent<RectTransform>().sizeDelta = new Vector2(contectSize.x, items.Count * itemHeight);
        else
            contect.GetComponent<RectTransform>().sizeDelta = contectSize;
    }*/

}
