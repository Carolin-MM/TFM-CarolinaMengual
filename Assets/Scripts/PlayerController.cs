using System;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    public GameObject arma;
    public LineRenderer lineRenderer;
    public Material liniaFuera, liniaDentro;
    public GameController.EstadoPersonaje estado = GameController.EstadoPersonaje.Parado;
    public int area;

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

        switch (estado)
        {
            case GameController.EstadoPersonaje.Parado:
                _controller.MarcarArea(area, transform.position);
                estado = GameController.EstadoPersonaje.Escogiendo;
                break;

            case GameController.EstadoPersonaje.Escogiendo:
                var camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (!Physics.Raycast(camRay, out var hit, Mathf.Infinity, LayerMask.GetMask("Suelo")))
                    return;

                Vector3 middlePoint;
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

                var finalPoint = new Vector3(_controller.CalculateCoord(hit.point.x, 'x'), 0.25f,
                    _controller.CalculateCoord(hit.point.z, 'z'));

                var initialPoint = new Vector3(_controller.CalculateCoord(pos.x, 'x'), 0.25f,
                    _controller.CalculateCoord(pos.z, 'z'));

                lineRenderer.SetPosition(0, initialPoint);
                lineRenderer.SetPosition(1, middlePoint);
                lineRenderer.SetPosition(2, finalPoint);

                if (Vector3.Distance(initialPoint, middlePoint) + Vector3.Distance(middlePoint, finalPoint) <= area * 2)
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
                    estado = GameController.EstadoPersonaje.Atacando;
                }

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
}
