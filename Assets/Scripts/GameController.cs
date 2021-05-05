using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject cuadricula;
    public Transform areaParent;
    public GameObject prefabCuadroArea, prefabCuadroAreaAtaque, prefabCuadroAreaSanar;
    public PnjController[] aliados, enemigos;
    public MenuController menuController;
    public Transform posicionesJugador;
    public Distribucion distribucionNivel;
    public float multiplicadorTipoFavor = 1.5f, multiplicadorTipoContra = 0.5f;

    private Vector3[] _verticesCuadricula;
    private int _numAliados, _numEnemigos;
    private List<PnjController> _todos;
    private int _indexTurno = -1;

    public enum EstadoPersonaje
    {
        Parado,
        EscogiendoDestino,
        Moviendo1,
        Moviendo2,
        EscogiendoAccion,
        Atacando,
        Muerto
    };

    public enum Tipo
    {
        Jefe,
        Sanador,
        Distancia,
        Defensa,
        Velocista
    };
    
    [Serializable]
    public struct Distribucion
    {
        public int sanadores;
        public int distancia;
        public int defensa;
        public int velocistas;
    }

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

        var lineRenderer = GetComponent<LineRenderer>();
        var index = 0;
        var aux = new List<PnjController>();
        var prefab = Resources.Load<GameObject>("Jugador/Lider");
        
        // Instanciar líder
        var child = posicionesJugador.GetChild(index++);
        var ins = Instantiate(prefab, child.position, child.rotation);
        var script = ins.GetComponent<PlayerController>();
        script.controller = this;
        script.lineRenderer = lineRenderer;
        aux.Add(script);

        // Instanciar sanadores
        prefab = Resources.Load<GameObject>("Jugador/Sanador");
        for (var i = 0; i < distribucionNivel.sanadores; i++)
        {
            child = posicionesJugador.GetChild(index++);
            ins = Instantiate(prefab, child.position, child.rotation);
            script = ins.GetComponent<PlayerController>();
            script.controller = this;
            script.lineRenderer = lineRenderer;
            aux.Add(script);
        }

        // Instanciar a distancia
        prefab = Resources.Load<GameObject>("Jugador/Distancia");
        for (var i = 0; i < distribucionNivel.distancia; i++)
        {
            child = posicionesJugador.GetChild(index++);
            ins = Instantiate(prefab, child.position, child.rotation);
            script = ins.GetComponent<PlayerController>();
            script.controller = this;
            script.lineRenderer = lineRenderer;
            aux.Add(script);
        }

        // Instanciar defensas
        prefab = Resources.Load<GameObject>("Jugador/Defensa");
        for (var i = 0; i < distribucionNivel.defensa; i++)
        {
            child = posicionesJugador.GetChild(index++);
            ins = Instantiate(prefab, child.position, child.rotation);
            script = ins.GetComponent<PlayerController>();
            script.controller = this;
            script.lineRenderer = lineRenderer;
            aux.Add(script);
        }

        // Instanciar velocistas
        prefab = Resources.Load<GameObject>("Jugador/Velocista");
        for (var i = 0; i < distribucionNivel.velocistas; i++)
        {
            child = posicionesJugador.GetChild(index++);
            ins = Instantiate(prefab, child.position, child.rotation);
            script = ins.GetComponent<PlayerController>();
            script.controller = this;
            script.lineRenderer = lineRenderer;
            aux.Add(script);
        }

        aliados = aux.ToArray();
        InicioNivel();
    }

    private void InicioNivel()
    {
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

    private void MarcarAreaMovimiento(int area, PnjController controller)
    {
        var pnjPosition = controller.GetPositon();
        pnjPosition.y = 0.25f;

        for (var i = -area; i <= area; i++)
        {
            var extrem = area - Math.Abs(i);

            for (var j = -extrem; j <= extrem; j++)
            {
                var pos = pnjPosition + new Vector3(i * 2, 0, j * 2);

                if (DentroArea(pos) && PosibleMovimiento(controller, pos))
                    Instantiate(prefabCuadroArea, pos, Quaternion.identity, areaParent);
            }
        }
    }

    public bool PosibleMovimiento(PnjController controller, Vector3 position)
    {
        return _todos.Where(pnj => pnj.estado != EstadoPersonaje.Muerto && pnj != controller)
            .All(pnj => pnj.GetPositon() != position);
    }

    public void LimpiarArea()
    {
        foreach (Transform child in areaParent) Destroy(child.gameObject);
    }

    public void MarcarAreaAccion(int area, int inicioArea, Vector3 pnjPosition, bool ataque)
    {
        pnjPosition.y = 0.25f;
        var prefabCuadro = ataque ? prefabCuadroAreaAtaque : prefabCuadroAreaSanar;

        for (var i = -area; i <= area; i++)
        {
            var absI = Math.Abs(i);
            var extrem = area - absI;

            for (var j = -extrem; j <= extrem; j++)
            {
                var absJ = Math.Abs(j);
                if ((absI == 0 && absJ == 0) || (absI <= inicioArea && absJ <= inicioArea && absI != absJ)) continue;

                var pos = pnjPosition + new Vector3(i * 2, 0, j * 2);

                if (DentroArea(pos)) Instantiate(prefabCuadro, pos, Quaternion.identity, areaParent);
            }
        }
    }

    public bool PosibleAccion(PnjController controller, Vector3 position, bool ataque)
    {
        var entities = controller is PlayerController ? ataque ? enemigos : aliados : ataque ? aliados : enemigos;

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
                MarcarAreaMovimiento(pnj.areaMovimiento, pnj);
            
            pnj.estado = EstadoPersonaje.EscogiendoDestino;
            siguiente = true;
        } while (!siguiente);
    }

    public PnjController ObjetivoCercano(PnjController controller)
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

    public PnjController ObjetivoCuracion(PnjController controller)
    {
        var entities = controller is PlayerController ? aliados : enemigos;
        var vida = float.PositiveInfinity;
        PnjController aux = null;

        foreach (var pnj in entities)
        {
            if (pnj.estado == EstadoPersonaje.Muerto || pnj == controller) continue;

            if (pnj.vidaActual > vida) continue;
            vida = pnj.vidaActual;
            aux = pnj;
        }

        return aux;
    }

    public float GetMultiplicador(Tipo tipoAtacante, Tipo tipoObjetivo)
    {
        switch (tipoAtacante)
        {
            case Tipo.Jefe:
                return multiplicadorTipoFavor;
            
            case Tipo.Distancia:
                if (tipoObjetivo == Tipo.Defensa) 
                    return multiplicadorTipoFavor;
                else if (tipoObjetivo == Tipo.Velocista) 
                    return multiplicadorTipoContra;
                break;
            
            case Tipo.Defensa:
                if (tipoObjetivo == Tipo.Velocista) 
                    return multiplicadorTipoFavor;
                else if (tipoObjetivo == Tipo.Distancia) 
                    return multiplicadorTipoContra;
                break;
            
            case Tipo.Velocista:
                if (tipoObjetivo == Tipo.Distancia) 
                    return multiplicadorTipoFavor;
                else if (tipoObjetivo == Tipo.Defensa) 
                    return multiplicadorTipoContra;
                break;
            
        }
        
        return 1;
    }
}
