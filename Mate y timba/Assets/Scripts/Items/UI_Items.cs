using UnityEngine;
using System.Collections.Generic;

public class UI_Items : MonoBehaviour
{
    #region Variables
    public Transform panelOpciones;
    public GameObject cartaUIPrefab;
    public List<Carta> cartasMostradas = new List<Carta>();

    private GameController game;
    private bool modoPucho = false;
    private Carta cartaSeleccionada = null;
    #endregion


    #region Unity Methods
    private void Start()
    {
        game = FindFirstObjectByType<GameController>();
        if (panelOpciones != null)
            panelOpciones.gameObject.SetActive(false);
    }
    #endregion

    #region Cerveza
    public void RevelarCartasParaCerveza()
    {
        cartasMostradas.Clear();

        int cantidad = Mathf.Min(3, game.mazo.cartas.Count);

        for (int i = 0; i < cantidad; i++)
        {
            Carta c = game.mazo.RobarCarta();
            cartasMostradas.Add(c);
        }

        panelOpciones.gameObject.SetActive(true);

        foreach (Transform child in panelOpciones)
            Destroy(child.gameObject);

        foreach (Carta c in cartasMostradas)
        {
            GameObject ui = Instantiate(cartaUIPrefab, panelOpciones);
            ui.GetComponent<UI_Carta>().Configurar(c, this);
        }
    }

    public void ElegirCarta(Carta seleccionada)
    {
        GameController gc = game;

        seleccionada.transform.SetParent(gc.manoJugador);
        seleccionada.enMano = true;
        gc.manoActual.Add(seleccionada);
        gc.ReordenarMano();
        seleccionada.MostrarFrente();

        foreach (Carta c in cartasMostradas)
        {
            if (c != seleccionada)
            {
                gc.mazo.cartas.Add(c);
                c.transform.SetParent(gc.mazo.transform);
                c.MostrarDorso();
            }
        }

        gc.mazo.Barajar();
        panelOpciones.gameObject.SetActive(false);
    }
    #endregion

    #region Pucho
    public void ActivarPucho()
    {
        modoPucho = true;

        for (int col = 0; col < game.tablero.columns; col++)
        {
            for (int fila = 4; fila <= 7; fila++)
            {
                Transform t = game.tablero.celdas[col, fila];
                if (t == null) continue;

                Cell celda = t.GetComponent<Cell>();
                if (celda.isOccupied && celda.carta != null)
                {
                    celda.carta.transform.localScale = new Vector3(0.2f, 0.2f, 1);
                }
            }
        }
    }

    public void SeleccionarCartaRival(Carta c)
    {
        if (!modoPucho) return;

        cartaSeleccionada = c;

        var sr = c.GetComponent<SpriteRenderer>();
        Color col = sr.color;
        col.a = 0.5f;
        sr.color = col;

        for (int colu = 0; colu < game.tablero.columns; colu++)
        {
            for (int fila = 4; fila <= 7; fila++)
            {
                Cell cell = game.tablero.celdas[colu, fila].GetComponent<Cell>();
                if (!cell.isOccupied)
                {
                    cell.sr.color = new Color(1f, 1f, 0.6f, 1f);
                }
            }
        }
    }

    public void SeleccionarCeldaRival(Cell celdaDestino)
    {
        if (!modoPucho || cartaSeleccionada == null) return;
        if (celdaDestino.isOccupied) return;

        Cell celdaOriginal = cartaSeleccionada.celdaActual;
        if (celdaOriginal != null)
            celdaOriginal.SetOccupied(null);

        cartaSeleccionada.ColocarEnCelda(celdaDestino);

        cartaSeleccionada.transform.localScale = new Vector3(0.1912268f, 0.1807186f, 1);

        FinalizarPucho();
    }

    private void FinalizarPucho()
    {
        modoPucho = false;

        for (int col = 0; col < game.tablero.columns; col++)
        {
            for (int fila = 4; fila <= 7; fila++)
            {
                Cell celda = game.tablero.celdas[col, fila].GetComponent<Cell>();

                if (celda.isOccupied && celda.carta != null)
                {
                    celda.carta.transform.localScale = new Vector3(0.1912268f, 0.1807186f, 1);

                    var sr = celda.carta.GetComponent<SpriteRenderer>();
                    Color colr = sr.color;
                    colr.a = 1f;
                    sr.color = colr;
                }

                celda.sr.color = celda.originalColor;
            }
        }

        cartaSeleccionada = null;
    }
    #endregion
}