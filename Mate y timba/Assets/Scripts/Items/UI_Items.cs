using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class UI_Items : MonoBehaviour
{
    #region Variables
    public Transform panelOpciones;
    public TMP_Text textoPanel;
    public GameObject cartaUIPrefab_Cerveza;
    public List<Carta> cartasMostradas = new List<Carta>();

    private GameController game;
    private bool modoPucho = false;
    private Carta cartaSeleccionada = null;

    private bool modoMateLavado = false;
    public GameObject cartaUIPrefab_Mate;
    #endregion

    #region ★ Resultado Mate
    public enum ResultadoMate
    {
        Rico,
        Lavado,
        Feo
    }

    public static event System.Action<ResultadoMate, System.Action> OnResultadoMate;
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

        MostrarPanelConTexto("Elegí una de estas cartas");

        foreach (Transform child in panelOpciones)
            Destroy(child.gameObject);

        foreach (Carta c in cartasMostradas)
        {
            GameObject ui = Instantiate(cartaUIPrefab_Cerveza, panelOpciones);
            ui.transform.localScale = Vector3.one * 0.65f;
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

    #region Mate
    public void ActivarMate()
    {
        float r = Random.value;

        if (r <= 0.70f)
        {
            OnResultadoMate?.Invoke(ResultadoMate.Rico, MateRico);
        }
        else if (r <= 0.95f)
        {
            OnResultadoMate?.Invoke(ResultadoMate.Lavado, MateLavado);
        }
        else
        {
            OnResultadoMate?.Invoke(ResultadoMate.Feo, MateFeo);
        }
    }

    private void MateRico()
    {
        StartCoroutine(MateRicoCoroutine());
    }

    private IEnumerator MateRicoCoroutine()
    {
        TurnManager tm = FindFirstObjectByType<TurnManager>();
        if (tm != null)
            tm.BloquearInputJugador();

        foreach (Carta c in new List<Carta>(game.manoActual))
        {
            if (c.celdaActual != null)
                c.celdaActual.SetOccupied(null);

            yield return c.StartCoroutine(c.AnimarDescartar(Vector3.down));
            Destroy(c.gameObject);
        }

        game.manoActual.Clear();
        yield return new WaitForSeconds(0.1f);

        int cantidad = 5;
        for (int i = 0; i < cantidad; i++)
        {
            Carta nueva = game.mazo.RobarCarta();
            nueva.enMano = true;
            nueva.MostrarFrente();
            nueva.transform.SetParent(game.manoJugador);

            game.manoActual.Add(nueva);
            game.ReordenarMano();

            Vector3 posFinal = nueva.transform.localPosition;
            nueva.transform.localPosition = Vector3.zero;

            nueva.AnimarRoboDesdeMazo(
                game.mazo.transform,
                game.manoJugador,
                posFinal,
                true
            );

            yield return new WaitForSeconds(0.15f);
        }

        game.ReordenarMano();
        if (tm != null)
            tm.HabilitarInputJugador();
    }

    private void MateLavado()
    {
        modoMateLavado = true;
        MostrarPanelConTexto("Elegí una carta para cambiarla");

        foreach (Transform child in panelOpciones)
            Destroy(child.gameObject);

        foreach (Carta c in game.manoActual)
        {
            GameObject ui = Instantiate(cartaUIPrefab_Mate, panelOpciones);
            ui.transform.localScale = Vector3.one * 0.65f;
            ui.GetComponent<UI_Carta>().Configurar(c, this);
        }
    }

    public void SeleccionarCartaParaDescartar(Carta c)
    {
        if (!modoMateLavado) return;

        modoMateLavado = false;
        OcultarPanel();
        StartCoroutine(DescartarYReponerMate(c));
    }

    private IEnumerator DescartarYReponerMate(Carta c)
    {
        if (c.celdaActual != null)
            c.celdaActual.SetOccupied(null);

        game.manoActual.Remove(c);

        yield return c.StartCoroutine(c.AnimarDescartar(Vector3.down));
        Destroy(c.gameObject);

        yield return new WaitForSeconds(0.1f);

        Carta nueva = game.mazo.RobarCarta();
        nueva.transform.SetParent(game.manoJugador);
        nueva.enMano = true;
        nueva.MostrarFrente();

        game.manoActual.Add(nueva);
        game.ReordenarMano();

        Vector3 posFinal = nueva.transform.localPosition;
        nueva.transform.localPosition = Vector3.zero;

        nueva.AnimarRoboDesdeMazo(
            game.mazo.transform,
            game.manoJugador,
            posFinal,
            true
        );
    }

    private void MateFeo()
    {
        FindFirstObjectByType<TurnManager>()?.ForzarFinTurnoJugador();
    }
    #endregion

    #region Panel opciones
    private void MostrarPanelConTexto(string mensaje)
    {
        panelOpciones.gameObject.SetActive(true);

        if (textoPanel != null)
        {
            textoPanel.text = mensaje;
            textoPanel.gameObject.SetActive(true);
        }
    }

    private void OcultarPanel()
    {
        panelOpciones.gameObject.SetActive(false);

        if (textoPanel != null)
            textoPanel.gameObject.SetActive(false);
    }
    #endregion
}
