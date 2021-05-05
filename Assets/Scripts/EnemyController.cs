using System;
using UnityEngine;

public class EnemyController : PnjController
{
    private PnjController _objetivo;

    protected override void Update()
    {
        base.Update();

        switch (estado)
        {
            case GameController.EstadoPersonaje.EscogiendoDestino:
                if (tipo != GameController.Tipo.Sanador && AccionDirecta()) break;
                var initialPoint = GetPositon();
                var middlePoint = GetPositon();
                var finalPoint = GetPositon();

                _objetivo = tipo == GameController.Tipo.Sanador
                    ? controller.ObjetivoCuracion(this)
                    : controller.ObjetivoCercano(this);

                var distance = float.PositiveInfinity;
                var distanciaCamino = float.PositiveInfinity;
                var objetivoPosition = _objetivo.GetPositon();

                for (var i = -areaMovimiento; i <= areaMovimiento; i++)
                {
                    var extrem = areaMovimiento - Math.Abs(i);

                    for (var j = -extrem; j <= extrem; j++)
                    {
                        if (i == 0 && j == 0) continue;

                        var pos = initialPoint + new Vector3(i * 2, 0, j * 2);

                        if (!controller.DentroArea(pos) || !controller.PosibleMovimiento(this, pos)) continue;

                        var middleAux = new Vector3(controller.CalculateCoord(initialPoint.x, 'x'), 0.25f,
                            controller.CalculateCoord(pos.z, 'z'));

                        var finalAux = new Vector3(controller.CalculateCoord(pos.x, 'x'), 0.25f,
                            controller.CalculateCoord(pos.z, 'z'));

                        var distAux = Math.Abs(objetivoPosition.x - finalAux.x) +
                                      Math.Abs(objetivoPosition.z - finalAux.z);

                        if (distAux <= areaMinimaAtaque) continue;

                        var distAuxCamino = Vector3.Distance(initialPoint, middleAux) +
                                            Vector3.Distance(middlePoint, finalAux);

                        if (distAux < distance || (Math.Abs(distAux - distance) < 0.1f && distAuxCamino < distanciaCamino))
                        {
                            middlePoint = middleAux;
                            finalPoint = finalAux;
                            distance = distAux;
                            distanciaCamino = distAuxCamino;
                        }
                    }
                }

                navMeshAgent.SetDestination(middlePoint);
                otherPoint = finalPoint;
                estado = GameController.EstadoPersonaje.Moviendo1;
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
                    estado = GameController.EstadoPersonaje.EscogiendoAccion;
                break;

            case GameController.EstadoPersonaje.EscogiendoAccion:
                controller.MarcarAreaAccion(areaAtaque, areaMinimaAtaque, GetPositon(),
                    tipo != GameController.Tipo.Sanador);

                var myPos = GetPositon();
                var objPos = _objetivo.GetPositon();
                middlePoint = new Vector3(myPos.x, 0.25f, objPos.z);

                distance = Vector3.Distance(myPos, middlePoint) + Vector3.Distance(middlePoint, objPos);
                if (distance <= areaAtaque * 2 && distance > areaMinimaAtaque * 2 &&
                    controller.PosibleAccion(this, objPos, tipo != GameController.Tipo.Sanador))
                {
                    transform.LookAt(_objetivo.transform.position);
                    estado = GameController.EstadoPersonaje.Atacando;
                    animator.SetTrigger("Aim");
                    return;
                }
                else
                {
                    // Si no puede atacar
                    EndAtack();
                }

                _objetivo = null;
                break;
        }
    }

    private bool AccionDirecta()
    {
        var initialPoint = GetPositon();

        for (var i = -areaAtaque; i <= areaAtaque; i++)
        {
            var absI = Math.Abs(i);
            var extrem = areaAtaque - absI;

            for (var j = -extrem; j <= extrem; j++)
            {
                var absJ = Math.Abs(j);
                if ((absI == 0 && absJ == 0) ||
                    (absI <= areaMinimaAtaque && absJ <= areaMinimaAtaque && absI != absJ)) continue;

                var pos = initialPoint + new Vector3(i * 2, 0, j * 2);

                if (!controller.DentroArea(pos) ||
                    !controller.PosibleAccion(this, pos, tipo != GameController.Tipo.Sanador)) continue;

                controller.MarcarAreaAccion(areaAtaque, areaMinimaAtaque, initialPoint, true);
                transform.LookAt(pos);
                estado = GameController.EstadoPersonaje.Atacando;
                animator.SetTrigger("Aim");
                return true;
            }
        }

        return false;
    }
}
