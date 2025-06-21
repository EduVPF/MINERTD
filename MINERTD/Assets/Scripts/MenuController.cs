using UnityEngine;
using UnityEngine.SceneManagement; // Essencial para gerenciar cenas

public class MenuController : MonoBehaviour
{
    // Esta função será chamada pelo botão "Iniciar Jogo"
    public void IniciarJogo()
    {
        // Carrega a cena do jogo. Certifique-se de que o nome da cena está correto.
        // Substitua "NomeDaCenaDoJogo" pelo nome da sua cena principal do jogo.
        SceneManager.LoadScene("Level1"); 
    }

    // Esta função será chamada pelo botão "Sair"
    public void SairDoJogo()
    {
        // Fecha a aplicação. Funciona na versão compilada (build) do jogo.
        Debug.Log("Saindo do jogo...");
        Application.Quit();
    }
}