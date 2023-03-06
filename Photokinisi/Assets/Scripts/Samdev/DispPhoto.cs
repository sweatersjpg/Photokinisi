using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DispPhoto : MonoBehaviour
{
    [SerializeField] int photoIndex = 0;

    Renderer mRenderer;

    private void Start()
    {
        mRenderer = GetComponent<Renderer>();
    }

    // Start is called before the first frame update
    void Update()
    {
        mRenderer.material.SetTexture("_MainTex", PhotoCapture.photos[photoIndex]);
        enabled = false;
    }

}
