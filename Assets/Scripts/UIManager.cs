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
    GameManager gameManager;
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
    bool onboarded;
    string userState;
    string userStateButtonText;

    internal async void Start()
    {
        gameManager = await GameManager.GetInstance();
        client = await gameManager.GetClient();

        // check logging in
        // the first check initializes all user data and rise all suitable events if the user is already logged in
        await client.Auth.IsLoggedIn();

        // set in UI the game name
        this.gameName.text = client.Game.Data.Name;
    }

    // Update is called once per frame
    internal void Update()
    {
        UISetLoginStateInfo();

        faBalancesUI.text = gameManager.balances;
        nfaDetailsUI.text = selectedNfaClassDetails;
        nfaInstancesUI.text = gameManager.ownedNfaInstancesText;
        userStateUI.text = userState;
        userStateButtonTextUI.text = userStateButtonText;

        onboardButton.interactable = gameManager.IsLoggedIn && !onboarded;
        buyButtonNfa.interactable = gameManager.IsLoggedIn && selectedNfaClassId is not null;
    }

    /// <summary>
    /// Sign in to the game as a default user
    /// </summary>
    /// <returns></returns>
    public async void OnClickSignInButton()
    {
        if (gameManager.IsLoggedIn)
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

    public async void OnClickOnboardButton()
    {
        await client.Mx.OnboardToGame();
    }

    /// <summary>
    /// Set UI texts based on login state
    /// </summary>
    void UISetLoginStateInfo()
    {
        if (client is null) return;

        bool loggedIn = gameManager.IsLoggedIn;

        string userName = " anonynous";
        if (client.Auth.UserInfo is not null)
            userName = $" ({client.Auth.UserInfo.DisplayName ?? client.Auth.UserInfo.Email})";
        
        userState = loggedIn ? "User Logged In" + userName : "User Logged Out";
        userStateButtonText = loggedIn ? "Logout" : "Sign In";

        onboarded = client.Game.IsOnboarded ?? false;
    }

    public async void OnChangedNfaClass(int _)
    {
        // in this example we have one to one mapping of dropdown options and id of classes.
        // in other cases we need the more complicated logic
        var id = nfaClassesDropdownUI.value;
        selectedNfaClassId = (uint)id;
        var details = await client.Nfa.GetClassDetails((uint)selectedNfaClassId);

        selectedNfaClassDetails = details.ToHuman();

        buyButtonText.text = $"Buy NFA {id}";
    }

    public async void OnClickBuyButton()
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
