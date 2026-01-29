using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_Carta : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public Image imagen;
    private Carta cartaReal;
    private UI_Items controlador;

    Vector3 escalaBase;
    Vector3 escalaHover;

    void Start()
    {
        escalaBase = transform.localScale;
        escalaHover = escalaBase * 1.15f;
    }

    public void Configurar(Carta carta, UI_Items ctrl)
    {
        cartaReal = carta;
        controlador = ctrl;
        imagen.sprite = carta.frente;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = escalaHover;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = escalaBase;
    }

    public void Seleccionar()
    {
        controlador.ElegirCarta(cartaReal);
    }

    public void OnClick()
    {
        controlador.SeleccionarCartaParaDescartar(cartaReal);
    }
}

