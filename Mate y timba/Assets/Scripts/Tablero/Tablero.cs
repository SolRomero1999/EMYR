using UnityEngine;

public class Tablero : MonoBehaviour
{
    public Transform[,] celdas; 

    public int columns = 4;
    public int rows = 8;

    private void Awake()
    {
        celdas = new Transform[columns, rows];
    }

    public void RegistrarCelda(int col, int fila, Transform celda)
    {
        if (col < 0 || col >= columns || fila < 0 || fila >= rows) return;
        celdas[col, fila] = celda;
    }

    public Transform ObtenerCelda(int col, int fila)
    {
        if (col < 0 || col >= columns || fila < 0 || fila >= rows) return null;
        return celdas[col, fila];
    }

    public bool EsFilaJugador(int fila)
    {
        return fila >= 0 && fila <= 3;
    }

    public bool EsFilaRival(int fila)
    {
        return fila >= 4 && fila <= 7;
    }

    public Cell ObtenerCeldaLibreIA()
    {
        for (int fila = 4; fila <= 7; fila++)
        {
            for (int col = 0; col < columns; col++)
            {
                Transform t = celdas[col, fila];
                if (t == null) continue;

                Cell celda = t.GetComponent<Cell>();
                if (celda != null && !celda.isOccupied)
                    return celda;
            }
        }

        return null; 
    }
}