using UnityEngine;
using System.Collections;
using UnityEngine.Video;
using TMPro;

public class CamcorderController : MonoBehaviour
{
    [Header("Player reference")]
    public PlayerController player;

    [Header("Battery settings")]
    public float maxBattery = 100f;
    public float batteryPercent = 100f;
    public float drainPerSecond = 0.2f;
    public float dashCost = 10f;
    public float teleportCost = 30f;

    [Header("Dash settings")]
    public float dashDistance = 20f;
    public float dashDuration = 2f;

    [Header("Teleport settings")]
    public Vector3 teleportDestination = new Vector3(7f, 70f, -16f);
    public float teleportNoiseTime = 0.5f;

    [Header("Optional FX")]
    public CanvasGroup noiseOverlay;
    public VideoPlayer noiseVideo;

    [Header("Battery UI")]
    public TextMeshProUGUI batteryText;

    [Header("Silent Step settings")]
    public GameObject silentStepIcon;
    public float silentStepMultiplier = 1.5f;

    private bool isDashing = false;
    private bool isTeleporting = false;
    private bool isBatteryLocked = false;
    private bool isSilentStepOn = false;

    public bool IsSilentStepOn => isSilentStepOn;

    private CharacterController cc;
    private PlayerFootsteps footsteps;

    void Awake()
    {
        if (player != null)
            cc = player.GetComponent<CharacterController>();

        footsteps = player.GetComponent<PlayerFootsteps>();
    }

    void Update()
    {
        if (player == null) return;
        if (!player.hasCamcorder) return;

        bool camOn = player.camcorderOn;

        if (isBatteryLocked)
        {
            ForceCameraOff();
            UpdateBatteryUI();
            return;
        }

        if (!camOn)
        {
            if (isSilentStepOn)
            {
                isSilentStepOn = false;
                UpdateSilentStepState();
            }
            UpdateBatteryUI();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) && !isDashing && !isTeleporting)
        {
            isSilentStepOn = !isSilentStepOn;
            UpdateSilentStepState();
        }

        if (camOn && !isDashing && !isTeleporting)
        {
            float multiplier = isSilentStepOn ? silentStepMultiplier : 1f;
            batteryPercent -= drainPerSecond * multiplier * Time.deltaTime;

            if (batteryPercent <= 0f)
            {
                batteryPercent = 0f;
                isBatteryLocked = true;
                StartCoroutine(AutoShutdownEffect());
                UpdateBatteryUI();
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) && !isDashing && !isTeleporting)
        {
            float half = dashCost * 0.5f;
            if (batteryPercent >= half)
            {
                float consume = Mathf.Min(batteryPercent, dashCost);
                batteryPercent -= consume;
                StartCoroutine(DashRoutine());
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && !isTeleporting && !isDashing)
        {
            float half = teleportCost * 0.5f;
            if (batteryPercent >= half)
            {
                // teleport invincibility must be applied immediately before starting the coroutine
                if (player != null)
                    player.isInvincible = true;

                float consume = Mathf.Min(batteryPercent, teleportCost);
                batteryPercent -= consume;
                StartCoroutine(TeleportRoutine());
            }
        }

        UpdateBatteryUI();
    }

    void UpdateSilentStepState()
    {
        if (silentStepIcon != null)
            silentStepIcon.SetActive(isSilentStepOn);

        if (footsteps != null)
            footsteps.SetMuted(isSilentStepOn);
    }

    IEnumerator AutoShutdownEffect()
    {
        if (noiseOverlay != null)
            yield return StartCoroutine(FadeCanvasGroup(noiseOverlay, 1f, 0.15f));

        if (noiseVideo != null) { noiseVideo.frame = 0; noiseVideo.Play(); }

        yield return new WaitForSecondsRealtime(0.3f);

        ForceCameraOff();

        if (noiseOverlay != null)
            yield return StartCoroutine(FadeCanvasGroup(noiseOverlay, 0f, 0.15f));

        if (noiseVideo != null) { noiseVideo.Pause(); noiseVideo.frame = 0; }
    }

    void ForceCameraOff()
    {
        if (!player.camcorderOn) return;
        player.camcorderOn = false;

        if (player.camcorderUI != null) player.camcorderUI.SetActive(false);
        if (player.camcorderBrightness != null) player.camcorderBrightness.alpha = 0f;

        if (isSilentStepOn)
        {
            isSilentStepOn = false;
            UpdateSilentStepState();
        }
    }

    IEnumerator DashRoutine()
    {
        isDashing = true;

        // invincible start
        if (player != null)
            player.isInvincible = true;

        Vector3 dir = player.transform.forward;
        float elapsed = 0f;
        float speed = dashDistance / dashDuration;

        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            Vector3 delta = dir * speed * Time.deltaTime;
            if (cc != null) cc.Move(delta);
            else player.transform.position += delta;
            yield return null;
        }

        // invincible extra duration
        yield return new WaitForSeconds(3f);

        // invincible end
        if (player != null)
            player.isInvincible = false;

        isDashing = false;
    }

    IEnumerator TeleportRoutine()
    {
        isTeleporting = true;

        // invincible start
        if (player != null)
            player.isInvincible = true;

        if (noiseOverlay != null) yield return StartCoroutine(FadeCanvasGroup(noiseOverlay, 1f, 0.25f));
        if (noiseVideo != null) { noiseVideo.frame = 0; noiseVideo.Play(); }

        yield return new WaitForSecondsRealtime(teleportNoiseTime);

        if (cc == null) cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            player.transform.position = teleportDestination;
            cc.enabled = true;
        }
        else player.transform.position = teleportDestination;

        if (noiseOverlay != null) yield return StartCoroutine(FadeCanvasGroup(noiseOverlay, 0f, 0.25f));
        if (noiseVideo != null) { noiseVideo.Pause(); noiseVideo.frame = 0; }

        // invincible extra duration
        yield return new WaitForSeconds(5f);

        // invincible end
        if (player != null)
            player.isInvincible = false;

        isTeleporting = false;
    }

    void UpdateBatteryUI()
    {
        if (batteryText == null) return;
        int value = Mathf.Clamp(Mathf.RoundToInt(batteryPercent), 0, (int)maxBattery);
        batteryText.text = value.ToString() + "%";
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float targetAlpha, float duration)
    {
        if (cg == null) yield break;
        float start = cg.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(start, targetAlpha, t / duration);
            yield return null;
        }
        cg.alpha = targetAlpha;
    }

    public void ResetBattery()
    {
        batteryPercent = maxBattery;
        isBatteryLocked = false;
        isSilentStepOn = false;

        UpdateSilentStepState();
        UpdateBatteryUI();

        StopAllCoroutines();
        isDashing = false;
        isTeleporting = false;

        if (noiseOverlay != null) noiseOverlay.alpha = 0f;
        if (noiseVideo != null) { noiseVideo.Pause(); noiseVideo.frame = 0; }
    }
}
