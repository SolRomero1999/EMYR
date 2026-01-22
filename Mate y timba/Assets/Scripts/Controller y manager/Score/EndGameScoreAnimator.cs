using UnityEngine;
using TMPro;
using System.Collections;

public class EndGameScoreAnimator : MonoBehaviour
{
    [Header("Referencias")]
    public ScoreManager scoreManager;

    [Header("Panel Final")]
    public GameObject panelFinal;

    [Header("Textos")]
    public TextMeshProUGUI contadorJugador;
    public TextMeshProUGUI contadorIA;

    [Header("Animación")]
    public float velocidadConteo = 10f;
    public float pausaEntreJugadores = 0.6f;

    public System.Action OnConteoFinalizado;

    private bool ejecutado = false;

    private void Start()
    {
        panelFinal.SetActive(false);
    }

    public void MostrarResultadoFinal()
    {
        if (ejecutado)
            return;

        ejecutado = true;

        scoreManager.ActualizarPuntajes();
        
        scoreManager.CalcularTotales();

        panelFinal.SetActive(true);

        contadorJugador.text = "0";
        contadorIA.text = "0";

        StartCoroutine(AnimarSecuencia());
    }

    private IEnumerator AnimarSecuencia()
    {
        yield return null;

        int totalJugador = scoreManager.TotalJugador;
        int totalIA = scoreManager.TotalIA;

        contadorJugador.text = "0";
        contadorIA.text = "0";

        yield return AnimarContador(contadorJugador, totalJugador);

        yield return new WaitForSeconds(pausaEntreJugadores);

        yield return AnimarContador(contadorIA, totalIA);

        OnConteoFinalizado?.Invoke();
    }

    private IEnumerator AnimarContador(TextMeshProUGUI texto, int objetivo)
    {
        if (objetivo == 0)
        {
            texto.text = "0";
            yield break;
        }

        float actual = 0f;
        float duracion = Mathf.Clamp(objetivo / velocidadConteo, 1f, 3f); 

        float tiempoInicio = Time.time;
        
        while (actual < objetivo)
        {
            float tiempoTranscurrido = Time.time - tiempoInicio;
            float progreso = tiempoTranscurrido / duracion;
            progreso = Mathf.Clamp01(progreso);
            
            actual = Mathf.Lerp(0, objetivo, progreso);
            texto.text = Mathf.RoundToInt(actual).ToString();
            yield return null;
        }

        texto.text = objetivo.ToString();
    }

    public IEnumerator AnimarContadorTutorial(TextMeshProUGUI texto, int objetivo)
    {
        yield return AnimarContador(texto, objetivo);
    }

}