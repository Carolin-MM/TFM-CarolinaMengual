using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject panelFinal, panelSeleccion;
    public Text texto, txtSeleccion;
    public Slider sliderSeleccion;
    public GameController gameController;
    public int miembrosMaximos;
    public Button botonEmpezar;
    public Text txtSanadores, txtDefensas, txtDistancia, txtVelocistas;

    private int _sanadores, _defensas, _distancia, _velocistas;

    public void FinalPartida(string ganador)
    {
        texto.text += ganador;
        panelFinal.SetActive(true);
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

    public void Miembros(string tipo)
    {
        int valor;
        try
        {
            valor = int.Parse(tipo.Substring(tipo.Length - 2));
        }
        catch
        {
            return;
        }

        switch (tipo.Substring(0, tipo.Length - 2))
        {
            case "Sanadores":
                _sanadores += valor;
                if (_sanadores < 0) _sanadores = 0;
                txtSanadores.text = _sanadores.ToString();
                break;
                
            case "Defensas":
                _defensas += valor;
                if (_defensas < 0) _defensas = 0;
                txtDefensas.text = _defensas.ToString();
                break;
                
            case "Distancia":
                _distancia += valor;
                if (_distancia < 0) _distancia = 0;
                txtDistancia.text = _distancia.ToString();
                break;
                
            case "Velocistas":
                _velocistas += valor;
                if (_velocistas < 0) _velocistas = 0;
                txtVelocistas.text = _velocistas.ToString();
                break;
        }
        ActualizarUI();
    }

    private void ActualizarUI()
    {
        float actual = _sanadores + _defensas + _velocistas + _distancia;
        txtSeleccion.text = "Miembros restantes: " + (miembrosMaximos - actual) + "/" + miembrosMaximos;
        sliderSeleccion.value = (miembrosMaximos - actual) / miembrosMaximos;
        botonEmpezar.interactable = actual <= miembrosMaximos;
    }

    public void Empezar()
    {
        panelSeleccion.SetActive(false);
        gameController.distribucionNivel = new GameController.Distribucion()
        {
            sanadores = _sanadores,
            defensa = _defensas,
            distancia = _distancia,
            velocistas = _velocistas
        };
        gameController.InstanciarAliados();
    }
}
