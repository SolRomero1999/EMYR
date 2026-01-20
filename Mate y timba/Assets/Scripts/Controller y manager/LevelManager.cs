using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelManagerFlags
{
    public static bool VieneDeDerrota = false;
}

public static class LevelManager
{
    public static int CurrentLevel = 0;
    public static bool reglasEliminacionActivas = false;
    public static bool tutorialDialogoVisto = false;
    public static int UltimoNivelCompletado = -1; 
    private const string SCENE_DIALOGO = "GrandsonScene";
    private const string SCENE_TUTORIAL = "0.Gameplay_Tutorial_Abuelo";

    private static readonly string[] GAME_SCENES =
    {
        "1.GameScene", // índice 0 
        "2.GameScene", 
        "3.GameScene"  
    };

    public static void IrADialogo()
    {
        SceneManager.LoadScene(SCENE_DIALOGO);
    }

    public static void IniciarTutorial()
    {
        reglasEliminacionActivas = false;
        CurrentLevel = 0;
        SceneManager.LoadScene(SCENE_TUTORIAL);
    }

    public static void IniciarNivelActual()
    {
        reglasEliminacionActivas = CurrentLevel >= 2;

        if (CurrentLevel == 0)
        {
            SceneManager.LoadScene(SCENE_TUTORIAL);
            return;
        }

        int index = CurrentLevel - 1;

        if (index >= 0 && index < GAME_SCENES.Length)
        {
            Debug.Log($"Cargando nivel {CurrentLevel} -> escena: {GAME_SCENES[index]}");
            SceneManager.LoadScene(GAME_SCENES[index]);
        }
        else
        {
            Debug.Log("No hay más niveles, volviendo al diálogo del nieto");
            SceneManager.LoadScene(SCENE_DIALOGO);
        }
    }

    public static void AvanzarNivel()
    {
        UltimoNivelCompletado = CurrentLevel;
        CurrentLevel++;
        Debug.Log($"Nivel {UltimoNivelCompletado} completado. Avanzando a nivel {CurrentLevel}");
    }

    public static bool HayMasNiveles()
    {
        int index = CurrentLevel - 1;
        return index >= 0 && index < GAME_SCENES.Length;
    }

    public static bool EsPrimerDialogo()
    {
        return !tutorialDialogoVisto;
    }

    public static bool EsPostTutorial()
    {
        return tutorialDialogoVisto && UltimoNivelCompletado == 0;
    }

    public static bool EsPostUltimoNivel()
    {
        return tutorialDialogoVisto &&
            UltimoNivelCompletado == GAME_SCENES.Length;
    }

}