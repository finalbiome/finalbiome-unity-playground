using FinalBiome.Sdk;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UISignInManager : MonoBehaviour
{
    GameManager gameManager;
    Client client;

    bool signUpMode;

    [SerializeField]
    Button signInButton;

    [SerializeField]
    TMP_InputField emailUI;
    [SerializeField]
    TMP_InputField passwordUI;
    [SerializeField]
    TextMeshProUGUI signInText;
    [SerializeField]
    TextMeshProUGUI signUpText;
    [SerializeField]
    TextMeshProUGUI signInButtonTextUI;

    Color32 activeColor = new(255, 255, 255, 255);
    Color32 passiveColor = new(102, 107, 128, 255);


    internal async void Start()
    {
        // get client
        gameManager = await GameManager.GetInstance();
        client = await gameManager.GetClient();

        signInButton.interactable = !gameManager.IsLoggedIn;
        emailUI.onEndEdit.AddListener(delegate { InputsEditHandler(); });
    }

    internal void Update()
    {
        if (signUpMode)
        {
            signUpText.color = activeColor;
            signInText.color = passiveColor;
            signInButtonTextUI.text = "Sign Up";
        }
        else
        {
            signUpText.color = passiveColor;
            signInText.color = activeColor;
            signInButtonTextUI.text = "Sign In";
        }
    }

    /// <summary>
    /// Sign in to the game as a default user
    /// </summary>
    /// <returns></returns>
    public async void SignIn()
    {
        signInButton.interactable = false;
        try
        {
            if (signUpMode)
            {
                await client.Auth.SignUpWithEmailAndPassword(emailUI.text, passwordUI.text);
            }
            else
            {
                await client.Auth.SignInWithEmailAndPassword(emailUI.text, passwordUI.text);
            }
        }
        finally
        {
            signInButton.interactable = true;
        }
        SceneManager.LoadScene("MainScene");
    }

    public void Back()
    {
        SceneManager.LoadScene("MainScene");
    }

    void InputsEditHandler()
    {
        signInButton.interactable = !gameManager.IsLoggedIn && emailUI.text.Length > 0 && passwordUI.text.Length > 0;
    }

    public void OnSignInToggle()
    {
        Debug.Log(message: "Click");
        signUpMode = !signUpMode;
    }
}
