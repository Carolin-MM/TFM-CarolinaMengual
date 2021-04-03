public class EnemyController : PnjController
{
    protected override void Update()
    {
        base.Update();

        switch (estado)
        {
            case GameController.EstadoPersonaje.Parado:
                break;
            
            case GameController.EstadoPersonaje.Escogiendo:
                break;
            
            case GameController.EstadoPersonaje.Moviendo1:
                break;
            
            case GameController.EstadoPersonaje.Moviendo2:
                break;
            
            case GameController.EstadoPersonaje.EscogiendoAtaque:
                break;
            
            case GameController.EstadoPersonaje.Atacando:
                break;
        }
    }
}
