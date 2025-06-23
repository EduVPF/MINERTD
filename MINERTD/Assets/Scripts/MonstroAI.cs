using UnityEngine;

public class MonstroAI : MonoBehaviour
{
    [Header("Atributos do Monstro")]
    public float velocidadeMinima = 1.2f; // <-- NOVO: Velocidade mínima
    public float velocidadeMaxima = 2.0f; // <-- NOVO: Velocidade máxima
    public float dano = 10f;
    public float alcanceAtaque = 0.5f;
    public float tempoEntreAtaques = 1.5f;

    private Transform alvo;
    private Saude saudeDoAlvo;
    private float timerAtaque;
    private Animator animator;
    private float velocidade; // A velocidade específica desta instância do monstro

    void Start()
    {
        animator = GetComponent<Animator>();
        // --- NOVO: Define uma velocidade aleatória para este monstro ---
        velocidade = Random.Range(velocidadeMinima, velocidadeMaxima);
        
        // Inicia o timer "cheio" para que ele possa atacar assim que chegar perto
        timerAtaque = tempoEntreAtaques; 
        
        // Já começa procurando um alvo
        EncontrarNovoAlvo();
    }

    void Update()
    {
        // Se não tem um alvo ou o alvo foi destruído, procura por um novo
        if (alvo == null || !alvo.gameObject.activeInHierarchy)
        {
            EncontrarNovoAlvo();
        }

        // Se, depois de procurar, ainda não tem um alvo, fica parado.
        if (alvo == null)
        {
            animator.SetInteger("Estado", 0); // Animação de Parado
            return;
        }

        // Se tem um alvo, executa a lógica de perseguição e ataque
        float distanciaParaAlvo = Vector2.Distance(transform.position, alvo.position);

        if (distanciaParaAlvo > alcanceAtaque)
        {
            // Perseguindo
            transform.position = Vector2.MoveTowards(transform.position, alvo.position, velocidade * Time.deltaTime);
            GetComponent<SpriteRenderer>().flipX = (alvo.position.x > transform.position.x);
            animator.SetInteger("Estado", 1); // Animação de andando
        }
        else 
        {
            // No alcance, para de andar e prepara para atacar
            animator.SetInteger("Estado", 0); // Animação de parado

            timerAtaque += Time.deltaTime;
            if (timerAtaque >= tempoEntreAtaques)
            {
                timerAtaque = 0f;
                Atacar(); 
            }
        }
    }

    // --- MÉTODO ATUALIZADO PARA ENCONTRAR O ALVO MAIS PRÓXIMO ---
    void EncontrarNovoAlvo()
    {
        // Pega uma lista de TODOS os mineradores na cena
        SeguirCaminho[] todosOsMineradores = FindObjectsByType<SeguirCaminho>(FindObjectsSortMode.None);

        // Se não há mineradores na cena, o alvo é nulo
        if (todosOsMineradores.Length == 0)
        {
            alvo = null;
            return;
        }

        Transform alvoMaisProximo = null;
        float menorDistancia = Mathf.Infinity; // Começa com uma distância "infinita"

        // Passa por cada minerador da lista
        foreach (SeguirCaminho minerador in todosOsMineradores)
        {
            // Calcula a distância deste monstro até o minerador atual
            float distancia = Vector2.Distance(transform.position, minerador.transform.position);

            // Se a distância para este minerador for menor que a menor distância encontrada até agora...
            if (distancia < menorDistancia)
            {
                // ...este minerador se torna o novo "mais próximo".
                menorDistancia = distancia;
                alvoMaisProximo = minerador.transform;
            }
        }

        // Define o alvo final como o mais próximo que foi encontrado
        alvo = alvoMaisProximo;
        if (alvo != null)
        {
            saudeDoAlvo = alvo.GetComponent<Saude>();
        }
    }

    void Atacar()
    {
        if (alvo == null) return;
        
        animator.SetTrigger("Atacar"); 

        if (saudeDoAlvo != null)
        {
            saudeDoAlvo.ReceberDano(dano);
        }
    }
}