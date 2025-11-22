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
    #endregion

    #region Variables Privadas
    private SpriteRenderer sr;
    private Vector3 posicionOriginal;

    // Movimiento / Hover
    private float alturaHover = 0.5f;
    private float alturaSeleccion = 1.0f;
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
        celda.SetOccupied(true);

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
    }
    #endregion

    #region Regla de eliminaciòn
    private void AplicarReglaEliminacion(Cell celda)
    {
        Tablero tablero = FindFirstObjectByType<Tablero>();
        if (tablero == null) return;

        int col = celda.column;
        int fila = celda.row;

        bool soyJugador = fila >= 0 && fila <= 3;  
        bool soyIA = fila >= 4 && fila <= 7;      

        int valorColocado = valor;
        int filaInicioRival = soyJugador ? 4 : 0;
        int filaFinRival = soyJugador ? 7 : 3;

        for (int f = filaInicioRival; f <= filaFinRival; f++)
        {
            Transform t = tablero.ObtenerCelda(col, f);
            if (t == null) continue;

            if (!t.TryGetComponent<Cell>(out Cell rivalCelda)) continue;
            if (rivalCelda.isOccupied == false) continue;

            Carta otraCarta = t.GetComponentInChildren<Carta>();
            if (otraCarta == null) continue;

            if (otraCarta.valor == valorColocado)
            {
                Debug.Log($"ELIMINACIÓN: Carta {otraCarta.valor} en columna {col}");

                rivalCelda.isOccupied = false;
                Destroy(otraCarta.gameObject);
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