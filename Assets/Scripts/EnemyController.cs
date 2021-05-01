using System;
using UnityEngine;

public class EnemyController : PnjController
{
    protected override void Update()
    {
        base.Update();

        switch (estado)
        {
            case GameController.EstadoPersonaje.Escogiendo:
                var middlePoint = GetPositon();
                var finalPoint = GetPositon();
                
                var initialPoint = GetPositon();
                
                for (var i = -areaAtaque; i <= areaAtaque; i++)
                {
                    var extrem = areaAtaque - Math.Abs(i);

                    for (var j = -extrem; j <= extrem; j++)
                    {
                        if (i == 0 && j == 0) continue;

                        var pos = initialPoint + new Vector3(i * 2, 0, j * 2);

                        if (controller.DentroArea(pos) && controller.PosibleAccion(this, pos, tipo != GameController.Tipo.Sanador))
                        {
                            navMeshAgent.SetDestination(middlePoint);
                            otherPoint = finalPoint;
                            estado = GameController.EstadoPersonaje.Moviendo1;
                            return;
                        }
                    }
                }

                var contrario = controller.ContrarioCercano(this);
                var distance = float.PositiveInfinity;
                var contrarioPosition = contrario.GetPositon();

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

                            if (Vector3.Distance(finalAux, contrarioPosition) == 0) continue;

                            var distAux = Math.Abs(contrarioPosition.x - finalAux.x) +
                                          Math.Abs(contrarioPosition.z - finalAux.z);

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
                var pnjPosition = GetPositon();
                
                for (var i = -areaAtaque; i <= areaAtaque; i++)
                {
                    var extrem = areaAtaque - Math.Abs(i);

                    for (var j = -extrem; j <= extrem; j++)
                    {
                        if (i == 0 && j == 0) continue;

                        var pos = pnjPosition + new Vector3(i * 2, 0, j * 2);

                        if (controller.DentroArea(pos) && controller.PosibleAccion(this, pos, tipo != GameController.Tipo.Sanador))
                        {
                            transform.LookAt(pos);
                            estado = GameController.EstadoPersonaje.Atacando;
                            animator.SetTrigger("Aim");
                            return;
                        }
                    }
                }
                
                // Si no puede atacar
                EndAtack();
                break;
        }
    }
}
