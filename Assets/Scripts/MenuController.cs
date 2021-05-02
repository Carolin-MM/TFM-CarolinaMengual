using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject panel;
    public Text texto;

    public void FinalPartida(string ganador)
    {
        texto.text += ganador;
        panel.SetActive(true);
    }

    public static void Replay()
    {
        SceneManager.LoadScene(0);
    }

    public static void Exit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
