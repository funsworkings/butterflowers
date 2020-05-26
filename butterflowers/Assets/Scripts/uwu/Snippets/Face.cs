using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face : MonoBehaviour
{
    [SerializeField] Material material;

    private void Awake() {
        material = GetComponent<MeshRenderer>().sharedMaterial;
    }

    public void Blit(Texture tex){
        if(material == null) return;

        if(tex != null)
            material.mainTexture = tex;
        else
            material.mainTexture = null;
    }
}
