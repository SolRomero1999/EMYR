using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("Referencias")]
    public Tablero tablero;

    [Header("UI Puntajes")]
    public TextMeshProUGUI puntajeTotalJugador;
    public TextMeshProUGUI puntajeTotalIA;

    public TextMeshProUGUI[] puntajeFilasJugador;
    public TextMeshProUGUI[] puntajeFilasIA;

    public TextMeshProUGUI[] puntajeColumnasJugador;
    public TextMeshProUGUI[] puntajeColumnasIA;

    #region Unity
    private void Start()
    {
        tablero = FindFirstObjectByType<Tablero>();
    }
    #endregion

    #region Actualización total
    public void ActualizarPuntajes()
    {
        CalcularFilas();
        CalcularColumnas();
        CalcularTotales();
    }
    #endregion

    #region Filas
    private void CalcularFilas()
    {
        int mitad = tablero.rows / 2;

        for (int fila = 0; fila < tablero.rows; fila++)
        {
            int puntaje = CalcularPuntajeFila(fila);

            if (fila < mitad)
                puntajeFilasJugador[fila].text = puntaje.ToString();
            else
                puntajeFilasIA[fila - mitad].text = puntaje.ToString();
        }
    }

    private int CalcularPuntajeFila(int fila)
    {
        int[] valores = new int[tablero.columns];
        int count = 0;

        for (int c = 0; c < tablero.columns; c++)
        {
            Transform t = tablero.celdas[c, fila];
            if (t == null) continue;

            Cell celda = t.GetComponent<Cell>();
            if (celda == null || !celda.isOccupied || celda.carta == null) continue;

            valores[count++] = celda.carta.valor;
        }

        return AplicarReglasPuntaje(valores, count);
    }
    #endregion

    #region Columnas
    private void CalcularColumnas()
    {
        int mitad = tablero.rows / 2;

        int columnasTablero = tablero.columns;
        int columnasUIJugador = puntajeColumnasJugador.Length;
        int columnasUIIA = puntajeColumnasIA.Length;

        int columnasACalcular = Mathf.Min(columnasTablero, columnasUIJugador, columnasUIIA);

        for (int col = 0; col < columnasACalcular; col++)
        {
            int pj = CalcularPuntajeColumna(col, 0, mitad - 1);
            int pi = CalcularPuntajeColumna(col, mitad, tablero.rows - 1);

            puntajeColumnasJugador[col].text = pj.ToString();
            puntajeColumnasIA[col].text = pi.ToString();
        }
    }

    private int CalcularPuntajeColumna(int col, int filaInicio, int filaFin)
    {
        int[] valores = new int[tablero.rows];
        int count = 0;

        for (int fila = filaInicio; fila <= filaFin; fila++)
        {
            Transform t = tablero.celdas[col, fila];
            if (t == null) continue;

            Cell celda = t.GetComponent<Cell>();
            if (celda == null || !celda.isOccupied || celda.carta == null) continue;

            valores[count++] = celda.carta.valor;
        }

        return AplicarReglasPuntaje(valores, count);
    }
    #endregion

    #region Totales
    private void CalcularTotales()
    {
        int mitad = tablero.rows / 2;
        int totalJugador = 0;
        int totalIA = 0;

        // Filas
        int filasJugador = Mathf.Min(mitad, puntajeFilasJugador.Length);
        int filasIA = Mathf.Min(mitad, puntajeFilasIA.Length);

        for (int i = 0; i < filasJugador; i++)
            totalJugador += int.Parse(puntajeFilasJugador[i].text);

        for (int i = 0; i < filasIA; i++)
            totalIA += int.Parse(puntajeFilasIA[i].text);

        // Columnas
        int columnasJugador = Mathf.Min(tablero.columns, puntajeColumnasJugador.Length);
        int columnasIA = Mathf.Min(tablero.columns, puntajeColumnasIA.Length);

        for (int j = 0; j < columnasJugador; j++)
            totalJugador += int.Parse(puntajeColumnasJugador[j].text);

        for (int j = 0; j < columnasIA; j++)
            totalIA += int.Parse(puntajeColumnasIA[j].text);

        puntajeTotalJugador.text = totalJugador.ToString();
        puntajeTotalIA.text = totalIA.ToString();
    }
    #endregion

    #region Reglas de puntaje
    private int AplicarReglasPuntaje(int[] valores, int count)
    {
        if (count == 0) return 0;

        System.Array.Sort(valores, 0, count);

        // 4 iguales
        if (count == 4 && valores[0] == valores[3])
        {
            int suma4 = valores[0] * 4;
            return suma4 * 4;
        }

        // ternas
        if (count >= 3)
        {
            bool t1 = valores[0] == valores[1] && valores[1] == valores[2];
            bool t2 = (count == 4 && valores[1] == valores[2] && valores[2] == valores[3]);

            if (t1 || t2)
            {
                int v = t1 ? valores[0] : valores[1];
                int suma = v * 3;
                int total = suma * 3;

                if (count == 4)
                    total += t1 ? valores[3] : valores[0];

                return total;
            }
        }

        // suma normal
        int s = 0;
        for (int i = 0; i < count; i++)
            s += valores[i];

        return s;
    }
    #endregion
}