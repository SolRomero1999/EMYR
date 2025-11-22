using UnityEngine;
using UnityEngine.InputSystem;

public class Carta : MonoBehaviour
{
    public int valor;
    public string palo;
    public Sprite frente;
    public Sprite dorso;

    [Header("Estado")]
    public bool enMano = false;

    private SpriteRenderer sr;
    private Vector3 posicionOriginal;
    private float alturaHover = 0.5f;
    private float alturaSeleccion = 1.0f;
    private float velocidadMovimiento = 15f;

    private bool estaEnHover = false;
    private bool hoverAnterior = false;
    private bool seleccionada = false;

    private Camera mainCamera;
    private BoxCollider2D boxCollider;
    private Mouse mouse;

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

        Vector3 posicionObjetivo = posicionOriginal;

        if (seleccionada)
            posicionObjetivo += Vector3.up * alturaSeleccion;
        else if (estaEnHover)
            posicionObjetivo += Vector3.up * alturaHover;

        transform.localPosition = Vector3.Lerp(transform.localPosition, posicionObjetivo, velocidadMovimiento * Time.deltaTime);

        if (enMano && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EstaMouseSobreCarta())
            {
                HacerSeleccion();
            }
        }
    }

    private bool EstaMouseSobreCarta()
    {
        if (mainCamera == null || boxCollider == null || mouse == null) return false;

        Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouse.position.ReadValue());
        return boxCollider.OverlapPoint(mouseWorldPos);
    }

    private void OnMouseDown()
    {
        if (!enMano) return;

        HacerSeleccion();
    }

    private void HacerSeleccion()
    {
        if (SeleccionCartas.Instance == null)
        {
            Debug.LogWarning("SeleccionCartas.Instance es null. Asegurate de tener el GameObject 'ControlSeleccion' con el script en la escena.");
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

    public void ColocarEnCelda(Cell celda)
    {
        seleccionada = false;
        enMano = false; 
        celda.SetOccupied(true);

        transform.SetParent(celda.transform);
        posicionOriginal = Vector3.zero;
        transform.localPosition = Vector3.zero;

        Debug.Log(name + " colocado en celda " + celda.column + "," + celda.row);
    }

    public void MostrarFrente()
    {
        if (sr != null) sr.sprite = frente;
    }

    public void MostrarDorso()
    {
        if (sr != null) sr.sprite = dorso;
    }

    public void SetPosicionOriginal(Vector3 nuevaPosicion)
    {
        posicionOriginal = nuevaPosicion;
        transform.localPosition = nuevaPosicion;
    }
}

