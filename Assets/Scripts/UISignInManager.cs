using FinalBiome.Sdk;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UISignInManager : MonoBehaviour
{
    GameManager gameManager;
    Client client;

    [SerializeField]
    Button signInButton;

    [SerializeField]
    TMP_InputField emailUI;
    [SerializeField]
    TMP_InputField passwordUI;
    internal async void Start()
    {
        // get client
        gameManager = await GameManager.GetInstance();
        client = await gameManager.GetClient();

        signInButton.interactable = !gameManager.IsLoggedIn;
        emailUI.onEndEdit.AddListener(delegate { InputsEditHandler(); });
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
            await client.Auth.SignInWithEmailAndPassword(emailUI.text, passwordUI.text);
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
}
