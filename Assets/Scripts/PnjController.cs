using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public abstract class PnjController : MonoBehaviour
{
    public GameObject arma;
    public GameController.EstadoPersonaje estado = GameController.EstadoPersonaje.Parado;
    public int areaMovimiento, areaAtaque;
    public GameController.Tipo tipo;
    public int vidaMaxima, vidaActual, daño;
    public Image indicadorVida;
    public GameObject backgrounVida;
    public PnjController siguienteAtaque;
    
    protected Animator Animator;
    protected NavMeshAgent NavMeshAgent;
    protected Vector3 OtherPoint;
    protected GameController Controller;
    
    private Vector3 _lastPosition;
    private float _speed;
    
    public void Aim()
    {
        arma.SetActive(!arma.activeSelf);
    }

    public void EndAtack()
    {
        estado = GameController.EstadoPersonaje.Parado;
    }

    public void StartAtack()
    {
        siguienteAtaque.RecibirDaño(daño);
        siguienteAtaque = null;
    }

    private void Start()
    {
        Animator = GetComponent<Animator>();
        NavMeshAgent = GetComponent<NavMeshAgent>();
        _lastPosition = transform.position;
        Controller = GameObject.Find("Controller").GetComponent<GameController>();
    }

    protected virtual void Update()
    {
        var pos = transform.position;
        _speed = Mathf.Lerp(_speed, (pos - _lastPosition).magnitude / Time.deltaTime, 0.75f);
        _lastPosition = pos;
        Animator.SetFloat("Velocity", _speed);
    }

    protected void GoToOtherPoint()
    {
        NavMeshAgent.SetDestination(OtherPoint);
        OtherPoint = Vector3.zero;
    }

    public Vector3 GetPositon()
    {
        var position = transform.position;

        return new Vector3(Controller.CalculateCoord(position.x, 'x'), 0.25f,
            Controller.CalculateCoord(position.z, 'z'));
    }

    private void RecibirDaño(int dañoRecibido)
    {
        var aux = vidaActual - dañoRecibido;
        if (aux < 0)
        {
            aux = 0;
            Animator.SetBool("Die", true);
            backgrounVida.SetActive(false);
            estado = GameController.EstadoPersonaje.Muerto;
        }
        else
        {
            Animator.SetTrigger("Damage");
        }

        vidaActual = aux;
        indicadorVida.fillAmount = (float) vidaActual / vidaMaxima;
    }
}
