using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
// using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class PhotoCapture : MonoBehaviour
{

    [HideInInspector] public static PhotoCapture instance;
    [HideInInspector] public static Texture2D[] photos;
    public static bool camEnabled = false;

    [SerializeField]
    GameObject cameraModel;
    [SerializeField]
    GameObject viewFinder;
    [SerializeField]
    GameObject flashLight;

    [SerializeField] Image fadeOverlay;

    Volume camVolume;
    Camera mCamera;

    [Header("Settings")]

    [SerializeField] int photoCount = 24;

    [SerializeField] Vector2 resolution;

    [Header("Effects")]

    [SerializeField] AnimationCurve fade;
    [SerializeField] float fadeDuration;
    [SerializeField] Color fadeTargetColor;

    Color fadeStartingColor;
    float fadeTimer = 0;
    bool camviewOn = false;

    int photoIndex = 0;

    RenderTexture rt;

    // Start is called before the first frame update
    void Start()
    {
        // makes it a singleton :)
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

        // get post processing effect

        camVolume = GetComponent<Volume>();

        fadeTimer = -fadeDuration;
        fadeStartingColor = fadeOverlay.color;
    }

    void CapturePhoto()
    {
        if (!camEnabled) return;

        Texture2D photo = photos[photoIndex];
        photoIndex = (photoIndex + 1) % photos.Length;
        viewFinder.SetActive(false);
        //if (ViewModelCamera.HasFlash) flashLight.SetActive(true);

        mCamera.targetTexture = rt;
        mCamera.Render();

        RenderTexture.active = rt;
        photo.ReadPixels(new Rect(0, 0, resolution.x, resolution.y), 0, 0);
        photo.Apply();

        mCamera.targetTexture = null;
        RenderTexture.active = null;
        viewFinder.SetActive(true);
        //flashLight.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) CapturePhoto();

        if (Input.GetMouseButtonDown(1))
        {
            if (camviewOn)
            {
                SetFadeTimer();
                Invoke("DisableCamview", fadeDuration / 2);
            }
            else
            {
                Invoke("SetFadeTimer", 0.3f);
                Invoke("EnableCamview", 0.3f + fadeDuration / 2);
            }
            camviewOn = !camviewOn;
        }

        float t = (Time.time - fadeTimer) / fadeDuration;
        //cam_exp.postExposure.value = Mathf.Lerp(0, intencity, fade.Evaluate(t));
        fadeOverlay.color = Color.Lerp(fadeStartingColor, fadeTargetColor, fade.Evaluate(t));

    }

    void EnableCamview()
    {
        SetCamVisibility(true);
    }

    void DisableCamview()
    {
        SetCamVisibility(false);
    }

    void SetCamVisibility(bool active)
    {
        SkinnedMeshRenderer[] ms = cameraModel.GetComponentsInChildren<SkinnedMeshRenderer>();
        for(int i = 0; i < ms.Length; i++)
        {
            ms[i].enabled = !active;
        }

        viewFinder.SetActive(active);
        //MeshRenderer[] vms = viewFinder.GetComponentsInChildren<MeshRenderer>();
        //for (int i = 0; i < vms.Length; i++)
        //{
        //    vms[i].enabled = active;
        //}

        camVolume.enabled = active;
        camEnabled = active;

        flashLight.SetActive(active && ViewModelCamera.HasFlash);
    }

    void SetFadeTimer()
    {
        fadeTimer = Time.time;
    }
}
