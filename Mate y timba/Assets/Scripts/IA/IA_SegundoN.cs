using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class IA_SegundoN : IA_Base
{
    private int valorTrioActivo = -1;
    private int columnaTrioActivo = -1;


    #region Turno IA
    public override void RobarCartaIA()
    {
        if (IntentarEliminarCartaJugador())
            return;

        if (game.manoIAActual.Count < 5)
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

        if (IntentarEliminarCartaJugador())
            return;

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

        Debug.Log($"[IA NIÑEZ] Jugó {cartaRandom.valor} al azar");
    }
    #endregion

    #region Eliminación Reactiva
    private bool IntentarEliminarCartaJugador()
    {
        int inicioFilaJugador = 0;
        int finFilaJugador = tablero.filasJugador - 1;

        for (int col = 0; col < tablero.columns; col++)
        {
            for (int fila = inicioFilaJugador; fila <= finFilaJugador; fila++)
            {
                Cell celdaJugador = tablero
                    .ObtenerCelda(col, fila)?
                    .GetComponent<Cell>();

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

                cartaIA.ColocarEnCelda(celdaLibreIA);
                cartaIA.MostrarFrente();
                game.manoIAActual.Remove(cartaIA);

                Debug.Log($"[IA NIÑEZ] Eliminó {cartaJugador.valor} en columna {col}");
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

        if (columnasLibres.Count > 0)
            columnaTrioActivo = columnasLibres[Random.Range(0, columnasLibres.Count)];
        else
            columnaTrioActivo = -1;

        Debug.Log($"[IA NIÑEZ] Inicia trío de {valorTrioActivo} en columna {columnaTrioActivo}");
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

        JugarCarta(carta);
        Debug.Log($"[IA NIÑEZ] Bajando trío de {valorTrioActivo}");
        return true;
    }
    #endregion

    #region Selección de cartas
    private Carta ElegirCartaNoDuplicada()
    {
        return game.manoIAActual
            .FirstOrDefault(c =>
                game.manoIAActual.Count(o => o.valor == c.valor) == 1);
    }
    #endregion

    #region Helpers
    private void JugarCarta(Carta carta)
    {
        Cell celda = tablero.ObtenerCeldaLibreIA();
        if (celda == null)
            return;

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
            Cell celda = tablero
                .ObtenerCelda(columna, fila)?
                .GetComponent<Cell>();

            if (celda != null && !celda.isOccupied)
                return celda;
        }

        return null;
    }
    #endregion
}