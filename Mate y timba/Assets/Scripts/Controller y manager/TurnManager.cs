using UnityEngine;
using System.Collections;

public class TurnManager : MonoBehaviour
{
    public GameController game;
    public float delayIA = 1f;

    [Header("Diálogo de resultado del nivel")]
    [SerializeField] private MonoBehaviour dialogoResultadoNivel;
    private IResultadoDialogo dialogoResultado;

    private bool turnoJugador = true;
    private bool partidaTerminada = false;

    [Header("Cheats (solo testing)")]
    public bool cheatsActivos = true;
    public KeyCode cheatVictoria = KeyCode.V;
    public KeyCode cheatDerrota = KeyCode.D;

    #region Inicio
    private void Start()
    {
        dialogoResultado = dialogoResultadoNivel as IResultadoDialogo;
        StartCoroutine(IniciarTurnos());
    }

    private IEnumerator IniciarTurnos()
    {
        yield return new WaitForSeconds(1f);
        IniciarTurnoJugador();
    }
    #endregion

    #region Update (Cheats)
    private void Update()
    {
        if (!cheatsActivos || partidaTerminada)
            return;

        if (Input.GetKeyDown(cheatVictoria))
        {
            Debug.Log("[CHEAT] Forzar victoria del jugador");
            ForzarResultado(true);
        }

        if (Input.GetKeyDown(cheatDerrota))
        {
            Debug.Log("[CHEAT] Forzar derrota del jugador");
            ForzarResultado(false);
        }
    }
    #endregion

    #region Turno Jugador
    private void IniciarTurnoJugador()
    {
        if (partidaTerminada) return;

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

        bool victoriaJugador = CalcularVictoriaJugador();
        ResolverResultado(victoriaJugador);
    }
    #endregion

    #region Cheat
    private void ForzarResultado(bool victoriaJugador)
    {
        if (partidaTerminada) return;

        partidaTerminada = true;
        ResolverResultado(victoriaJugador);
    }
    #endregion

    #region Resolución
    private bool CalcularVictoriaJugador()
    {
        ScoreManager sm = FindFirstObjectByType<ScoreManager>();
        sm.ActualizarPuntajes();

        int puntosJugador = int.Parse(sm.puntajeTotalJugador.text);
        int puntosIA = int.Parse(sm.puntajeTotalIA.text);

        return puntosJugador > puntosIA;
    }

    private void ResolverResultado(bool victoriaJugador)
    {
        // 🔹 1. Diálogo final si existe
        if (dialogoResultado != null)
        {
            if (victoriaJugador && dialogoResultado.TieneDialogoVictoria())
            {
                dialogoResultado.MostrarDialogoVictoria(() =>
                {
                    ResolverNivel(victoriaJugador);
                });
                return;
            }

            if (!victoriaJugador && dialogoResultado.TieneDialogoDerrota())
            {
                dialogoResultado.MostrarDialogoDerrota(() =>
                {
                    ResolverNivel(victoriaJugador);
                });
                return;
            }
        }

        // 🔹 2. Sin diálogo → flujo directo
        ResolverNivel(victoriaJugador);
    }

    private void ResolverNivel(bool victoriaJugador)
    {
        if (LevelManager.CurrentLevel == 0)
        {
            if (victoriaJugador)
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

        if (victoriaJugador)
        {
            LevelManager.AvanzarNivel();
        }
        else
        {
            LevelManagerFlags.VieneDeDerrota = true;
        }

        StartCoroutine(VolverADialogo());
    }
    #endregion

    #region Transición
    private IEnumerator VolverADialogo()
    {
        yield return new WaitForSeconds(2.5f);
        LevelManager.IrADialogo();
    }
    #endregion
}
