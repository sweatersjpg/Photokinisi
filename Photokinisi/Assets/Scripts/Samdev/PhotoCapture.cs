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

    [Header("Teleport")]

    public GameObject player;
    public Transform teleport;

    [Header("Assets")]

    [SerializeField]
    GameObject cameraModel;
    [SerializeField]
    GameObject viewFinder;
    [SerializeField]
    GameObject flashLight;

    [SerializeField] Image fadeOverlay;
    [SerializeField] GameObject shutter;

    [Header("SFX")]
    [SerializeField] AudioClip shutterSFX;

    Volume camVolume;
    Camera mCamera;

    [Header("Settings")]

    [SerializeField] int photoCount = 24;

    [SerializeField] Vector2 resolution;

    [Header("Effects")]

    [SerializeField] AnimationCurve fade;
    [SerializeField] float fadeDuration;
    [SerializeField] Color fadeTargetColor;

    GameObject hud;
    Color fadeStartingColor;
    float fadeTimer = 0;
    bool camviewOn = false;
    bool canTakePhoto = true;

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
        //rt.filterMode = FilterMode.Point;

        photos = new Texture2D[photoCount];

        for(int i = 0; i < photos.Length; i++)
        {
            photos[i] = new Texture2D((int) resolution.x, (int) resolution.y, TextureFormat.RGBA32, false);
            photos[i].filterMode = FilterMode.Point;
        }

        camVolume = GetComponent<Volume>();

        hud = GetComponentInChildren<Text>().gameObject;
        hud.SetActive(false);

        fadeTimer = -fadeDuration;
        fadeStartingColor = fadeOverlay.color;

    }

    void CapturePhoto()
    {
        if (!camEnabled || !canTakePhoto) return;
        canTakePhoto = false;

        shutter.SetActive(true);
        Invoke("HideShutter", 0.2f);

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

        if(photoIndex == 0) player.transform.position = teleport.position;
        AudioSource.PlayClipAtPoint(shutterSFX, mCamera.transform.position);

        Invoke("ToggleCamera", 0.4f);
    }

    private void LateUpdate()
    {
        if (Input.GetMouseButtonDown(0)) CapturePhoto();

        if (Input.GetMouseButtonDown(1))
        {
            ToggleCamera();
        }

        float t = (Time.time - fadeTimer) / fadeDuration;
        //cam_exp.postExposure.value = Mathf.Lerp(0, intencity, fade.Evaluate(t));
        fadeOverlay.color = Color.Lerp(fadeStartingColor, fadeTargetColor, fade.Evaluate(t));

    }

    //void LineUpShot_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    //{
    //    ToggleCamera();

    //    Debug.Log("Line up shot");
    //}

    void ToggleCamera()
    {

        if (camviewOn)
        {
            SetFadeTimer();
            Invoke("DisableCamview", fadeDuration / 2);
            Invoke("SetAbilityToRetake", fadeDuration * 2);
        }
        else
        {
            if (!canTakePhoto) return;
            Invoke("SetFadeTimer", 0.3f);
            Invoke("EnableCamview", 0.3f + fadeDuration / 2);
        }
        camviewOn = !camviewOn;

        cameraModel.SendMessage("ToggleCamera");
    }

    void SetAbilityToRetake() => canTakePhoto = true;

    void EnableCamview() => SetCamVisibility(true);

    void DisableCamview() => SetCamVisibility(false);

    void SetCamVisibility(bool active)
    {
        SkinnedMeshRenderer[] ms = cameraModel.GetComponentsInChildren<SkinnedMeshRenderer>();
        for(int i = 0; i < ms.Length; i++)
        {
            ms[i].enabled = !active;
        }

        viewFinder.SetActive(active);

        hud.SetActive(active);

        camVolume.enabled = active;
        camEnabled = active;

        flashLight.SetActive(active && ViewModelCamera.HasFlash);
    }

    void HideShutter() => shutter.SetActive(false);

    void SetFadeTimer() => fadeTimer = Time.time;
}
