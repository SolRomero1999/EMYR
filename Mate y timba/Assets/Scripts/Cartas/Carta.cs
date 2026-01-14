using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

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

    // Hover / selección
    private float alturaHover = 0.3f;
    private float alturaSeleccion = 0.4f;
    private float velocidadMovimiento = 15f;
    private bool estaEnHover = false;
    private bool hoverAnterior = false;
    private bool seleccionada = false;
    private bool enAnimacion = false;

    [Header("Animación de colocación")]
    [SerializeField] private float duracionMovimientoACelda = 0.25f;
    private Coroutine moverCoroutine;

    // Componentes
    private Camera mainCamera;
    private BoxCollider2D boxCollider;

    // Referencias cacheadas
    private TurnManager tm;
    private GameController gc;
    private Tablero tablero;
    private UI_Items ui;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        posicionOriginal = transform.localPosition;

        tm = FindFirstObjectByType<TurnManager>();
        gc = FindFirstObjectByType<GameController>();
        tablero = FindFirstObjectByType<Tablero>();
        ui = FindFirstObjectByType<UI_Items>();
    }

    private void OnDisable()
    {
        enAnimacion = false;
        if (boxCollider != null)
            boxCollider.enabled = true;
    }

    private void Update()
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (enAnimacion)
            return;

        ActualizarHover();
        ActualizarPosicion();

        if (!enMano || tm == null || !tm.PuedeInteractuarJugador())
            return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EstaMouseSobreCarta())
                HacerSeleccion();
        }
    }
    #endregion

    #region Hover
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
        if (boxCollider == null)
            return false;

        Camera cam = Camera.main;
        if (cam == null)
            return false;

        if (Mouse.current == null)
            return false;

        Vector2 mouseWorldPos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        return boxCollider.OverlapPoint(mouseWorldPos);
    }
    #endregion

    #region Selección
    private void HacerSeleccion()
    {
        if (SeleccionCartas.Instance == null)
        {
            Debug.LogWarning("SeleccionCartas.Instance es null");
            return;
        }

        seleccionada = true;
        SeleccionCartas.Instance.SeleccionarCarta(this);
    }

    public void Deseleccionar()
    {
        seleccionada = false;
    }
    #endregion

    #region Movimiento visual
    private void ActualizarPosicion()
    {
        Vector3 objetivo = posicionOriginal;

        if (seleccionada)
            objetivo += Vector3.up * alturaSeleccion;
        else if (estaEnHover)
            objetivo += Vector3.up * alturaHover;

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            objetivo,
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
        if (moverCoroutine != null)
            StopCoroutine(moverCoroutine);

        moverCoroutine = StartCoroutine(MoverACelda(celda));
    }

    private IEnumerator MoverACelda(Cell celda)
    {
        enAnimacion = true;
        seleccionada = false;
        enMano = false;
        boxCollider.enabled = false;

        transform.SetParent(null, true);

        Vector3 inicio = transform.position;
        Vector3 destino = celda.transform.position;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duracionMovimientoACelda;
            transform.position = Vector3.Lerp(inicio, destino, t);
            yield return null;
        }

        transform.position = destino;
        transform.SetParent(celda.transform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        posicionOriginal = Vector3.zero;

        celda.SetOccupied(this);

        FinalizarColocacion(celda);
        enAnimacion = false;
    }

    private void FinalizarColocacion(Cell celda)
    {
        if (this is CartaComodin comodin && tablero != null)
        {
            comodin.ConfigurarValorInicial(celda, tablero);
        }

        if (gc != null && gc.manoActual.Contains(this))
        {
            gc.manoActual.Remove(this);
            gc.ReordenarMano();
        }

        AplicarReglaEliminacion(celda);

        ScoreManager sm = FindFirstObjectByType<ScoreManager>();
        sm?.ActualizarPuntajes();

        boxCollider.enabled = true;
    }
    #endregion

    #region Regla de eliminación
    private void AplicarReglaEliminacion(Cell celda)
    {
        if (!LevelManager.reglasEliminacionActivas || tablero == null)
            return;

        int col = celda.column;
        int fila = celda.row;

        bool soyJugador = tablero.EsFilaJugador(fila);
        int valorColocado = valor;

        int filaInicioRival = soyJugador ? tablero.filasJugador : 0;
        int filaFinRival = soyJugador ? tablero.rows - 1 : tablero.filasJugador - 1;

        for (int f = filaInicioRival; f <= filaFinRival; f++)
        {
            Cell rivalCelda = tablero.ObtenerCelda(col, f)?.GetComponent<Cell>();
            if (rivalCelda == null || !rivalCelda.isOccupied)
                continue;

            Carta otra = rivalCelda.GetComponentInChildren<Carta>();
            if (otra == null)
                continue;

            if (otra.valor == valorColocado)
            {
                if (soyJugador && gc != null && gc.ia != null)
                    gc.ia.OnCartaEliminadaPorJugador();

                rivalCelda.SetOccupied(null);
                Destroy(otra.gameObject);
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

    #region Robo desde mazo
    public void AnimarRoboDesdeMazo(
        Transform mazo,
        Transform mano,
        Vector3 posicionFinalLocal,
        System.Action onFinish = null
    )
    {
        if (moverCoroutine != null)
            StopCoroutine(moverCoroutine);

        moverCoroutine = StartCoroutine(RoboDesdeMazoCoroutine(
            mazo, mano, posicionFinalLocal, onFinish
        ));
    }

    private IEnumerator RoboDesdeMazoCoroutine(
        Transform mazo,
        Transform mano,
        Vector3 posicionFinalLocal,
        System.Action onFinish
    )
    {
        enAnimacion = true;
        boxCollider.enabled = false;

        transform.SetParent(null, true);
        transform.position = mazo.position;
        MostrarDorso();

        Vector3 inicio = transform.position;
        Vector3 destino = mano.TransformPoint(posicionFinalLocal);

        float t = 0f;
        float duracion = 0.35f;

        while (t < 1f)
        {
            t += Time.deltaTime / duracion;
            transform.position = Vector3.Lerp(inicio, destino, t);
            yield return null;
        }

        transform.SetParent(mano);
        transform.localPosition = posicionFinalLocal;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        enAnimacion = false;
        boxCollider.enabled = true;

        onFinish?.Invoke();
    }
    #endregion
}
