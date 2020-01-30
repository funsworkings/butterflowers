using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
    using UnityEditor;
#endif

[System.Serializable]
public class InputEventData {
    public string id;

    public Vector3 position;

    public Vector3 m_delta;
    public Vector3 delta {
        get{
            return m_delta;
        }
        set{
            m_delta = value;
            
            // Top: 90, Bot: -90, Left: -180, Right: 0
            if(value.x != 0f)
                angle = Mathf.Atan2(value.y, value.x);  
            else
                angle = 0f;
        }
    }
    
    ///<summary>Angle of input measured from delta in degrees*</summary>
    public float angle = 0f;

    public float timer = 0f;
    public enum State {
        Began,
        Continue,
        Cancel
    }
    public State state = State.Began;

    public void Update(float dt){ timer += dt; }

    public InputEventData(){
        position = Vector3.zero;
        m_delta = Vector3.zero;
    }

    public InputEventData(string id, Vector3 position){
        this.id = id;
        this.position = position;
    }
}


#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(InputEventData))]
    public class InputEventDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 16f * 2;
        }

        // Draw the property inside the given rect
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var positionProp = property.FindPropertyRelative("position");
            var deltaProp = property.FindPropertyRelative("m_delta");
            var timerProp = property.FindPropertyRelative("timer");
            var stateProp = property.FindPropertyRelative("state");

            EditorGUIUtility.wideMode = true;
            EditorGUIUtility.labelWidth = 30;

            rect.height /= 2;
            positionProp.vector3Value = EditorGUI.Vector3Field(rect, "pos", positionProp.vector3Value);
            rect.y += rect.height;
            deltaProp.vector3Value = EditorGUI.Vector3Field(rect, "delta", deltaProp.vector3Value);
        /* rect.y += rect.height/4;
            timerProp.floatValue = EditorGUI.FloatField(rect, "t", timerProp.floatValue);
            rect.y += rect.height/4;
            stateProp.enumValueIndex = EditorGUI.IntField(rect, "state", stateProp.enumValueIndex);*/

        }
    }
#endif


public class InputHandler : Singleton<InputHandler>
{
    public delegate void OnDown(InputEventData e);
    public event OnDown onDown;

    public delegate void OnMove(InputEventData e);
    public event OnMove onMove;

    public delegate void OnUp(InputEventData e);
    public event OnUp onUp;

    public delegate void OnClick(InputEventData e);
    public event OnClick onClick;

    public delegate void OnDrag(InputEventData e);
    public event OnDrag onDrag;

    [SerializeField] InputEventData eventData, lastEventData;
                     Vector3 a, b;

    [SerializeField] float sleep = .03f;
                     float time = 0f;
    [SerializeField] float dragThreshold = 0f;
                     bool drag = false;

    int touches = 0, maxTouches = 0;

    private void Start() {
        eventData = new InputEventData();
        lastEventData = new InputEventData();
    }
	
    bool up = false, down = false, cont = false, lastDown = false;
	// Update is called once per frame
	void Update () {
        Vector3 pos = Vector3.zero;

        // Reset input states to false
        down = false; cont = false; up = false; lastDown = false;

        pos = Position = Input.mousePosition;

        if (Input.GetMouseButtonDown(0))
        {
            Down(pos); down = true;
        }
        else if (Input.GetMouseButton(0))
        {
            Continue(pos); cont = true; 
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Up(pos); up = true;
        }

        if(Input.anyKeyDown && !down) lastDown = true;
    }

    // Mirrors for classic input
    public bool GetDown(){ return down; }
    public bool Get(){ return cont; }
    public bool GetUp(){ return up; }

    public bool GetLastDown(){ return lastDown; } // Get latest touch down

    void Down(Vector3 pos){
        a = Position = pos;
        time = 0f;

        if(onDown != null)
            onDown(eventData);
    }

    void Continue(Vector3 pos){
        b = Position = pos;

        float dist = Vector3.Distance(a, b);
        if(dist > dragThreshold){
            if(onMove != null)
                onMove(eventData);
            
            drag = true; // Global drag event on up
        }

        if(time <= 0f || time >= sleep){
            Delta = (b - a);
            a = b;

            if(time >= sleep)
                time = 0.001f;
        }
        else
            time += Time.deltaTime;
    }

    void Up(Vector3 pos){
        b = Position = pos;

        if(!drag){
            if(onClick != null)
                onClick(eventData);
        } else {
            if(onDrag != null)
                onDrag(eventData);
        }

        drag = false;
        if(onUp != null)
            onUp(eventData);
    }

    public Vector3 GetTap(){

        return Vector3.zero;
    }

    public Vector3 GetDrag(){

        return Vector3.zero;
    }
    

    public Vector3 Position {
        get{ 
            return eventData.position; 
        }

        set { 
            eventData.position = value; 
        }
    }

    public Vector3 LastPosition {
        get {
            return lastEventData.position;
        }

        set {
            lastEventData.position = value;
        }
    }

    public Vector3 Delta { 
        get{
            return eventData.delta;
        }

        set {
            eventData.delta = value;
        }
    }

    public float Speed {
        get{
            return eventData.delta.magnitude;
        }
    }
}
