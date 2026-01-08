using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class IA_TercerN : IA_Base
{
    private int valorTrioActivo = -1;
    private int columnaTrioActivo = -1;

    private int turnosEnojo = 0;
    private const int DURACION_ENOJO = 2;

    #region Notificaciones externas
    public override void OnCartaEliminadaPorJugador()
    {
        turnosEnojo = DURACION_ENOJO;
        Debug.Log("[IA SECUNDARIA] Se enojó por eliminación");
    }

    public override void OnTrioJugadorDetectado()
    {
        turnosEnojo = DURACION_ENOJO;
        Debug.Log("[IA SECUNDARIA] Se enojó por trío del jugador");
    }
    #endregion

    #region Turno IA
    public override void RobarCartaIA()
    {
        if (game.manoIAActual.Count < game.maxCartasMano)
        {
            game.RobarCartaIA();
            return;
        }

        JugarTurno();
    }

    public override void JugarTurno()
    {
        if (game.manoIAActual.Count == 0)
            return;

        if (valorTrioActivo != -1)
        {
            if (JugarCartaDeTrio())
                return;

            valorTrioActivo = -1;
            columnaTrioActivo = -1;
        }

        if (turnosEnojo > 0)
        {
            if (IntentarEliminarCartaJugador())
            {
                turnosEnojo--;
                return;
            }

            turnosEnojo--;
        }

        if (BuscarYActivarTrio())
            return;

        Carta cartaNoDuplicada = ElegirCartaNoDuplicada();
        if (cartaNoDuplicada != null)
        {
            JugarCarta(cartaNoDuplicada);
            return;
        }

        Carta cartaRandom = game.manoIAActual[Random.Range(0, game.manoIAActual.Count)];
        JugarCarta(cartaRandom);
    }
    #endregion

    #region Eliminación reactiva (solo en enojo)
    private bool IntentarEliminarCartaJugador()
    {
        int inicioFilaJugador = 0;
        int finFilaJugador = tablero.filasJugador - 1;

        for (int col = 0; col < tablero.columns; col++)
        {
            for (int fila = inicioFilaJugador; fila <= finFilaJugador; fila++)
            {
                Cell celdaJugador = tablero.ObtenerCelda(col, fila)?.GetComponent<Cell>();
                if (celdaJugador == null || !celdaJugador.isOccupied)
                    continue;

                Carta cartaJugador = celdaJugador.GetComponentInChildren<Carta>();
                if (cartaJugador == null)
                    continue;

                Carta cartaIA = game.manoIAActual
                    .FirstOrDefault(c => c.valor == cartaJugador.valor);

                if (cartaIA == null)
                    continue;

                Cell celdaLibreIA = ObtenerCeldaLibreIAEnColumna(col);
                if (celdaLibreIA == null)
                    continue;

                JugarCartaEnCelda(cartaIA, celdaLibreIA);
                Debug.Log($"[IA SECUNDARIA] Eliminó por enojo {cartaJugador.valor}");
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Tríos
    private bool BuscarYActivarTrio()
    {
        var grupo = game.manoIAActual
            .GroupBy(c => c.valor)
            .FirstOrDefault(g => g.Count() >= 3);

        if (grupo == null)
            return false;

        valorTrioActivo = grupo.Key;

        var columnasLibres = tablero.ObtenerCeldasLibresIA()
            .Select(c => c.column)
            .Distinct()
            .ToList();

        columnaTrioActivo = columnasLibres.Count > 0
            ? columnasLibres[Random.Range(0, columnasLibres.Count)]
            : -1;

        Debug.Log($"[IA SECUNDARIA] Inicia trío de {valorTrioActivo}");
        return JugarCartaDeTrio();
    }

    private bool JugarCartaDeTrio()
    {
        Carta carta = game.manoIAActual
            .FirstOrDefault(c => c.valor == valorTrioActivo);

        if (carta == null)
            return false;

        Cell celda = columnaTrioActivo != -1
            ? ObtenerCeldaLibreIAEnColumna(columnaTrioActivo)
            : tablero.ObtenerCeldaLibreIA();

        if (celda == null)
            return false;

        JugarCartaEnCelda(carta, celda);
        return true;
    }
    #endregion

    #region Selección
    private Carta ElegirCartaNoDuplicada()
    {
        return game.manoIAActual
            .FirstOrDefault(c => game.manoIAActual.Count(o => o.valor == c.valor) == 1);
    }
    #endregion

    #region Helpers
    private void JugarCarta(Carta carta)
    {
        Cell celda = tablero.ObtenerCeldaLibreIA();
        if (celda == null) return;

        JugarCartaEnCelda(carta, celda);
    }

    private void JugarCartaEnCelda(Carta carta, Cell celda)
    {
        carta.ColocarEnCelda(celda);
        carta.MostrarFrente();
        game.manoIAActual.Remove(carta);
    }

    private Cell ObtenerCeldaLibreIAEnColumna(int columna)
    {
        int inicioFilaIA = tablero.filasJugador;
        int finFilaIA = tablero.filasJugador + tablero.filasIA - 1;

        for (int fila = inicioFilaIA; fila <= finFilaIA; fila++)
        {
            Cell celda = tablero.ObtenerCelda(columna, fila)?.GetComponent<Cell>();
            if (celda != null && !celda.isOccupied)
                return celda;
        }

        return null;
    }
    #endregion
}
