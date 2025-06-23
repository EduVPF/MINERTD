using UnityEngine;

public class Saude : MonoBehaviour
{
    public float vidaMaxima = 100f;
    public float tempoAnimacaoMorte = 1f; 
    
    private float vidaAtual;
    private Animator animator;
    private bool estaMorto = false;
    private GameManager gameManager; // Referência para o GameManager

    void Awake()
    {
        if (vidaMaxima <= 0)
        {
            vidaMaxima = 100f;
        }
        vidaAtual = vidaMaxima;
        animator = GetComponent<Animator>();
        // Encontra o GameManager na cena para poder se comunicar com ele
        gameManager = FindFirstObjectByType<GameManager>();
    }

    public void ReceberDano(float dano)
    {
        if (estaMorto) return; 

        vidaAtual -= dano;
        
        if (vidaAtual > vidaMaxima) vidaAtual = vidaMaxima;
        
        // Debug.Log(gameObject.name + " recebeu " + dano + " de dano. Vida restante: " + vidaAtual);

        if (vidaAtual <= 0)
        {
            vidaAtual = 0;
            Morrer();
        }
        else if (dano > 0 && animator != null)
        {
            animator.SetTrigger("LevouDano");
        }
    }

    void Morrer()
    {
        if (estaMorto) return;
        estaMorto = true;
        
        // Se este objeto é um minerador, avisa o GameManager que uma vaga foi liberada
        if (GetComponent<SeguirCaminho>() != null)
        {
            if (gameManager != null)
            {
                gameManager.RegistrarMorteDeMinerador();
            }
        }
        
        if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;
        
        if (animator != null) animator.SetTrigger("Morreu");
        
        Destroy(gameObject, tempoAnimacaoMorte);
    }
}