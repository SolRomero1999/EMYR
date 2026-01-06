using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GameController : MonoBehaviour
{
    #region Variables públicas
    public Mazo mazo;
    public Tablero tablero;
    public Transform manoJugador;
    public GameObject cartaPrefab;
    public Sprite dorsoCarta;
    public Sprite[] frentes;
    public List<Carta> manoActual = new List<Carta>();
    public int maxCartasMano = 5;
    public bool jugadorYaRobo = false;
    public Transform manoIA;
    public List<Carta> manoIAActual = new List<Carta>();
    public UI_Items UI_Items;
    [Header("IA")]
    public IA_Base ia;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        tablero = FindFirstObjectByType<Tablero>();

        ia = FindFirstObjectByType<IA_Base>();
        if (ia != null)
            ia.Inicializar(this);

        CrearMazo();
        mazo.Barajar();
        StartCoroutine(RepartirCartasConDelay(5));
        StartCoroutine(RepartirCartasIA(5));
    }
    #endregion

    #region Crear y preparar mazo
    private void CrearMazo()
    {
        for (int i = 0; i < frentes.Length; i++)
        {
            GameObject nueva = Instantiate(cartaPrefab);
            Carta c = nueva.GetComponent<Carta>();

            c.frente = frentes[i];
            c.dorso = dorsoCarta;
            c.valor = (i % 12) + 1;
            c.palo = "SinUsarPorAhora";
            c.MostrarDorso();

            nueva.transform.SetParent(mazo.transform);
            nueva.transform.localPosition = new Vector3(-50f, i * 0.01f, 0);
            nueva.name = "Carta_" + i;
            mazo.cartas.Add(c);
        }
    }
    #endregion

    #region Reparto inicial
    private IEnumerator RepartirCartasConDelay(int cantidad)
    {
        yield return new WaitForSeconds(0.5f);

        float spacing = 1.2f;
        float totalWidth = (cantidad - 1) * spacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < cantidad; i++)
        {
            Carta robada = mazo.RobarCarta();

            if (robada != null)
            {
                robada.transform.SetParent(manoJugador);
                robada.enMano = true;
                manoActual.Add(robada);
                float x = startX + i * spacing;
                Vector3 nuevaPosicion = new Vector3(x, 0, 0);
                robada.SetPosicionOriginal(nuevaPosicion);
                robada.MostrarFrente();
            }
        }
    }

    private IEnumerator RepartirCartasIA(int cantidad)
    {
        yield return new WaitForSeconds(0.5f);

        float spacing = 1.2f;
        float totalWidth = (cantidad - 1) * spacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < cantidad; i++)
        {
            Carta robada = mazo.RobarCarta();

            if (robada != null)
            {
                robada.transform.SetParent(manoIA); 
                robada.enMano = false;
                robada.MostrarDorso();

                manoIAActual.Add(robada);

                float x = startX + i * spacing;
                Vector3 nuevaPosicion = new Vector3(x, 0, 0);
                robada.SetPosicionOriginal(nuevaPosicion);
            }
        }

        Debug.Log("IA recibió 5 cartas");
    }
    #endregion

    #region Robar cartas durante la partida
    public void IntentarRobarCarta()
    {
        if (jugadorYaRobo)
        {
            Debug.Log("Ya robaste una carta este turno.");
            return;
        }

        if (manoActual.Count >= maxCartasMano)
        {
            Debug.Log("No puedes tener más de 5 cartas");
            return;
        }

        Carta robada = mazo.RobarCarta();

        if (robada == null)
        {
            Debug.Log("El mazo está vacío");
            return;
        }

        jugadorYaRobo = true; 

        robada.transform.SetParent(manoJugador);
        robada.enMano = true;
        manoActual.Add(robada);
        ReordenarMano();
        robada.MostrarFrente();
        Debug.Log("Robaste: " + robada.name);

        TurnManager tm = FindFirstObjectByType<TurnManager>();
        tm.TerminarTurnoJugador();
    }

    public void RobarCartaIA()
    {
        if (manoIAActual.Count >= maxCartasMano) return;

        Carta robada = mazo.RobarCarta();
        if (robada == null) return;

        robada.transform.SetParent(manoIA);
        robada.enMano = false;
        robada.MostrarDorso();

        manoIAActual.Add(robada);

        float spacing = 1.5f;
        float totalWidth = (manoIAActual.Count - 1) * spacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < manoIAActual.Count; i++)
        {
            float x = startX + i * spacing;
            manoIAActual[i].SetPosicionOriginal(new Vector3(x, 0, 0));
        }
    }

    public void IA_JugarCarta()
    {
        if (manoIAActual.Count == 0) return;

        Carta carta = manoIAActual[Random.Range(0, manoIAActual.Count)];
        Cell celda = tablero.ObtenerCeldaLibreIA();

        if (celda == null)
        {
            Debug.Log("La IA no tiene celdas disponibles.");
            return;
        }

        carta.ColocarEnCelda(celda);
        carta.MostrarFrente();
        manoIAActual.Remove(carta);

        ScoreManager sm = FindFirstObjectByType<ScoreManager>();
        if (sm != null) sm.ActualizarPuntajes();

        Debug.Log($"IA jugó carta {carta.valor} en [{celda.column},{celda.row}]");
        FindFirstObjectByType<TurnManager>().VerificarFinDePartida();
    }
    #endregion

    #region Organización de la mano
    public void ReordenarMano()
    {
        float spacing = 1.2f;
        float totalWidth = (manoActual.Count - 1) * spacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < manoActual.Count; i++)
        {
            Carta c = manoActual[i];
            float x = startX + i * spacing;
            c.SetPosicionOriginal(new Vector3(x, 0, 0));
        }
    }
    #endregion
}