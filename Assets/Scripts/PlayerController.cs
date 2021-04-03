using System;
using UnityEngine;

public class PlayerController : PnjController
{
    public LineRenderer lineRenderer;
    public Material liniaFuera, liniaDentro;

    protected override void Update()
    {
        base.Update();

        var pos = transform.position;
        Ray camRay;
        RaycastHit hit;
        Vector3 initialPoint, middlePoint, finalPoint;

        switch (estado)
        {
            case GameController.EstadoPersonaje.Parado:
                Controller.MarcarAreaMovimiento(areaMovimiento, GetPositon());
                estado = GameController.EstadoPersonaje.Escogiendo;
                break;

            case GameController.EstadoPersonaje.Escogiendo:
                camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (!Physics.Raycast(camRay, out hit, Mathf.Infinity, LayerMask.GetMask("Suelo")))
                    return;

                if (Math.Abs(hit.point.x - pos.x) > Math.Abs(hit.point.z - pos.z))
                {
                    middlePoint = new Vector3(Controller.CalculateCoord(hit.point.x, 'x'), 0.25f,
                        Controller.CalculateCoord(pos.z, 'z'));
                }
                else
                {
                    middlePoint = new Vector3(Controller.CalculateCoord(pos.x, 'x'), 0.25f,
                        Controller.CalculateCoord(hit.point.z, 'z'));
                }

                finalPoint = new Vector3(Controller.CalculateCoord(hit.point.x, 'x'), 0.25f,
                    Controller.CalculateCoord(hit.point.z, 'z'));

                initialPoint = new Vector3(Controller.CalculateCoord(pos.x, 'x'), 0.25f,
                    Controller.CalculateCoord(pos.z, 'z'));

                lineRenderer.SetPosition(0, initialPoint);
                lineRenderer.SetPosition(1, middlePoint);
                lineRenderer.SetPosition(2, finalPoint);

                if (Vector3.Distance(initialPoint, middlePoint) + Vector3.Distance(middlePoint, finalPoint) <=
                    areaMovimiento * 2)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        NavMeshAgent.SetDestination(middlePoint);
                        OtherPoint = finalPoint;
                        estado = GameController.EstadoPersonaje.Moviendo1;
                    }

                    lineRenderer.material = liniaDentro;
                }
                else lineRenderer.material = liniaFuera;

                break;

            case GameController.EstadoPersonaje.Moviendo1:
                if (!NavMeshAgent.pathPending && NavMeshAgent.remainingDistance < 0.5f)
                {
                    estado = GameController.EstadoPersonaje.Moviendo2;
                    GoToOtherPoint();
                }

                break;

            case GameController.EstadoPersonaje.Moviendo2:
                if (!NavMeshAgent.pathPending && NavMeshAgent.remainingDistance < 0.5f)
                {
                    lineRenderer.SetPosition(0, Vector3.zero);
                    lineRenderer.SetPosition(1, Vector3.zero);
                    lineRenderer.SetPosition(2, Vector3.zero);
                    Controller.LimpiarArea();
                    Controller.MarcarAreaAtaque(1, 0, GetPositon());
                    estado = GameController.EstadoPersonaje.EscogiendoAtaque;
                }

                break;

            case GameController.EstadoPersonaje.EscogiendoAtaque:
                camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (!Physics.Raycast(camRay, out hit, Mathf.Infinity, LayerMask.GetMask("Suelo")))
                    return;

                if (Math.Abs(hit.point.x - pos.x) > Math.Abs(hit.point.z - pos.z))
                {
                    middlePoint = new Vector3(Controller.CalculateCoord(hit.point.x, 'x'), 0.25f,
                        Controller.CalculateCoord(pos.z, 'z'));
                }
                else
                {
                    middlePoint = new Vector3(Controller.CalculateCoord(pos.x, 'x'), 0.25f,
                        Controller.CalculateCoord(hit.point.z, 'z'));
                }

                finalPoint = new Vector3(Controller.CalculateCoord(hit.point.x, 'x'), 0.25f,
                    Controller.CalculateCoord(hit.point.z, 'z'));

                initialPoint = new Vector3(Controller.CalculateCoord(pos.x, 'x'), 0.25f,
                    Controller.CalculateCoord(pos.z, 'z'));

                lineRenderer.SetPosition(0, initialPoint);
                lineRenderer.SetPosition(1, middlePoint);
                lineRenderer.SetPosition(2, finalPoint);

                if (Vector3.Distance(initialPoint, middlePoint) + Vector3.Distance(middlePoint, finalPoint) <=
                    areaAtaque * 2 && Controller.PosibleAtaque(this, finalPoint))
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        transform.LookAt(finalPoint);
                        lineRenderer.SetPosition(0, Vector3.zero);
                        lineRenderer.SetPosition(1, Vector3.zero);
                        lineRenderer.SetPosition(2, Vector3.zero);
                        Controller.LimpiarArea();
                        estado = GameController.EstadoPersonaje.Atacando;
                        Animator.SetTrigger("Aim");
                    }

                    lineRenderer.material = liniaDentro;
                }
                else lineRenderer.material = liniaFuera;

                break;

            case GameController.EstadoPersonaje.Atacando:
                break;
        }
    }
}
