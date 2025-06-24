using UnityEngine;

public class DestruidorIA : MonoBehaviour
{
    [Header("Atributos")]
    public float velocidade = 2f;
    public float dano = 40f;
    public float alcanceAtaque = 1f;
    public float tempoEntreAtaques = 2f;

    private Transform alvo;
    private Saude saudeDoAlvo;
    private Animator animator;
    private float timerAtaque;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        GameManager.OnTorreConstruida += DefinirAlvoDaTorre;
    }

    private void OnDisable()
    {
        GameManager.OnTorreConstruida -= DefinirAlvoDaTorre;
    }

    void Update()
    {
        // Se o alvo foi destruído, limpa a referência e volta a observar.
        if (alvo != null && !alvo.gameObject.activeInHierarchy)
        {
            alvo = null;
        }

        if (alvo == null)
        {
            // Se não tem alvo, garante que o Animator saiba disso para voltar ao estado Wall/Observando
            animator.SetBool("TemAlvo", false);
            return;
        }

        // Se tem um alvo, persegue e ataca
        if (Vector2.Distance(transform.position, alvo.position) > alcanceAtaque)
        {
            // Perseguindo (O Animator já está no estado de andar porque TemAlvo é true)
            transform.position = Vector2.MoveTowards(transform.position, alvo.position, velocidade * Time.deltaTime);
            GetComponent<SpriteRenderer>().flipX = (alvo.position.x > transform.position.x);
        }
        else
        {
            // No alcance, para de andar (a animação de ataque vai sobrepor) e ataca
            timerAtaque += Time.deltaTime;
            if (timerAtaque >= tempoEntreAtaques)
            {
                timerAtaque = 0f;
                Atacar();
            }
        }
    }

    private void DefinirAlvoDaTorre(TorreIA torreAlvo)
    {
        if (alvo == null && torreAlvo != null)
        {
            alvo = torreAlvo.transform;
            saudeDoAlvo = torreAlvo.GetComponent<Saude>();
            
            // Avisa o Animator que agora temos um alvo, ativando a transição para andar
            animator.SetBool("TemAlvo", true);
        }
    }

    private void Atacar()
    {
        // Dispara o gatilho que toca a animação de ataque UMA VEZ
        animator.SetTrigger("Atacar");

        if (saudeDoAlvo != null)
        {
            saudeDoAlvo.ReceberDano(dano);
        }
    }
}