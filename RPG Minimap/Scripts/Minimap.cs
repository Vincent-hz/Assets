using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class Minimap : MonoBehaviour {
	
	Camera cam;								//The camera component of the minimap
	
	public Transform target;				//The target the minimap should follow
	
	public Texture2D minimapFrame;			//The frame texture around the minimap
	public Texture2D plusbutton;			//The plus button texture used to zoom out
	public Texture2D minusbutton;			//The minus button texture used to zoom in
	
	public Transform depthPlane;			//The plane that makes sure the minimap is draw an a circle
	public Transform playerCircle;			//The plane that holds the texture we want in the middle of the minimap
	
	public int minSize;						//How far can the player zoom in?
	public int maxSize;						//How far can the player zoom out?
    public GameObject manYouCam;

   // public float xz = 0;

	void Start () {
		cam = GetComponent<Camera>();
       
        playerCircle.localScale = new Vector3(2,2,2);
        // Debug.Log("渲染层数："+ cam.cullingMask);
        //计算小地图的大小和位置
        cam.pixelRect = new Rect(Screen.width * 0.83f - Screen.height * 0.015f - 35, Screen.height - Screen.width * 0.175f - Screen.height * 0.015f + 10, Screen.width * 0.18f, Screen.width * 0.175f);
        
    }
    void OnEnable() {
        
        this.transform.position = new Vector3(target.position.x-4, 500, target.position.z+2);
        
    }
	// Update is called once per frame
	void Update () {
        //Scale the depth plane so that the minimap is always as a perfect circle
        depthPlane.localScale = new Vector3(cam.orthographicSize, cam.orthographicSize, cam.orthographicSize);
		
		
	}
	
	void OnGUI() {
		Event curEvent = Event.current;
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                //Debug.Log("小地图：" + hitInfo.point);
                manYouCam.transform.position = new Vector3(hitInfo.point.x,hitInfo.point.y+1f,hitInfo.point.z);
            }
        }
        //计算小地图上的框架的大小和位置
        Rect frameRect = new Rect(cam.pixelRect.x+20 , Screen.height - cam.pixelRect.y - cam.pixelRect.height - 10, cam.pixelRect.height+10, cam.pixelRect.height+10);
        // Rect frameRect = new Rect(cam.pixelRect.x, Screen.height - cam.pixelRect.y - cam.pixelRect.height, cam.pixelRect.width, cam.pixelRect.height);
        GUI.DrawTexture(frameRect, minimapFrame);
		GUILayout.BeginArea(frameRect);
		//Calculate the size and positon of the frame around the plus button
		Rect plus = new Rect(frameRect.width * 0.9f+3, frameRect.height * 0.6f+10, frameRect.width * 0.15f-15, frameRect.width * 0.15f-15);
		//Calculate the size and positon of the frame around the minus button
		Rect minus = new Rect(frameRect.width * 0.9f+3, frameRect.height * 0.825f-15, frameRect.width * 0.15f-15, frameRect.width * 0.15f-15);
		GUI.DrawTexture(plus, plusbutton);
		GUI.DrawTexture(minus, minusbutton);
		
		//If we press the minus button zoom out
		if(plus.Contains(curEvent.mousePosition) && curEvent.type == EventType.MouseUp && curEvent.button == 0 && cam.orthographicSize > minSize) {
			cam.orthographicSize -= 2;
		}
		//If we press the plus button zoom in
		else if(minus.Contains(curEvent.mousePosition) && curEvent.type == EventType.MouseUp && curEvent.button == 0 && cam.orthographicSize < maxSize) {
			cam.orthographicSize += 2;
		}
		GUILayout.EndArea();
	}
}
