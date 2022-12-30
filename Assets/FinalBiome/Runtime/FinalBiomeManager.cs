using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using FinalBiome.Sdk;
using UnityEngine;

/// <summary>
/// Manages a FinalBiome client
/// </summary>
public class FinalBiomeManager : MonoBehaviour
{
    private const string SingletonName = "FinalBiomeManager";
    private static readonly SemaphoreSlim Lock = new(1,1);

    private static FinalBiomeManager _instance;

    internal ClientConfig config;
    private Client _client;
    /// <summary>
    /// Instance of the FinalBiome Client
    /// </summary>
    /// <value></value>
    public Client Client {
        get
        {
            if (_client is null) throw new System.Exception("Client not initialized. Run Initialize() first.");
            return _client;
        }
        internal set
        {
            _client = value;
        }
    }

    /// <summary>
    /// Contains all existing FAs in the game and their balance.
    /// </summary>
    public Dictionary<uint, BigInteger> FaBalances { get; internal set; } = new();

    /// <summary>
    /// Contains all NFAs owned by the gamer and their details.
    /// </summary>
    public Dictionary<(uint classId, uint instanceId), FinalBiome.Api.Types.PalletSupport.TypesNfa.AssetDetails> NfaInstances { get; internal set; } = new();

    /// <summary>
    /// The singleton instance of the FinalBiome Sdk Manager
    /// </summary>
    /// <value></value>
    public static async Task<FinalBiomeManager> GetInstance()
    {
        await Lock.WaitAsync();
        try
        {
            if (_instance != null) return _instance;

            var go = GameObject.Find(SingletonName);
            if (go == null)
            {
                go = new GameObject(SingletonName);
            }

            if (go.GetComponent<FinalBiomeManager>() == null)
            {
                go.AddComponent<FinalBiomeManager>();
            }
            DontDestroyOnLoad(go);
            _instance = go.GetComponent<FinalBiomeManager>();

            return _instance;
        }
        finally
        {
            Lock.Release();
        }
    }

    /// <summary>
    /// Initialize FinalBiome client
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public static async Task Initialize(ClientConfig config)
    {
        var instance = await GetInstance();

        if (instance._client is not null) throw new Exception("FinalBiome Client already initialized");

        instance.config = config;
        instance.config.PersistenceDataPath = Application.persistentDataPath;
        instance.Client = await Client.Create(instance.config);
        // listen signing in
        instance.Client.Auth.StateChanged += instance.UserStateChangedHandler;
        // listen Fa balances changes
        instance.Client.Fa.FaBalanceChanged += instance.FaBalanceChangedHandler;
        // listen nfa changes
        instance.Client.Nfa.NfaInstanceChanged += instance.NfaInstanceChangedHandler;
    }

    /// <summary>
    /// Keeps up to date with all changes with FAs.
    /// </summary>
    /// <param name="o"></param>
    /// <param name="e"></param>
    void FaBalanceChangedHandler(object o, FaBalanceChangedEventArgs e)
    {
        FaBalances[e.Id] = e.Balance;
    }

    /// <summary>
    /// Keeps up to date with all changes with NFA Instances.
    /// </summary>
    /// <param name="o"></param>
    /// <param name="e"></param>
    void NfaInstanceChangedHandler(object o, NfaInstanceChangedEventArgs e)
    {
        NfaInstances[(e.classId, e.instanceId)] = e.details;
    }

    async Task UserStateChangedHandler(bool loggedIn)
    {
        if (!loggedIn)
        {
            NfaInstances.Clear();
        }
        await Task.Yield();
    }

    internal void OnApplicationQuit()
    {
        Client.Dispose();
    }
}
