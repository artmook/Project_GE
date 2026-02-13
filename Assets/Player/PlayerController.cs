using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Video;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // ================================================================
    // 1. 변수 선언
    // ================================================================
    [Header("Status")]
    public bool hasCamcorder = false;  // 캠코더 보유 여부
    public bool IsRunning { get; private set; }

    [Header("Camcorder Settings")]
    public bool camcorderOn = false;
    public GameObject camcorderUI;
    public CanvasGroup camcorderBrightness;
    public float camBrightnessAmount = 0.04f;

    // [자동 연결] 배터리 초기화를 위한 컨트롤러
    public CamcorderController camcorderController;

    // [중요] 리셋 시 다시 나타날 바닥의 캠코더 아이템
    public GameObject worldCamcorderItem;

    public bool isInvincible = false;

    [Header("Camcorder FX")]
    public CanvasGroup noiseOverlay;
    public VideoPlayer noiseVideo;
    public float noiseDuration = 1f;
    private bool isSwitchingCam = false;

    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 3f;
    public float crawlSpeed = 1f;
    public float acceleration = 4f;
    public float deceleration = 6f;

    [Header("Physics Settings")]
    public float gravity = -9.81f;
    public float gravityMultiplier = 2.5f;
    [Range(0.5f, 3f)] public float jumpPower = 1.2f;

    [Header("Camera Settings")]
    public Camera playerCamera;
    public Transform cameraSet;
    public Transform Cameraset;
    public float mouseSensitivity = 250f;
    public float minPitch = -85f;
    public float maxPitch = 85f;
    public float standCamHeight = 1.65f;
    public float crawlCamHeight = 0.7f;
    public float camHeightLerpSpeed = 8f;

    [Header("Interaction Settings")]
    public float lookDistance = 3f;
    public float pickupDistance = 3f;
    public float lookDownThreshold = 25.0f;
    public float interactDownAmount = 0.7f;
    public float interactDuration = 0.4f;
    public float interactSmooth = 10f;

    [Header("Camera Bobbing Settings")]
    public float idleBobFreq = 1.2f;
    public float idleBobAmp = 0.08f;
    public float walkBobFreq = 3f;
    public float walkBobAmp = 0.20f;
    public float runBobFreq = 10f;
    public float runBobAmp = 0.30f;

    [Header("References")]
    public Animator anim;
    public UIManager uiManager;

    // 내부 변수들
    private CharacterController cc;
    private Vector3 moveVelocity;
    private float verticalVelocity;
    private bool isGrounded;
    private float pitch;
    private IInteractable currentItem;

    private bool isInteracting = false;
    private bool isInteractingCrouch = false;
    private bool isInteractCam = false;
    private float interactTimer = 0f;
    private float cameraBaseY;

    private float bobTimer = 0f;
    private Vector3 defaultCamLocalPos;

    private Vector3 startPosition;
    private Quaternion startRotation;


    // ================================================================
    // 2. 초기화 (Start)
    // ================================================================
    void Start()
    {
        InitializeComponents();
        InitializeCamera();
        InitializeCamcorder();

        // ▼▼▼ [수정됨] 3단계에 걸쳐서 끈질기게 찾기 ▼▼▼

        // 1. 내 자식들 중에서 찾기
        if (camcorderController == null)
            camcorderController = GetComponentInChildren<CamcorderController>();

        // 2. 그래도 없으면 내 부모님에게서 찾기
        if (camcorderController == null)
            camcorderController = GetComponentInParent<CamcorderController>();

        // 3. 그래도 없으면 게임 세상 전체를 뒤져서 찾기 (가장 확실!)
        if (camcorderController == null)
            camcorderController = FindAnyObjectByType<CamcorderController>();

        // 4. 확인 사살 (그래도 없으면 에러 메시지 띄움)
        if (camcorderController == null)
            Debug.LogError("🚨 CamcorderController를 찾을 수 없습니다! 스크립트가 씬에 있는지 확인하세요.");

        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        startPosition = transform.position;
        startRotation = transform.rotation;
        IsRunning = false;
    }


    // ================================================================
    // 3. 메인 루프 (Update)
    // ================================================================
    void Update()
    {
        // 인벤토리 열기/닫기
        if (UIManager.Instance != null)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                UIManager.Instance.ToggleInventory();
            }

            if (UIManager.Instance.isInventoryOpen)
            {
                IsRunning = false;
                return; // 인벤토리 열려있으면 동작 정지
            }
        }

        HandleLook();
        HandleMovement();
        HandleJump();

        HandleInteractCamMove();
        HandleCameraBobbing();

        CheckForLookableItem();
        if (!isInteracting && Input.GetKeyDown(KeyCode.F)) TryInteract();

        // 캠코더를 가지고 있을 때만 켜짐/꺼짐 가능
        if (hasCamcorder && Input.GetMouseButtonDown(0)&&UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="SampleScene")
        {
            if (!isSwitchingCam) StartCoroutine(CamcorderToggleRoutine());
        }

        if (Input.GetKeyDown(KeyCode.Escape)) ToggleCursor();
    }


    // ================================================================
    // 4. 리셋 기능 (배터리, 위치, 아이템 복구)
    // ================================================================
    public void ResetPlayer()
    {
        // 1. 위치 및 회전 초기화
        if (cc != null) cc.enabled = false;
        transform.position = startPosition;
        transform.rotation = startRotation;
        if (cc != null) cc.enabled = true;

        // 2. 카메라 각도 초기화
        pitch = 0f;
        cameraBaseY = standCamHeight;
        if (cameraSet != null)
        {
            Vector3 p = cameraSet.localPosition;
            p.y = standCamHeight;
            p.x = 0; p.z = 0;
            cameraSet.localPosition = p;
        }
        if (playerCamera != null)
        {
            playerCamera.transform.localPosition = Vector3.zero;
            playerCamera.transform.localRotation = Quaternion.identity;
        }

        // 3. 상태 변수 초기화
        IsRunning = false;
        isInteracting = false;
        isInteractingCrouch = false;
        moveVelocity = Vector3.zero;
        verticalVelocity = 0f;
        isInteractCam = false;
        interactTimer = 0f;

        // -----------------------------------------------------------
        // ▼▼▼ [핵심] 캠코더 관련 리셋 로직 ▼▼▼
        // -----------------------------------------------------------

        // A. 캠코더 뺏기 (사용 불가 상태)
        hasCamcorder = false;

        // B. 캠코더 기능 끄기
        camcorderOn = false;
        isSwitchingCam = false;
        VisionManager.Instance.CantSeeMonster();

        if (camcorderUI != null) camcorderUI.SetActive(false);
        if (camcorderBrightness != null) camcorderBrightness.alpha = 0f;
        if (noiseOverlay != null) noiseOverlay.alpha = 0f;
        if (noiseVideo != null) { noiseVideo.Pause(); noiseVideo.frame = 0; }

        // C. 배터리 100% 충전 (컨트롤러 호출)
        if (camcorderController != null)
        {
            camcorderController.ResetBattery();
        }

        // D. 바닥에 있는 캠코더 다시 스폰 (보이게 하기)
        if (worldCamcorderItem != null)
        {
            worldCamcorderItem.SetActive(true);
        }
        // -----------------------------------------------------------

        // 4. 조작 활성화
        this.enabled = true;

        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    // ================================================================
    // 5. 기타 보조 함수들
    // ================================================================
    void InitializeComponents()
    {
        cc = GetComponent<CharacterController>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void InitializeCamera()
    {
        if (cameraSet == null && Cameraset != null) cameraSet = Cameraset;
        if (Cameraset == null && cameraSet != null) Cameraset = cameraSet;

        if (cameraSet != null)
        {
            Vector3 p = cameraSet.localPosition;
            p.y = standCamHeight;
            cameraSet.localPosition = p;
            defaultCamLocalPos = cameraSet.localPosition;
        }
        cameraBaseY = standCamHeight;
    }

    void InitializeCamcorder()
    {
        if (camcorderUI != null) camcorderUI.SetActive(false);
        if (camcorderBrightness != null) camcorderBrightness.alpha = 0f;
        if (noiseOverlay != null) noiseOverlay.alpha = 0f;

        if (noiseVideo != null)
        {
            noiseVideo.isLooping = true;
            noiseVideo.Prepare();
            StartCoroutine(NoisePrepare());
        }
    }

    IEnumerator NoisePrepare()
    {
        while (!noiseVideo.isPrepared) yield return null;
        noiseVideo.Pause();
        noiseVideo.frame = 0;
    }

    void ToggleCursor()
    {
        if (Cursor.lockState == CursorLockMode.Locked) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        else { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
    }

    void HandleLook()
    {
        if (playerCamera == null || Cursor.lockState != CursorLockMode.Locked) return;
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y + mouseX, 0f);
        pitch = Mathf.Clamp(pitch - mouseY, minPitch, maxPitch);
        playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool runHold = Input.GetMouseButton(1);
        bool crawlHold = Input.GetKey(KeyCode.LeftShift);

        if (runHold || !isGrounded) crawlHold = false;
        if (crawlHold) runHold = false;

        Vector3 inputDir = (transform.right * h + transform.forward * v).normalized;
        float inputMag = Mathf.Clamp01(new Vector2(h, v).magnitude);

        IsRunning = (inputMag > 0.01f) && runHold;

        float targetSpeed = walkSpeed;
        if (crawlHold) targetSpeed = crawlSpeed;
        else if (runHold) targetSpeed = runSpeed;

        Vector3 targetVelocity = inputDir * targetSpeed * inputMag;

        moveVelocity = (inputMag > 0.01f)
            ? Vector3.Lerp(moveVelocity, targetVelocity, Time.deltaTime * acceleration)
            : Vector3.Lerp(moveVelocity, Vector3.zero, Time.deltaTime * deceleration);

        isGrounded = cc.isGrounded;
        if (isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        else verticalVelocity += gravity * gravityMultiplier * Time.deltaTime;

        Vector3 totalMove = moveVelocity;
        totalMove.y = verticalVelocity;
        cc.Move(totalMove * Time.deltaTime);

        if (anim != null)
        {
            anim.SetFloat("Speed", moveVelocity.magnitude);
            anim.SetBool("Crouch", crawlHold || isInteractingCrouch);
            anim.SetBool("IsGrounded", isGrounded);
        }
    }

    void HandleJump()
    {
        if (!isGrounded) return;
        if (Input.GetKey(KeyCode.LeftShift) || isInteracting) return;
        if (Input.GetKeyDown(KeyCode.Space)) verticalVelocity = jumpPower * 5f;
    }

    void HandleInteractCamMove()
    {
        bool isCrouch = (anim != null) && anim.GetBool("Crouch");
        float targetBaseY = (isCrouch || isInteractingCrouch) ? crawlCamHeight : standCamHeight;
        cameraBaseY = Mathf.Lerp(cameraBaseY, targetBaseY, Time.deltaTime * camHeightLerpSpeed);

        float offsetY = 0f;
        if (isInteractCam)
        {
            interactTimer += Time.deltaTime;
            float half = interactDuration;
            if (interactTimer <= half) offsetY = Mathf.Lerp(0f, -interactDownAmount, interactTimer / half);
            else if (interactTimer <= half * 2f) offsetY = Mathf.Lerp(-interactDownAmount, 0f, (interactTimer - half) / half);
            else isInteractCam = false;
        }

        if (cameraSet != null)
        {
            Vector3 p = cameraSet.localPosition;
            p.y = Mathf.Lerp(p.y, cameraBaseY + offsetY, Time.deltaTime * interactSmooth);
            cameraSet.localPosition = p;
        }
    }

    void HandleCameraBobbing()
    {
        if (cameraSet == null) return;
        float speed = new Vector2(moveVelocity.x, moveVelocity.z).magnitude;
        float freq = idleBobFreq; float amp = idleBobAmp;

        if (speed > 0.1f && isGrounded)
        {
            if (speed < runSpeed * 0.9f) { freq = walkBobFreq; amp = walkBobAmp; }
            else { freq = runBobFreq; amp = runBobAmp; }

            bobTimer += Time.deltaTime * freq;
            float bobY = Mathf.Sin(bobTimer) * amp;
            float bobX = Mathf.Sin(bobTimer * 0.5f) * amp * 0.4f;
            float bobZ = Mathf.Cos(bobTimer * 0.7f) * amp * 0.2f;

            Vector3 targetPos = defaultCamLocalPos + new Vector3(bobX, bobY, bobZ);
            cameraSet.localPosition = Vector3.Lerp(cameraSet.localPosition, targetPos, Time.deltaTime * 8f);

            float yaw = Mathf.Sin(bobTimer * 0.8f) * amp * (speed < runSpeed ? 6f : 14f);
            float roll = Mathf.Sin(bobTimer * 1.2f) * amp * (speed < runSpeed ? 5f : 10f);
            float pitch = Mathf.Cos(bobTimer * 0.5f) * amp * (speed < runSpeed ? 2f : 5f);

            cameraSet.localRotation = Quaternion.Euler(pitch, yaw, roll);
        }
        else
        {
            cameraSet.localPosition = Vector3.Lerp(cameraSet.localPosition, defaultCamLocalPos, Time.deltaTime * 8f);
            cameraSet.localRotation = Quaternion.Lerp(cameraSet.localRotation, Quaternion.identity, Time.deltaTime * 8f);
        }
    }

    void CheckForLookableItem()
    {
        if (UIManager.Instance == null) return;
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, lookDistance))
        {
            IInteractable item = hit.collider.GetComponent<IInteractable>();
            if (item != null)
            {
                if (currentItem != item)
                {
                    uiManager.ShowItemNameText(item.GetInteractPrompt());
                    currentItem = item;
                }
                // 아이템을 보고 있으면 크로스헤어 활성화
                uiManager.SetCrosshair(true);
            }
            else ClearCurrentItem();
        }
        else ClearCurrentItem();
    }

    void ClearCurrentItem()
    {
        if (uiManager != null) 
        {
            uiManager.ClearItemNameText();
            // 아이템이 없으면 크로스헤어 비활성화
            uiManager.SetCrosshair(false);
        }
        currentItem = null;
    }

    void TryInteract()
    {
        if (!isGrounded) return;
        if (moveVelocity.magnitude > walkSpeed + 0.05f) return;
        if (anim != null && anim.GetBool("Crouch")) return;

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, pickupDistance))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            Item itemComponent = hit.collider.GetComponent<Item>();

            if (interactable != null)
            {
                float cameraPitch = playerCamera.transform.localEulerAngles.x;
                bool isLookingDown = (cameraPitch > lookDownThreshold && cameraPitch < 90f);
                bool isItem = (itemComponent != null);

                if (isItem && isLookingDown) StartCoroutine(CrouchInteractRoutine(interactable));
                else PerformInteract(interactable);
            }
        }
    }

    private void PerformInteract(IInteractable item)
    {
        item.Interact();
        ClearCurrentItem();
        if (anim != null) anim.SetTrigger("IntAct");
        isInteractCam = true;
        interactTimer = 0f;
    }

    private IEnumerator CrouchInteractRoutine(IInteractable item)
    {
        isInteracting = true;
        isInteractingCrouch = true;
        yield return new WaitForSeconds(0.2f);
        item.Interact();
        ClearCurrentItem();
        if (anim != null) anim.SetTrigger("IntAct");
        yield return new WaitForSeconds(0.4f);
        isInteractingCrouch = false;
        isInteracting = false;
    }

    IEnumerator CamcorderToggleRoutine()
    {
        isSwitchingCam = true;
        if (noiseOverlay != null) yield return StartCoroutine(FadeCanvasGroup(noiseOverlay, 0.6f, 0.15f));
        if (noiseVideo != null) { noiseVideo.frame = 0; noiseVideo.Play(); }
        yield return new WaitForSeconds(noiseDuration * 0.5f);
        camcorderOn = !camcorderOn;
        if (camcorderUI != null) camcorderUI.SetActive(camcorderOn);
        if (camcorderBrightness != null) camcorderBrightness.alpha = camcorderOn ? camBrightnessAmount : 0f;
        yield return new WaitForSeconds(noiseDuration * 0.5f);
        if (noiseOverlay != null) yield return StartCoroutine(FadeCanvasGroup(noiseOverlay, 0f, 0.15f));
        if (noiseVideo != null) { noiseVideo.Pause(); noiseVideo.frame = 0; }
        isSwitchingCam = false;
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float targetAlpha, float duration)
    {
        float start = cg.alpha;
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, targetAlpha, time / duration);
            yield return null;
        }
        cg.alpha = targetAlpha;
    }
}