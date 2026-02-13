using UnityEngine;

public class LockedDoor : MonoBehaviour,IInteractable
{
    [Header("상호작용 텍스트")]
    public string interactString = "Door Open(F)";
    public string afterTryString = "It's Locked";
    
    [Header("상호작용 피드백")]
    public AudioClip lockedSound;
    private AudioSource audioSource;
    
    [HideInInspector]
    public string uniqueID;
    void Awake()
    {
        string sceneName=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string posStr=transform.position.ToString("F1");
        uniqueID=$"{sceneName}_{posStr}";
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    public string GetInteractPrompt()
    {
        if (GameStateManager.Instance.IsDoorTried(uniqueID)) return afterTryString;
        else return interactString;
    }

    public void Interact()
    {
        if (!GameStateManager.Instance.IsDoorTried(uniqueID))
        {
            if (lockedSound != null)
            {
                audioSource.PlayOneShot(lockedSound);
            }
            GameStateManager.Instance.AddTriedDoor(uniqueID);
        }
        
    }
}
