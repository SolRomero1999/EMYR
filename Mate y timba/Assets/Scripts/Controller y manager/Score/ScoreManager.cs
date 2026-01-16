using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    [Header("Referencias")]
    public Tablero tablero;
    public ScoreFeedbackVisualizer feedback;
    public ScoreTextAnimator textAnimator;

    [Header("UI Puntajes")]
    public TextMeshProUGUI puntajeTotalJugador;
    public TextMeshProUGUI puntajeTotalIA;

    public TextMeshProUGUI[] puntajeFilasJugador;
    public TextMeshProUGUI[] puntajeFilasIA;

    public TextMeshProUGUI[] puntajeColumnasJugador;
    public TextMeshProUGUI[] puntajeColumnasIA;

    private int[] ultimoPuntajeFilasJugador;
    private int[] ultimoPuntajeFilasIA;
    private int[] ultimoPuntajeColumnasJugador;
    private int[] ultimoPuntajeColumnasIA;

    private void Start()
    {
        tablero = FindFirstObjectByType<Tablero>();
        feedback = FindFirstObjectByType<ScoreFeedbackVisualizer>();
        textAnimator = FindFirstObjectByType<ScoreTextAnimator>();

        ultimoPuntajeFilasJugador = new int[puntajeFilasJugador.Length];
        ultimoPuntajeFilasIA = new int[puntajeFilasIA.Length];
        ultimoPuntajeColumnasJugador = new int[puntajeColumnasJugador.Length];
        ultimoPuntajeColumnasIA = new int[puntajeColumnasIA.Length];
    }

    public void ActualizarPuntajes()
    {
        feedback.Limpiar();
        CalcularFilas();
        CalcularColumnas();
        CalcularTotales();
    }

    #region FILAS
    private void CalcularFilas()
    {
        int mitad = tablero.rows / 2;

        for (int fila = 0; fila < tablero.rows; fila++)
        {
            List<Cell> celdas = ObtenerCeldasFila(fila);
            int puntaje = CalcularPuntajeFila(fila);

            ComboType tipo;
            Transform[] cartas;
            bool hayCombo = EvaluarCombo(celdas, out tipo, out cartas);

            if (hayCombo)
                feedback.MostrarCombo(tipo, cartas);

            if (fila < mitad)
            {
                if (hayCombo && puntaje != ultimoPuntajeFilasJugador[fila])
                {
                    textAnimator.Animar(puntajeFilasJugador[fila], puntaje, tipo);
                }
                else
                {
                    puntajeFilasJugador[fila].text = puntaje.ToString();
                }

                ultimoPuntajeFilasJugador[fila] = puntaje;
            }
            else
            {
                int idx = fila - mitad;

                if (hayCombo && puntaje != ultimoPuntajeFilasIA[idx])
                {
                    textAnimator.Animar(puntajeFilasIA[idx], puntaje, tipo);
                }
                else
                {
                    puntajeFilasIA[idx].text = puntaje.ToString();
                }

                ultimoPuntajeFilasIA[idx] = puntaje;
            }
        }
    }

    private List<Cell> ObtenerCeldasFila(int fila)
    {
        List<Cell> celdas = new();

        for (int c = 0; c < tablero.columns; c++)
        {
            Transform t = tablero.celdas[c, fila];
            if (!t) continue;

            Cell cell = t.GetComponent<Cell>();
            if (cell && cell.isOccupied && cell.carta != null)
                celdas.Add(cell);
        }

        return celdas;
    }
    #endregion

    #region COLUMNAS
    private void CalcularColumnas()
    {
        int mitad = tablero.rows / 2;

        int columnasACalcular = Mathf.Min(
            tablero.columns,
            puntajeColumnasJugador.Length,
            puntajeColumnasIA.Length
        );

        for (int col = 0; col < columnasACalcular; col++)
        {
            List<Cell> celdasJugador = ObtenerCeldasColumna(col, 0, mitad - 1);
            int pj = CalcularPuntajeColumna(col, 0, mitad - 1);

            ComboType tipoJ;
            Transform[] cartasJ;
            bool comboJ = EvaluarCombo(celdasJugador, out tipoJ, out cartasJ);

            if (comboJ && pj != ultimoPuntajeColumnasJugador[col])
            {
                feedback.MostrarCombo(tipoJ, cartasJ);
                textAnimator.Animar(puntajeColumnasJugador[col], pj, tipoJ);
            }
            else
            {
                puntajeColumnasJugador[col].text = pj.ToString();
            }

            ultimoPuntajeColumnasJugador[col] = pj;

            List<Cell> celdasIA = ObtenerCeldasColumna(col, mitad, tablero.rows - 1);
            int pi = CalcularPuntajeColumna(col, mitad, tablero.rows - 1);

            ComboType tipoIA;
            Transform[] cartasIA;
            bool comboIA = EvaluarCombo(celdasIA, out tipoIA, out cartasIA);

            if (comboIA && pi != ultimoPuntajeColumnasIA[col])
            {
                feedback.MostrarCombo(tipoIA, cartasIA);
                textAnimator.Animar(puntajeColumnasIA[col], pi, tipoIA);
            }
            else
            {
                puntajeColumnasIA[col].text = pi.ToString();
            }

            ultimoPuntajeColumnasIA[col] = pi;
        }
    }

    private List<Cell> ObtenerCeldasColumna(int col, int filaInicio, int filaFin)
    {
        List<Cell> celdas = new();

        for (int fila = filaInicio; fila <= filaFin; fila++)
        {
            Transform t = tablero.celdas[col, fila];
            if (!t) continue;

            Cell cell = t.GetComponent<Cell>();
            if (cell && cell.isOccupied && cell.carta != null)
                celdas.Add(cell);
        }

        return celdas;
    }
    #endregion

    #region COMBOS
    private bool EvaluarCombo(List<Cell> celdas, out ComboType tipo, out Transform[] cartas)
    {
        tipo = ComboType.Trio;
        cartas = null;

        if (celdas.Count < 3)
            return false;

        celdas.Sort((a, b) => a.carta.valor.CompareTo(b.carta.valor));

        if (celdas.Count == 4 &&
            celdas[0].carta.valor == celdas[3].carta.valor)
        {
            tipo = ComboType.Cuarteto;
            cartas = celdas.ConvertAll(c => c.carta.transform).ToArray();
            return true;
        }

        for (int i = 0; i <= celdas.Count - 3; i++)
        {
            if (celdas[i].carta.valor == celdas[i + 1].carta.valor &&
                celdas[i].carta.valor == celdas[i + 2].carta.valor)
            {
                tipo = ComboType.Trio;
                cartas = new Transform[]
                {
                    celdas[i].carta.transform,
                    celdas[i + 1].carta.transform,
                    celdas[i + 2].carta.transform
                };
                return true;
            }
        }

        return false;
    }
    #endregion

    #region PUNTAJE
    private int CalcularPuntajeFila(int fila)
    {
        int[] valores = new int[tablero.columns];
        int count = 0;

        for (int c = 0; c < tablero.columns; c++)
        {
            Transform t = tablero.celdas[c, fila];
            if (!t) continue;

            Cell celda = t.GetComponent<Cell>();
            if (celda && celda.isOccupied && celda.carta != null)
                valores[count++] = celda.carta.valor;
        }

        return AplicarReglasPuntaje(valores, count);
    }

    private int CalcularPuntajeColumna(int col, int filaInicio, int filaFin)
    {
        int[] valores = new int[tablero.rows];
        int count = 0;

        for (int fila = filaInicio; fila <= filaFin; fila++)
        {
            Transform t = tablero.celdas[col, fila];
            if (!t) continue;

            Cell celda = t.GetComponent<Cell>();
            if (celda && celda.isOccupied && celda.carta != null)
                valores[count++] = celda.carta.valor;
        }

        return AplicarReglasPuntaje(valores, count);
    }

    private int AplicarReglasPuntaje(int[] valores, int count)
    {
        if (count == 0) return 0;

        System.Array.Sort(valores, 0, count);

        if (count == 4 && valores[0] == valores[3])
            return valores[0] * 16;

        if (count >= 3)
        {
            bool t1 = valores[0] == valores[1] && valores[1] == valores[2];
            bool t2 = count == 4 && valores[1] == valores[2] && valores[2] == valores[3];

            if (t1 || t2)
            {
                int v = t1 ? valores[0] : valores[1];
                int total = v * 9;
                if (count == 4)
                    total += t1 ? valores[3] : valores[0];
                return total;
            }
        }

        int s = 0;
        for (int i = 0; i < count; i++)
            s += valores[i];

        return s;
    }
    #endregion

    private void CalcularTotales()
    {
        int mitad = tablero.rows / 2;
        int totalJugador = 0;
        int totalIA = 0;

        for (int i = 0; i < Mathf.Min(mitad, puntajeFilasJugador.Length); i++)
            totalJugador += int.Parse(puntajeFilasJugador[i].text);

        for (int i = 0; i < Mathf.Min(mitad, puntajeFilasIA.Length); i++)
            totalIA += int.Parse(puntajeFilasIA[i].text);

        for (int j = 0; j < Mathf.Min(tablero.columns, puntajeColumnasJugador.Length); j++)
            totalJugador += int.Parse(puntajeColumnasJugador[j].text);

        for (int j = 0; j < Mathf.Min(tablero.columns, puntajeColumnasIA.Length); j++)
            totalIA += int.Parse(puntajeColumnasIA[j].text);

        puntajeTotalJugador.text = totalJugador.ToString();
        puntajeTotalIA.text = totalIA.ToString();
    }
}