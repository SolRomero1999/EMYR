using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class DialogueManualAdvance : MonoBehaviour
{
    public TMP_Text dialogueText;
    public Button continuarButton;

    [Header("Diálogos")]
    [TextArea] public string[] linesInicial;
    [TextArea] public string[] linesPostTutorial;
    [TextArea] public string[] linesPostNivel1;
    [TextArea] public string[] linesPostNivel2;
    [TextArea] public string[] linesDerrota;
    [TextArea] public string[] linesCierre;

    public float charsPerSecond = 40f;

    private string[] lines;
    private int index = 0;
    private bool isTyping;
    private Coroutine typingCoroutine;

    private void Start()
    {
        continuarButton.onClick.AddListener(NextLine);
        dialogueText.text = "";

        SeleccionarDialogo();
        NextLine();
    }

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
            dialogueText.text = lines[Mathf.Clamp(index - 1, 0, lines.Length - 1)];
            isTyping = false;
            return;
        }

        typingCoroutine = StartCoroutine(TypeLine(lines[index]));
        index++;
    }

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

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(1f / charsPerSecond);
        }

        isTyping = false;
    }

    private void OnDestroy()
    {
        continuarButton.onClick.RemoveListener(NextLine);
    }
}

