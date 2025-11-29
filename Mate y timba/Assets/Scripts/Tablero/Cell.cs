using UnityEngine;

public class Cell : MonoBehaviour
{
    public int column;
    public int row;

    public SpriteRenderer sr;
    public Sprite baseSprite;
    public Sprite highlightSprite;

    public bool useTintOnHover = true;
    public Color hoverTint = new Color(0.8f, 1f, 0.8f, 1f);

    [HideInInspector] public bool isOccupied = false;
    [HideInInspector] public Carta carta = null; 

    [HideInInspector] public Color originalColor;
    Sprite originalSprite;

    private void Awake()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
        originalSprite = sr.sprite;
        if (baseSprite != null) sr.sprite = baseSprite;
    }

    public void SetOccupied(Carta c)
    {
        isOccupied = c != null;
        carta = c;
        if (c != null)
        c.celdaActual = this;
    }

    private void OnMouseDown()
    {
        UI_Items ui = FindFirstObjectByType<UI_Items>();
        if (ui != null)
            ui.SeleccionarCeldaRival(this);
    }
}
