using UnityEngine;

public enum ComboType
{
    Trio,
    Cuarteto
}

[System.Serializable]
public struct ComboDetectado
{
    public ComboType tipo;
    public Transform[] celdas;
}
