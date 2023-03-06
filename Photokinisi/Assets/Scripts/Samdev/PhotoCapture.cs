using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class PhotoCapture : MonoBehaviour
{

    [HideInInspector] public static PhotoCapture instance;
    [HideInInspector] public static Texture2D[] photos;

    [SerializeField]
    Camera mCamera;

    [SerializeField]
    int photoCount = 24;

    [SerializeField]
    Vector2 resolution;

    int photoIndex = 0;

    RenderTexture rt;

    // Start is called before the first frame update
    void Start()
    {
        // make it a singleton :)
        if (instance == null) instance = this;
        else
        {
            enabled = false;
            return;
        }

        if (mCamera == null) mCamera = Camera.main;

        rt = new RenderTexture((int)resolution.x, (int)resolution.y, 32);
        rt.filterMode = FilterMode.Point;

        photos = new Texture2D[photoCount];

        for(int i = 0; i < photos.Length; i++)
        {
            photos[i] = new Texture2D((int) resolution.x, (int) resolution.y, TextureFormat.RGBA32, false);
            photos[i].filterMode = FilterMode.Point;
        }
    }

    void CapturePhoto()
    {
        Texture2D photo = photos[photoIndex];
        photoIndex = (photoIndex + 1) % photos.Length;

        mCamera.GetComponent<PixelPerfectCamera>().enabled = false;

        mCamera.targetTexture = rt;
        mCamera.Render();

        RenderTexture.active = rt;
        photo.ReadPixels(new Rect(0, 0, resolution.x, resolution.y), 0, 0);
        photo.Apply();

        mCamera.targetTexture = null;
        RenderTexture.active = null;
        mCamera.GetComponent<PixelPerfectCamera>().enabled = true;
    }

    private void Update()
    {
        // temporary!!!!

        if (Input.GetMouseButtonDown(0)) CapturePhoto();
    }
}
