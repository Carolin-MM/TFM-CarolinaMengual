using System;
using UnityEngine;
using Debug = UnityEngine.Debug;

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
            case GameController.EstadoPersonaje.EscogiendoDestino:
                camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (!Physics.Raycast(camRay, out hit, Mathf.Infinity, LayerMask.GetMask("Suelo")))
                    return;

                if (Math.Abs(hit.point.x - pos.x) > Math.Abs(hit.point.z - pos.z))
                {
                    middlePoint = new Vector3(controller.CalculateCoord(hit.point.x, 'x'), 0.25f,
                        controller.CalculateCoord(pos.z, 'z'));
                }
                else
                {
                    middlePoint = new Vector3(controller.CalculateCoord(pos.x, 'x'), 0.25f,
                        controller.CalculateCoord(hit.point.z, 'z'));
                }

                finalPoint = new Vector3(controller.CalculateCoord(hit.point.x, 'x'), 0.25f,
                    controller.CalculateCoord(hit.point.z, 'z'));

                initialPoint = new Vector3(controller.CalculateCoord(pos.x, 'x'), 0.25f,
                    controller.CalculateCoord(pos.z, 'z'));

                lineRenderer.SetPosition(0, initialPoint);
                lineRenderer.SetPosition(1, middlePoint);
                lineRenderer.SetPosition(2, finalPoint);

                if (Vector3.Distance(initialPoint, middlePoint) + Vector3.Distance(middlePoint, finalPoint) <=
                    areaMovimiento * 2 && controller.PosibleMovimiento(this, finalPoint))
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        navMeshAgent.SetDestination(middlePoint);
                        otherPoint = finalPoint;
                        estado = GameController.EstadoPersonaje.Moviendo1;
                    }

                    lineRenderer.material = liniaDentro;
                }
                else lineRenderer.material = liniaFuera;

                break;

            case GameController.EstadoPersonaje.Moviendo1:
                if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
                {
                    estado = GameController.EstadoPersonaje.Moviendo2;
                    GoToOtherPoint();
                }

                break;

            case GameController.EstadoPersonaje.Moviendo2:
                if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
                {
                    lineRenderer.SetPosition(0, Vector3.zero);
                    lineRenderer.SetPosition(1, Vector3.zero);
                    lineRenderer.SetPosition(2, Vector3.zero);
                    controller.LimpiarArea();
                    controller.MarcarAreaAccion(areaAtaque, areaMinimaAtaque, GetPositon(),
                        tipo != GameController.Tipo.Sanador);
                    estado = GameController.EstadoPersonaje.EscogiendoAccion;
                }

                break;

            case GameController.EstadoPersonaje.EscogiendoAccion:
                camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (!Physics.Raycast(camRay, out hit, Mathf.Infinity, LayerMask.GetMask("Suelo")))
                    return;

                if (Math.Abs(hit.point.x - pos.x) > Math.Abs(hit.point.z - pos.z))
                {
                    middlePoint = new Vector3(controller.CalculateCoord(hit.point.x, 'x'), 0.25f,
                        controller.CalculateCoord(pos.z, 'z'));
                }
                else
                {
                    middlePoint = new Vector3(controller.CalculateCoord(pos.x, 'x'), 0.25f,
                        controller.CalculateCoord(hit.point.z, 'z'));
                }

                finalPoint = new Vector3(controller.CalculateCoord(hit.point.x, 'x'), 0.25f,
                    controller.CalculateCoord(hit.point.z, 'z'));

                initialPoint = new Vector3(controller.CalculateCoord(pos.x, 'x'), 0.25f,
                    controller.CalculateCoord(pos.z, 'z'));

                lineRenderer.SetPosition(0, initialPoint);
                lineRenderer.SetPosition(1, middlePoint);
                lineRenderer.SetPosition(2, finalPoint);

                var distance = Vector3.Distance(initialPoint, middlePoint) + Vector3.Distance(middlePoint, finalPoint);
                if (distance <= areaAtaque * 2 && ((distance > areaMinimaAtaque * 2 &&
                      controller.PosibleAccion(this, finalPoint, tipo != GameController.Tipo.Sanador)) ||
                     finalPoint == initialPoint))
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        lineRenderer.SetPosition(0, Vector3.zero);
                        lineRenderer.SetPosition(1, Vector3.zero);
                        lineRenderer.SetPosition(2, Vector3.zero);

                        if (finalPoint == initialPoint) EndAtack();
                        else
                        {
                            estado = GameController.EstadoPersonaje.Atacando;
                            transform.LookAt(finalPoint);
                            animator.SetTrigger("Aim");
                        }
                    }

                    lineRenderer.material = liniaDentro;
                }
                else lineRenderer.material = liniaFuera;

                break;
        }
    }
}
