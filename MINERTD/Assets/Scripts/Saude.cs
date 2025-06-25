using UnityEngine;

public class Saude : MonoBehaviour
{
    public float vidaMaxima = 100f;
    public float tempoAnimacaoMorte = 1f; 
    
    private float vidaAtual;
    private Animator animator;
    private bool estaMorto = false;

    void Awake()
    {
        if (vidaMaxima <= 0) vidaMaxima = 100f;
        vidaAtual = vidaMaxima;
        animator = GetComponent<Animator>();
    }

    public void ReceberDano(float dano)
    {
        if (estaMorto) return;

        vidaAtual -= dano;
        if (vidaAtual > vidaMaxima) vidaAtual = vidaMaxima;
        
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
    Debug.Log(gameObject.name + " morreu!");


    if (GetComponent<SeguirCaminho>() != null)
    {
        GameManager.Instance.RegistrarMorteDeMinerador();
    }
    

    if (GetComponent<MonstroAI>() != null)
    {
        GameManager.Instance.AdicionarPontosPorMorteDeMonstro();
    }

        TorreIA torreScript = GetComponent<TorreIA>();
        if (torreScript != null)
        {
            torreScript.enabled = false;
        }

        MonstroAI monstroScript = GetComponent<MonstroAI>();
        if (monstroScript != null)
        {
            monstroScript.enabled = false;
        }


        if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;
        if (animator != null) animator.SetTrigger("Morreu");
        
        Destroy(gameObject, tempoAnimacaoMorte);
    }
}