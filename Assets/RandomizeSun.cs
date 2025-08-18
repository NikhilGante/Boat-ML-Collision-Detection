using UnityEngine;

public class RandomizeSun : MonoBehaviour
{
    public float minIntensity = 1.0f;
    public float maxIntensity = 1.5f;
    public float changeInterval = 1f; // seconds between randomizations

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= changeInterval)
        {
            RandomizeLighting();
            timer = 0f;
        }
    }

    void RandomizeLighting()
    {
        Vector3 euler = new Vector3(
            Random.Range(50f, 75f), // elevation
            Random.Range(0f, 360f), // azimuth
            0f
        );
        transform.rotation = Quaternion.Euler(euler);

        GetComponent<Light>().intensity = Random.Range(minIntensity, maxIntensity);
    }
}
