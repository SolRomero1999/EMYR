using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class DialogueManualAdvance : MonoBehaviour
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
    [TextArea] public string[] linesInicial;
    [TextArea] public string[] linesPostTutorial;
    [TextArea] public string[] linesPostNivel1;
    [TextArea] public string[] linesPostNivel2;
    [TextArea] public string[] linesDerrota;
    [TextArea] public string[] linesCierre;
    #endregion

    #region Config
    public float charsPerSecond = 40f;
    #endregion

    #region Estado
    private string[] lines;
    private int index = 0;
    private bool isTyping;
    private Coroutine typingCoroutine;
    #endregion

    #region Unity
    private void Start()
    {
        continuarButton.onClick.AddListener(NextLine);
        dialogueText.text = "";

        SeleccionarDialogo();
        NextLine();
    }

    private void OnDestroy()
    {
        continuarButton.onClick.RemoveListener(NextLine);
    }
    #endregion

    #region Flujo principal
    private void SeleccionarDialogo()
    {
        if (LevelManager.EsPostUltimoNivel())
        {
            lines = linesCierre;
            return;
        }

        if (LevelManagerFlags.VieneDeDerrota)
        {
            lines = new string[]
            {
                linesDerrota[Random.Range(0, linesDerrota.Length)]
            };
            return;
        }

        if (LevelManager.EsPostTutorial())
        {
            lines = linesPostTutorial;
            return;
        }

        if (LevelManager.EsPrimerDialogo())
        {
            lines = linesInicial;
            return;
        }

        int nivelCompletado = LevelManager.UltimoNivelCompletado;

        if (nivelCompletado == 1)
            lines = linesPostNivel1;
        else
            lines = linesPostNivel2;
    }

    private void NextLine()
    {
        if (index >= lines.Length && !isTyping)
        {
            IrASiguienteEscena();
            return;
        }

        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            MostrarLineaCompleta(lines[Mathf.Clamp(index - 1, 0, lines.Length - 1)]);
            isTyping = false;
            return;
        }

        typingCoroutine = StartCoroutine(TypeLine(lines[index]));
        index++;
    }
    #endregion

    #region Otros
    private void IrASiguienteEscena()
    {
        if (LevelManager.EsPostUltimoNivel())
        {
            SceneManager.LoadScene("Menu");
            return;
        }

        if (LevelManagerFlags.VieneDeDerrota)
        {
            LevelManagerFlags.VieneDeDerrota = false;
            LevelManager.IniciarNivelActual();
            return;
        }

        if (!LevelManager.tutorialDialogoVisto)
        {
            LevelManager.IniciarTutorial();
        }
        else
        {
            LevelManager.IniciarNivelActual();
        }
    }

    private IEnumerator TypeLine(string line)
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

    #region Speaker & Color helpers
    private string ParseSpeakerAndColor(string line)
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

    private void MostrarLineaCompleta(string line)
    {
        dialogueText.text = ParseSpeakerAndColor(line);
    }
    #endregion
}
