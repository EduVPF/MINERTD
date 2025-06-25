using UnityEngine;

public class MonstroAI : MonoBehaviour
{
    [Header("Atributos (Velocidade Aleatória)")]
    public float velocidadeMinima = 1.2f;
    public float velocidadeMaxima = 2.0f;
    
    [Header("Atributos de Combate")]
    public float dano = 10f;
    public float multiplicadorDanoContraTorres = 2f; 
    public float alcanceAtaque = 1.0f;
    public float tempoEntreAtaques = 1.5f;

    // --- Variáveis Internas ---
    private Transform alvo;
    private Saude saudeDoAlvo;
    private float timerAtaque;
    private Animator animator;
    private float velocidadeInstancia;

    void Start()
    {
        animator = GetComponent<Animator>();
        velocidadeInstancia = Random.Range(velocidadeMinima, velocidadeMaxima);
        timerAtaque = tempoEntreAtaques; 
        EncontrarNovoAlvo();
    }

    void Update()
    {
        if (alvo == null || !alvo.gameObject.activeInHierarchy) { EncontrarNovoAlvo(); }
        if (alvo == null) { if(animator != null) animator.SetInteger("Estado", 0); return; }

        float distanciaParaAlvo = Vector2.Distance(transform.position, alvo.position);
        if (distanciaParaAlvo > alcanceAtaque)
        {
            transform.position = Vector2.MoveTowards(transform.position, alvo.position, velocidadeInstancia * Time.deltaTime);
            GetComponent<SpriteRenderer>().flipX = (alvo.position.x > transform.position.x);
            if(animator != null) animator.SetInteger("Estado", 1);
        }
        else 
        {
            if(animator != null) animator.SetInteger("Estado", 0);
            timerAtaque += Time.deltaTime;
            if (timerAtaque >= tempoEntreAtaques)
            {
                timerAtaque = 0f;
                Atacar(); 
            }
        }
    }

    void EncontrarNovoAlvo()
    {
        Saude[] todosOsAlvosComVida = FindObjectsByType<Saude>(FindObjectsSortMode.None);
        Transform alvoMaisProximo = null;
        float menorDistancia = Mathf.Infinity;
        foreach (Saude saudeAlvo in todosOsAlvosComVida)
        {
            if (saudeAlvo.transform == this.transform) continue;
            if (saudeAlvo.GetComponent<SeguirCaminho>() != null || saudeAlvo.GetComponent<TorreIA>() != null)
            {
                float distancia = Vector2.Distance(transform.position, saudeAlvo.transform.position);
                if (distancia < menorDistancia)
                {
                    menorDistancia = distancia;
                    alvoMaisProximo = saudeAlvo.transform;
                }
            }
        }
        alvo = alvoMaisProximo;
        if (alvo != null) { saudeDoAlvo = alvo.GetComponent<Saude>(); }
    }

    void Atacar()
    {
        if (alvo == null || saudeDoAlvo == null) return;
        
        if(animator != null) animator.SetTrigger("Atacar");


        float danoFinal = this.dano;

        if (alvo.GetComponent<TorreIA>() != null)
        {
            danoFinal *= multiplicadorDanoContraTorres;
            Debug.Log("Dano bônus contra torre! Dano total: " + danoFinal);
        }
        
        saudeDoAlvo.ReceberDano(danoFinal);
    }
}