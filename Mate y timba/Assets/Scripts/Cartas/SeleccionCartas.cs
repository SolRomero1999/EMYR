using UnityEngine;
using UnityEngine.InputSystem;

public class SeleccionCartas : MonoBehaviour
{
    public static SeleccionCartas Instance;

    private Carta cartaSeleccionada;
    private Camera cam;

    private void Awake()
    {
        Instance = this;
        cam = Camera.main;
    }

    private void OnDisable()
    {
        if (Instance == this)
            Instance = null;

        cartaSeleccionada = null;
    }

    public void SeleccionarCarta(Carta c)
    {
        if (cartaSeleccionada != null && cartaSeleccionada != c)
            cartaSeleccionada.Deseleccionar();

        cartaSeleccionada = c;
    }

    private void Update()
    {
        if (cartaSeleccionada == null)
            return;

        if (Mouse.current == null)
            return;

        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null) return;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(mousePos);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider == null)
                return;

            if (!hit.collider.CompareTag("Celda"))
                return;

            Cell celda = hit.collider.GetComponent<Cell>();
            if (celda == null || celda.isOccupied)
                return;

            if (celda.row < 0 || celda.row > 3)
            {
                Debug.Log("No puedes colocar cartas en las filas del rival.");
                return;
            }

            cartaSeleccionada.ColocarEnCelda(celda);

            cartaSeleccionada = null;
        }
    }
}
