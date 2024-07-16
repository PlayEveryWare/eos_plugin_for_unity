<a href="/README.md"><img src="/com.playeveryware.eos/Documentation~/images/PlayEveryWareLogo.gif" alt="Readme" width="5%"/></a>

# Configuring the Plugin

To function, the plugin needs some information from your EOS project. Be sure to read the Epic Documentation on [getting started](https://dev.epicgames.com/docs/epic-account-services/getting-started?sessionInvalidated=true) with Epic Online Services.

1) Open your Unity project with the integrated EOS Unity Plugin. 
2) In the Unity editor, Open ```Tools -> EOS Plugin -> Dev Portal Configuration```.

    ![EOS Config Menu](/com.playeveryware.eos/Documentation~/images/dev-portal-configuration-editor-menu.png)

    ![EOS Config UI](/com.playeveryware.eos/Documentation~/images/eosconfig_ui.gif)

3) From the [Developer Portal](https://dev.epicgames.com/portal/), copy the configuration values listed below, and paste them into the similarly named fields in the editor tool window pictured above:

     > [!NOTE]
     > For more detailed information, check out Epic's Documentation on [Creating the Platform Interface](https://dev.epicgames.com/docs/game-services/eos-platform-interface#creating-the-platform-interface).

    * ProductName
    * ProductVersion
    * [ProductID](https://dev.epicgames.com/docs/services/en-US/Glossary/index.html#D?term=ProductId)
    * [SandboxID](https://dev.epicgames.com/docs/services/en-US/Glossary/index.html#D?term=SandboxId)
    * [DeploymentID](https://dev.epicgames.com/docs/services/en-US/Glossary/index.html#D?term=DeploymentId)
    * [ClientSecret](https://dev.epicgames.com/docs/services/en-US/Glossary/index.html#D?term=ClientSecret)
    * [ClientID](https://dev.epicgames.com/docs/services/en-US/Glossary/index.html#D?term=ClientId)
    * EncryptionKey

    <br />

    > [!NOTE]
    > Click the "Generate" button to create a random key, if you haven't already configured an encryption key in the EOS portal. You can then add the generated key to the [Developer Portal](https://dev.epicgames.com/portal/).
    > The Encryption Key is Used for Player Data Storage and Title Storage, if you do not plan to use these features in your project or the samples (and don't want to create an Encryption Key) then the field can be left blank.

4) Click `Save All Changes`.

5) Navigate to `Packages/Epic Online Services for Unity/Runtime` via the `Project` window.

6) Add the `EOSManager.prefab`, to each of your game's scenes.

7) Attach `EOSManager.cs (Script)` to a Unity object, and it will initialize the plugin with the specified configuration in `OnAwake()`.

> [!NOTE]
The included [samples](http://github.com/PlayEveryWare/eos_plugin_for_unity/blob/development/com.playeveryware.eos/README.md#samples) already have configuration values set for you to experiment with!

If you would like to see specific examples of various EOS features in action, import the sample Unity scenes that are described below.
