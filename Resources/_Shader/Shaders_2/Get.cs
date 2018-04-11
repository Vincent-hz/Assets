using UnityEngine;
using System.Collections;

public class Get : MonoBehaviour {

	void Start ()
    {
        Mesh ms=transform.GetComponent<MeshFilter>().mesh;
     
	}
	
}
