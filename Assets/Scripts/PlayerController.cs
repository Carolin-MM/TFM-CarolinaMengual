using System;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    public GameObject arma;
    public LineRenderer lineRenderer;
    public bool posicionFijada = false;

    private Animator _animator;
    private NavMeshAgent _navMeshAgent;
    private Vector3 _otherPoint, _lastPosition;
    private float _speed;

    public void Aim()
    {
        arma.SetActive(!arma.activeSelf);
    }

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _lastPosition = transform.position;
    }

    private void Update()
    {
        var pos = transform.position;
        _speed = Mathf.Lerp(_speed, (pos - _lastPosition).magnitude / Time.deltaTime, 0.75f);
        _lastPosition = pos;
        _animator.SetFloat("Velocity", _speed);

        if (!posicionFijada)
        {
            var camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(camRay, out var hit, Mathf.Infinity, LayerMask.GetMask("Suelo"))) return;
            
            var middlePoint = pos;
            if (Math.Abs(hit.point.x - pos.x) > Math.Abs(hit.point.z - pos.z))
            {
                middlePoint.x = CalculateCoord(hit.point.x);
                middlePoint.z = CalculateCoord(middlePoint.z);
            }
            else
            {
                middlePoint.x = CalculateCoord(middlePoint.x);
                middlePoint.z = CalculateCoord(hit.point.z);
            }

            var hitPoint = new Vector3(CalculateCoord(hit.point.x), pos.y, CalculateCoord(hit.point.z));

            lineRenderer.SetPosition(0, pos);
            lineRenderer.SetPosition(1, middlePoint);
            lineRenderer.SetPosition(2, hitPoint);

            if (!Input.GetMouseButtonDown(0)) return;
            _navMeshAgent.SetDestination(middlePoint);
            _otherPoint = hitPoint;
            posicionFijada = true;
        }
        else if (_otherPoint != Vector3.zero)
        {
            if (!_navMeshAgent.pathPending && _navMeshAgent.remainingDistance < 0.5f)
                GoToOtherPoint();
        }
    }

    private void GoToOtherPoint()
    {
        _navMeshAgent.SetDestination(_otherPoint);
        _otherPoint = Vector3.zero;
    }

    private static float CalculateCoord(float realCoord)
    {
        const int factor = 2;
        
        var coord = realCoord / factor;
        coord = (float) Math.Round(coord);
        return coord * factor;
    }
}
