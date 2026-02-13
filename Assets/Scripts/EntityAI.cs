using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class EntityAI : MonoBehaviour
{
    [Header("Stats")]
    public float chaseSpeed = 5f;
    public float walkSpeed = 2.5f;

    [Header("Patrol Settings")]
    public Transform patrolCenter;
    public float patrolRadius = 30f;
    public float patrolInterval = 0f;
    public float minPatrolDistance = 10f;

    [Header("Chase Settings")]
    public float chaseTimeLimit = 10f;
    private float currentChaseTimer = 0f;

    [Header("Sense")]
    public float visualAwarenessDistance = 20f;
    public float hearingDistance = 40f;
    public float catchDistance = 1.5f;
    public float fieldOfViewAngle = 110f;
    public LayerMask obstacleMask;

    [Header("Audio - SFX")]
    public AudioClip screamSound;
    public AudioClip attackSound;

    [Header("Audio - Heartbeat")]
    public AudioClip heartbeatSound;
    public float heartbeatDistance = 15f;
    [Range(0, 2)] public float proximityVolume = 2.0f;

    private AudioSource heartbeatSource;

    [Header("Components")]
    public Transform player;
    public GameObject playerModel;
    public Image blackScreenPanel;

    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private AudioSource mainAudioSource;
    private PlayerController playerMovement;

    private bool isStunned = false;
    private bool isDeadScenePlaying = false;
    private bool isAlerted = false;
    private bool isChasingVisually = false;
    private Vector3 lastKnownPosition;
    private bool hasScreamed = false;
    private bool isInvestigatingMove = false;

    private Vector3 startPosition;
    private Quaternion startRotation;

    public static bool isAnyMonsterAttacking = false;

    // 캠코더 컨트롤러
    private CamcorderController camcorder;


    void Start()
    {
        Time.timeScale = 1.0f;

        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        mainAudioSource = GetComponent<AudioSource>();

        startPosition = transform.position;
        startRotation = transform.rotation;

        SetupHeartbeatAudio();

        // 플레이어 찾기
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player != null)
        {
            playerMovement = player.GetComponentInChildren<PlayerController>();

            // 캠코더 찾기 (1차 시도)
            camcorder = FindAnyObjectByType<CamcorderController>();
            if (camcorder == null) camcorder = player.GetComponentInChildren<CamcorderController>();

            if (playerModel == null)
            {
                Renderer rend = player.GetComponentInChildren<Renderer>();
                if (rend != null) playerModel = rend.gameObject;
            }
        }

        if (navMeshAgent != null)
            navMeshAgent.speed = walkSpeed;

        lastKnownPosition = transform.position;

        if (blackScreenPanel != null)
        {
            blackScreenPanel.gameObject.SetActive(false);
            Color c = blackScreenPanel.color;
            c.a = 0;
            blackScreenPanel.color = c;
        }
    }


    void Update()
    {
        if (!this.enabled || isDeadScenePlaying || isAnyMonsterAttacking) return;
        if (navMeshAgent == null) return;

        if (isStunned)
        {
            StopMovement();
            return;
        }

        if (player == null || playerMovement == null)
        {
            if (navMeshAgent.isOnNavMesh) Patrol();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        UpdateHeartbeatAudio(distanceToPlayer);

        if (isAlerted && distanceToPlayer <= catchDistance)
        {
            // 무적 검사 추가
            PlayerController pcCheck = player.GetComponentInChildren<PlayerController>();
            if (pcCheck != null && pcCheck.isInvincible)
            {
                // 무적일 경우 죽음 연출을 실행하지 않음
                return;
            }

            StartCoroutine(DeathSequence(player.gameObject));
            return;
        }

        // =========================================================
        // ▼▼▼ 감각 시스템 (음소거 확실하게 적용) ▼▼▼
        // =========================================================

        // 1. 캠코더 컨트롤러가 없으면 다시 찾기 (연결 끊김 방지)
        if (camcorder == null)
        {
            camcorder = FindAnyObjectByType<CamcorderController>();
        }

        // 2. 시각 감지
        bool canSee = CheckVisibility(distanceToPlayer);

        // 3. 청각 감지 (초기화)
        bool canHear = false;

        // 4. 사일런트 여부 확인 (단순화: 컨트롤러가 켜졌다고 하면 무조건 믿음)
        bool isSilent = (camcorder != null && camcorder.IsSilentStepOn);

        // [핵심] 사일런트가 아닐 때만 소리 계산!
        if (isSilent == false)
        {
            // 달리고 있고 + 청각 범위 내에 있으면 => 들킴
            if (playerMovement.IsRunning && distanceToPlayer <= hearingDistance)
            {
                canHear = true;
            }
        }
        else
        {
            // 사일런트가 켜져 있으면 => 무조건 안 들림 (강제)
            canHear = false;
        }
        // =========================================================

        if (canSee)
        {
            currentChaseTimer = 0f;
            if (!isChasingVisually)
            {
                PlayScream();
                isChasingVisually = true;
            }
            isAlerted = true;
            isInvestigatingMove = false;
            lastKnownPosition = player.position;
            MoveTo(lastKnownPosition, chaseSpeed, 1.1f);
        }
        else if (canHear)
        {
            currentChaseTimer = 0f;
            if (!isAlerted) PlayScream();

            isAlerted = true;
            isChasingVisually = false;
            isInvestigatingMove = false;
            lastKnownPosition = player.position;
            MoveTo(lastKnownPosition, chaseSpeed, 1.1f);
        }
        else if (isAlerted)
        {
            isChasingVisually = false;
            currentChaseTimer += Time.deltaTime;

            if (currentChaseTimer > chaseTimeLimit)
            {
                isAlerted = false;
                hasScreamed = false;
                currentChaseTimer = 0f;
                isInvestigatingMove = false;
            }
            else
            {
                Investigate();
            }
        }
        else
        {
            isChasingVisually = false;
            isInvestigatingMove = false;
            Patrol();
        }
    }

    void SetupHeartbeatAudio()
    {
        if (heartbeatSound != null)
        {
            heartbeatSource = gameObject.AddComponent<AudioSource>();
            heartbeatSource.clip = heartbeatSound;
            heartbeatSource.loop = true;
            heartbeatSource.playOnAwake = false;
            heartbeatSource.spatialBlend = 0f;
            heartbeatSource.volume = 0f;
        }
    }

    void UpdateHeartbeatAudio(float dist)
    {
        if (heartbeatSource != null)
        {
            if (dist <= heartbeatDistance)
            {
                if (!heartbeatSource.isPlaying) heartbeatSource.Play();
                float vol = Mathf.Clamp01(1f - (dist / heartbeatDistance));
                heartbeatSource.volume = vol * proximityVolume;
            }
            else
            {
                if (heartbeatSource.isPlaying) heartbeatSource.Stop();
            }
        }
    }

    void StopHeartbeatAudio()
    {
        if (heartbeatSource != null) heartbeatSource.Stop();
    }

    bool CheckVisibility(float dist)
    {
        if (dist > visualAwarenessDistance) return false;
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        if (Vector3.Angle(transform.forward, dirToPlayer) > fieldOfViewAngle / 2f) return false;

        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 target = player.position + Vector3.up * 1.5f;

        if (Physics.Linecast(origin, target, out RaycastHit hit, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform == player) return true;
            return false;
        }
        return true;
    }

    void Investigate()
    {
        if (!isInvestigatingMove)
        {
            MoveTo(lastKnownPosition, chaseSpeed, 1.1f);
            isInvestigatingMove = true;
        }
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 1.0f)
            animator.SetFloat("Speed", 0f);
    }

    void Patrol()
    {
        if (!navMeshAgent.hasPath || navMeshAgent.remainingDistance < 0.5f)
            SetNewPatrolDestination();
        else
        {
            if (navMeshAgent.destination != null)
            {
                if (navMeshAgent.isStopped) navMeshAgent.isStopped = false;
                animator.SetFloat("Speed", 0.5f);
            }
        }
    }

    void SetNewPatrolDestination()
    {
        Vector3 originPos = (patrolCenter != null) ? patrolCenter.position : transform.position;
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
            randomDirection += originPos;
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(randomDirection, out navHit, patrolRadius, NavMesh.AllAreas))
            {
                float dist = Vector3.Distance(transform.position, navHit.position);
                if (dist > minPatrolDistance)
                {
                    navMeshAgent.isStopped = false;
                    navMeshAgent.speed = walkSpeed;
                    navMeshAgent.SetDestination(navHit.position);
                    animator.SetFloat("Speed", 0.5f);
                    return;
                }
            }
        }
    }

    void MoveTo(Vector3 target, float speed, float animSpeed)
    {
        if (!navMeshAgent.isOnNavMesh) return;
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = speed;
        navMeshAgent.SetDestination(target);
        animator.SetFloat("Speed", animSpeed);
    }

    void StopMovement()
    {
        if (navMeshAgent.isOnNavMesh) navMeshAgent.isStopped = true;
        animator.SetFloat("Speed", 0f);
    }

    void PlayScream()
    {
        if (hasScreamed) return;
        if (mainAudioSource != null && screamSound != null)
        {
            if (!mainAudioSource.isPlaying)
            {
                mainAudioSource.PlayOneShot(screamSound);
                hasScreamed = true;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isDeadScenePlaying || isAnyMonsterAttacking) return;

        if (other.CompareTag("Player"))
        {
            // 무적 검사 추가
            PlayerController pc = other.GetComponentInChildren<PlayerController>();
            if (pc != null && pc.isInvincible)
            {
                // 무적이면 죽음 연출을 실행하지 않음
                return;
            }

            StartCoroutine(DeathSequence(other.gameObject));
        }
    }

    IEnumerator DeathSequence(GameObject playerObj)
    {
        isDeadScenePlaying = true;
        isAnyMonsterAttacking = true;
        StopHeartbeatAudio();

        VisionManager vision = FindAnyObjectByType<VisionManager>();
        if (vision != null) vision.ForceReveal();

        PlayerController pc = playerObj.GetComponentInChildren<PlayerController>();
        if (pc != null)
        {
            pc.enabled = false;
            AudioSource pcAudio = pc.GetComponent<AudioSource>();
            if (pcAudio != null) pcAudio.Stop();
            AudioSource[] allAudios = pc.GetComponentsInChildren<AudioSource>();
            foreach (var audio in allAudios) audio.Stop();
        }

        if (playerModel != null) playerModel.SetActive(false);

        StopMovement();
        navMeshAgent.updateRotation = false;
        navMeshAgent.enabled = false;

        Vector3 pushDir = (transform.position - playerObj.transform.position).normalized;
        if (pushDir == Vector3.zero) pushDir = transform.forward * -1;
        transform.position += pushDir * 1.0f;

        Vector3 dirToPlayer = playerObj.transform.position - transform.position;
        dirToPlayer.y = 0;
        if (dirToPlayer != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dirToPlayer);

        Transform cam = Camera.main.transform;
        Vector3 startPos = cam.position;
        Vector3 facePos = transform.position + Vector3.up * 1.6f;
        Vector3 targetPos = transform.position + (transform.forward * 1.2f) + (Vector3.up * 1.5f);

        RaycastHit hit;
        bool blocked = Physics.Linecast(startPos, targetPos, out hit, obstacleMask);
        if (blocked && hit.collider.gameObject != gameObject && hit.collider.gameObject != playerObj)
            targetPos = startPos;

        if (attackSound != null)
            mainAudioSource.PlayOneShot(attackSound);

        for (int i = 0; i < 3; i++)
        {
            animator.SetTrigger("Attack");
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 5f;
                if (i == 0) cam.position = Vector3.Lerp(startPos, targetPos, t);
                Vector3 lookDir = (facePos - cam.position).normalized;
                if (lookDir != Vector3.zero)
                {
                    Quaternion newRot = Quaternion.LookRotation(lookDir);
                    cam.rotation = Quaternion.Slerp(cam.rotation, newRot, t);
                }
                yield return null;
            }
            yield return new WaitForSeconds(2.0f);
        }

        if (blackScreenPanel != null)
        {
            blackScreenPanel.gameObject.SetActive(true);
            float t = 0;
            Color c = blackScreenPanel.color;
            c.a = 0;
            while (t < 1f)
            {
                t += Time.deltaTime;
                c.a = t;
                blackScreenPanel.color = c;
                yield return null;
            }
            c.a = 1;
            blackScreenPanel.color = c;
        }

        yield return new WaitForSeconds(3.0f);
        StartCoroutine(ResetGameRoutine());

        var mgr = FindAnyObjectByType<UInVideoManager>();
        if (mgr != null)
        {
            mgr.RegisterDeath();
        }
    }

    IEnumerator ResetGameRoutine()
    {
        yield return StartCoroutine(FadeInScreen());
        EntityAI[] monsters = FindObjectsByType<EntityAI>(FindObjectsSortMode.None);
        foreach (var m in monsters) m.ResetMonster();
        VisionManager.Instance.isDead = false;
        if (player != null)
        {
            PlayerController pc = player.GetComponentInChildren<PlayerController>();
            if (pc != null) pc.ResetPlayer();
        }
        if (playerModel != null) playerModel.SetActive(true);
        isAnyMonsterAttacking = false;
        Time.timeScale = 1.0f;
    }

    public void ResetMonster()
    {
        StopAllCoroutines();
        StopHeartbeatAudio();

        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = true;
            if (patrolCenter != null) navMeshAgent.Warp(patrolCenter.position);
            else navMeshAgent.Warp(startPosition);
            navMeshAgent.isStopped = false;
            navMeshAgent.speed = walkSpeed;
            navMeshAgent.updateRotation = true;
        }

        isAlerted = false;
        isChasingVisually = false;
        isInvestigatingMove = false;
        hasScreamed = false;
        isDeadScenePlaying = false;

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
        this.enabled = true;
    }

    IEnumerator FadeInScreen()
    {
        if (blackScreenPanel != null)
        {
            float t = 0;
            Color c = blackScreenPanel.color;
            c.a = 1;
            float duration = 1.0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(1, 0, t / duration);
                blackScreenPanel.color = c;
                yield return null;
            }
            c.a = 0;
            blackScreenPanel.color = c;
            blackScreenPanel.gameObject.SetActive(false);
        }
    }

    public void Stun(float duration)
    {
        isStunned = true;
        animator.SetTrigger("Stun");
        StartCoroutine(StunRoutine(duration));
    }

    IEnumerator StunRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        isStunned = false;
        if (navMeshAgent.isOnNavMesh) navMeshAgent.isStopped = false;
    }
}
