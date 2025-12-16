using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BlinkController : MonoBehaviour
{
    public static BlinkController Instance;

    public Image overlay;
    public float blinkTime = 0.2f;

    private void Awake()
    {
        Instance = this;
        overlay.color = new Color(0, 0, 0, 0);
        overlay.gameObject.SetActive(false);
    }

    public void StartBlink(System.Action accion)
    {
        StartCoroutine(Blink(accion));
    }

    public IEnumerator Blink(System.Action accion)
    {
        overlay.gameObject.SetActive(true);

        yield return Fade(0, 1);

        accion?.Invoke();

        yield return Fade(1, 0);

        overlay.gameObject.SetActive(false);
    }

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / blinkTime;
            float alpha = Mathf.Lerp(from, to, t);

            overlay.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        overlay.color = new Color(0, 0, 0, to);
    }
}
