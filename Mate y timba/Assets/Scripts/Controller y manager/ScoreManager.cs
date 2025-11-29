using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    #region Variables
    public Tablero tablero;
    public TextMeshProUGUI puntajeTotalJugador;
    public TextMeshProUGUI puntajeTotalIA;
    public TextMeshProUGUI[] puntajeFilasJugador;   
    public TextMeshProUGUI[] puntajeFilasIA;        
    public TextMeshProUGUI[] puntajeColumnasJugador; 
    public TextMeshProUGUI[] puntajeColumnasIA;
    #endregion

    #region Unity Methods
    private void Start()
    {
        tablero = FindFirstObjectByType<Tablero>();
    }
    #endregion

    #region Public Methods
    public void ActualizarPuntajes()
    {
        CalcularFilas();
        CalcularColumnas();
        CalcularTotales();
    }
    #endregion

    #region Cálculo de Filas
    private void CalcularFilas()
    {
        for (int fila = 0; fila < 8; fila++)
        {
            int puntaje = CalcularPuntajeFila(fila);

            if (fila <= 3)
                puntajeFilasJugador[fila].text = puntaje.ToString();
            else
                puntajeFilasIA[fila - 4].text = puntaje.ToString();
        }
    }

    private int CalcularPuntajeFila(int fila)
    {
        int[] valores = new int[4];
        int count = 0;

        for (int c = 0; c < 4; c++)
        {
            Transform celdaT = tablero.celdas[c, fila];
            if (celdaT == null) continue;

            Cell cell = celdaT.GetComponent<Cell>();
            if (cell == null || !cell.isOccupied || cell.carta == null) continue;

            valores[count] = cell.carta.valor;
            count++;
        }

        return AplicarReglasPuntaje(valores, count);
    }
    #endregion

    #region Cálculo de Columnas
    private void CalcularColumnas()
    {
        for (int col = 0; col < 4; col++)
        {
            int puntajeJugador = CalcularPuntajeColumna(col, 0, 3);
            int puntajeIA = CalcularPuntajeColumna(col, 4, 7);

            puntajeColumnasJugador[col].text = puntajeJugador.ToString();
            puntajeColumnasIA[col].text = puntajeIA.ToString();
        }
    }

    private int CalcularPuntajeColumna(int col, int filaInicio, int filaFin)
    {
        int[] valores = new int[4];
        int count = 0;

        for (int fila = filaInicio; fila <= filaFin; fila++)
        {
            Transform celdaT = tablero.celdas[col, fila];
            if (celdaT == null) continue;

            Cell cell = celdaT.GetComponent<Cell>();
            if (cell == null || !cell.isOccupied || cell.carta == null) continue;

            valores[count] = cell.carta.valor;
            count++;
        }

        return AplicarReglasPuntaje(valores, count);
    }
    #endregion

    #region Cálculo de Totales
    private void CalcularTotales()
    {
        int totalJugador = 0;
        int totalIA = 0;

        for (int i = 0; i < 4; i++)
            totalJugador += int.Parse(puntajeFilasJugador[i].text);

        for (int i = 0; i < 4; i++)
            totalIA += int.Parse(puntajeFilasIA[i].text);

        for (int i = 0; i < 4; i++)
            totalJugador += int.Parse(puntajeColumnasJugador[i].text);

        for (int i = 0; i < 4; i++)
            totalIA += int.Parse(puntajeColumnasIA[i].text);

        puntajeTotalJugador.text = totalJugador.ToString();
        puntajeTotalIA.text = totalIA.ToString();
    }
    #endregion

    #region Reglas de Puntaje
    private int AplicarReglasPuntaje(int[] valores, int count)
    {
        if (count == 0) return 0;

        System.Array.Sort(valores, 0, count);

        if (count == 4 &&
            valores[0] == valores[1] &&
            valores[1] == valores[2] &&
            valores[2] == valores[3])
        {
            int suma4 = valores[0] * 4;
            return suma4 * 4;
        }

        if (count >= 3)
        {
            bool trio1 = valores[0] == valores[1] && valores[1] == valores[2];
            bool trio2 = count == 4 && valores[1] == valores[2] && valores[2] == valores[3];

            if (trio1 || trio2)
            {
                int valorTrio = trio1 ? valores[0] : valores[1];
                int sumaTrio = valorTrio * 3; 
                int totalTrio = sumaTrio * 3;

                if (count == 4)
                {
                    if (trio1) totalTrio += valores[3];
                    else totalTrio += valores[0];
                }

                return totalTrio;
            }
        }
        int suma = 0;
        for (int i = 0; i < count; i++)
            suma += valores[i];

        return suma;
    }
    #endregion
}