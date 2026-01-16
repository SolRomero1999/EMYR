using UnityEngine;
using TMPro;
using System.Collections;

public class ScoreTextAnimator : MonoBehaviour
{
    [Header("Animación")]
    public float escalaMax = 1.3f;
    public float duracionPop = 0.25f;
    public float velocidadConteo = 0.02f;

    [Header("Colores")]
    public Color colorTrio = Color.green;
    public Color colorCuarteto = new Color(0.2f, 0.8f, 1f);

    public void Animar(TextMeshProUGUI texto, int valorFinal, ComboType tipo)
    {
        StopAllCoroutines();
        StartCoroutine(Animacion(texto, valorFinal, tipo));
    }

    private IEnumerator Animacion(TextMeshProUGUI texto, int valorFinal, ComboType tipo)
    {
        Color colorOriginal = texto.color;
        Vector3 escalaOriginal = texto.transform.localScale;

        texto.color = tipo == ComboType.Trio ? colorTrio : colorCuarteto;

        int valorActual = 0;
        int pasos = Mathf.Clamp(valorFinal / 4, 6, 15);

        for (int i = 0; i < pasos; i++)
        {
            valorActual = Mathf.RoundToInt(Mathf.Lerp(0, valorFinal, i / (float)pasos));
            texto.text = valorActual.ToString();
            yield return new WaitForSeconds(velocidadConteo);
        }

        texto.text = valorFinal.ToString();

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duracionPop;
            float escala = Mathf.Lerp(1f, escalaMax, Mathf.Sin(t * Mathf.PI));
            texto.transform.localScale = escalaOriginal * escala;
            yield return null;
        }

        texto.transform.localScale = escalaOriginal;
        texto.color = colorOriginal;
    }
}
