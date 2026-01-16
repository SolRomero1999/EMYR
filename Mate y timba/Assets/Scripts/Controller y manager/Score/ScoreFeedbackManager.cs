using UnityEngine;
using System.Collections.Generic;

public class ScoreFeedbackVisualizer : MonoBehaviour
{
    [Header("Prefab")]
    public LineRenderer marcoPrefab;

    [Header("Visual")]
    public Color colorTrio = Color.green;
    public Color colorCuarteto = new Color(0.2f, 0.8f, 1f);
    public float padding = 0.3f;

    private readonly List<LineRenderer> activos = new();

    #region Public API
    public void Limpiar()
    {
        foreach (var lr in activos)
            if (lr) Destroy(lr.gameObject);

        activos.Clear();
    }

    public void MostrarCombo(ComboType tipo, Transform[] cartas)
    {
        foreach (var t in cartas)
        {
            if (!t) continue;

            ComboCardAnimator anim = t.GetComponent<ComboCardAnimator>();
            if (anim != null)
                anim.Reproducir();
        }
    }
    #endregion

    #region Utils
    private Bounds CalcularBounds(Transform[] cartas)
    {
        SpriteRenderer sr = cartas[0].GetComponent<SpriteRenderer>();
        Bounds bounds = sr.bounds;

        for (int i = 1; i < cartas.Length; i++)
        {
            sr = cartas[i].GetComponent<SpriteRenderer>();
            if (sr != null)
                bounds.Encapsulate(sr.bounds);
        }

        float shrink = 0.3f; 
        bounds.Expand(-shrink);

        return bounds;
    }
    #endregion
}
