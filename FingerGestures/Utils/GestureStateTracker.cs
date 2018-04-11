using UnityEngine;
using System.Collections;

public class GestureStateTracker : MonoBehaviour 
{
    public GestureRecognizer Gesture;

	void Awake() 
    {
        if( !Gesture )
            Gesture = GetComponent<GestureRecognizer>();
	}

    void OnEnable()
    {
        if( Gesture )
            Gesture.OnStateChanged += Gesture_OnStateChanged;
    }

    void OnDisable()
    {
        if( Gesture )
            Gesture.OnStateChanged -= Gesture_OnStateChanged;
    }

    void Gesture_OnStateChanged( GestureRecognizer source )
    {
        Debug.Log( "Gesture " + source + " changed from " + source.PreviousState + " to " + source.State );
    }
}
