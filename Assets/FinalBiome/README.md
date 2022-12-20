# FinalBiome Unity
Unity client for the FinalBiome Network.

[FinalBiome](https://finalbiome.net) is a decentralized game deployment platform.

This client is built on the [FinalBiome.Sdk](https://www.nuget.org/packages/FinalBiome.Sdk/) with extensions for Unity Engine.

## Getting Started
You will need to be connected to a production or test FanalBiome Network or run a local FinalBiome [node](https://github.com/finalbiome/finalbiome-node) to connect with the client.

### Installing the Sdk
1. Obtain a connection to FinalBiome Network or run a local node.
2. Download the .unitypackage from the releases [page](https://github.com/finalbiome/finalbiome-unity/releases) and import it into your project.

    Depending on the project template you have chosen, it is possible that the package with the Newtonsoft.Json.dll assembly already exists in the project. In this case, after importing you will see an error:
    "Multiple precompiled assemblies with the same name Newtonsoft.Json.dll included on the current platform".
    
    In this case:
    1. Remove the Newtonsoft.Json.dll assembly from the Assets/FimalBiome/Runtime/Plugins folder
    2. Disable Assembly Version Validation in the Player Settings "Assembly Version Validation"
3. Specify the connection credentials to work with a FinalBiomeManager client object.
    ```cs
    public string Endpoint = "ws://127.0.0.1:9944";
    public string GameAddressSS58Format = "5HGjWAeFDfFCWPsjFQdVV2Msvz2XtMktvgocEZcCj68kUMaw";
    ```

## Usage

The client object has the ability to use any of the FanalBiome Network features.
Package include self created singleton game object `FinalBiomeManager` which hold a clent and some data.

```cs
// get manager
var manager = await FinalBiomeManager.GetInstance();
// get client
var client = manager.Client;
```

And if the game involves the issuance of some kind of starter package of assets, then you need to make an onboarding to the game for new players.

```cs
if (!client.Game.IsOnboarded)
{
    await client.Mx.OnboardToGame();
}
```

### Authenticate

There are two types of authentication that can be used in the game - anonymous and email/password. There are only such options, other authentication options will be added in the future.

When game is launched for the first time, user (aka gamer) obtain an anonimous credentials with whith them can playing without any limits. But if the game is reinstalled or launched on the new devices, all prevously results will be lost.
To save the current results in the game and be able to play on other devices, the gamer needs to set up the email/password auth.

```cs
await client.Auth.SignInWithEmailAndPassword(emailUI.text, passwordUI.text);
```

### Assets State

The client includes lots of builtin APIs for various features of the FinalBiome Network. These can be accessed with the async methods.

```cs
var classId = 1u;
var details = await client.Nfa.GetClassDetails(classId);
```

You can also subscribe on the network changes.

```cs
// listen Fa balances changes
client.Fa.FaBalanceChanged += FaBalanceChangedHandler;
// ... skip ...
void FaBalanceChangedHandler(object o, FaBalanceChangedEventArgs e)
{
    // refresh an interface if FAs has been changed
}
```

### Execute Mechanics

All existed mechanics in the FinalBiome Network can be executed by the next maner.

```cs
var resultBet = await client.Mx.ExecBet(classId, instanceId);

Assert.That(resultBet.Status, expression: Is.EqualTo(ResultStatus.Finished));
Assert.That(resultBet.Result.Outcomes, expression: Has.Count.AtLeast(1));
```

## Playground

You can find examples of FinalBiome Unity Client work in the [playground](https://github.com/finalbiome/finalbiome-unity-playground) Unity project.

## Contribute
The development roadmap is managed as GitHub issues and pull requests are welcome. 

This project can be opened in Unity to create a ".unitypackage".

## License

This project is licensed under the [Apache-2 License](./LICENSE).
