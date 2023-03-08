using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class CameraSettings : MonoBehaviour
{
    [SerializeField]
    Volume camVolume;

    DepthOfField dof;
    ColorAdjustments ca;
    MotionBlur mb;

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

    float startingFov;

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

        startingFov = Camera.main.fieldOfView;
    }

    // Update is called once per frame
    void Update()
    {
        if(!PhotoCapture.camEnabled)
        {
            Camera.main.fieldOfView = startingFov;
            return;
        }

        // get inputs
        focus = Mathf.Clamp(focus + Input.mouseScrollDelta.y * focusSensitivity, 0, 0.99f);

        zoom += Input.GetAxis("Vertical") * Time.deltaTime * zoomSensitivity;
        zoom = Mathf.Clamp(zoom, 0, 1);

        if (Input.GetKeyDown(KeyCode.D)) shutterSpeed *= 2;
        if (Input.GetKeyDown(KeyCode.A)) shutterSpeed /= 2;
        shutterSpeed = Mathf.Clamp(shutterSpeed, 32, 2048);

        if (Input.GetKeyDown(KeyCode.E)) aperture += 0.1f;
        if (Input.GetKeyDown(KeyCode.Q)) aperture -= 0.1f;
        aperture = Mathf.Clamp(aperture, 0.15f, 1);

        // do calculations
        float focalLength = Mathf.Lerp(minFocalLength, maxFocalLength, zoom);
        float fstop = focalLength / (diameter*aperture);
        float EV = 2 * Mathf.Log(fstop, 2) - Mathf.Log(1 / shutterSpeed);

        //Debug.Log(fstop + " : " + EV);

        // apply values
        dof.focusDistance.value = -0.4f / (focus - 1);
        dof.focalLength.value = focalLength;
        dof.aperture.value = fstop;

        float[] blurIntensities = { 0.75f, 0.25f, 0.06f, 0.01f, 0, 0, 0};
        mb.intensity.value = blurIntensities[(int) Mathf.Log(shutterSpeed, 2) - 5];

        ca.postExposure.value = -EV*EVSensitivity + EVOffset;

        Camera.main.focalLength = focalLength;

    }
}
