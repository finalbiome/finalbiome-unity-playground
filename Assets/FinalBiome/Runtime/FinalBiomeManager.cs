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

    public string Endpoint = "ws://127.0.0.1:9944";
    public string GameAddressSS58Format = "5HGjWAeFDfFCWPsjFQdVV2Msvz2XtMktvgocEZcCj68kUMaw";
    internal ClientConfig config;
    /// <summary>
    /// Instance of the FinalBiome Client
    /// </summary>
    /// <value></value>
    public Client Client { get; internal set; }

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

            // init sdk
            _instance.config = new(_instance.GameAddressSS58Format, _instance.Endpoint)
            {
                PersistenceDataPath = Application.persistentDataPath
            };
            _instance.Client = await Client.Create(_instance.config);
            // listen signing in
            _instance.Client.Auth.StateChanged += _instance.UserStateChangedHandler;
            // listen Fa balances changes
            _instance.Client.Fa.FaBalanceChanged += _instance.FaBalanceChangedHandler;
            // listen nfa changes
            _instance.Client.Nfa.NfaInstanceChanged += _instance.NfaInstanceChangedHandler;

            return _instance;
        }
        finally
        {
            Lock.Release();
        }
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
