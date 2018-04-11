using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class SetParameters : MonoBehaviour {

    public int rate;
    public Scrollbar bar;
    public Text text;
	void Start () {
        //Application.targetFrameRate = 30;
        

    }
	
	// Update is called once per frame
	void Update () {
        //Application.targetFrameRate = (int)((bar.value) * 100)+30;
        //text.text = Application.targetFrameRate.ToString();
        //Debug.Log("帧率："+Application.targetFrameRate);
	}
    void SetAntialias() {
		
    }
}
