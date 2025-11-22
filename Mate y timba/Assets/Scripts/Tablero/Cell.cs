using UnityEngine;

public class Cell : MonoBehaviour
{
    public int column;
    public int row;

    [Header("Visual")]
    public SpriteRenderer sr;
    public Sprite baseSprite;
    public Sprite highlightSprite;
    public bool useTintOnHover = true;
    public Color hoverTint = new Color(0.8f, 1f, 0.8f, 1f);

    [HideInInspector] public bool isOccupied = false;

    Color originalColor;
    Sprite originalSprite;

    private void Awake()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
        originalSprite = sr.sprite;
        if (baseSprite != null) sr.sprite = baseSprite;
    }

    private void OnMouseEnter()
    {
        if (useTintOnHover)
            sr.color = hoverTint;
        else if (highlightSprite != null)
            sr.sprite = highlightSprite;
    }

    private void OnMouseExit()
    {
        if (useTintOnHover)
            sr.color = originalColor;
        else if (highlightSprite != null)
            sr.sprite = baseSprite != null ? baseSprite : originalSprite;
    }

    public void SetOccupied(bool o)
    {
        isOccupied = o;
    }
}
