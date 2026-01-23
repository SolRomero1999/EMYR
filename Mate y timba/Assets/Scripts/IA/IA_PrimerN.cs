using UnityEngine;
using System.Linq;
using System;

public class IA_PrimerN : IA_Base
{
    public static event Action OnIACambiaAModoAltas;
    private int turnosJugados = 0;
    private bool prioridadAltas = false;

    #region Turno IA
    public override void RobarCartaIA()
    {
        turnosJugados++;
        EvaluarCambioDeEstrategia();

        if (prioridadAltas)
        {
            RobarModoAltas();
            return;
        }

        if (TieneCartaMedia())
        {
            JugarTurno();
            return;
        }

        if (game.manoIAActual.Count < 5)
        {
            game.RobarCartaIA();
            return;
        }

        JugarTurno();
    }

    public override void JugarTurno()
    {
        if (game.manoIAActual.Count == 0) return;

        Carta carta = prioridadAltas ? ElegirCartaAlta() : ElegirCartaNormal();
        if (carta == null) return;

        Cell celda = tablero.ObtenerCeldaLibreIA();
        if (celda == null) return;

        carta.ColocarEnCelda(celda);
        carta.MostrarFrente();
        game.manoIAActual.Remove(carta);

        Debug.Log($"[IA PRIMER NIVEL] Jugó {carta.valor} | Modo: {(prioridadAltas ? "ALTAS" : "NORMAL")}");
    }
    #endregion

    #region Elección de Carta
    private Carta ElegirCartaNormal()
    {
        var media = game.manoIAActual
            .Where(c => c.valor >= 6 && c.valor <= 9)
            .OrderBy(c => c.valor)
            .FirstOrDefault();

        if (media != null) return media;

        var baja = game.manoIAActual
            .Where(c => c.valor < 6)
            .OrderByDescending(c => c.valor)
            .FirstOrDefault();

        if (baja != null) return baja;

        return game.manoIAActual
            .Where(c => c.valor > 9)
            .OrderBy(c => c.valor)
            .FirstOrDefault();
    }

    private Carta ElegirCartaAlta()
    {
        return game.manoIAActual
            .OrderByDescending(c => c.valor)
            .FirstOrDefault();
    }
    #endregion

    #region Cambio de Estrategia
    private void EvaluarCambioDeEstrategia()
    {
        if (prioridadAltas) return;
        if (turnosJugados < 6) return;

        int probabilidad = Mathf.Clamp(50 + (turnosJugados - 6) * 10, 50, 100);
        int tirada = UnityEngine.Random.Range(0, 100);

        Debug.Log($"[IA PRIMER NIVEL] Tirada {tirada} / Prob {probabilidad}");

        if (tirada < probabilidad || probabilidad >= 100)
        {
            prioridadAltas = true;
            Debug.Log("[IA PRIMER NIVEL] CAMBIA A PRIORIDAD ALTAS");
            OnIACambiaAModoAltas?.Invoke();
        }
    }
    #endregion

    #region Robo Modo Altas
    private void RobarModoAltas()
    {
        bool tieneAlta = game.manoIAActual.Any(c => c.valor >= 8);

        if (tieneAlta)
        {
            JugarTurno();
            return;
        }

        if (game.manoIAActual.Count < 5)
        {
            game.RobarCartaIA();
            return;
        }

        JugarTurno();
    }
    #endregion

    #region Helpers
    private bool TieneCartaMedia()
    {
        return game.manoIAActual.Any(c => c.valor >= 6 && c.valor <= 9);
    }
    #endregion
}