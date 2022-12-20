using System.Collections.Generic;
using System.Threading.Tasks;
using FinalBiome.Api.Types.PalletSupport.TypesNfa;
using FinalBiome.Sdk;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    FinalBiomeManager manager;
    Client client;

    [SerializeField]
    TextMeshProUGUI gameName;
    [SerializeField]
    TextMeshProUGUI faBalancesUI;
    [SerializeField]
    TextMeshProUGUI userStateUI;
    [SerializeField]
    TextMeshProUGUI userStateButtonTextUI;

    [SerializeField]
    TextMeshProUGUI nfaInstancesUI;

    [SerializeField]
    TMP_Dropdown nfaClassesDropdownUI;

    [SerializeField]
    TextMeshProUGUI nfaDetailsUI;

    [SerializeField]
    TextMeshProUGUI buyButtonText;
    [SerializeField]
    Button buyButtonNfa;
    [SerializeField]
    Button onboardButton;

    uint? selectedNfaClassId;
    string selectedNfaClassDetails;

    internal string balances;
    internal string ownedNfaInstancesText;
#nullable enable
    internal readonly Dictionary<(uint classId, uint instanceId), AssetDetails?> ownedNfaInstances = new();
#nullable restore
    bool loggedIn;
    bool onboarded;

    internal async void Start()
    {
        // get manager
        manager = await FinalBiomeManager.GetInstance();
        // get client
        client = manager.Client;

        // listen signing in
        client.Auth.StateChanged += UserStateChangedHandler;
        // listen Fa balances changes
        client.Fa.FaBalanceChanged += FaBalanceChangedHandler;
        // listen nfa changes
        client.Nfa.NfaInstanceChanged += NfaInstanceChangedHandler;
        
        // init UI user state
        await UserStateChangedHandler(client.Auth.UserIsSet);
        // show game name
        UISetGameName(client.Game.Data.Name);
        if (client.Auth.UserIsSet)
        {
            // init current Fa balances
            FaBalancesToText();
            NfaBalancesToText();
        }
    }

    // Update is called once per frame
    internal void Update()
    {
        faBalancesUI.text = balances;
        nfaDetailsUI.text = selectedNfaClassDetails;
        nfaInstancesUI.text = ownedNfaInstancesText;

        onboardButton.interactable = loggedIn && !onboarded;
        buyButtonNfa.interactable = loggedIn && selectedNfaClassId is not null;
    }

    /// <summary>
    /// Sign in to the game as a default user
    /// </summary>
    /// <returns></returns>
    public async void SignIn()
    {
        if (client.Auth.UserIsSet)
        {
            await client.Auth.SignOut();
            buyButtonText.text = $"Buy NFA";
            selectedNfaClassId = null;
            buyButtonNfa.interactable = false;
        }
        else
        {
            SceneManager.LoadScene("SignInScene");
        }
    }

    public async void Onboard()
    {
        await client.Mx.OnboardToGame();
    }

    void UISetGameName(string name)
    {
        this.gameName.text = name;
    }

    async Task UserStateChangedHandler(bool loggedIn)
    {
        this.loggedIn = loggedIn;
        userStateUI.text = loggedIn ? "User Logged In" : "User Logged Out";
        userStateButtonTextUI.text = loggedIn ? "Logout" : "Sign In";

        if (!loggedIn)
        {
            balances = "";
            ownedNfaInstancesText = "";
            ownedNfaInstances.Clear();
        }

        onboarded = client.Game.IsOnboarded ?? false;
        
        await Task.Yield();
    }

    void FaBalanceChangedHandler(object o, FaBalanceChangedEventArgs e)
    {
        // refresh an interface if fas has bee changed
        FaBalancesToText();
    }

    /// <summary>
    /// Makes string as a list of FAs
    /// </summary>
    void FaBalancesToText()
    {
        // because we need show all fa balances at once, we don't use event data,
        // and collect all data from sdk
        List<string> text = new();
        foreach (var (id, balance) in manager.FaBalances)
        {
            text.Add($" ☼ Fa Id: {id} - {balance}");
        }

        var s = string.Join("\n", text);

        balances = s;
    }

    void NfaInstanceChangedHandler(object o, NfaInstanceChangedEventArgs e)
    {
        // refresh an interface if nfas has bee changed
        NfaBalancesToText();
    }

    /// <summary>
    /// Makes string as a list of NFAs
    /// </summary>
    void NfaBalancesToText()
    {
        List<string> text = new();
        foreach (var (classId, instanceId) in manager.NfaInstances.Keys)
        {
            text.Add($" ● Nfa Id: {classId}-{instanceId}");
        }
        var s = string.Join("\n", text);
        ownedNfaInstancesText = s;
    }


    public async void OnChangedNfaClass(int _)
    {
        // in this example we have one to one mapping of dropdown options and id of classes.
        // in other cases we need the more complicated logic
        var id = nfaClassesDropdownUI.value;
        selectedNfaClassId = (uint)id;
        buyButtonNfa.interactable = loggedIn;
        var details = await client.Nfa.GetClassDetails((uint)selectedNfaClassId);
        // selectedNfaClasseDetails = Newtonsoft.Json.JsonConvert.SerializeObject(details);
        selectedNfaClassDetails = details.ToHuman();

        buyButtonText.text = $"Buy NFA {id}";
    }

    public async void OnClickButtonBuy()
    {
        Debug.Log("Buy NFA");
        if (selectedNfaClassId is not null)
        {
            buyButtonNfa.interactable = false;
            Debug.Log("Buying NFA...");
            var res = await client.Mx.ExecBuyNfa((uint)selectedNfaClassId, 0);
            Debug.Log($"Asset with id {selectedNfaClassId}-{res.Result.InstanceId} has been purchased");
            buyButtonNfa.interactable = true;
        }
    }
}
