using UnityEngine;
using System.Collections;

public class TurnManager : MonoBehaviour
{
    public GameController game;   // Referencia al GameController
    public float delayIA = 1f;    // Tiempo que la IA tarda en actuar

    private bool turnoJugador = true;

    private void Start()
    {
        StartCoroutine(IniciarTurnos());
    }

    private IEnumerator IniciarTurnos()
    {
        yield return new WaitForSeconds(1f);
        IniciarTurnoJugador();
    }

    private void IniciarTurnoJugador()
    {
        turnoJugador = true;
        Debug.Log("TURNO DEL JUGADOR");
        // Aquí no hacemos nada más: el jugador actúa con sus clicks
    }

    public void TerminarTurnoJugador()
    {
        if (!turnoJugador) return;

        turnoJugador = false;
        Debug.Log("Jugador terminó su turno");

        StartCoroutine(TurnoIA());
    }

    private IEnumerator TurnoIA()
    {
        Debug.Log("TURNO DE LA IA");

        yield return new WaitForSeconds(delayIA);

        // 1. Si la IA tiene menos de 5, roba
        if (game.manoIAActual.Count < 5)
        {
            Debug.Log("IA roba carta");
            game.RobarCartaIA();
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            // 2. Si tiene 5, juega una carta aleatoria
            Debug.Log("IA juega carta");

            game.IA_JugarCarta();
            yield return new WaitForSeconds(0.5f);
        }

        // Después de actuar vuelve al jugador
        IniciarTurnoJugador();
    }
}
