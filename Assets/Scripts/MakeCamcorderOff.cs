using UnityEngine;

public class MakeCamcorderOff : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerController player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        if (player == null) return;
        if (!player.camcorderOn) return;

        player.camcorderOn = false;

        if (player.camcorderUI != null)
            player.camcorderUI.SetActive(false);

        if (player.camcorderBrightness != null)
            player.camcorderBrightness.alpha = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
