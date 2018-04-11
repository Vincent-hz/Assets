using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class GestureMag: MonoBehaviour {
    /*
     * 功能：管理所有手势识别操作
     * 时间：2017/9/27
     * 作者：胡哲
     * **/
    //默认下所有手势操作都能通过鼠标实现
    public bool SelectedMove=true;//勾选移动标识符
    public bool SelectedRotate = true;//勾选旋转标识符
    public bool SelectedScale1 = true;//勾选放大标识符
    public bool SelectedScale2 = true;//勾选缩小标识符
    public static GestureMag instance;
   public void Awake()
    {
       
        instance = this;
    }
    void Update()
    {
       
    }
    /// <summary>
    /// 记录上次角度坐标便于计算下次计算
    /// </summary>
    Vector3 IniAngle; 
    //----------------------------------end----------------------------------------//
    /// <summary>
    /// 允许用户使用缩放手势来影响轨道距离
    /// </summary>
    public bool allowPinchZoom = true;
    public float idealDistance = 0;
    /// <summary>
    /// 摄像机与目标之间的最小距离
    /// </summary>
    public float minDistance = 1.0f;
    /// <summary>
    /// 初始相机距离
    /// </summary>
    public float initialDistance = 1.0f;
    /// <summary>
    /// 相机与目标的最大距离
    /// </summary>
    public float maxDistance = 200.0f;
    /// <summary>
    /// 速度影响触控放大
    /// </summary>
    public float pinchZoomSensitivity = 2.0f;
    /// <summary>
    /// 两个手指是否能移动相机.
    /// 相机目标点应用偏移量
    /// </summary>
    public bool allowPanning = false;
    public float panningSensitivity = 1.0f;
    /// <summary>
    /// 水平旋转速度
    /// </summary>
    public float yawSensitivity = 80.0f;
    public Transform panningPlane; 
    public bool invertPanningDirections = false;
    public static  Transform PreSelectedObject;
    private Material PreSelectedObjectMat;
    public  Vector3 idealPanOffset = Vector3.zero;
    public   Vector3 panOffset = Vector3.zero;
    public string IsLocal = "1";
    float lastPanTime = 0;
    public   float idealYaw = 0;
    private Transform selectedObj;
    public Material mSelectedMat;
    /// <summary>
    /// 垂直旋转速度
    /// </summary>
    public float pitchSensitivity = 80.0f;
    /// <summary>
    /// 是否用光滑的相机运动
    /// </summary>
    public bool smoothMotion = true;
    public float smoothZoomSpeed = 3.0f;
    public float smoothOrbitSpeed = 4.0f;
    public float idealPitch = 0;
    /// <summary>
    /// 在最小螺距和最大螺距之间保持的角值
    /// </summary>
    public bool clampPitchAngle = true;
    public float minPitch = -20;
    public float maxPitch = 80;
    private string objname;


    /// <summary>
    /// 锁定挂载相机的角度，限制旋转的角度范围
    /// </summary>
    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;

        if (angle > 360)
            angle -= 360;

        return Mathf.Clamp(angle, min, max);
    }

    /// <summary>
    /// 注册手势事件
    /// </summary> 
    void OnEnable()
    {
        loadMoudle.start_load += loadComplete;
        FingerGestures.OnFingerDragMove += OnFingerDragMove;
        FingerGestures.OnPinchMove += FingerGestures_OnPinchMove;
        FingerGestures.OnFingerTap += OnFingerTap;
        //FingerGestures.OnFingerLongPress += OnFingerLongPress;

        // FingerGestures.OnTwoFingerDragMove += FingerGestures_OnTwoFingerDragMove;
            idealYaw = 0;
            panOffset = Vector3.zero;
            idealDistance = 25;
            idealPitch = 30;
        idealPanOffset = Vector3.zero;
    }
    /// <summary>
    /// 注销手势事件
    /// </summary> 
    void OnDisable()
    {
        loadMoudle.start_load -= loadComplete;
        FingerGestures.OnFingerDragMove -= OnFingerDragMove;
        FingerGestures.OnPinchMove -= FingerGestures_OnPinchMove;
        FingerGestures.OnFingerTap -= OnFingerTap;
        //FingerGestures.OnFingerLongPress -= OnFingerLongPress;
        
    }

    //public GameObject componentNameM;
    //public GameObject componentNameR;
    //public GameObject componentNameS;
    //public GameObject componentNameF;
	//长按事件
	//void OnFingerLongPress(int fingerIndex, Vector2 fingerPos){
	//	Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
	//	RaycastHit hitInfo;
	//	if (Physics.Raycast (ray, out hitInfo)) {
		
	//		string name = hitInfo.transform.name;
	//		if (name.Contains ("[")) {
	//			int startIndex = name.IndexOf ("[");
	//			int endIndex = name.IndexOf ("]");
	//			string ww = name.Substring (startIndex+1,endIndex-startIndex-1);
	//			ComponentWebGL.instance.Dic [ww].SetActive (false);
	//		}
	//	}

	//}
    //手指按下固定
    void OnFingerTap(int fingerIndex, Vector2 fingerPos, int tapCount)
    {



            ////屏蔽UI层的点击
#if (IPHONE || ANDROID||WEBGL)
                    if (BaseInputModule.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
#else
            if (EventSystem.current.IsPointerOverGameObject())
#endif
                return;
           // chejuCtrl.touch = true;
            string preSelectedObjectName = "";
            string preSelectedObjectMatName = "";

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);//从摄像机发出到点击坐标的射线
            RaycastHit hitInfo;

            //if (Physics.Raycast(ray, out hitInfo) && !chejuCtrl.ceJuMode && !UICamera.isOverUI)
            //{
            //    if (hitInfo.collider)
            //    {
            //        objname = hitInfo.transform.name;
            //    if (scene == SceneNmae.Vr)
            //    {
            //    SelectName.text = objname;
            //    }

            //        //回复原对象的材质
            //        if (PreSelectedObject != null)
            //        {
            //            PreSelectedObject.GetComponentInChildren<MeshRenderer>().material = PreSelectedObjectMat;
            //            preSelectedObjectName = PreSelectedObject.name;
            //            preSelectedObjectMatName = PreSelectedObjectMat.name;
            //        }

            //        //远程同步
            //        //objname = "远程调用1";
            //        if (IsLocal == "0")
            //        {
            //            object[] para = new object[4];
            //            para[0] = hitInfo.transform.name;
            //            para[1] = mSelectedMat.name;
            //            para[2] = preSelectedObjectName;
            //            para[3] = preSelectedObjectMatName;

            //        }

            //        PreSelectedObject = hitInfo.transform;
            //        PreSelectedObjectMat = hitInfo.transform.GetComponentInChildren<MeshRenderer>().material;

            //        selectedObj = hitInfo.transform;
            //        //当选择某个对象时，要向显隐模块和进度模块传递数据
            //        jingDu.selectedObj = selectedObj;
            //        xianYing.selectObj = selectedObj;
            //        hitInfo.transform.GetComponentInChildren<MeshRenderer>().material = mSelectedMat;

            //    }
            //}        

    }
    /// <summary>
    /// 计算模型合适的倾斜的系数
    /// </summary>
    public float IdealPitch
    {
        get { return idealPitch; }
        set { idealPitch = clampPitchAngle ? ClampAngle(value, minPitch, maxPitch) : value; }
    }
    /// <summary>
    /// 设置模型合适的偏移的系数
    /// </summary>
    public float IdealYaw
    {
        get { return idealYaw; }
        set { idealYaw = value; }
    }
    /// <summary>
    /// 设置模型合适的偏移的三维坐标点
    /// </summary>
    public Vector3 IdealPanOffset
    {
        get { return idealPanOffset; }
        set { idealPanOffset = value; }
    }
    /// <summary>
    /// 绕目标物体旋转
    /// </summary>
    public Transform target;
    // Use this for initialization
    void Start()
    {
        if (!panningPlane)
        {
            panningPlane = this.transform;

        }
        // panningPlane = this.transform.position;
        Vector3 angles = transform.eulerAngles;
        IniAngle = angles;
        distance = IdealDistance = initialDistance;
        yaw = IdealYaw = angles.y;
        pitch = IdealPitch = angles.x;
    }
    /// <summary>
    /// 目标物体事件
    /// </summary>
    void loadComplete(Transform text)
    {
        target=text;
    }
    /// <summary>
    /// 计算模型与相机的合适距离
    /// </summary>
    public float IdealDistance
    {
        get { return idealDistance; }
        set { idealDistance = Mathf.Clamp(value, minDistance, maxDistance); }
    }
    /// <summary>
    /// 屏幕坐标系转换成世界坐标系
    /// </summary> 
    Vector3 GetWorldPos(Vector2 screenPos)
    {
        Camera mainCamera = Camera.main;
        return mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(transform.position.z - mainCamera.transform.position.z)));

    }


    //滑动中
    void OnFingerDragMove(int fingerIndex, Vector2 fingerPos, Vector2 delta)
    {

            #region
            if (SelectedRotate&&Input.GetMouseButton(2)&& !EventSystem.current.IsPointerOverGameObject())//只能旋转   //if (SelectedRotate)//只能旋转
        {
            //Debug.Log("正在旋转");
                if (Time.time - lastPanTime < 0.25f)
                    return;
#if (IPHONE || ANDROID||WEBGL)
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
#else
                if (EventSystem.current.IsPointerOverGameObject())
#endif
                    return;
                if (target)
                {
                    IdealYaw += delta.x * yawSensitivity * 0.01f;
                    IdealPitch -= delta.y * pitchSensitivity * 0.01f;
                }
            }
        #endregion
        if (SelectedMove&&Input.GetMouseButton(1)&& !EventSystem.current.IsPointerOverGameObject()) //只能移动    
        {
            if (allowPanning)          //if (allowPanning)
            {
                panningPlane.Translate(Vector3.right, Space.World);
                panningPlane.Translate(Vector3.up,Space.World);
                Vector3 move = -0.2f * panningSensitivity * (panningPlane.right * delta.x + Vector3.up* delta.y);
                Debug.Log("move:"+ panningPlane.right);
                if (invertPanningDirections)
                    IdealPanOffset -= move;
                else
                    IdealPanOffset += move;

                lastPanTime = Time.time;
            }
        }




    }

    //缩放
    void FingerGestures_OnPinchMove(Vector2 fingerPos1, Vector2 fingerPos2, float delta)
    {
        if (SelectedScale1)  //只能放大
        {
           // chejuCtrl.empty = true;
            ////屏蔽UI层的点击
            //if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            //{
            //    return;
            //}
#if (IPHONE || ANDROID||WEBGL)
                    if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
#else
            if (EventSystem.current.IsPointerOverGameObject())
#endif
                return;

            if (allowPinchZoom)
                IdealDistance -= delta * pinchZoomSensitivity * 0.1f;
        }
        else if (SelectedScale2)  //只能缩小
        {
            // chejuCtrl.empty = true;
            ////屏蔽UI层的点击
            //if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            //{
            //    return;
            //}
#if (IPHONE || ANDROID||WEBGL)
                    if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
#else
            if (EventSystem.current.IsPointerOverGameObject())
#endif
                return;

            if (allowPinchZoom)
                IdealDistance -= delta * pinchZoomSensitivity * 0.1f;
        }

    }

   
    float distance = 10.0f;
    float yaw = 0;
    float pitch = 0;
    public bool smoothPanning = true;
    public float smoothPanningSpeed = 8.0f;
    void Apply()  //这个方法是放在LateUpdate中的
    {

        if (target) // 场景相机盯的物体
        {
            if (smoothMotion)
            {
                distance = Mathf.Lerp(distance, IdealDistance, Time.deltaTime * smoothZoomSpeed);
                yaw = Mathf.Lerp(yaw, IdealYaw, Time.deltaTime * smoothOrbitSpeed);
                pitch = Mathf.Lerp(pitch, IdealPitch, Time.deltaTime * smoothOrbitSpeed);
            }
            else
            {
                distance = IdealDistance;
                yaw = IdealYaw;
                pitch = IdealPitch;
            }

            if (smoothPanning)
                panOffset = Vector3.Lerp(panOffset, idealPanOffset, Time.deltaTime * smoothPanningSpeed);
            else
                panOffset = idealPanOffset;

            transform.rotation = Quaternion.Euler(pitch, yaw, 0);
                transform.position = (target.position + panOffset*0.1f) - distance * transform.forward;
                //transform.position = (new Vector3(0,0,0)+ panOffset * 0.05f) - distance * transform.forward;
          // Debug.Log("target:"+target.position+target.transform.name+"this:"+transform.position);
          
           

        }



        /*
		if (!PhotonNetwork.connected)
		{
			//Debug.Log("Update() was called by Unity. Scene is loaded. Let's connect to the Photon Master Server. Calling: PhotonNetwork.ConnectUsingSettings();");

			//PhotonNetwork.ConnectToBestCloudServer("1");
			PhotonNetwork.ConnectUsingSettings ("1");
		}
		*/
    }

    bool a = false;
 
    
    //系统调用
    void LateUpdate()
    {
        Apply();


        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                //SelectedMove = true;
                //SelectedRotate = true;
                //SelectedScale = true;

            }
           
        }
        
    }
}


 //if (UIUtilities.IsCursorOnUI())
 //       {
 //           SelectedMove=false;
 //           SelectedRotate = false;
 //           SelectedScale = false;
 //       }
 //       else
 //       {
 //         