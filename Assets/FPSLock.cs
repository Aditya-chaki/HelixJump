using UnityEngine;

public class FPSLock : MonoBehaviour
{
    void Start()
    {
        // Disable vsync (optional but recommended for consistent results)
        QualitySettings.vSyncCount = 0;

        // Set the target frame rate to 60
        Application.targetFrameRate = 60;
    }
}
