using UnityEngine;
using System.Threading;

public class RecursoMineravel : MonoBehaviour
{
    [Header("Configurações do Recurso")]
    public float durabilidadeMaxima = 100f;
    public int capacidadeInicialDeMineradores = 2;

    [Header("Visual de Desgaste")]
    public GameObject[] iconesDeDurabilidade;
    
    [HideInInspector] public float durabilidadeAtual;
    public SemaphoreSlim VagasDeMineracao { get; private set; }
    public bool EstaEsgotado { get; private set; }
    private readonly object lockDurabilidade = new object();
    private int capacidadeAtualDaMina;

    void Awake()
    {
        durabilidadeAtual = durabilidadeMaxima;
        EstaEsgotado = false;
        capacidadeAtualDaMina = capacidadeInicialDeMineradores;
        
        // --- MUDANÇA PRINCIPAL AQUI ---
        // Criamos o semáforo com a capacidade inicial, mas com um limite máximo alto (ex: 100)
        VagasDeMineracao = new SemaphoreSlim(capacidadeInicialDeMineradores, 100); 
        
        AtualizarVisual();
    }

    public void AumentarCapacidade()
    {
        // Este método agora funciona, pois o semáforo tem espaço para crescer até o limite de 100.
        VagasDeMineracao.Release();
        capacidadeAtualDaMina++;
        Debug.Log($"Capacidade da mina aumentada! Vagas atuais: {capacidadeAtualDaMina}");
    }

    // O resto do script continua igual
    public void ReceberDanoDeMineracao(float dano)
    {
        lock (lockDurabilidade)
        {
            if (durabilidadeAtual > 0)
            {
                durabilidadeAtual -= dano;
                if (durabilidadeAtual <= 0)
                {
                    durabilidadeAtual = 0;
                    EstaEsgotado = true;
                }
            }
        }
    }
    
    public void AtualizarVisual()
    {
        if (EstaEsgotado && gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
            return;
        }

        if (iconesDeDurabilidade == null || iconesDeDurabilidade.Length == 0) return;
        float porcentagemVida = durabilidadeAtual / durabilidadeMaxima;
        int iconesAtivos = Mathf.CeilToInt(porcentagemVida * iconesDeDurabilidade.Length);
        for (int i = 0; i < iconesDeDurabilidade.Length; i++)
        {
            if (iconesDeDurabilidade[i] != null && iconesDeDurabilidade[i].activeSelf != (i < iconesAtivos))
            {
                iconesDeDurabilidade[i].SetActive(i < iconesAtivos);
            }
        }
    }
}