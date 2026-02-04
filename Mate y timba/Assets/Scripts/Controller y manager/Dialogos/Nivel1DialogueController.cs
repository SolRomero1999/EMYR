using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class Nivel1DialogueController : MonoBehaviour, IResultadoDialogo
{
    #region UI
    public TMP_Text dialogueText;
    public Button continuarButton;
    #endregion

    #region Colores de diálogo
    public Color colorContra;
    public Color colorProta;
    #endregion

    #region Diálogos
    [TextArea] public string[] introLines;
    [TextArea] public string[] introTrasPerdidaLines;
    [TextArea] public string[] iaAltasLines;
    [TextArea] public string[] iaAltasTrasPerdidaLines;
    [TextArea] public string[] victoriaLines;
    [TextArea] public string[] derrotaLines;
    #endregion

    #region Config
    public float charsPerSecond = 40f;
    #endregion

    #region Estado
    const string KEY_JUGADOR_PERDIO = "JugadorPerdioNivel1";
    int index = 0;
    bool isTyping;
    bool enDialogoAltas;
    bool enDialogoFinal;
    bool esVictoria;
    Coroutine typingCoroutine;
    System.Action callbackFinal;
    #endregion

    #region Unity
    void Update()
    {
    #if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKeyDown(KeyCode.S))
        {
            SkipToLastDialogue();
        }
    #endif
    }
    void Start()
    {
        continuarButton.onClick.AddListener(NextLine);
        IA_PrimerN.OnIACambiaAModoAltas += IniciarDialogoAltas;

        FindFirstObjectByType<TurnManager>()?.BloquearInputJugador();

        dialogueText.text = "";
        BlinkController.Instance.StartBlink(CameraController.Instance.IrADialogo);

        NextLine();
    }

    void OnDestroy()
    {
        continuarButton.onClick.RemoveListener(NextLine);
        IA_PrimerN.OnIACambiaAModoAltas -= IniciarDialogoAltas;
    }
    #endregion

    #region Interface IResultadoDialogo
    public bool TieneDialogoVictoria()
    {
        return victoriaLines != null && victoriaLines.Length > 0;
    }

    public bool TieneDialogoDerrota()
    {
        return derrotaLines != null && derrotaLines.Length > 0;
    }

    public void MostrarDialogoVictoria(System.Action alFinal)
    {
        callbackFinal = alFinal;
        IniciarDialogoFinal(true);
    }

    public void MostrarDialogoDerrota(System.Action alFinal)
    {
        PlayerPrefs.SetInt(KEY_JUGADOR_PERDIO, 1);
        PlayerPrefs.Save();

        callbackFinal = alFinal;
        IniciarDialogoFinal(false);
    }
    #endregion

    #region Flujo principal
    void NextLine()
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            MostrarLineaCompleta(ObtenerLineaActual());
            isTyping = false;
            return;
        }

        if (index >= LineasActuales().Length)
        {
            if (enDialogoFinal)
            {
                FinalizarDialogoFinal();
                return;
            }

            StartCoroutine(PasarAGameplay());
            return;
        }

        typingCoroutine = StartCoroutine(TypeLine(LineasActuales()[index]));
        index++;
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        string content = ParseSpeakerAndColor(line);

        foreach (char c in content)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(1f / charsPerSecond);
        }

        isTyping = false;
    }
    #endregion

    #region Transiciones
    IEnumerator PasarAGameplay()
    {
        dialogueText.text = "";

        yield return Blink(CameraController.Instance.IrAGameplay);

        enDialogoAltas = false;
        index = 0;

        FindFirstObjectByType<TurnManager>()?.HabilitarInputJugador();
    }

    void IniciarDialogoFinal(bool victoria)
    {
        esVictoria = victoria;
        enDialogoFinal = true;
        index = 0;
        dialogueText.text = "";

        FindFirstObjectByType<TurnManager>()?.BloquearInputJugador();

        BlinkController.Instance.StartBlink(() =>
        {
            CameraController.Instance.IrADialogo();
            NextLine(); 
        });
    }

    void FinalizarDialogoFinal()
    {
        enDialogoFinal = false;

        dialogueText.text = "";
        continuarButton.interactable = false;

        callbackFinal?.Invoke();
        callbackFinal = null;
    }

    IEnumerator Blink(System.Action accion)
    {
        bool done = false;
        BlinkController.Instance.StartBlink(() =>
        {
            accion();
            done = true;
        });

        yield return new WaitUntil(() => done);
    }
    #endregion

    #region Diálogo IA ALTAS
    void IniciarDialogoAltas()
    {
        if (enDialogoAltas || enDialogoFinal) return;

        enDialogoAltas = true;
        index = 0;
        dialogueText.text = "";

        FindFirstObjectByType<TurnManager>()?.BloquearInputJugador();
        BlinkController.Instance.StartBlink(CameraController.Instance.IrADialogo);

        NextLine();
    }
    #endregion

    #region Helpers
    string[] LineasActuales()
    {
        bool perdioAntes = JugadorPerdioAntes();

        if (enDialogoFinal)
            return esVictoria ? victoriaLines : derrotaLines;

        if (enDialogoAltas)
        {
            if (perdioAntes && iaAltasTrasPerdidaLines != null && iaAltasTrasPerdidaLines.Length > 0)
                return iaAltasTrasPerdidaLines;

            return iaAltasLines;
        }

        if (perdioAntes && introTrasPerdidaLines != null && introTrasPerdidaLines.Length > 0)
            return introTrasPerdidaLines;

        return introLines;
    }

    string ObtenerLineaActual()
    {
        int i = Mathf.Clamp(index - 1, 0, LineasActuales().Length - 1);
        return LineasActuales()[i];
    }

    string ParseSpeakerAndColor(string line)
    {
        string content = line;

        if (line.Contains("|"))
        {
            var split = line.Split('|', 2);
            string speaker = split[0].Trim();
            content = split[1].Trim();

            if (speaker == "PROTA")
                dialogueText.color = colorProta;
            else if (speaker == "CONTRA")
                dialogueText.color = colorContra;
        }

        return content;
    }

    void MostrarLineaCompleta(string line)
    {
        dialogueText.text = ParseSpeakerAndColor(line);
    }

    bool JugadorPerdioAntes()
    {
        return PlayerPrefs.GetInt(KEY_JUGADOR_PERDIO, 0) == 1;
    }

    void SkipToLastDialogue()
    {
        var currentLines = LineasActuales();

        if (currentLines == null || currentLines.Length == 0)
            return;

        if (isTyping && typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        index = currentLines.Length;
        MostrarLineaCompleta(currentLines[currentLines.Length - 1]);
        isTyping = false;
    }
    #endregion
}
