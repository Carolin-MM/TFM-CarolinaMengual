using System;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject cuadricula;
    public Transform areaParent;
    public GameObject prefabCuadroArea, prefabCuadroAreaAtaque;

    private Vector3[] _verticesCuadricula;

    public enum EstadoPersonaje
    {
        Parado,
        Escogiendo,
        Moviendo1,
        Moviendo2,
        EscogiendoAtaque,
        Atacando
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

    private bool DentroArea(Vector3 positio)
    {
        return positio.x < _verticesCuadricula[0].x && positio.x > _verticesCuadricula[1].x &&
               positio.z < _verticesCuadricula[0].z && positio.z > _verticesCuadricula[2].z;
    }

    public void MarcarAreaMovimiento(int area, Vector3 pnjPosition)
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
}
