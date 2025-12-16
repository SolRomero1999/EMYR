using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    public Transform gameplayView;
    public Transform dialogueView;
    public float moveDuration = 0.6f;

    private void Awake()
    {
        Instance = this;
    }

    public void IrAGameplay()
    {
        StartCoroutine(MoverCamara(gameplayView.position));
    }

    public void IrADialogo()
    {
        StartCoroutine(MoverCamara(dialogueView.position));
    }

    private IEnumerator MoverCamara(Vector3 destino)
    {
        Vector3 inicio = transform.position;
        destino.z = inicio.z;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            transform.position = Vector3.Lerp(inicio, destino, t);
            yield return null;
        }

        transform.position = destino;
    }
}
