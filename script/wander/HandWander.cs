using UnityEngine;
using System.Collections;

public class HandWander : MonoBehaviour
{
    private float a01;  //a01和a02控制左右旋转
    private float a02;

    private float b01;  //b01和b02控制上下旋转
    private float b02;

   

  
	// Use this for initialization
	void Start () {

	}
    void Update()
    {
    }
    public void backWander() //退出漫游手动漫游时取消事件
    {
        FingerGestures.OnDragMove -= dragMove;
        FingerGestures.OnFingerStationary -= onFingerStationary;
    }
    void OnEnable()
    {
        a01=a02 = transform.localEulerAngles.y;
        b01=b02 = transform.localEulerAngles.x;
        FingerGestures.OnDragMove += dragMove;
        FingerGestures.OnFingerStationary += onFingerStationary;
    }
	// Update is called once per frame
    Vector2 previousPos;
    void dragMove(Vector2 fingerPos, Vector2 delta)
    {
        a01 += delta.x * 0.06f;
        a02 = Mathf.Lerp(a02, a01, Time.deltaTime * 30f);
        b01 += delta.y * 0.5f;
        b02 = Mathf.Lerp(b02, b01, Time.deltaTime * 30f);
        b02 = Mathf.Clamp(b02, -3, 3);
        transform.rotation = Quaternion.Euler(b02, a02, transform.localEulerAngles.z);               

    }

    void onFingerStationary(int fingerIndex, Vector2 fingerPos, float elapsedTime)
    {
        if (elapsedTime>0.7f)
        {
            GetComponent<CharacterController>().SimpleMove(transform.forward);
        }

    }

    public void dingwei01() //主卧
    {
        transform.position = new Vector3(-2.04f, 1.9f, -0.72f);
        transform.rotation = Quaternion.Euler(1.2032f, 178.486f, 0);
        a01 = a02 = transform.localEulerAngles.y;
        b01 = b02 = transform.localEulerAngles.x;
    }
    public void dingwei02() //厨房
    {
        transform.position = new Vector3(-11.33f, 2.06f, 6.78f);
        transform.rotation = Quaternion.Euler(1.2032f, 178.486f, 0);
        a01 = a02 = transform.localEulerAngles.y;
        b01 = b02 = transform.localEulerAngles.x;
    }

    public void dingwei03() //书房
    {
        transform.position = new Vector3(-14.54f, 2.06f, 6.78f);
        transform.rotation = Quaternion.Euler(1.2f,178.4f,-1.33f);
        a01 = a02 = transform.localEulerAngles.y;
        b01 = b02 = transform.localEulerAngles.x;
    }
    public void dingwei04() //阳台
    {
        transform.position = new Vector3(-18.39f, 2.06f, 6.6f);
        transform.rotation = Quaternion.Euler(1.2f, 178.4f, -1.33f);
        a01 = a02 = transform.localEulerAngles.y;
        b01 = b02 = transform.localEulerAngles.x;
    }
    public void dingwei05() //卫生间
    {
        transform.position = new Vector3(-14.091f, 1.966f, 2.13f);
        transform.rotation = Quaternion.Euler(-0.1008f, -86.7088f, 1.199f);
        a01 = a02 = transform.localEulerAngles.y;
        b01 = b02 = transform.localEulerAngles.x;
    }
    public void dingwei06() //次卧
    {
        transform.position = new Vector3(1.45f, 1.99f, 3.77f);
        transform.rotation = Quaternion.Euler(1.2f, 178.4f, -1.33f);
        a01 = a02 = transform.localEulerAngles.y;
        b01 = b02 = transform.localEulerAngles.x;
    }



}
