using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SwipeHandler : MonoBehaviour
{
    public UnityEvent OnSwipeRight, OnSwipeLeft, OnSwipeUp, OnSwipeDown;

    public delegate void onSwipeRight(float delta); public event onSwipeRight SwipeRight;
    public delegate void onSwipeLeft(float delta); public event onSwipeLeft SwipeLeft;
    public delegate void onSwipeUp(float delta); public event onSwipeUp SwipeUp;
    public delegate void onSwipeDown(float delta); public event onSwipeDown SwipeDown;
    
    InputHandler input;

    Vector3 a, b;
    
    [Range(0f, 1f)]
    public float swipeThreshold = 1f;
           float baseThreshold = Screen.width;
           float swipe;

    [SerializeField] bool inverted = true;

    private void Awake() {
        input = InputHandler.Instance;
    }

    void Start() {
        swipe = swipeThreshold * baseThreshold;
    }

    // Update is called once per frame
    void Update()
    {
        if(input.GetDown()){
            a = input.Position;
        }
        else if(input.Get()){
            b = input.Position;

            var diff = (b - a);
            if(diff.magnitude >= swipe)
                OnSwipe(diff);
        }
    }

    void OnSwipe(Vector3 diff){
        float dir = (inverted)? -1f:1f;
        
        float dx = diff.x * dir;
        if(Mathf.Abs(dx) > swipe){
            
            if(dx > 0f) {
                OnSwipeRight.Invoke();
                if(SwipeRight != null) SwipeRight(Mathf.Abs(dx));
            } else {   
                OnSwipeLeft.Invoke();
                if(SwipeLeft != null) SwipeLeft(Mathf.Abs(dx));
            }
        }

        float dy = diff.y * dir;
        if(Mathf.Abs(dy) > swipe){

            if(dy > 0f) {
                OnSwipeUp.Invoke();
                if(SwipeUp != null) SwipeUp(Mathf.Abs(dy));
            } else {
                OnSwipeDown.Invoke();
                if(SwipeDown != null) SwipeDown(Mathf.Abs(dy));
            }
        }

        a = b;
    }
}
