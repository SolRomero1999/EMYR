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

        bool esTutorial = ia is IA_Tuto;

        float spacing = 1.2f;
        float totalWidth = (cantidad - 1) * spacing;
        float startX = -totalWidth / 2f;

        int cartasDadas = 0;
        int intentos = 0;
        int maxIntentos = mazo.cartas.Count * 2;

        while (cartasDadas < cantidad && intentos < maxIntentos)
        {
            intentos++;

            Carta robada = mazo.RobarCarta();
            if (robada == null)
                break;

            if (esTutorial && robada.valor == 2)
            {
                mazo.cartas.Add(robada); 
                continue;
            }

            robada.transform.SetParent(manoJugador);
            robada.enMano = true;
            manoActual.Add(robada);

            float x = startX + cartasDadas * spacing;
            robada.SetPosicionOriginal(new Vector3(x, 0, 0));
            robada.MostrarFrente();

            cartasDadas++;
        }

        if (intentos >= maxIntentos)
            Debug.LogWarning("Reparto jugador tutorial abortado por seguridad (demasiados intentos)");
    }

    private IEnumerator RepartirCartasIA(int cantidad)
    {
        yield return new WaitForSeconds(0.5f);

        bool esTutorial = ia is IA_Tuto;
        List<Carta> cartasIA = new List<Carta>();

        if (esTutorial)
        {
            int valorDemo = 2;
            int maxPermitidas = 3;

            for (int i = mazo.cartas.Count - 1; i >= 0 && cartasIA.Count < maxPermitidas; i--)
            {
                if (mazo.cartas[i].valor == valorDemo)
                {
                    cartasIA.Add(mazo.cartas[i]);
                    mazo.cartas.RemoveAt(i);
                }
            }

            if (cartasIA.Count < maxPermitidas)
            {
                Debug.LogError("No hay suficientes cartas valor 2 en el mazo para el tutorial");
            }

            while (cartasIA.Count < cantidad)
            {
                Carta extra = mazo.RobarCarta();
                if (extra == null) break;

                if (extra.valor == valorDemo)
                {
                    mazo.cartas.Add(extra);
                    continue;
                }

                cartasIA.Add(extra);
            }
        }
        else
        {
            for (int i = 0; i < cantidad; i++)
            {
                Carta robada = mazo.RobarCarta();
                if (robada != null)
                    cartasIA.Add(robada);
            }
        }

        float spacing = 1.2f;
        float totalWidth = (cartasIA.Count - 1) * spacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < cartasIA.Count; i++)
        {
            Carta robada = cartasIA[i];
            if (robada == null) continue;

            robada.transform.SetParent(manoIA);
            robada.enMano = false;
            robada.MostrarDorso();
            manoIAActual.Add(robada);

            float x = startX + i * spacing;
            robada.SetPosicionOriginal(new Vector3(x, 0, 0));
        }

        Debug.Log("IA recibió cartas (tutorial: " + esTutorial + ")");
    }
    #endregion

    #region Robar cartas durante la partida
    public void IntentarRobarCarta()
    {
        TurnManager tm = FindFirstObjectByType<TurnManager>();
        if (tm == null || !tm.PuedeInteractuarJugador())
            return;

        if (jugadorYaRobo) return;
        if (manoActual.Count >= maxCartasMano) return;

        Carta robada = mazo.RobarCarta();
        if (robada == null) return;

        jugadorYaRobo = true;

        manoActual.Add(robada);
        robada.enMano = true;

        float spacing = 1.2f;
        float totalWidth = (manoActual.Count - 1) * spacing;
        float startX = -totalWidth / 2f;
        Vector3 posicionFinal = new Vector3(
            startX + (manoActual.Count - 1) * spacing,
            0,
            0
        );

        robada.AnimarRoboDesdeMazo(
            mazo.transform,
            manoJugador,
            posicionFinal,
            () =>
            {
                ReordenarMano();
                robada.MostrarFrente();

                tm.TerminarTurnoJugador();
            }
        );
    }

    public void RobarCartaIA()
    {
        if (manoIAActual.Count >= maxCartasMano) return;

        Carta robada = mazo.RobarCarta();
        if (robada == null) return;

        manoIAActual.Add(robada);
        robada.enMano = false;

        float spacing = 1.5f;
        float totalWidth = (manoIAActual.Count - 1) * spacing;
        float startX = -totalWidth / 2f;
        Vector3 posicionFinal = new Vector3(
            startX + (manoIAActual.Count - 1) * spacing,
            0,
            0
        );

        robada.AnimarRoboDesdeMazo(
            mazo.transform,
            manoIA,
            posicionFinal,
            () =>
            {
                ReordenarManoIA();
                robada.MostrarDorso(); 
            }
        );
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

    private void ReordenarManoIA()
    {
        float spacing = 1.5f;
        float totalWidth = (manoIAActual.Count - 1) * spacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < manoIAActual.Count; i++)
        {
            float x = startX + i * spacing;
            manoIAActual[i].SetPosicionOriginal(new Vector3(x, 0, 0));
        }
    }
    #endregion
}