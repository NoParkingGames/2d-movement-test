using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Default Shake Settings")]
    [Range(0f, 2f)] public float defaultDuration = 0.15f;
    [Range(0f, 1f)] public float defaultMagnitude = 0.2f;

    private Vector3 originalPos;

    void Awake() => originalPos = transform.localPosition;

    // This version uses the sliders from the Inspector
    public void ShakeDefault()
    {
        StopAllCoroutines();
        StartCoroutine(ProcessShake(defaultDuration, defaultMagnitude));
    }

    // This version allows the Player script to send custom numbers
    public void Shake(float duration, float magnitude)
    {
        StopAllCoroutines();
        StartCoroutine(ProcessShake(duration, magnitude));
    }

    private System.Collections.IEnumerator ProcessShake(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}
