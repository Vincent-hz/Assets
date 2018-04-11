using UnityEngine;
using System.Collections;

public class DragPoint : MonoBehaviour
{

    /*
     * 功能：该脚本主要用于触发测量点拖拽事件，实时更新反馈点的坐标信息
     * 时间：2017/12/5
     */

    private Vector3 _vec3TargetScreenSpace;// 目标物体的屏幕空间坐标  
    private Vector3 _vec3TargetWorldSpace;// 目标物体的世界空间坐标  
    private Transform _trans;// 目标物体的空间变换组件  
    private Vector3 _vec3MouseScreenSpace;// 鼠标的屏幕空间坐标  
    private Vector3 _vec3Offset;// 偏移
    private bool IsCollider;//碰撞标识符
    private Vector3 leavePos;//离开碰撞坐标点
    private float distance;
    void Awake()
    {
        _trans = transform;
    }
    void Start()
    {
        //Shader shader = Shader.Find("Yogi2/ImageEffect/Occlusion");
        //BallMat = new Material(shader);
        //BallMat.hideFlags = HideFlags.HideAndDontSave;
        //BallMat.SetColor("_MainColor", Color.red);
        //Debug.Log("变色");
    }
    void Update() {
        distance=Vector3.Distance(this.transform.position,Camera.main.transform.position);
        this.transform.localScale = new Vector3(distance/50, distance / 50, distance / 50);
    }
    void OnTriggerStay(Collider collider)
    {
        IsCollider = true;
        if (Input.GetMouseButtonUp(0)) {
            leavePos = this.transform.position;
        }
      // BallMat.SetColor("_MainColor", Color.red);
        GetComponent<MeshRenderer>().material.color = Color.red;
       // MouseHighlight.instance.SetObjectsHighlight(this.gameObject);
    }
    void OnTriggerExit(Collider collider)
    {
        IsCollider = false;
        Debug.Log("离开了碰撞");
       // BallMat.SetColor("_MainColor", Color.yellow);
       // leavePos = this.transform.position;
        GetComponent<MeshRenderer>().material.color = Color.yellow;
        //MouseHighlight.instance.RemoveComponent(this.gameObject);
    }
    IEnumerator OnMouseDown()
    {
        
            // 把目标物体的世界空间坐标转换到它自身的屏幕空间坐标   
            _vec3TargetScreenSpace = Camera.main.WorldToScreenPoint(_trans.position);
            // 存储鼠标的屏幕空间坐标（Z值使用目标物体的屏幕空间坐标）   
            _vec3MouseScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, _vec3TargetScreenSpace.z);
            // _vec3MouseScreenSpace = new Vector3(Input.mousePosition.x,  _vec3TargetScreenSpace.z,Input.mousePosition.y);
            // 计算目标物体与鼠标物体在世界空间中的偏移量   
            _vec3Offset = _trans.position - Camera.main.ScreenToWorldPoint(_vec3MouseScreenSpace);
            // _vec3Offset = cam.ScreenToWorldPoint(_vec3MouseScreenSpace);

            // 鼠标左键按下   
            while (Input.GetMouseButton(0))
            {
                
                    // 存储鼠标的屏幕空间坐标（Z值使用目标物体的屏幕空间坐标）  
                    _vec3MouseScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, _vec3TargetScreenSpace.z);
                    // 把鼠标的屏幕空间坐标转换到世界空间坐标（Z值使用目标物体的屏幕空间坐标），加上偏移量，以此作为目标物体的世界空间坐标  
                    _vec3TargetWorldSpace = Camera.main.ScreenToWorldPoint(_vec3MouseScreenSpace) + _vec3Offset;
                    // 更新目标物体的世界空间坐标   
                    _trans.position = _vec3TargetWorldSpace;
                    //拖拽点基本逻辑，区分面积点与直线点，主要通过下标与对应的名字来区分（注意中间点是反复存储，首尾点缝合起来的逻辑）
                    DragBall(this.transform.position);
                // 等待固定更新 
                    yield return new WaitForFixedUpdate();

            }
            //if (Input.GetMouseButtonUp(0) && IsCollider) {
            //    leavePos = this.transform.position;
            //}
             if (Input.GetMouseButtonUp(0) && !IsCollider)
            {
                DragBall(leavePos);
                this.transform.position = leavePos;
            }
           
        }
    //拖拽点基本逻辑，区分面积点与直线点，主要通过下标与对应的名字来区分（注意中间点是反复存储，首尾点缝合起来的逻辑）
    void DragBall(Vector3 currentPos) {  
            int index = int.Parse(this.name);
                    if (index == 1)
                    {
                        RayMeasure.instance.lv[0] = currentPos;
                        if (RayMeasure.instance.IsArea)
                        {
                            RayMeasure.instance.lv[RayMeasure.instance.lv.Count - 1] = currentPos;
                        }
                    }
                    else if (RayMeasure.instance.lv.Count == 2) {
                        RayMeasure.instance.lv[1] = currentPos;
                    }
                    else if (RayMeasure.instance.lv.Count > 2)
                    {
                        if (RayMeasure.instance.IsArea)
                        {
                            RayMeasure.instance.lv[(index - 1) * 2 - 1] = currentPos;
                            RayMeasure.instance.lv[(index - 1) * 2] = currentPos;
                        }
                        else
                        {
                            if (RayMeasure.instance.lv.Count == ((index - 1) * 2))
                            {
                                RayMeasure.instance.lv[(index - 1) * 2 - 1] = currentPos;
                                //Debug.Log("最后一个点"+this.name);
                            }
                            else
                            {
                                RayMeasure.instance.lv[(index - 1) * 2 - 1] = currentPos;
                                RayMeasure.instance.lv[(index - 1) * 2] = currentPos;
                            }
                        }
                    }
    }
}

