using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinalBiome.Sdk;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const string GameObjectName = "GameManager";
    private static readonly SemaphoreSlim Lock = new(1,1);
    private static readonly SemaphoreSlim LockClient = new(1,1);
    private static GameManager _instance;
    private FinalBiomeManager fbManager;

    private Client client;

    /// <summary>
    /// Address of the game
    /// </summary>
    public string GameAddress = "5HGjWAeFDfFCWPsjFQdVV2Msvz2XtMktvgocEZcCj68kUMaw";
    /// <summary>
    /// Endpoint of FinalBiome Network
    /// </summary>
    public string Endpoint = "ws://127.0.0.1:9944";

    public bool IsLoggedIn = false;

    internal string balances;
    internal string ownedNfaInstancesText;

    /// <summary>
    /// Returns Game Manager instance. If it not exists, creates new one.
    /// </summary>
    /// <returns></returns>
    public static async Task<GameManager> GetInstance()
    {
        await Lock.WaitAsync();
        try
        {
            Debug.Log("Get GM");
            if (_instance != null) return _instance;
            Debug.Log("Init GM");

            var go = GameObject.Find(GameObjectName);
            if (go == null)
            {
                go = new GameObject(GameObjectName);
            }

            if (go.GetComponent<GameManager>() == null)
            {
                go.AddComponent<GameManager>();
            }
            DontDestroyOnLoad(go);
            _instance = go.GetComponent<GameManager>();

            return _instance;
        }
        finally
        {
            Lock.Release();
        }
    }

    /// <summary>
    /// Returns FinabBiome Client. If it not exists, create and initialized new one.
    /// </summary>
    /// <returns></returns>
    public async Task<Client> GetClient()
    {
        await LockClient.WaitAsync();
        try
        {
            Debug.Log("Get Cli");
            if (client is null)
            {
                Debug.Log("Init Cli");

                // get instance of the FinalBiome manager
                ClientConfig config = new(GameAddress, Endpoint);
                
                fbManager = await FinalBiomeManager.GetInstance();
                await FinalBiomeManager.Initialize(config);

                client = fbManager.Client;

                // listen user state changes
                client.Auth.StateChanged += UserStateChangedHandler;
                // listen Fa balances changes
                client.Fa.FaBalanceChanged += FaBalanceChangedHandler;
                // listen nfa changes
                client.Nfa.NfaInstanceChanged += NfaInstanceChangedHandler;
            
            }
            return client;
        }
        finally
        {
            LockClient.Release();
        }
    }

  private void NfaInstanceChangedHandler(object sender, NfaInstanceChangedEventArgs e)
  {
        // refresh an interface if nfas has been changed
        NfaBalancesToText();
  }

  private void FaBalanceChangedHandler(object sender, FaBalanceChangedEventArgs e)
  {
        FaBalancesToText();
  }

  // Start is called before the first frame update
  internal async void Start()
    {
        client = await GetClient();
    }

    private async Task UserStateChangedHandler(bool loggedIn)
    {
        IsLoggedIn = loggedIn;

        if (!loggedIn)
        {
            balances = "";
            ownedNfaInstancesText = "";
        }
        await Task.Yield();
    }

    /// <summary>
    /// Makes string with a list of FAs
    /// </summary>
    void FaBalancesToText()
    {
        // because we need show all fa balances at once, we don't use event data,
        // and collect all data from sdk
        List<string> text = new();
        foreach (var (id, balance) in client.Fa.Balances)
        {
            text.Add($" ☼ Fa Id: {id} - {balance}");
        }

        var s = string.Join("\n", text);

        balances = s;
    }

    /// <summary>
    /// Makes string with a list of NFAs
    /// </summary>
    void NfaBalancesToText()
    {
        List<string> text = new();
        foreach (var (classId, instanceId) in fbManager.NfaInstances.Keys)
        {
            text.Add($" ● Nfa Id: {classId}-{instanceId}");
        }
        var s = string.Join("\n", text);
        ownedNfaInstancesText = s;
    }
}
