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
    public GameController controller;
    public GameObject prefabParticulas;
    
    protected Animator animator;
    protected NavMeshAgent navMeshAgent;
    protected Vector3 otherPoint;
    
    private Vector3 _lastPosition;
    private float _speed;
    
    public void Aim()
    {
        if (tipo == GameController.Tipo.Sanador) return;
        arma.SetActive(!arma.activeSelf);
    }

    public void EndAtack()
    {
        estado = GameController.EstadoPersonaje.Parado;
        controller.SiguienteTurno();
    }

    public void StartAtack()
    {
        if (tipo != GameController.Tipo.Sanador) siguienteAtaque.RecibirDaño(daño);
        else siguienteAtaque.RecibirCura(daño);
        siguienteAtaque = null;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        _lastPosition = transform.position;
    }

    protected virtual void Update()
    {
        var pos = transform.position;
        _speed = Mathf.Lerp(_speed, (pos - _lastPosition).magnitude / Time.deltaTime, 0.75f);
        _lastPosition = pos;
        animator.SetFloat("Velocity", _speed);
    }

    protected void GoToOtherPoint()
    {
        navMeshAgent.SetDestination(otherPoint);
        otherPoint = Vector3.zero;
    }

    public Vector3 GetPositon()
    {
        var position = transform.position;

        return new Vector3(controller.CalculateCoord(position.x, 'x'), 0.25f,
            controller.CalculateCoord(position.z, 'z'));
    }

    private void RecibirDaño(int dañoRecibido)
    {
        var aux = vidaActual - dañoRecibido;
        if (aux < 0)
        {
            aux = 0;
            animator.SetBool("Die", true);
            backgrounVida.SetActive(false);
            estado = GameController.EstadoPersonaje.Muerto;
            controller.Muerte(this);
        }
        else
        {
            animator.SetTrigger("Damage");
        }

        vidaActual = aux;
        indicadorVida.fillAmount = (float) vidaActual / vidaMaxima;
    }

    private void RecibirCura(int cura)
    {
        var ins = Instantiate(prefabParticulas, transform.position, Quaternion.identity);
        Destroy(ins, 2f);
        
        var aux = vidaActual + cura;
        if (aux > vidaMaxima) vidaActual = vidaMaxima;
        vidaActual = aux;
        indicadorVida.fillAmount = (float) vidaActual / vidaMaxima;
    }
}
