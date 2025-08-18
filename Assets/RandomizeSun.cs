using UnityEngine;
using System.Collections;

public class SunRandomizer : MonoBehaviour
{
    public float interval = 5f; // seconds between updates
    private Light sunLight;

    void Start()
    {
        sunLight = GetComponent<Light>();
        if (sunLight != null && sunLight.type == LightType.Directional)
        {
            StartCoroutine(RandomizeSunRoutine());
        }
    }

    IEnumerator RandomizeSunRoutine()
    {
        while (true)
        {
            RandomizeSun();
            yield return new WaitForSeconds(interval);
        }
    }

    void RandomizeSun()
    {
        // Random rotation
        float yaw = Random.Range(0f, 360f);     // Spin around Y axis
        float pitch = Random.Range(30f, 60f);   // Keep above horizon
        transform.rotation = Quaternion.Euler(pitch, yaw, 0);

        // Random intensity
        sunLight.intensity = Random.Range(0.8f, 1.5f);

        // Random color (warm to cool daylight)
        sunLight.color = Color.Lerp(
            new Color(1f, 0.95f, 0.85f),   // warm
            new Color(0.9f, 0.95f, 1f),    // cool
            Random.value
        );
    }
}
