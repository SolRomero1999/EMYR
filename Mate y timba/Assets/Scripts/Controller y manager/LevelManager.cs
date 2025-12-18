using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelManager
{
    public static int CurrentLevel = 1;

    public static bool reglasEliminacionActivas = true;
    public static bool dialogoPostTutorial = false;
    public static bool tutorialDialogoVisto = false;
    private const string SCENE_TUTORIAL = "0.Gameplay_Tutorial_Abuelo";
    private const string SCENE_DIALOGO = "GrandsonScene";

    private static readonly string[] GAME_SCENES =
    {
        "1.GameScene",   
        "2.GameScene",  
        "3.GameScene"    
    };

    public static void StartLevelTuto()
    {
        reglasEliminacionActivas = false;
        SceneManager.LoadScene(SCENE_TUTORIAL);
    }

    public static void GoToDialogue()
    {
        dialogoPostTutorial = false;
        SceneManager.LoadScene(SCENE_DIALOGO);
    }

    public static void GoToDialogue_PostTutorial()
    {
        dialogoPostTutorial = true;
        SceneManager.LoadScene(SCENE_DIALOGO);
    }

    public static void StartLevelNormal()
    {
        reglasEliminacionActivas = CurrentLevel > 1;

        int index = Mathf.Clamp(CurrentLevel - 1, 0, GAME_SCENES.Length - 1);
        SceneManager.LoadScene(GAME_SCENES[index]);
    }

    public static void NextLevel()
    {
        CurrentLevel++;
    }
}
