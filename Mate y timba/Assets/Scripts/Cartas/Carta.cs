using UnityEngine;
using UnityEngine.InputSystem;

public class Carta : MonoBehaviour
{
    #region Variables Públicas
    public int valor;
    public string palo;
    public Sprite frente;
    public Sprite dorso;

    [Header("Estado")]
    public bool enMano = false;
    [HideInInspector] public Cell celdaActual;

    #endregion

    #region Variables Privadas
    private SpriteRenderer sr;
    private Vector3 posicionOriginal;

    // Movimiento / Hover
    private float alturaHover = 0.3f;
    private float alturaSeleccion = 0.4f;
    private float velocidadMovimiento = 15f;
    private bool estaEnHover = false;
    private bool hoverAnterior = false;
    private bool seleccionada = false;

    // Componentes
    private Camera mainCamera;
    private BoxCollider2D boxCollider;
    private Mouse mouse;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
        boxCollider = GetComponent<BoxCollider2D>();
        mouse = Mouse.current;
    }

    private void Start()
    {
        posicionOriginal = transform.localPosition;
    }

    private void Update()
    {
        ActualizarHover();
        ActualizarPosicion();
        if (enMano && mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            if (EstaMouseSobreCarta())
                HacerSeleccion();
        }
    }

    private void OnMouseDown()
    {
        if (enMano)
            HacerSeleccion();
        UI_Items ui = FindFirstObjectByType<UI_Items>();
        if (ui != null)
            ui.SeleccionarCartaRival(this);
    }
    #endregion

    #region Input y Hover
    private void ActualizarHover()
    {
        if (!seleccionada && enMano)
        {
            bool hoverActual = EstaMouseSobreCarta();
            if (hoverActual != hoverAnterior)
            {
                estaEnHover = hoverActual;
                hoverAnterior = hoverActual;
            }
        }
        else
        {
            estaEnHover = false;
        }
    }

    private bool EstaMouseSobreCarta()
    {
        if (!mainCamera || !boxCollider || mouse == null) return false;
        Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouse.position.ReadValue());
        return boxCollider.OverlapPoint(mouseWorldPos);
    }
    #endregion

    #region Selección
    private void HacerSeleccion()
    {
        if (SeleccionCartas.Instance == null)
        {
            Debug.LogWarning("SeleccionCartas.Instance es null — falta el objeto ControlSeleccion en la escena.");
            return;
        }

        seleccionada = true;
        SeleccionCartas.Instance.SeleccionarCarta(this);
        Debug.Log(name + " seleccionado");
    }

    public void Deseleccionar()
    {
        seleccionada = false;
        Debug.Log(name + " deseleccionado");
    }
    #endregion

    #region Movimiento / Posición
    private void ActualizarPosicion()
    {
        Vector3 posicionObjetivo = posicionOriginal;

        if (seleccionada)
            posicionObjetivo += Vector3.up * alturaSeleccion;
        else if (estaEnHover)
            posicionObjetivo += Vector3.up * alturaHover;

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            posicionObjetivo,
            velocidadMovimiento * Time.deltaTime
        );
    }

    public void SetPosicionOriginal(Vector3 nuevaPosicion)
    {
        posicionOriginal = nuevaPosicion;
        transform.localPosition = nuevaPosicion;
    }
    #endregion

    #region Colocación en celdas
    public void ColocarEnCelda(Cell celda)
    {
        seleccionada = false;
        enMano = false;
        celda.SetOccupied(this);

        if (this is CartaComodin comodin)
        {
            Tablero tablero = FindFirstObjectByType<Tablero>();
            if (tablero != null)
            {
                comodin.ConfigurarValorInicial(celda, tablero);
            }
        }

        GameController gc = FindFirstObjectByType<GameController>();

        if (gc != null && gc.manoActual.Contains(this))
        {
            gc.manoActual.Remove(this);
            gc.ReordenarMano();
        }

        transform.SetParent(celda.transform);
        posicionOriginal = Vector3.zero;
        transform.localPosition = Vector3.zero;

        Debug.Log(name + $" colocado en celda {celda.column},{celda.row}");
        
        AplicarReglaEliminacion(celda);

        ScoreManager sm = FindFirstObjectByType<ScoreManager>();
        if (sm != null) 
        {
            sm.ActualizarPuntajes();
            Debug.Log($"Puntajes actualizados inmediatamente para {name} (valor: {valor})");
        }
    }

    private void DetectarTrioJugador(Cell celda)
    {
        Tablero tablero = FindFirstObjectByType<Tablero>();
        if (tablero == null) return;

        if (!tablero.EsFilaJugador(celda.row))
            return;

        int valorActual = valor;
        int coincidencias = 0;

        for (int c = 0; c < tablero.columns; c++)
        {
            Cell otra = tablero.ObtenerCelda(c, celda.row)?.GetComponent<Cell>();
            if (otra != null && otra.isOccupied && otra.carta.valor == valorActual)
                coincidencias++;
        }

        for (int f = 0; f < tablero.filasJugador; f++)
        {
            Cell otra = tablero.ObtenerCelda(celda.column, f)?.GetComponent<Cell>();
            if (otra != null && otra.isOccupied && otra.carta.valor == valorActual)
                coincidencias++;
        }

        if (coincidencias >= 3)
        {
            GameController gc = FindFirstObjectByType<GameController>();
            if (gc != null && gc.ia != null)
                gc.ia.OnTrioJugadorDetectado();
        }
    }
    #endregion

    #region Regla de eliminaciòn
    private void AplicarReglaEliminacion(Cell celda)
    {
        if (!LevelManager.reglasEliminacionActivas)
            return; 

        Tablero tablero = FindFirstObjectByType<Tablero>();
        if (tablero == null) return;

        int col = celda.column;
        int fila = celda.row;

        bool soyJugador = tablero.EsFilaJugador(fila);
        bool soyIA = tablero.EsFilaRival(fila);

        int valorColocado = valor;

        int filaInicioRival = soyJugador ? tablero.filasJugador : 0;
        int filaFinRival = soyJugador ? tablero.rows - 1 : tablero.filasJugador - 1;

        for (int f = filaInicioRival; f <= filaFinRival; f++)
        {
            Cell rivalCelda = tablero.ObtenerCelda(col, f)?.GetComponent<Cell>();
            if (rivalCelda == null || !rivalCelda.isOccupied)
                continue;

            Carta otraCarta = rivalCelda.GetComponentInChildren<Carta>();
            if (otraCarta == null) continue;

            if (otraCarta.valor == valorColocado)
            {
                Debug.Log($"ELIMINACIÓN: {otraCarta.valor} en columna {col}");

                if (soyJugador)
                {
                    GameController gc = FindFirstObjectByType<GameController>();
                    if (gc != null && gc.ia != null)
                    {
                        gc.ia.OnCartaEliminadaPorJugador();
                    }
                }

                rivalCelda.SetOccupied(null);
                Destroy(otraCarta.gameObject);

                ScoreManager sm = FindFirstObjectByType<ScoreManager>();
                sm?.ActualizarPuntajes();
            }
        }
    }
    #endregion

    #region Visual
    public void MostrarFrente()
    {
        if (sr) sr.sprite = frente;
    }

    public void MostrarDorso()
    {
        if (sr) sr.sprite = dorso;
    }
    #endregion
}