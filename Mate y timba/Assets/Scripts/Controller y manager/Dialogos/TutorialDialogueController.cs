using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class TutorialDialogueController : MonoBehaviour, IResultadoDialogo
{
    #region UI
    public TMP_Text dialogueText;
    public Button continuarButton;
    public EndGameScoreAnimator endGameScoreAnimator;
    #endregion

    #region Colores de diálogo
    public Color colorContra;
    public Color colorProta;
    #endregion

    #region Diálogos
    [TextArea] public string[] introLines;
    [TextArea] public string[] explicacionLines;
    [TextArea] public string[] trioLines;
    [TextArea] public string[] conteoJugadorLines;
    [TextArea] public string[] conteoIALines;
    [TextArea] public string[] victoriaLines;
    [TextArea] public string[] derrotaLines;
    #endregion

    #region Config
    public float charsPerSecond = 40f;
    public float pausaAntesConteo = 1f;
    #endregion

    #region Estado
    int index = 0;
    bool isTyping;

    bool enExplicacion;
    bool enDialogoTrio;
    bool enDialogoFinal;
    bool esVictoria;

    bool enConteoJugador;
    bool enConteoIA;

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
        IA_Tuto.OnTrioTutorialCompletado += IniciarDialogoTrio;

        FindFirstObjectByType<TurnManager>()?.BloquearInputJugador();

        if (LevelManager.tutorialDialogoVisto)
        {
            gameObject.SetActive(false);
            return;
        }

        dialogueText.text = "";
        BlinkController.Instance.StartBlink(CameraController.Instance.IrADialogo);
        NextLine();
    }

    void OnDestroy()
    {
        continuarButton.onClick.RemoveListener(NextLine);
        IA_Tuto.OnTrioTutorialCompletado -= IniciarDialogoTrio;
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

        if (enConteoJugador && index >= conteoJugadorLines.Length)
        {
            enConteoJugador = false;
            StartCoroutine(SecuenciaConteoJugador());
            return;
        }

        if (enConteoIA && index >= conteoIALines.Length)
        {
            enConteoIA = false;
            StartCoroutine(SecuenciaConteoIA());
            return;
        }

        if (index >= LineasActuales().Length)
        {
            if (enDialogoFinal)
            {
                FinalizarDialogoFinal();
                return;
            }

        if (enDialogoTrio)
        {
            enDialogoTrio = false;
            enExplicacion = false;

            FindFirstObjectByType<TurnManager>()?.HabilitarInputJugador();
            BlinkController.Instance.StartBlink(CameraController.Instance.IrAGameplay);
            dialogueText.text = "";

            return;
        }

            if (!enExplicacion)
            {
                StartCoroutine(MirarTablero());
                return;
            }

            FinalizarTutorial();
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

    #region Resultado final
    public void MostrarDialogoVictoria(System.Action alFinal)
    {
        callbackFinal = alFinal;
        IniciarConteo(true);
    }

    public void MostrarDialogoDerrota(System.Action alFinal)
    {
        callbackFinal = alFinal;
        IniciarConteo(false);
    }

    void IniciarConteo(bool victoria)
    {
        esVictoria = victoria;
        index = 0;
        dialogueText.text = "";

        enConteoJugador = true;

        BlinkController.Instance.StartBlink(CameraController.Instance.IrADialogo);
        NextLine();
    }

    IEnumerator SecuenciaConteoJugador()
    {
        yield return Blink(CameraController.Instance.IrAGameplay);
        yield return new WaitForSeconds(pausaAntesConteo);

        endGameScoreAnimator.scoreManager.ActualizarPuntajes();
        endGameScoreAnimator.scoreManager.CalcularTotales();

        endGameScoreAnimator.panelFinal.SetActive(true);

        endGameScoreAnimator.contadorJugador.text = "0";
        endGameScoreAnimator.contadorIA.text = "0";

        yield return endGameScoreAnimator.AnimarContadorTutorial(
            endGameScoreAnimator.contadorJugador,
            endGameScoreAnimator.scoreManager.TotalJugador
        );

        yield return Blink(CameraController.Instance.IrADialogo);

        enConteoIA = true;
        index = 0;
        NextLine();
    }

    IEnumerator SecuenciaConteoIA()
    {
        yield return Blink(CameraController.Instance.IrAGameplay);
        yield return new WaitForSeconds(pausaAntesConteo);

        endGameScoreAnimator.contadorJugador.text =
            endGameScoreAnimator.scoreManager.TotalJugador.ToString();

        endGameScoreAnimator.contadorIA.text = "0";

        yield return endGameScoreAnimator.AnimarContadorTutorial(
            endGameScoreAnimator.contadorIA,
            endGameScoreAnimator.scoreManager.TotalIA
        );

        yield return Blink(CameraController.Instance.IrADialogo);

        IniciarDialogoFinal(esVictoria);
    }

    void IniciarDialogoFinal(bool victoria)
    {
        enDialogoFinal = true;
        index = 0;
        dialogueText.text = "";
        NextLine();
    }

    void FinalizarDialogoFinal()
    {
        enDialogoFinal = false;
        gameObject.SetActive(false);
        callbackFinal?.Invoke();
        callbackFinal = null;
    }
    #endregion

    #region Otros
    IEnumerator MirarTablero()
    {
        dialogueText.text = "";
        yield return Blink(CameraController.Instance.IrAGameplay);
        yield return new WaitForSeconds(2f);
        yield return Blink(CameraController.Instance.IrADialogo);

        enExplicacion = true;
        index = 0;
        NextLine();
    }

    void IniciarDialogoTrio()
    {
        enDialogoTrio = true;
        index = 0;
        dialogueText.text = "";
        BlinkController.Instance.StartBlink(CameraController.Instance.IrADialogo);
        NextLine();
    }

    void FinalizarTutorial()
    {
        LevelManager.tutorialDialogoVisto = true;
        FindFirstObjectByType<TurnManager>()?.HabilitarInputJugador();
        BlinkController.Instance.StartBlink(CameraController.Instance.IrAGameplay);
        dialogueText.text = "";
    }

    IEnumerator Blink(System.Action accion)
    {
        bool done = false;
        BlinkController.Instance.StartBlink(() => { accion(); done = true; });
        yield return new WaitUntil(() => done);
    }

    string[] LineasActuales()
    {
        if (enConteoJugador) return conteoJugadorLines;
        if (enConteoIA) return conteoIALines;
        if (enDialogoFinal) return esVictoria ? victoriaLines : derrotaLines;
        if (enDialogoTrio) return trioLines;
        return enExplicacion ? explicacionLines : introLines;
    }

    string ObtenerLineaActual()
    {
        int i = Mathf.Clamp(index - 1, 0, LineasActuales().Length - 1);
        return LineasActuales()[i];
    }
    #endregion

    #region Speaker & Color helpers
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
