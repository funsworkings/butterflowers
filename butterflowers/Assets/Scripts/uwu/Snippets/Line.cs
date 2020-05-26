using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

using System.Collections.Generic;
using System.Linq;

using UnityEngine.UI;

[ExecuteInEditMode]
public class Line : MonoBehaviour {

    [SerializeField] RectTransform origin, destination;

    RectTransform rect;
    Image image;

    [SerializeField] GameObject dotPrefab = null;
    [SerializeField] List<LineDot> dots = new List<LineDot>();
    
    int numberOfDots = 0, previousNumberOfDots = 0;

    public bool active = true;
    public bool dotted = false;

    public float length {
        get{
            return rect.sizeDelta.y;
        }
    }

    public Vector2 start {
        get{
            return new Vector2(0f, -length/2f);
        }
    }

    public Vector2 end {
        get{
            return new Vector2(0f, length/2f);
        }
    }

    [Header("Dotted Attributes")]
        //[Range(0f, 1f)] 
        //public float dotDistribution = 1f;
        public float dotSize = 32f;
        

    
    void Update() {
        if(rect == null)
            rect = GetComponent<RectTransform>();   
        if(image == null)
            image = GetComponent<Image>(); 


        bool clearFlag = true;
        
        if(active){
            if(origin != null && destination != null){
                Vector2 dir = (destination.anchoredPosition - origin.anchoredPosition);
                float sa = (destination.sizeDelta.magnitude)/2f; float sb = (origin.sizeDelta.magnitude)/2f;
                
                rect.anchoredPosition = (origin.anchoredPosition + destination.anchoredPosition) / 2f - dir.normalized * (sa - sb)/2f;
                
                rect.sizeDelta = new Vector2(dotSize / 2f, Mathf.Max(0f, dir.magnitude - sa - sb));

                transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f);

                clearFlag = false;
            } 
        }

        if(!clearFlag){
            clearFlag = true;

            if(dotted){
                image.enabled = false;

                if(dotPrefab != null){
                    dotSize = Mathf.Max(0f, dotSize); // Ensure dot size is always >= 0
                    
                    previousNumberOfDots = transform.childCount; // Settle previous number of dots with current
                    numberOfDots = CalculateNumberOfDots();

                    if(numberOfDots > previousNumberOfDots)
                        AddDots((numberOfDots - previousNumberOfDots));
                    else if(numberOfDots < previousNumberOfDots)
                        RemoveDots((previousNumberOfDots - numberOfDots));

                    dots = GetComponentsInChildren<LineDot>().ToList();
                    
                    if(dots.Count > 0){
                        int index = 0; float interval = 0f;

                        Vector2 pos = start;
                        LineDot[] tempDots = dots.ToArray();
                        for(int i = 0; i < tempDots.Length; i++){
                            LineDot ld = tempDots[i];
                            if(ld != null){
                                index = ld.transform.GetSiblingIndex();
                                interval = (index * 1f)/numberOfDots;

                                pos = start + (end - start).normalized * interval * length;
                                    ld.x = pos.x;
                                    ld.y = pos.y;
                                    ld.radius = dotSize;
                            }
                            else
                                dots.RemoveAt(i);
                        }
                    }

                    clearFlag = false;
                }
            }
            else
                image.enabled = true;
        }
        else
            image.enabled = false;

        if(clearFlag)
            ClearDots();
    }

    int CalculateNumberOfDots(){
        int maxDots = Mathf.FloorToInt(length / (dotSize));

        float space = Mathf.Max(0f, length);
        int numDots = (space < dotSize)? 0:Mathf.FloorToInt(space / dotSize);

        return numDots;
    }

    void AddDots(int amount){
        GameObject dot_o;
        LineDot dot_sc;
        
        for(int i = 0; i < amount; i++){
            dot_o = Instantiate(dotPrefab, transform) as GameObject;
            dot_sc = dot_o.GetComponent<LineDot>();

            dots.Add(dot_sc);
        }
    }

    void RemoveDots(int amount){
        for(int i = 0; i < amount; i++){
            if(dots.Count >= i){
                if(dots[i] != null){
                    #if UNITY_EDITOR  
                        if(EditorApplication.isPlaying)
                            GameObject.Destroy(dots[i].gameObject);
                        else
                            GameObject.DestroyImmediate(dots[i].gameObject);
                    #else
                        GameObject.Destroy(dots[i].gameObject);
                    #endif
                }

                dots.RemoveAt(0);
            }
        }
    }

    void ClearDots(){
        dots = GetComponentsInChildren<LineDot>().ToList();

        while(dots.Count > 0){
            #if UNITY_EDITOR  
                if(EditorApplication.isPlaying)
                    GameObject.Destroy(dots[0].gameObject);
                else
                    GameObject.DestroyImmediate(dots[0].gameObject);
            #else
                GameObject.Destroy(dots[0].gameObject);
            #endif

            dots.RemoveAt(0);
        }
    }

}