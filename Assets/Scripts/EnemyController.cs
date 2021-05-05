using System;
using UnityEngine;

public class EnemyController : PnjController
{
    private PnjController objetivo;

    protected override void Update()
    {
        base.Update();

        switch (estado)
        {
            case GameController.EstadoPersonaje.EscogiendoDestino:
                var middlePoint = GetPositon();
                var finalPoint = GetPositon();

                var initialPoint = GetPositon();

                /*for (var i = -areaAtaque; i <= areaAtaque; i++)
                {
                    var absI = Math.Abs(i);
                    var extrem = areaAtaque - absI;

                    for (var j = -extrem; j <= extrem; j++)
                    {
                        var absJ = Math.Abs(j);
                        if ((absI == 0 && absJ == 0) || (absI <= areaMinimaAtaque && absJ <= areaMinimaAtaque && absI != absJ)) continue;

                        var pos = initialPoint + new Vector3(i * 2, 0, j * 2);

                        if (controller.DentroArea(pos) && controller.PosibleAccion(this, pos, tipo != GameController.Tipo.Sanador))
                        {
                            navMeshAgent.SetDestination(middlePoint);
                            otherPoint = finalPoint;
                            estado = GameController.EstadoPersonaje.Moviendo1;
                            return;
                        }
                    }
                }*/

                objetivo = tipo == GameController.Tipo.Sanador
                    ? controller.ObjetivoCuracion(this)
                    : controller.ObjetivoCercano(this);

                var distance = float.PositiveInfinity;
                var objetivoPosition = objetivo.GetPositon();

                for (var i = -areaMovimiento; i <= areaMovimiento; i++)
                {
                    var extrem = areaMovimiento - Math.Abs(i);

                    for (var j = -extrem; j <= extrem; j++)
                    {
                        if (i == 0 && j == 0) continue;

                        var pos = initialPoint + new Vector3(i * 2, 0, j * 2);

                        if (controller.DentroArea(pos))
                        {
                            var middleAux = new Vector3(controller.CalculateCoord(initialPoint.x, 'x'), 0.25f,
                                controller.CalculateCoord(pos.z, 'z'));

                            var finalAux = new Vector3(controller.CalculateCoord(pos.x, 'x'), 0.25f,
                                controller.CalculateCoord(pos.z, 'z'));

                            if (Vector3.Distance(finalAux, objetivoPosition) <= areaMinimaAtaque) continue;

                            var distAux = Math.Abs(objetivoPosition.x - finalAux.x) +
                                          Math.Abs(objetivoPosition.z - finalAux.z);

                            if (distAux < distance)
                            {
                                middlePoint = middleAux;
                                finalPoint = finalAux;
                                distance = distAux;
                            }
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
                /*var pnjPosition = GetPositon();

                for (var i = -areaAtaque; i <= areaAtaque; i++)
                {
                    var extrem = areaAtaque - Math.Abs(i);

                    for (var j = -extrem; j <= extrem; j++)
                    {
                        if (i == 0 && j == 0) continue;

                        var pos = pnjPosition + new Vector3(i * 2, 0, j * 2);

                        if (controller.DentroArea(pos) &&
                            controller.PosibleAccion(this, pos, tipo != GameController.Tipo.Sanador))
                        {
                            
                        }
                    }
                }*/

                if (controller.PosibleAccion(this, objetivo.GetPositon(), tipo != GameController.Tipo.Sanador))
                {
                    transform.LookAt(objetivo.GetPositon());
                    estado = GameController.EstadoPersonaje.Atacando;
                    animator.SetTrigger("Aim");
                    objetivo = null;
                    return;
                }

                objetivo = null;
                // Si no puede atacar
                EndAtack();
                break;
        }
    }
}
