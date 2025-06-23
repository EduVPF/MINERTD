using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // A única função deste script é carregar a cena do jogo.
    public void IniciarJogo()
    {
        // Lembre-se de que o nome "Level1" deve ser idêntico ao nome do seu arquivo de cena.
        SceneManager.LoadScene("Level1");
    }

    // A função de sair do jogo.
    public void SairDoJogo()
    {
        Debug.Log("Saindo do jogo...");
        Application.Quit();
    }
}