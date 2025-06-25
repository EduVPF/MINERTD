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
    private bool jaAvisou = false;

public void ReceberDanoDeMineracao(float dano)
{
    lock (lockDurabilidade)
    {
        if (durabilidadeAtual > 0)
        {
            durabilidadeAtual -= dano;
            // LOG 1: VERIFICAR SE O DANO ESTÁ SENDO RECEBIDO
            Debug.Log($"Mina recebeu dano. Durabilidade agora: {durabilidadeAtual}");

            if (durabilidadeAtual <= 0)
            {
                durabilidadeAtual = 0;
                EstaEsgotado = true;
                // LOG 2: VERIFICAR SE A MINA REALMENTE 'ESGOTA'
                Debug.Log($"!!! MINA '{gameObject.name}' ESGOTADA !!! O estado 'EstaEsgotado' agora é true.");
            }
        }
    }
}
    void Awake()
    {
        durabilidadeAtual = durabilidadeMaxima;
        EstaEsgotado = false;
        jaAvisou = false;
        VagasDeMineracao = new SemaphoreSlim(capacidadeInicialDeMineradores, 100);



    }
    void Start()
    {

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegistrarNovaMina();
        }
        else
        {
            Debug.LogError("ERRO: RecursoMineravel não conseguiu encontrar a instância do GameManager!");
        }
    }
    void Update()
    {
        AtualizarVisual();
    }



    public void AumentarCapacidade()
    {
        VagasDeMineracao.Release();
    }

public void AtualizarVisual()
{
    if (EstaEsgotado && !jaAvisou)
    {
        // LOG 3: VERIFICAR SE A LÓGICA DE NOTIFICAÇÃO É ATIVADA
        Debug.Log($"CONDIÇÃO DE AVISO ATINGIDA para '{gameObject.name}'. Tentando avisar o GameManager...");

        jaAvisou = true;
        if (GameManager.Instance != null)
{
    GameManager.Instance.RegistrarMinaEsgotada();
}
else
{
     Debug.LogError($"ERRO CRÍTICO: Tentou avisar o GameManager, mas a instância Singleton do GameManager não foi encontrada!");
}
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
