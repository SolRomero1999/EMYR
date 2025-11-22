using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour
{
    public Mazo mazo;
    public Transform manoJugador;
    public GameObject cartaPrefab;
    public Sprite dorsoCarta;
    public Sprite[] frentes;

    void Start()
    {
        CrearMazo();
        mazo.Barajar();
        StartCoroutine(RepartirCartasConDelay(5));
    }

    void CrearMazo()
    {
        for(int i = 0; i < frentes.Length; i++)
        {
            GameObject nueva = Instantiate(cartaPrefab);
            Carta c = nueva.GetComponent<Carta>();

            c.frente = frentes[i];
            c.dorso = dorsoCarta;
            c.valor = (i % 13) + 1;
            c.palo = "SinUsarPorAhora";

            c.MostrarDorso();
            nueva.transform.SetParent(mazo.transform);
            nueva.transform.localPosition = Vector3.zero;
            nueva.name = "Carta_" + i; 

            mazo.cartas.Add(c);
        }
    }

    IEnumerator RepartirCartasConDelay(int cantidad)
    {
        yield return new WaitForSeconds(0.5f); 

        float spacing = 1.5f;
        float totalWidth = (cantidad - 1) * spacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < cantidad; i++)
        {
            Carta robada = mazo.RobarCarta();

            if (robada != null)
            {
                robada.transform.SetParent(manoJugador);
                robada.enMano = true; 
                
                float x = startX + (i * spacing);
                Vector3 nuevaPosicion = new Vector3(x, 0, 0);
                
                robada.SetPosicionOriginal(nuevaPosicion);
                robada.MostrarFrente();

                Debug.Log("Carta repartida: " + robada.name + " en posición: " + nuevaPosicion);
                
                yield return new WaitForSeconds(0.1f); 
            }
        }
    }
}