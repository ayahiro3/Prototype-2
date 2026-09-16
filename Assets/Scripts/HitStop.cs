using System.Collections;
using UnityEngine;

public class HitStop : MonoBehaviour
{
    [SerializeField, Min(0f)] private float duration = 0.06f;
    [Header("Hit Effects")]
    [SerializeField] private ParticleSystem hitParticles;
    [SerializeField] private AudioSource hitAudio;
    [SerializeField] private AudioClip hitSound;
    [Header("Camera Shake")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField, Min(0f)] private float shakeStrength = 0.05f;

    private Vector3 originalCameraPosition;

    private bool isStopping;
    private float previousTimeScale;
    public bool IsPlaying => isStopping;

    public void Play(Vector3 hitPosition)
    {
        if (!isActiveAndEnabled) return;

        if (hitParticles != null)
        {
            Instantiate(hitParticles, hitPosition, Quaternion.identity);
        }

        if (hitAudio != null && hitSound != null)
        {
            hitAudio.PlayOneShot(hitSound);
        }

        if (isStopping || Time.timeScale == 0f) return;

        StartCoroutine(StopBriefly());
    }

    private IEnumerator StopBriefly()
    {
        isStopping = true;
        previousTimeScale = Time.timeScale;

        if (cameraTransform != null)
        {
            originalCameraPosition = cameraTransform.localPosition;
        }

        Time.timeScale = 0f;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (cameraTransform != null)
            {
                float fade = 1f - elapsed / duration;
                Vector2 offset = Random.insideUnitCircle * shakeStrength * fade;

                cameraTransform.localPosition = originalCameraPosition + new Vector3(offset.x, offset.y, 0f);
            }

            yield return null;
            elapsed += Time.unscaledDeltaTime;
        }

        RestoreCamera();
        Time.timeScale = previousTimeScale;
        isStopping = false;
    }

    private void OnDisable()
    {
        if (!isStopping) return;

        StopAllCoroutines();
        RestoreCamera();
        Time.timeScale = previousTimeScale;
        isStopping = false;
    }

    private void RestoreCamera()
    {
        if (cameraTransform != null)
        {
            cameraTransform.localPosition = originalCameraPosition;
        }
    }
}