using UnityEngine;
using System.Collections;

public class TurnManager : MonoBehaviour
{
    public GameController game;
    public float delayIA = 1f;

    private bool turnoJugador = true;
    private bool partidaTerminada = false;

    #region Inicio
    private void Start()
    {
        StartCoroutine(IniciarTurnos());
    }

    private IEnumerator IniciarTurnos()
    {
        yield return new WaitForSeconds(1f);
        IniciarTurnoJugador();
    }
    #endregion

    #region Turno Jugador
    private void IniciarTurnoJugador()
    {
        turnoJugador = true;
        game.jugadorYaRobo = false;
    }

    public void TerminarTurnoJugador()
    {
        if (!turnoJugador || partidaTerminada) return;

        turnoJugador = false;
        VerificarFinDePartida();
        StartCoroutine(TurnoIA());
    }

    public void ForzarFinTurnoJugador()
    {
        if (partidaTerminada) return;

        turnoJugador = false;
        StartCoroutine(TurnoIA());
    }
    #endregion

    #region Turno IA
    private IEnumerator TurnoIA()
    {
        yield return new WaitForSeconds(delayIA);

        if (partidaTerminada) yield break;

        if (game.manoIAActual.Count < game.maxCartasMano)
        {
            game.ia.RobarCartaIA();
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            game.ia.JugarTurno();
            yield return new WaitForSeconds(0.5f);
        }

        VerificarFinDePartida();
        IniciarTurnoJugador();
    }
    #endregion

    #region Fin de Partida
    public void VerificarFinDePartida()
    {
        if (partidaTerminada) return;

        bool jugadorTieneCeldas = game.tablero.HayCeldasDisponiblesJugador();
        bool iaTieneCeldas = game.tablero.HayCeldasDisponiblesIA();
        bool mazoVacio = game.mazo.cartas.Count == 0;

        if (!jugadorTieneCeldas || !iaTieneCeldas || mazoVacio)
        {
            FinalizarPartida();
        }
    }

    private void FinalizarPartida()
    {
        if (partidaTerminada) return;
        partidaTerminada = true;

        ScoreManager sm = FindFirstObjectByType<ScoreManager>();
        sm.ActualizarPuntajes();

        int puntosJugador = int.Parse(sm.puntajeTotalJugador.text);
        int puntosIA = int.Parse(sm.puntajeTotalIA.text);

        bool jugadorGana = puntosJugador > puntosIA;

        if (LevelManager.CurrentLevel == 0)
        {
            if (jugadorGana)
            {
                LevelManager.tutorialDialogoVisto = true;
                LevelManager.UltimoNivelCompletado = 0;
                LevelManager.CurrentLevel = 1;
            }
            else
            {
                LevelManagerFlags.VieneDeDerrota = true;
            }

            StartCoroutine(VolverADialogo());
            return;
        }

        if (jugadorGana)
        {
            LevelManager.AvanzarNivel();
        }
        else
        {
            LevelManagerFlags.VieneDeDerrota = true;
        }

        StartCoroutine(VolverADialogo());
    }

    private IEnumerator VolverADialogo()
    {
        yield return new WaitForSeconds(2.5f);
        LevelManager.IrADialogo();
    }
    #endregion
}
