using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewModelCamera : MonoBehaviour
{
    SC_FPSController PlayerInstance;
    Controls PlayerInput;

    [SerializeField] Animator animator;

    public static bool HasFlash = true; // security go brrr :-) (i am so lazy)
    Coroutine SwapingFlash;
    [SerializeField] float FlashSwapTime = 0.4f;

    public bool ToggleLineUpShot;
    bool LUS;

    //sway
    Vector3 CamTargetPos;

    private void Awake()
    {
        PlayerInput = new Controls();

        PlayerInput.FPS.FlashOnOff.performed += FlashOnOff_performed;
        // PlayerInput.FPS.LineUpShot.performed += LineUpShot_performed; // now called from PhotoCapture
    }

    //private void LineUpShot_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    //{
    //    ToggleCamera();
    //}

    void ToggleCamera()
    {
        if (!ToggleLineUpShot)
        {
            return;
        }

        LUS = !LUS;

        if (LUS)
        {
            animator.SetBool("LineUpShot", (PlayerInput.FPS.LineUpShot.ReadValue<float>() > 0));
        }
        else
        {
            //animator.SetBool("LineUpShot", (PlayerInput.FPS.LineUpShot.ReadValue<float>() > 1));
            Invoke("StupidExtraFunctionToUseInAnInvokeForSettingABool", 0.2f);
        }
    }

    void StupidExtraFunctionToUseInAnInvokeForSettingABool() => animator.SetBool("LineUpShot", (PlayerInput.FPS.LineUpShot.ReadValue<float>() > 1));

    private void FlashOnOff_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(SwapingFlash == null)
        {
            if (PhotoCapture.camEnabled)
            {
                Invoke("AnotherFunctionAbstractionJustForTimingButThisTimeForFlash", 0.5f);
                PhotoCapture.instance.SendMessage("ToggleCamera");
            }
            else AnotherFunctionAbstractionJustForTimingButThisTimeForFlash();
        }
    }

    void AnotherFunctionAbstractionJustForTimingButThisTimeForFlash() => StartCoroutine(SwicthFlashModes());

    private void Start()
    {
        PlayerInput.Enable();
    }

    private void Update()
    {
        transform.localPosition = CamTargetPos;

        if (!ToggleLineUpShot)
        {
            LineUpShot();
        }
        MovementFunc();
        CameraSway();
    }

    void MovementFunc()
    {
        Vector2 InputVector = PlayerInput.FPS.Move.ReadValue<Vector2>();

        float MoveX = InputVector.x;
        float MoveY = InputVector.y;

        bool Sprinting = PlayerInput.FPS.Sprint.ReadValue<float>() != 0;

        if (MoveX != 0 || MoveY != 0)
        {
            animator.SetBool("Walk", !Sprinting);
            animator.SetBool("Sprint", Sprinting);
        }
        else
        {
            animator.SetBool("Walk", false);
            animator.SetBool("Sprint", false);
        }
    }

    void CameraSway()
    {
        Vector2 InputVector = PlayerInput.FPS.Move.ReadValue<Vector2>();
        Vector2 MousePos = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * 25;

        Quaternion RotationZ = Quaternion.AngleAxis(MousePos.x * 1.5f, Vector3.forward);

        Vector3 CamSwayPos = Vector3.zero;

        CamSwayPos.x = -((MousePos.x / 10) * 0.06f);
        CamSwayPos.y = -((MousePos.y / 10) * 0.06f);

        CamTargetPos = Vector3.Lerp(CamTargetPos, CamSwayPos, Time.deltaTime * 8);
    }

    void LineUpShot()
    {
        animator.SetBool("LineUpShot", (PlayerInput.FPS.LineUpShot.ReadValue<float>() > 0));
    }

    IEnumerator SwicthFlashModes()
    {
        if(HasFlash)
        {
            animator.SetTrigger("FlashOff");
        }
        else
        {
            animator.SetTrigger("FlashOn");
        }

        yield return new WaitForSeconds(FlashSwapTime);

        if (HasFlash)
        {
            animator.SetLayerWeight(1, 1);
        }
        else
        {
            animator.SetLayerWeight(1, 0);
        }

        HasFlash = !HasFlash;
    }
}
