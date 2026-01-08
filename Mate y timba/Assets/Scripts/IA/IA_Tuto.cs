using UnityEngine;
using System;
using System.Linq;

public class IA_Tuto : IA_Base
{
    public static event Action OnTrioTutorialCompletado;

    private int valorTrioActivo = -1;
    private bool forzarRoboSiguienteTurno = false;

    public override void RobarCartaIA()
    {
        if (forzarRoboSiguienteTurno)
        {
            forzarRoboSiguienteTurno = false;
            game.RobarCartaIA();
            return;
        }

        if (valorTrioActivo != -1)
        {
            JugarTurno();
            return;
        }

        if (game.manoIAActual.Count < 3)
            game.RobarCartaIA();
        else
            JugarTurno();
    }

    public override void JugarTurno()
    {
        if (game.manoIAActual.Count == 0) return;

        if (valorTrioActivo == -1)
        {
            var grupo = game.manoIAActual
                .GroupBy(c => c.valor)
                .FirstOrDefault(g => g.Count() >= 3);

            if (grupo != null)
                valorTrioActivo = grupo.Key;
        }

        if (valorTrioActivo != -1)
        {
            Carta cartaTrio = game.manoIAActual
                .FirstOrDefault(c => c.valor == valorTrioActivo);

            if (cartaTrio != null)
            {
                JugarCarta(cartaTrio);
                Debug.Log($"[IA TUTO] Jugó {cartaTrio.valor} (trío)");

                bool quedanMas = game.manoIAActual.Any(c => c.valor == valorTrioActivo);

                if (!quedanMas)
                {
                    valorTrioActivo = -1;
                    forzarRoboSiguienteTurno = true;
                    OnTrioTutorialCompletado?.Invoke();
                }

                return;
            }
        }

        Carta carta = game.manoIAActual
            .OrderBy(c => c.valor)
            .First();

        JugarCarta(carta);
        Debug.Log($"[IA TUTO] Jugó {carta.valor}");
    }

    private void JugarCarta(Carta carta)
    {
        Cell celda = tablero.ObtenerCeldaLibreIA();
        if (celda == null) return;

        carta.ColocarEnCelda(celda);
        carta.MostrarFrente();
        game.manoIAActual.Remove(carta);
    }
}
