using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraSettings : MonoBehaviour
{
    [SerializeField]
    Volume camVolume;

    AudioSource audioSource;

    DepthOfField dof;
    ColorAdjustments ca;
    MotionBlur mb;
    Text hud;

    [Header("SFX")]
    [SerializeField] AudioClip apertureClick;
    [SerializeField] AudioClip shutterClick;

    [Header("Sensitivity")]

    [SerializeField]
    float focusSensitivity;
    [SerializeField]
    float zoomSensitivity = 1;
    [SerializeField]
    float EVOffset = 16;
    [SerializeField]
    float EVSensitivity = 1;

    [Header("Lens Metrics")]

    [SerializeField]
    float minFocalLength = 50;
    [SerializeField]
    float maxFocalLength = 200;

    [SerializeField]
    float diameter = 27.77f;

    // float startingFov;

    [Header("Settings")]

    [SerializeField] float zoom = 0; // percentage
    [SerializeField] float focus = 0; // percentage (0==0.4m, 1==infinity)
    [SerializeField] float aperture = 1; // percentage (1 == fully open, 0 == fully closed)
    [SerializeField] float shutterSpeed = 64; // 1/shutterSpeed == exposure duration

    // Start is called before the first frame update
    void Start()
    {
        camVolume.profile.TryGet<DepthOfField>(out dof);
        camVolume.profile.TryGet<ColorAdjustments>(out ca);
        camVolume.profile.TryGet<MotionBlur>(out mb);

        // startingFov = Camera.main.fieldOfView;

        Text[] hudText = GetComponentsInChildren<Text>();
        hud = hudText[0];

        audioSource = GetComponent<AudioSource>();

    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotoCapture.camEnabled)
        {
            //Camera.main.fieldOfView = startingFov;
            Camera.main.fieldOfView = PauseSystem.pauseSystem.FOV;
            return;
        }

        if (PauseSystem.paused) return;

        // get inputs
        focus = Mathf.Clamp(focus - Input.mouseScrollDelta.y * focusSensitivity, 0, 0.99f);
        dof.focusDistance.value = -0.4f / (focus - 1);

        // allow focusing but nothing else when player can move
        if (SC_FPSController.PlayerController.canMove) return;

        zoom += Input.GetAxis("Vertical") * Time.deltaTime * zoomSensitivity;
        zoom = Mathf.Clamp(zoom, 0, 1);

        float tempShutterSpeed = shutterSpeed;
        if (Input.GetKeyDown(KeyCode.D)) shutterSpeed *= 2;
        if (Input.GetKeyDown(KeyCode.A)) shutterSpeed /= 2;
        shutterSpeed = Mathf.Clamp(shutterSpeed, 32, 2048);
        if(tempShutterSpeed != shutterSpeed) audioSource.PlayOneShot(shutterClick);

        float tempAperture = aperture;

        float[] fstops = { 1.8f, 2.2f, 2.8f, 3.3f, 4, 4.7f, 5.6f, 6.7f, 8, 9.4f, 11, 13.3f, 16, 18.6f, 22 };
        int i = FindClosest(50 / (diameter * aperture), fstops);

        if (Input.GetKeyDown(KeyCode.E) && i < fstops.Length - 1) aperture = 50 / (fstops[i+1] * diameter);
        if (Input.GetKeyDown(KeyCode.Q) && i > 0) aperture = 50 / (fstops[i - 1] * diameter);

        // aperture = focalLength / (fstops[i] * diameter);

        if(tempAperture != aperture) audioSource.PlayOneShot(apertureClick);

        // do calculations
        float focalLength = Mathf.Lerp(minFocalLength, maxFocalLength, zoom);
        float fstop = focalLength / (diameter*aperture);
        float EV = 2 * Mathf.Log(fstop, 2) - Mathf.Log(1 / shutterSpeed);

        //Debug.Log(fstop + " : " + EV);

        // apply values
        dof.focalLength.value = focalLength;
        dof.aperture.value = fstop;

        float[] blurIntensities = { 0.75f, 0.25f, 0.06f, 0.01f, 0, 0, 0};
        mb.intensity.value = blurIntensities[(int) Mathf.Log(shutterSpeed, 2) - 5];

        ca.postExposure.value = -EV*EVSensitivity + EVOffset;

        Camera.main.focalLength = focalLength;

        UpdateHUD(shutterSpeed, 50 / (diameter * aperture));
    }

    void UpdateHUD(float shutterSpeed, float fstop)
    {
        float[] fstops = { 1.8f, 2.2f, 2.8f, 3.3f, 4, 4.7f, 5.6f, 6.7f, 8, 9.4f, 11, 13.3f, 16, 18.6f, 22 };
        float[] sspeeds = { 30, 60, 125, 250, 500, 1000, 2000 };

        float f = fstops[FindClosest(fstop, fstops)];
        float s = sspeeds[FindClosest(shutterSpeed, sspeeds)];

        string text = "1/" + s + "\nF" + f;
        if (ViewModelCamera.HasFlash) text += "\nFLASH";

        hud.text = text;
    }

    int FindClosest(float n, float[] l)
    {
        int best = 0;

        for(int i = 0; i < l.Length; i++)
        {
            float a = Mathf.Abs(n - l[i]);
            float b = Mathf.Abs(n - l[best]);

            if (best == -1 || a < b) best = i;
        }

        return best;
    }
}
