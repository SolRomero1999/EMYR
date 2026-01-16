using UnityEngine;
using System.Collections;

public class ComboCardAnimator : MonoBehaviour
{
    [Header("Escala")]
    [Tooltip("Escala máxima durante la entrada del combo")]
    public float escalaCombo = 1.12f;

    [Tooltip("Escala que mantiene la carta mientras el combo está activo")]
    public float escalaComboIdle = 1.08f;

    [Tooltip("Duración de la animación de entrada")]
    public float duracionEntrada = 0.2f;

    [Header("Flotación de entrada")]
    public float alturaEntrada = 0.08f;
    public float velocidadEntrada = 3f;
    public float duracionEntradaTotal = 1.2f;

    [Header("Flotación idle (combo activo)")]
    public float alturaIdle = 0.025f;
    public float velocidadIdle = 1.5f;

    private Coroutine rutinaEntrada;
    private Coroutine rutinaIdle;

    private Vector3 escalaInicial;
    private Vector3 escalaIdle;

    private bool comboActivo;

    private Carta carta;

    private void Awake()
    {
        carta = GetComponent<Carta>();

        escalaInicial = transform.localScale;
        escalaIdle = escalaInicial * escalaComboIdle;
    }

    public void Reproducir()
    {
        if (comboActivo)
            return;

        comboActivo = true;
        DetenerTodo();
        rutinaEntrada = StartCoroutine(EntradaCombo());
    }

    public void DetenerCombo()
    {
        comboActivo = false;
        DetenerTodo();

        transform.localScale = escalaInicial;
        carta.SetOffsetVisual(Vector3.zero);
    }

    private void DetenerTodo()
    {
        if (rutinaEntrada != null)
            StopCoroutine(rutinaEntrada);

        if (rutinaIdle != null)
            StopCoroutine(rutinaIdle);

        rutinaEntrada = null;
        rutinaIdle = null;
    }

    private IEnumerator EntradaCombo()
    {
        Vector3 escalaEntrada = escalaInicial * escalaCombo;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duracionEntrada;
            transform.localScale = Vector3.Lerp(escalaInicial, escalaEntrada, t);
            yield return null;
        }

        float tiempo = 0f;
        while (tiempo < duracionEntradaTotal)
        {
            tiempo += Time.deltaTime;
            float offset = Mathf.Sin(Time.time * velocidadEntrada) * alturaEntrada;
            carta.SetOffsetVisual(Vector3.up * offset);
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duracionEntrada;
            transform.localScale = Vector3.Lerp(escalaEntrada, escalaIdle, t);
            yield return null;
        }

        transform.localScale = escalaIdle;

        rutinaIdle = StartCoroutine(IdleCombo());
    }

    private IEnumerator IdleCombo()
    {
        while (comboActivo)
        {
            float offset = Mathf.Sin(Time.time * velocidadIdle) * alturaIdle;
            carta.SetOffsetVisual(Vector3.up * offset);
            yield return null;
        }
    }
}
