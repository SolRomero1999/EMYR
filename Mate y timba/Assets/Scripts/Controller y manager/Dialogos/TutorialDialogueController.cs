using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class TutorialDialogueController : MonoBehaviour, IResultadoDialogo
{
    #region Referencias UI
    public TMP_Text dialogueText;
    public Button continuarButton;
    #endregion

    #region Líneas de diálogo
    [Header("Diálogo inicial del abuelo")]
    [TextArea] public string[] introLines;

    [Header("Diálogo explicativo")]
    [TextArea] public string[] explicacionLines;

    [Header("Diálogo después del trío")]
    [TextArea] public string[] trioLines;

    [Header("Diálogo final - Victoria")]
    [TextArea] public string[] victoriaLines;

    [Header("Diálogo final - Derrota")]
    [TextArea] public string[] derrotaLines;
    #endregion

    #region Configuración
    public float charsPerSecond = 40f;
    #endregion

    #region Estado interno
    private int index = 0;
    private bool isTyping = false;

    private bool enExplicacion = false;
    private bool esperandoVistaTablero = false;
    private bool enDialogoTrio = false;
    private bool enDialogoFinal = false;
    private bool esVictoria = false;

    private Coroutine typingCoroutine;
    private System.Action callbackFinDialogoFinal;
    #endregion

    #region Unity Events
    private void Start()
    {
        continuarButton.onClick.AddListener(NextLine);
        IA_Tuto.OnTrioTutorialCompletado += IniciarDialogoTrio;

        if (LevelManager.tutorialDialogoVisto)
        {
            gameObject.SetActive(false);
            return;
        }

        dialogueText.text = "";
        index = 0;
        gameObject.SetActive(true);

        BlinkController.Instance.StartBlink(() =>
        {
            CameraController.Instance.IrADialogo();
        });

        NextLine();
    }

    private void OnDestroy()
    {
        continuarButton.onClick.RemoveListener(NextLine);
        IA_Tuto.OnTrioTutorialCompletado -= IniciarDialogoTrio;
    }
    #endregion

    #region Flujo principal de diálogo
    private void NextLine()
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = ObtenerLineaActual();
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

            if (enDialogoTrio)
            {
                StartCoroutine(FinalizarDialogoTrio());
                return;
            }

            if (!enExplicacion && !esperandoVistaTablero)
            {
                StartCoroutine(MirarTableroYVolver());
                return;
            }

            if (enExplicacion)
            {
                FinalizarTutorialDialogo();
                return;
            }
        }

        typingCoroutine = StartCoroutine(TypeLine(LineasActuales()[index]));
        index++;
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(1f / charsPerSecond);
        }

        isTyping = false;
    }
    #endregion

    #region Secuencias especiales
    private IEnumerator MirarTableroYVolver()
    {
        esperandoVistaTablero = true;
        dialogueText.text = "";

        yield return BlinkYCambiarCamara(CameraController.Instance.IrAGameplay);
        yield return new WaitForSeconds(2f);
        yield return BlinkYCambiarCamara(CameraController.Instance.IrADialogo);

        enExplicacion = true;
        esperandoVistaTablero = false;
        index = 0;

        NextLine();
    }

    private void IniciarDialogoTrio()
    {
        if (enDialogoTrio || enDialogoFinal) return;

        gameObject.SetActive(true);
        enDialogoTrio = true;
        index = 0;

        StartCoroutine(BlinkYCambiarCamara(CameraController.Instance.IrADialogo, NextLine));
    }

    private IEnumerator FinalizarDialogoTrio()
    {
        dialogueText.text = "";

        yield return BlinkYCambiarCamara(CameraController.Instance.IrAGameplay);

        enDialogoTrio = false;
        gameObject.SetActive(false);
    }

    private void FinalizarTutorialDialogo()
    {
        LevelManager.tutorialDialogoVisto = true;

        BlinkController.Instance.StartBlink(() =>
        {
            CameraController.Instance.IrAGameplay();
        });

        gameObject.SetActive(false);
    }
    #endregion

    #region Diálogo de resultado (IResultadoDialogo)
    public bool TieneDialogoVictoria() => victoriaLines.Length > 0;
    public bool TieneDialogoDerrota() => derrotaLines.Length > 0;

    public void MostrarDialogoVictoria(System.Action alFinalizar)
    {
        callbackFinDialogoFinal = alFinalizar;
        IniciarDialogoFinal(true);
    }

    public void MostrarDialogoDerrota(System.Action alFinalizar)
    {
        callbackFinDialogoFinal = alFinalizar;
        IniciarDialogoFinal(false);
    }

    private void IniciarDialogoFinal(bool victoria)
    {
        esVictoria = victoria;
        enDialogoFinal = true;

        gameObject.SetActive(true);
        dialogueText.text = "";
        index = 0;

        StartCoroutine(BlinkYCambiarCamara(CameraController.Instance.IrADialogo, NextLine));
    }

    private void FinalizarDialogoFinal()
    {
        dialogueText.text = "";

        BlinkController.Instance.StartBlink(() =>
        {
            callbackFinDialogoFinal?.Invoke();
            callbackFinDialogoFinal = null;
        });

        enDialogoFinal = false;
        gameObject.SetActive(false);
    }
    #endregion

    #region Utilidades
    private string[] LineasActuales()
    {
        if (enDialogoFinal)
            return esVictoria ? victoriaLines : derrotaLines;

        if (enDialogoTrio)
            return trioLines;

        return enExplicacion ? explicacionLines : introLines;
    }

    private string ObtenerLineaActual()
    {
        int i = Mathf.Clamp(index - 1, 0, LineasActuales().Length - 1);
        return LineasActuales()[i];
    }

    private IEnumerator BlinkYCambiarCamara(System.Action accion, System.Action onFinish = null)
    {
        bool terminado = false;

        BlinkController.Instance.StartBlink(() =>
        {
            accion?.Invoke();
            terminado = true;
        });

        yield return new WaitUntil(() => terminado);
        onFinish?.Invoke();
    }
    #endregion
}