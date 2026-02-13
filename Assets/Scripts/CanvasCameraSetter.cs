using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class CanvasCameraSetter : MonoBehaviour
{
    void Start()
    {
        Canvas myCanvas = GetComponent<Canvas>();
        if (myCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            if (Camera.main != null)
            {
                myCanvas.worldCamera = Camera.main;
            }
        }
    }
}