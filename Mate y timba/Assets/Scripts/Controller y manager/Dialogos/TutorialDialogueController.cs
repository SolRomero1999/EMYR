using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class TutorialDialogueController : MonoBehaviour
{
    public TMP_Text dialogueText;
    public Button continuarButton;

    [Header("Diálogo del abuelo (tutorial)")]
    [TextArea] public string[] tutorialLines;

    public float charsPerSecond = 40f;

    private int index = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private void Start()
    {
        if (LevelManager.tutorialDialogoVisto)
        {
            gameObject.SetActive(false);
            return;
        }

        continuarButton.onClick.AddListener(NextLine);
        dialogueText.text = "";
        index = 0;

        StartCoroutine(BlinkController.Instance.Blink(() =>
        {
            CameraController.Instance.IrADialogo();
        }));

        NextLine();
    }

    private void NextLine()
    {
        if (index >= tutorialLines.Length && !isTyping)
        {
            FinalizarTutorialDialogo();
            return;
        }

        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = tutorialLines[Mathf.Clamp(index, 0, tutorialLines.Length - 1)];
            isTyping = false;
            return;
        }

        typingCoroutine = StartCoroutine(TypeLine(tutorialLines[index]));
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

    private void FinalizarTutorialDialogo()
    {
        LevelManager.tutorialDialogoVisto = true;

        BlinkController.Instance.StartBlink(() =>
        {
            CameraController.Instance.IrAGameplay();
        });

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        continuarButton.onClick.RemoveListener(NextLine);
    }
}


