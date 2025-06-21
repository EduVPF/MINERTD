using UnityEngine;
using System.Collections;

public class SeguirCaminho : MonoBehaviour
{
    private enum EstadoMinerador
    {
        IndoParaMina,
        Minerando,
        VoltandoParaBase,
        DepositandoRecursos,
        Ocioso
    }

    private Transform localDeMineracao;
    private Transform localDaBase;

    [Header("Configurações")]
    public float velocidade = 2f;
    public float tempoDeMineracao = 3f;

    [Header("Sprites Especiais (Opcional)")]
    public Sprite spriteMinerando;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private EstadoMinerador estadoAtual;
    private Transform alvoAtual;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        if (animator == null || spriteRenderer == null)
        {
            Debug.LogError("ERRO: Componente Animator ou SpriteRenderer não encontrado!");
            enabled = false;
        }
        estadoAtual = EstadoMinerador.Ocioso;
    }

    public void Inicializar(Transform mina, Transform baseTransform)
    {
        this.localDeMineracao = mina;
        this.localDaBase = baseTransform;

        if (localDeMineracao == null || localDaBase == null)
        {
            Debug.LogError("ERRO: O GameManager não passou um Local de Mineração ou Base válido!");
            enabled = false;
            return;
        }

        MudarEstado(EstadoMinerador.IndoParaMina);
    }

    void Update()
    {
        float velocidadeAtual = 0f;
        if (alvoAtual != null && Vector2.Distance(transform.position, alvoAtual.position) > 0.01f)
        {
            velocidadeAtual = velocidade;
        }
        
        animator.SetFloat("Velocidade", velocidadeAtual);

        if (estadoAtual == EstadoMinerador.Minerando || estadoAtual == EstadoMinerador.DepositandoRecursos) return;

        if (alvoAtual != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, alvoAtual.position, velocidade * Time.deltaTime);

            if (Vector2.Distance(transform.position, alvoAtual.position) < 0.1f)
            {
                if (estadoAtual == EstadoMinerador.IndoParaMina)
                {
                    MudarEstado(EstadoMinerador.Minerando);
                }
                else if (estadoAtual == EstadoMinerador.VoltandoParaBase)
                {
                    MudarEstado(EstadoMinerador.DepositandoRecursos);
                }
            }
        }
    }

    void MudarEstado(EstadoMinerador novoEstado)
    {
        estadoAtual = novoEstado;

        switch (estadoAtual)
        {
            case EstadoMinerador.IndoParaMina:
                alvoAtual = localDeMineracao;
                // --- ADICIONADO: Inverte o sprite com base na posição do alvo ---
                spriteRenderer.flipX = (alvoAtual.position.x < transform.position.x);
                break;

            case EstadoMinerador.Minerando:
                alvoAtual = null;
                GetComponent<SpriteRenderer>().sprite = spriteMinerando;
                StartCoroutine(ExecutarMineracao());
                break;

            case EstadoMinerador.VoltandoParaBase:
                alvoAtual = localDaBase;
                // --- ADICIONADO: Inverte o sprite com base na posição do alvo ---
                spriteRenderer.flipX = (alvoAtual.position.x < transform.position.x);
                break;

            case EstadoMinerador.DepositandoRecursos:
                alvoAtual = null;
                StartCoroutine(IniciarNovoCiclo());
                break;
                
            case EstadoMinerador.Ocioso:
                 alvoAtual = null;
                 break;
        }
    }

    IEnumerator ExecutarMineracao()
    {
        yield return new WaitForSeconds(tempoDeMineracao);
        MudarEstado(EstadoMinerador.VoltandoParaBase);
    }

    IEnumerator IniciarNovoCiclo()
    {
        // Ao voltar para a base, o Animator deve ir para o estado Idle
        // A velocidade já será 0, então a transição deve ocorrer naturalmente.
        yield return new WaitForSeconds(1f);
        MudarEstado(EstadoMinerador.IndoParaMina);
    }
}