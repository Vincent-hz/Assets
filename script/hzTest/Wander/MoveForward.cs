using UnityEngine;
using System.Collections;

public class MoveForward : MonoBehaviour
{

    float Speed = 13.5f;

    void OnEnable() {

        // this.transform.position = new Vector3( loadMoudle.instance.ModelCenter.x,loadMoudle.instance.ModelCenter.y+0.9f,loadMoudle.instance.ModelCenter.z);
        this.transform.position = loadMoudle.instance.ModelCenter;
    }
    void Update()
    {

        //Debug.Log("持续检测键盘");
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(transform.forward * 0.03f, Space.World);
          //  Debug.Log("W键");
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(-transform.forward * 0.03f, Space.World);
           // Debug.Log("S键");
        }

        if (Input.GetKey(KeyCode.A))
        {
            // transform.Translate(-transform.right * 0.03f, Space.World);
            transform.Rotate(new Vector3(0.0f, -Time.deltaTime * 30f * 4.0f, 0.0f));
           // Debug.Log("A键");
        }

        if (Input.GetKey(KeyCode.D))
        {
            // transform.Translate(transform.right * 0.03f, Space.World);
            transform.Rotate(new Vector3(0.0f, Time.deltaTime * 30 * 4.0f, 0.0f));
           // Debug.Log("D键");
        }
        if (Input.GetKey(KeyCode.Q)) {
            transform.Translate(transform.up * 0.03f, Space.World);
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.Translate(-transform.up * 0.03f, Space.World);
        }
    }
}

