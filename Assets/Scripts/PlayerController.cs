using System;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    public GameObject arma;
    public LineRenderer lineRenderer;
    public Material liniaFuera, liniaDentro;
    public GameController.EstadoPersonaje estado = GameController.EstadoPersonaje.Parado;
    public int areaMovimiento, areaAtaque;

    private Animator _animator;
    private NavMeshAgent _navMeshAgent;
    private Vector3 _otherPoint, _lastPosition;
    private float _speed;
    private GameController _controller;

    public void Aim()
    {
        arma.SetActive(!arma.activeSelf);
    }

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _lastPosition = transform.position;
        _controller = GameObject.Find("Controller").GetComponent<GameController>();
    }

    private void Update()
    {
        var pos = transform.position;
        _speed = Mathf.Lerp(_speed, (pos - _lastPosition).magnitude / Time.deltaTime, 0.75f);
        _lastPosition = pos;
        _animator.SetFloat("Velocity", _speed);

        Ray camRay;
        RaycastHit hit;
        Vector3 initialPoint, middlePoint, finalPoint;

        switch (estado)
        {
            case GameController.EstadoPersonaje.Parado:
                _controller.MarcarAreaMovimiento(areaMovimiento, GetPositon());
                estado = GameController.EstadoPersonaje.Escogiendo;
                break;

            case GameController.EstadoPersonaje.Escogiendo:
                camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (!Physics.Raycast(camRay, out hit, Mathf.Infinity, LayerMask.GetMask("Suelo")))
                    return;

                if (Math.Abs(hit.point.x - pos.x) > Math.Abs(hit.point.z - pos.z))
                {
                    middlePoint = new Vector3(_controller.CalculateCoord(hit.point.x, 'x'), 0.25f,
                        _controller.CalculateCoord(pos.z, 'z'));
                }
                else
                {
                    middlePoint = new Vector3(_controller.CalculateCoord(pos.x, 'x'), 0.25f,
                        _controller.CalculateCoord(hit.point.z, 'z'));
                }

                finalPoint = new Vector3(_controller.CalculateCoord(hit.point.x, 'x'), 0.25f,
                    _controller.CalculateCoord(hit.point.z, 'z'));

                initialPoint = new Vector3(_controller.CalculateCoord(pos.x, 'x'), 0.25f,
                    _controller.CalculateCoord(pos.z, 'z'));

                lineRenderer.SetPosition(0, initialPoint);
                lineRenderer.SetPosition(1, middlePoint);
                lineRenderer.SetPosition(2, finalPoint);

                if (Vector3.Distance(initialPoint, middlePoint) + Vector3.Distance(middlePoint, finalPoint) <=
                    areaMovimiento * 2)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        _navMeshAgent.SetDestination(middlePoint);
                        _otherPoint = finalPoint;
                        estado = GameController.EstadoPersonaje.Moviendo1;
                    }

                    lineRenderer.material = liniaDentro;
                }
                else lineRenderer.material = liniaFuera;

                break;

            case GameController.EstadoPersonaje.Moviendo1:
                if (!_navMeshAgent.pathPending && _navMeshAgent.remainingDistance < 0.5f)
                {
                    estado = GameController.EstadoPersonaje.Moviendo2;
                    GoToOtherPoint();
                }

                break;

            case GameController.EstadoPersonaje.Moviendo2:
                if (!_navMeshAgent.pathPending && _navMeshAgent.remainingDistance < 0.5f)
                {
                    lineRenderer.SetPosition(0, Vector3.zero);
                    lineRenderer.SetPosition(1, Vector3.zero);
                    lineRenderer.SetPosition(2, Vector3.zero);
                    _controller.LimpiarArea();
                    _controller.MarcarAreaAtaque(1, 0, GetPositon());
                    estado = GameController.EstadoPersonaje.EscogiendoAtaque;
                }

                break;

            case GameController.EstadoPersonaje.EscogiendoAtaque:
                camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (!Physics.Raycast(camRay, out hit, Mathf.Infinity, LayerMask.GetMask("Suelo")))
                    return;

                if (Math.Abs(hit.point.x - pos.x) > Math.Abs(hit.point.z - pos.z))
                {
                    middlePoint = new Vector3(_controller.CalculateCoord(hit.point.x, 'x'), 0.25f,
                        _controller.CalculateCoord(pos.z, 'z'));
                }
                else
                {
                    middlePoint = new Vector3(_controller.CalculateCoord(pos.x, 'x'), 0.25f,
                        _controller.CalculateCoord(hit.point.z, 'z'));
                }

                finalPoint = new Vector3(_controller.CalculateCoord(hit.point.x, 'x'), 0.25f,
                    _controller.CalculateCoord(hit.point.z, 'z'));

                initialPoint = new Vector3(_controller.CalculateCoord(pos.x, 'x'), 0.25f,
                    _controller.CalculateCoord(pos.z, 'z'));

                lineRenderer.SetPosition(0, initialPoint);
                lineRenderer.SetPosition(1, middlePoint);
                lineRenderer.SetPosition(2, finalPoint);

                if (Vector3.Distance(initialPoint, middlePoint) + Vector3.Distance(middlePoint, finalPoint) <=
                    areaAtaque * 2)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        transform.LookAt(finalPoint);
                        lineRenderer.SetPosition(0, Vector3.zero);
                        lineRenderer.SetPosition(1, Vector3.zero);
                        lineRenderer.SetPosition(2, Vector3.zero);
                        _controller.LimpiarArea();
                        estado = GameController.EstadoPersonaje.Atacando;
                        _animator.SetTrigger("Aim");
                    }

                    lineRenderer.material = liniaDentro;
                }
                else lineRenderer.material = liniaFuera;

                break;

            case GameController.EstadoPersonaje.Atacando:
                break;
        }
    }

    private void GoToOtherPoint()
    {
        _navMeshAgent.SetDestination(_otherPoint);
        _otherPoint = Vector3.zero;
    }

    public Vector3 GetPositon()
    {
        var position = transform.position;

        return new Vector3(_controller.CalculateCoord(position.x, 'x'), 0.25f,
            _controller.CalculateCoord(position.z, 'z'));
    }
}
