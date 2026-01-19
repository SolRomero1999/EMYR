using UnityEngine;
using System.Collections.Generic;

public class ScoreFeedbackVisualizer : MonoBehaviour
{
    public void Limpiar()
    {
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
}
