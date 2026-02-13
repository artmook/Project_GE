using UnityEngine;

public class RegisterSpawnPointSelf : MonoBehaviour
{
    void Awake()
    {
        PlayerSpawnManager spawnManager= FindAnyObjectByType<PlayerSpawnManager>();
        if(spawnManager!=null) spawnManager.RegisterSpawnPoint(gameObject.name,transform);
    }
}
