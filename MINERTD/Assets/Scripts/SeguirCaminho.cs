using UnityEngine;
using System.Threading;
using System;
using System.Collections.Generic;

public class SeguirCaminho : MonoBehaviour
{
    private enum EstadoMinerador { Ocioso_Parado, Andando, Minerando, Depositando }

    [Header("Configurações do Minerador")]
    public float velocidade = 2f;
    public int capacidadeDaBolsa = 15;
    public int danoPorHit = 5;
    public int mineriosPorHit = 5;
    public int tempoPorHitMs = 1000;

    private Transform localDeMineracao;
    private Transform localDaBase;
    private RecursoMineravel recursoAlvo;
    private Animator animator;
    private GameManager gameManager;
    private EstadoMinerador estadoAtual;
    private Transform alvoAtual;
    private int mineriosNaBolsa = 0;
    private readonly Queue<Action> acoesThreadPrincipal = new Queue<Action>();

    void Awake()
    {
        animator = GetComponent<Animator>();
        gameManager = FindFirstObjectByType<GameManager>(); 
        if (animator == null || gameManager == null)
        {
            Debug.LogError("ERRO: Componente Animator ou GameManager não foi encontrado na cena!");
            enabled = false;
        }
        estadoAtual = EstadoMinerador.Ocioso_Parado;
    }

    public void AplicarUpgrades(float bonusVelocidade, float bonusVida)
    {
        this.velocidade += bonusVelocidade;
        Saude saudeDoMinerador = GetComponent<Saude>();
        if (saudeDoMinerador != null)
        {
            Debug.Log($"Aplicando upgrade de vida em {gameObject.name}. Vida Máxima ANTES: {saudeDoMinerador.vidaMaxima}");
            saudeDoMinerador.vidaMaxima += bonusVida;
            Debug.Log($"Vida Máxima DEPOIS: {saudeDoMinerador.vidaMaxima}");
            saudeDoMinerador.ReceberDano(-bonusVida);
        }
    }

    public void Inicializar(Transform mina, Transform baseTransform)
    {
        this.localDeMineracao = mina;
        this.localDaBase = baseTransform;
        this.recursoAlvo = mina.GetComponent<RecursoMineravel>();
        alvoAtual = localDeMineracao;
        MudarEstado(EstadoMinerador.Andando);
    }

    void Update()
    {
        lock (acoesThreadPrincipal)
        {
            while (acoesThreadPrincipal.Count > 0) { acoesThreadPrincipal.Dequeue().Invoke(); }
        }
        if (alvoAtual != null)
        {
            if (Vector2.Distance(transform.position, alvoAtual.position) < 0.1f)
            {
                transform.position = alvoAtual.position;
                EstadoMinerador proximoEstado = EstadoMinerador.Ocioso_Parado;
                if(estadoAtual == EstadoMinerador.Andando)
                {
                    if(transform.position == localDeMineracao.position) { proximoEstado = EstadoMinerador.Minerando; }
                    else if(transform.position == localDaBase.position) { proximoEstado = EstadoMinerador.Depositando; }
                }
                alvoAtual = null;
                MudarEstado(proximoEstado);
            }
            else
            {
                transform.position = Vector2.MoveTowards(transform.position, alvoAtual.position, velocidade * Time.deltaTime);
                GetComponent<SpriteRenderer>().flipX = (alvoAtual.position.x < transform.position.x);
            }
        }
        if(estadoAtual == EstadoMinerador.Minerando && recursoAlvo != null)
        {
            recursoAlvo.AtualizarVisual();
        }
    }

    void MudarEstado(EstadoMinerador novoEstado)
    {
        Debug.Log($"MUDANDO ESTADO de '{estadoAtual}' para '{novoEstado}' no frame: {Time.frameCount}");
        estadoAtual = novoEstado;
        int estadoAnimacao = 0;
        if (novoEstado == EstadoMinerador.Andando) estadoAnimacao = 1;
        if (novoEstado == EstadoMinerador.Minerando) estadoAnimacao = 2;
        Debug.Log($"Enviando para o Animator: SetInteger('Estado', {estadoAnimacao})");
        animator.SetInteger("Estado", estadoAnimacao);
        switch (estadoAtual)
        {
            case EstadoMinerador.Minerando:
                Thread threadDeMineracao = new Thread(LoopDeMineracao);
                threadDeMineracao.Start();
                break;
            case EstadoMinerador.Depositando:
                if (mineriosNaBolsa > 0) { gameManager.DepositarMoedas(mineriosNaBolsa); mineriosNaBolsa = 0; }
                Invoke("IniciarNovoCiclo", 1f);
                break;
        }
    }

    private void LoopDeMineracao()
    {
        if (recursoAlvo == null || recursoAlvo.EstaEsgotado)
        {
            EnfileirarAcao(() => { alvoAtual = localDaBase; MudarEstado(EstadoMinerador.Andando); });
            return;
        }
        try
        {
            recursoAlvo.VagasDeMineracao.Wait();
            while (mineriosNaBolsa < capacidadeDaBolsa && !recursoAlvo.EstaEsgotado)
            {
                Debug.Log("THREAD: Dando um 'hit' no minério e esperando " + tempoPorHitMs + "ms.");
                Thread.Sleep(tempoPorHitMs);
                if (!recursoAlvo.EstaEsgotado) { recursoAlvo.ReceberDanoDeMineracao(danoPorHit); mineriosNaBolsa += mineriosPorHit; }
            }
        }
        finally
        {
            recursoAlvo.VagasDeMineracao.Release();
        }
        Debug.Log("THREAD: Mineração concluída. Enfileirando ordem para voltar.");
        EnfileirarAcao(() => { alvoAtual = localDaBase; MudarEstado(EstadoMinerador.Andando); });
    }
    
    private void EnfileirarAcao(Action acao) { lock (acoesThreadPrincipal) { acoesThreadPrincipal.Enqueue(acao); } }
    private void IniciarNovoCiclo() { alvoAtual = localDeMineracao; MudarEstado(EstadoMinerador.Andando); }
}