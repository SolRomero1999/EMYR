using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class Nivel3DialogueController : MonoBehaviour, IResultadoDialogo
{
    #region UI
    public TMP_Text dialogueText;
    public Button continuarButton;
    public IA_TercerN iaTercerNivel;
    #endregion

    #region Colores
    public Color colorContra;
    public Color colorProta;
    #endregion

    #region Diálogos
    [TextArea] public string[] introLines;
    [TextArea] public string[] primeraEliminacionLines;
    [TextArea] public string[] primerComboLines;
    [TextArea] public string[] victoriaLines;
    [TextArea] public string[] derrotaLines;
    #endregion

    #region Config
    public float charsPerSecond = 40f;
    #endregion

    #region Estado
    int index = 0;
    bool isTyping;

    bool enPrimeraEliminacion;
    bool enPrimerCombo;
    bool enDialogoFinal;
    bool esVictoria;

    Coroutine typingCoroutine;
    System.Action callbackFinal;
    #endregion

    #region Unity
    void Start()
    {
        continuarButton.onClick.AddListener(NextLine);

        if (iaTercerNivel != null)
        {
            iaTercerNivel.OnPrimeraEliminacionJugador += IniciarDialogoPrimeraEliminacion;
            iaTercerNivel.OnPrimerComboJugador += IniciarDialogoCombo;
        }
        else
        {
            Debug.LogError("[Nivel3DialogueController] IA_TercerN no asignada");
        }

        FindFirstObjectByType<TurnManager>()?.BloquearInputJugador();

        dialogueText.text = "";
        BlinkController.Instance.StartBlink(CameraController.Instance.IrADialogo);

        NextLine();
    }

    void OnDestroy()
    {
        continuarButton.onClick.RemoveListener(NextLine);

        if (iaTercerNivel != null)
        {
            iaTercerNivel.OnPrimeraEliminacionJugador -= IniciarDialogoPrimeraEliminacion;
            iaTercerNivel.OnPrimerComboJugador -= IniciarDialogoCombo;
        }
    }
    #endregion

    #region IResultadoDialogo
    public bool TieneDialogoVictoria() => victoriaLines.Length > 0;
    public bool TieneDialogoDerrota() => derrotaLines.Length > 0;

    public void MostrarDialogoVictoria(System.Action alFinal)
    {
        callbackFinal = alFinal;
        IniciarDialogoFinal(true);
    }

    public void MostrarDialogoDerrota(System.Action alFinal)
    {
        callbackFinal = alFinal;
        IniciarDialogoFinal(false);
    }
    #endregion

    #region Flujo
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

            StartCoroutine(VolverAGameplay());
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
    IEnumerator VolverAGameplay()
    {
        dialogueText.text = "";
        yield return Blink(CameraController.Instance.IrAGameplay);

        ResetEstados();
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
        gameObject.SetActive(false);
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

    #region Inicios de diálogo
    void IniciarDialogoPrimeraEliminacion()
    {
        if (enDialogoFinal) return;

        enPrimeraEliminacion = true;
        PrepararDialogo();
    }

    void IniciarDialogoCombo()
    {
        if (enDialogoFinal || enPrimeraEliminacion) return;

        enPrimerCombo = true;
        PrepararDialogo();
    }

    void PrepararDialogo()
    {
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
        if (enDialogoFinal)
            return esVictoria ? victoriaLines : derrotaLines;

        if (enPrimeraEliminacion)
            return primeraEliminacionLines;

        if (enPrimerCombo)
            return primerComboLines;

        return introLines;
    }

    void ResetEstados()
    {
        enPrimeraEliminacion = false;
        enPrimerCombo = false;
        index = 0;
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

            dialogueText.color =
                speaker == "PROTA" ? colorProta : colorContra;
        }

        return content;
    }

    void MostrarLineaCompleta(string line)
    {
        dialogueText.text = ParseSpeakerAndColor(line);
    }
    #endregion
}
