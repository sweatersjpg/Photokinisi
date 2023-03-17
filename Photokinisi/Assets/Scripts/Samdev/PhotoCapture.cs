using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
// using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class PhotoCapture : MonoBehaviour
{

    [HideInInspector] public static PhotoCapture instance;
    [HideInInspector] public static Texture2D[] photos;
    public bool camEnabled = false;

    GameObject player;

    [Header("Assets")]

    [SerializeField]
    GameObject cameraModel;
    [SerializeField]
    GameObject viewFinder;
    [SerializeField]
    GameObject flashLight;

    [Space]
    [SerializeField] Image fadeOverlay;
    [SerializeField] GameObject shutter;

    [Header("SFX")]
    [SerializeField] AudioClip shutterSFX;
    [SerializeField] AudioClip advFilm;
    [SerializeField] AudioClip flashOn;
    [SerializeField] AudioClip flashOff;

    Volume camVolume;
    Camera mCamera;

    [Header("Settings")]

    [SerializeField] int photoCount = 24;

    [SerializeField] Vector2 resolution;

    [SerializeField] bool changeSceneOnPhotoCapture = false;

    [Header("Effects")]

    [SerializeField] AnimationCurve fade;
    [SerializeField] float fadeDuration;
    [SerializeField] Color fadeTargetColor;

    [Space]
    public Text hudSettings;
    public Text hudCount;

    Color fadeStartingColor;
    float fadeTimer = 0;
    bool camviewOn = false;
    bool canTakePhoto = false;

    int photoIndex = 0;

    int lastScene = 1; // set this to starting scene
    bool sceneIsLoading = false;
    bool nextSceneReady = false;
    bool loadSceneNow = false;

    public int galleryBuildIndex = 2;

    RenderTexture rt;
    AudioSource audioSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Application.backgroundLoadingPriority = ThreadPriority.Low;

        audioSource = GetComponent<AudioSource>();

        player = SC_FPSController.PlayerController.gameObject;

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

        // Text[] hudText = GetComponentsInChildren<Text>();

        // hudSettings = hudText[0];
        // hudCount = hudText[1];
        hudCount.text = photoCount+"";

        hudCount.gameObject.SetActive(false);
        hudSettings.gameObject.SetActive(false);

        fadeTimer = -fadeDuration;
        fadeStartingColor = fadeOverlay.color;

        if(changeSceneOnPhotoCapture) PrepareNextScene(true);
    }

    void CapturePhoto()
    {
        if (!camEnabled || !canTakePhoto) return;
        canTakePhoto = false;

        shutter.SetActive(true);
        Invoke("HideShutter", 0.2f);

        Texture2D photo = photos[photoIndex];

        photoIndex = (photoIndex + 1) % photos.Length;
        hudCount.text = (photoCount - photoIndex) + "";
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

        // if(photoIndex == 0) player.transform.position = teleport.position;
        //AudioSource.PlayClipAtPoint(shutterSFX, mCamera.transform.position);

        Invoke("ToggleCamera", 0.4f);

        //Invoke("AdvanceFilm", 1f);
        //Invoke("AdvanceFilmSound", 1.1f);

        if (changeSceneOnPhotoCapture) PrepareNextScene(false);

        audioSource.PlayOneShot(shutterSFX);
        //if(!loadSceneNow) AudioSource.PlayClipAtPoint(shutterSFX, mCamera.transform.position);
    }

    void PrepareNextScene(bool firstTime)
    {
        if (sceneIsLoading) return;

        if (nextSceneReady)
        {
            //Debug.Log("activating scene change");
            loadSceneNow = true;
        }

        int i = Random.Range(1, SceneManager.sceneCountInBuildSettings);
        while (i == galleryBuildIndex || i == lastScene) i = Random.Range(1, SceneManager.sceneCountInBuildSettings);
        if (photoIndex == photoCount - 1) i = galleryBuildIndex;

        // Debug.Log(i);

        if (i != lastScene && lastScene != galleryBuildIndex)
        {
            lastScene = i;
            //Debug.Log("Starting Coroutine");
            Invoke("StartAsyncLoad", 1f);
        } else
        {
            Invoke("AdvanceFilm", 1.1f);
            Invoke("AdvanceFilmSound", 1.2f);
            Invoke("SetAbilityToRetake", 1.5f);
        }
    }

    void StartAsyncLoad() => StartCoroutine(LoadLevel());

    IEnumerator LoadLevel()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(lastScene, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false;
        sceneIsLoading = true;

        //Debug.Log("Level starting to load");

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                if (!nextSceneReady)
                {
                    Debug.Log("Scene ready :)");

                    Invoke("AdvanceFilm", 0.1f);
                    Invoke("AdvanceFilmSound", 0.2f);
                    Invoke("SetAbilityToRetake", 0.5f);

                }

                nextSceneReady = true;
                sceneIsLoading = false;

                if (loadSceneNow)
                {
                    //Debug.Log("Loading scene");

                    sceneIsLoading = false;
                    nextSceneReady = false;
                    loadSceneNow = false;

                    asyncLoad.allowSceneActivation = true;
                }
            }

            yield return null;
        }
    }

    private void LateUpdate()
    {
        if (PauseSystem.paused) return;

        if (Input.GetMouseButtonDown(0) && !sceneIsLoading) CapturePhoto();

        if (Input.GetMouseButtonDown(1))
        {
            ToggleCamera();
        }

        if(camEnabled)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                SC_FPSController.PlayerController.canMove = true;
                SC_FPSController.PlayerController.SendMessage("EngageCrouch");
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                SC_FPSController.PlayerController.canMove = false;
                SC_FPSController.PlayerController.SendMessage("DisengageCrouch");
            }
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
    void AdvanceFilm() => cameraModel.SendMessage("AdvanceFilm");
    void AdvanceFilmSound() => audioSource.PlayOneShot(advFilm);

    void PlayFlashOnSound() => Invoke("FlashOnSound", 0.4f);
    void PlayFlashOffSound() => Invoke("FlashOffSound", 0.4f);

    void FlashOnSound() => audioSource.PlayOneShot(flashOn, 0.2f);
    void FlashOffSound() => audioSource.PlayOneShot(flashOff, 0.2f);

    void ToggleCamera()
    {

        if (camviewOn)
        {
            SetFadeTimer();
            Invoke("DisableCamview", fadeDuration / 2);
            // Invoke("SetAbilityToRetake", 1);
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

    void DisableCamview()
    {
        SetCamVisibility(false);
        SC_FPSController.PlayerController.SendMessage("DisengageCrouch");
    }

    void SetCamVisibility(bool active)
    {
        SkinnedMeshRenderer[] ms = cameraModel.GetComponentsInChildren<SkinnedMeshRenderer>();
        for(int i = 0; i < ms.Length; i++)
        {
            ms[i].enabled = !active;
        }

        viewFinder.SetActive(active);

        hudCount.gameObject.SetActive(active);
        hudSettings.gameObject.SetActive(active);

        camVolume.enabled = active;
        camEnabled = active;

        flashLight.SetActive(active && ViewModelCamera.HasFlash);

        SC_FPSController.PlayerController.canMove = !active;
    }

    void HideShutter() => shutter.SetActive(false);

    void SetFadeTimer() => fadeTimer = Time.time;
}
