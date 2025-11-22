using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    [Header("Prefabs / Sprites")]
    public GameObject cellPrefab;

    [Header("Grid")]
    public int columns = 4;
    public int rows = 8; 
    public float spacingX = 1.2f;
    public float spacingY = 1.4f;

    [Header("Parent / Offset")]
    public Transform boardParent;

    [Header("Inspector Helpers")]
    public bool generateOnStart = true;

    private Tablero tablero;

    private void Start()
    {
        tablero = GetComponent<Tablero>();
        if (generateOnStart) GenerateGrid();
    }

    public void GenerateGrid()
    {
        if (cellPrefab == null) { Debug.LogError("Cell prefab no asignado"); return; }
        if (boardParent == null) boardParent = this.transform;
        for (int i = boardParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(boardParent.GetChild(i).gameObject);
        }

        float totalWidth = (columns - 1) * spacingX;
        float totalHeight = (rows - 1) * spacingY;
        Vector3 origin = new Vector3(-totalWidth / 2f, -totalHeight / 2f, 0f);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                Vector3 pos = origin + new Vector3(c * spacingX, r * spacingY, 0f);
                GameObject go = Instantiate(cellPrefab, boardParent);
                go.transform.localPosition = pos;
                go.name = $"Cell_{c}_{r}";

                Cell cellScript = go.GetComponent<Cell>();
                if (cellScript != null)
                {
                    cellScript.column = c;
                    cellScript.row = r;
                }

                if (tablero != null)
                {
                    tablero.RegistrarCelda(c, r, go.transform);
                }
            }
        }
    }
}
