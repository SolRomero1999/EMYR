using UnityEngine;
using System.Collections;

public class BlinkController : MonoBehaviour
{
    public static BlinkController Instance;

    [Header("Animación de parpadeo")]
    public Sprite[] blinkFrames;
    public float frameRate = 0.05f;

    [Header("Referencias")]
    public SpriteRenderer spriteRenderer;
    public Transform cameraTransform;

    void Awake()
    {
        Instance = this;

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        transform.SetParent(cameraTransform, true);

        transform.localPosition = new Vector3(0f, 0f, 0.5f);
        transform.localRotation = Quaternion.identity;
        spriteRenderer.enabled = false;
    }

    public void StartBlink(System.Action accion)
    {
        StartCoroutine(BlinkCoroutine(accion));
    }

    IEnumerator BlinkCoroutine(System.Action accion)
    {
        spriteRenderer.enabled = true;

        for (int i = 0; i < blinkFrames.Length; i++)
        {
            spriteRenderer.sprite = blinkFrames[i];
            yield return new WaitForSeconds(frameRate);
        }

        accion?.Invoke();

        for (int i = blinkFrames.Length - 1; i >= 0; i--)
        {
            spriteRenderer.sprite = blinkFrames[i];
            yield return new WaitForSeconds(frameRate);
        }

        spriteRenderer.enabled = false;
    }
}
