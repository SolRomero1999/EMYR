using UnityEngine;
using System.Collections.Generic;

public class Mazo : MonoBehaviour
{
    public List<Carta> cartas = new List<Carta>();

    [Header("Visual del mazo")]
    public Sprite spriteMazoCompleto;
    public Sprite spriteMazoPocas;
    public SpriteRenderer spriteRenderer;
    public int limitePocasCartas = 10;

    void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        ActualizarVisualMazo();
    }

    public void Barajar()
    {
        for (int i = 0; i < cartas.Count; i++)
        {
            Carta temp = cartas[i];
            int randomIndex = Random.Range(i, cartas.Count);
            cartas[i] = cartas[randomIndex];
            cartas[randomIndex] = temp;
        }

        ActualizarVisualMazo();
    }

    public Carta RobarCarta()
    {
        if (cartas.Count == 0) return null;

        Carta c = cartas[0];
        cartas.RemoveAt(0);

        ActualizarVisualMazo();
        return c;
    }

    void ActualizarVisualMazo()
    {
        if (cartas.Count == 0)
        {
            spriteRenderer.sprite = null;
            spriteRenderer.enabled = false;
        }
        else if (cartas.Count <= limitePocasCartas)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.sprite = spriteMazoPocas;
        }
        else
        {
            spriteRenderer.enabled = true;
            spriteRenderer.sprite = spriteMazoCompleto;
        }
    }
}
