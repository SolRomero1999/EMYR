using UnityEngine;

public class ItemPucho : MonoBehaviour
{
    private GameController game;

    private void Start()
    {
        game = FindFirstObjectByType<GameController>();
    }

    private void OnMouseDown()
    {
        Usar();
    }

    public void Usar()
    {
        Debug.Log("PUCHO activado → puedes mover una carta del rival.");

        game.UI_Items.ActivarPucho();
        Destroy(gameObject);
    }
}
