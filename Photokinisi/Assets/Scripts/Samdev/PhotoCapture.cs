using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class PhotoCapture : MonoBehaviour
{

    [HideInInspector] public static PhotoCapture instance;
    [HideInInspector] public static Texture2D[] photos;

    [SerializeField]
    Volume mainVolume;
    [SerializeField]
    GameObject vCamera;
    [SerializeField]
    GameObject viewFinder;

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

    DepthOfField cam_dof;
    MotionBlur cam_mb;
    FilmGrain cam_fg;
    ColorAdjustments cam_exp;

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

        mainVolume.profile.TryGet<ColorAdjustments>(out cam_exp);
        mainVolume.profile.TryGet<DepthOfField>(out cam_dof);
        mainVolume.profile.TryGet<MotionBlur>(out cam_mb);
        mainVolume.profile.TryGet<FilmGrain>(out cam_fg);

        fadeTimer = -fadeDuration;
        fadeStartingColor = fadeOverlay.color;
    }

    void CapturePhoto()
    {
        Texture2D photo = photos[photoIndex];
        photoIndex = (photoIndex + 1) % photos.Length;
        bool vactive = viewFinder.activeSelf;
        viewFinder.SetActive(false);

        mCamera.targetTexture = rt;
        mCamera.Render();

        RenderTexture.active = rt;
        photo.ReadPixels(new Rect(0, 0, resolution.x, resolution.y), 0, 0);
        photo.Apply();

        mCamera.targetTexture = null;
        RenderTexture.active = null;
        viewFinder.SetActive(vactive);
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
        SetCamVisibility(false);
    }

    void DisableCamview()
    {
        SetCamVisibility(true);
    }

    void SetCamVisibility(bool onoff)
    {
        SkinnedMeshRenderer[] ms = vCamera.GetComponentsInChildren<SkinnedMeshRenderer>();
        for(int i = 0; i < ms.Length; i++)
        {
            ms[i].enabled = onoff;
        }

        MeshRenderer[] vms = viewFinder.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < vms.Length; i++)
        {
            vms[i].enabled = !onoff;
        }

        cam_fg.active = !onoff;
        cam_dof.active = !onoff;
        cam_mb.active = !onoff;
    }

    void SetFadeTimer()
    {
        fadeTimer = Time.time;
    }
}
