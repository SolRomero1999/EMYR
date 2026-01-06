using UnityEngine;

public abstract class IA_Base : MonoBehaviour
{
    protected GameController game;
    protected Tablero tablero;

    public virtual void OnCartaEliminadaPorJugador() { }
    public virtual void OnTrioJugadorDetectado() { }
    public virtual void Inicializar(GameController gameController)
    {
        game = gameController;
        tablero = gameController.tablero;
    }

    public virtual void RobarCartaIA()
    {
        game.RobarCartaIA();
    }

    public abstract void JugarTurno();
}
