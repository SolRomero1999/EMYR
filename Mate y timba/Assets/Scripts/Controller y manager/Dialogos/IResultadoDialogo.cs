public interface IResultadoDialogo
{
    bool TieneDialogoVictoria();
    bool TieneDialogoDerrota();

    void MostrarDialogoVictoria(System.Action alFinalizar);
    void MostrarDialogoDerrota(System.Action alFinalizar);
}
