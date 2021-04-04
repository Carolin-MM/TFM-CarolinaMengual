using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject cuadricula;
    public Transform areaParent;
    public GameObject prefabCuadroArea, prefabCuadroAreaAtaque;
    public PnjController[] aliados, enemigos;
    public MenuController menuController;

    private Vector3[] _verticesCuadricula;
    private int _numAliados, _numEnemigos;
    private List<PnjController> _todos;
    private int _indexTurno = -1;

    public enum EstadoPersonaje
    {
        Parado,
        Escogiendo,
        Moviendo1,
        Moviendo2,
        EscogiendoAtaque,
        Atacando,
        Muerto
    };

    public enum Tipo
    {
        Jefe,
        Medico,
        Distancia,
        Defensa,
        Velocista
    };

    private void Start()
    {
        var verticeList = cuadricula.GetComponent<MeshFilter>().sharedMesh.vertices;
        var trans = cuadricula.transform;

        _verticesCuadricula = new[]
        {
            trans.TransformPoint(verticeList[0]),
            trans.TransformPoint(verticeList[10]),
            trans.TransformPoint(verticeList[110]),
            trans.TransformPoint(verticeList[120])
        };

        _numAliados = aliados.Length;
        _numEnemigos = enemigos.Length;

        _todos = new List<PnjController>();
        _todos.AddRange(aliados);
        _todos.AddRange(enemigos);

        SiguienteTurno();
    }

    public float CalculateCoord(float realCoord, char axis)
    {
        const int factor = 2;

        var coord = realCoord / factor;
        coord = (float) Math.Round(coord);
        coord *= factor;

        switch (axis)
        {
            case 'x':
                if (coord > _verticesCuadricula[0].x)
                    coord = _verticesCuadricula[0].x - 1;

                else if (coord < _verticesCuadricula[1].x)
                    coord = _verticesCuadricula[1].x + 1;

                break;

            case 'z':
                if (coord > _verticesCuadricula[0].z)
                    coord = _verticesCuadricula[0].z - 1;

                else if (coord < _verticesCuadricula[2].z)
                    coord = _verticesCuadricula[2].z + 1;

                break;
        }

        return coord;
    }

    public bool DentroArea(Vector3 position)
    {
        return position.x < _verticesCuadricula[0].x && position.x > _verticesCuadricula[1].x &&
               position.z < _verticesCuadricula[0].z && position.z > _verticesCuadricula[2].z;
    }

    private void MarcarAreaMovimiento(int area, Vector3 pnjPosition)
    {
        pnjPosition.y = 0.25f;

        for (var i = -area; i <= area; i++)
        {
            var extrem = area - Math.Abs(i);

            for (var j = -extrem; j <= extrem; j++)
            {
                var pos = pnjPosition + new Vector3(i * 2, 0, j * 2);

                if (DentroArea(pos)) Instantiate(prefabCuadroArea, pos, Quaternion.identity, areaParent);
            }
        }
    }

    public void LimpiarArea()
    {
        foreach (Transform child in areaParent) Destroy(child.gameObject);
    }

    public void MarcarAreaAtaque(int area, int inicioArea, Vector3 pnjPosition)
    {
        pnjPosition.y = 0.25f;

        for (var i = -area; i <= area; i++)
        {
            var extrem = area - Math.Abs(i);

            for (var j = -extrem; j <= extrem; j++)
            {
                if (i == 0 && j == 0) continue;

                var pos = pnjPosition + new Vector3(i * 2, 0, j * 2);

                if (DentroArea(pos)) Instantiate(prefabCuadroAreaAtaque, pos, Quaternion.identity, areaParent);
            }
        }
    }

    public bool PosibleAtaque(PnjController controller, Vector3 position)
    {
        var entities = controller is PlayerController ? enemigos : aliados;

        foreach (var pnj in entities)
        {
            if (pnj.estado == EstadoPersonaje.Muerto || pnj.GetPositon() != position) continue;

            controller.siguienteAtaque = pnj;
            return true;
        }

        return false;
    }

    public void Muerte(PnjController pnjController)
    {
        if (pnjController is PlayerController) _numAliados--;
        else _numEnemigos--;

        if (_numAliados == 0) menuController.FinalPartida("Enemigo");
        else if (_numEnemigos == 0) menuController.FinalPartida("Jugador");
    }

    public void SiguienteTurno()
    {
        var siguiente = false;

        do
        {
            _indexTurno++;
            _indexTurno %= _todos.Count;

            var pnj = _todos[_indexTurno];
            if (pnj.estado == EstadoPersonaje.Muerto) continue;

            if (pnj is PlayerController)
                MarcarAreaMovimiento(pnj.areaMovimiento, pnj.GetPositon());
            
            pnj.estado = EstadoPersonaje.Escogiendo;
            siguiente = true;
        } while (!siguiente);
    }

    public PnjController ContrarioCercano(PnjController controller)
    {
        var entities = controller is PlayerController ? enemigos : aliados;
        var position = controller.GetPositon();
        var distance = float.PositiveInfinity;
        PnjController aux = null;

        foreach (var pnj in entities)
        {
            if (pnj.estado == EstadoPersonaje.Muerto) continue;

            var dis = Vector3.Distance(pnj.GetPositon(), position);
            if (dis > distance) continue;
            distance = dis;
            aux = pnj;
        }

        return aux;
    }
}
