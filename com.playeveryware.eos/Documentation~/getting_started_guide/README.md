<a href="/com.playeveryware.eos/README.md"><img src="/com.playeveryware.eos/Documentation~/images/PlayEveryWareLogo.gif" alt="README.md" width="5%"/></a>

# Full Guide to Implementing the EOS Plugin for Unity

This document serves as the starting point for a beginning-to-end guide on how to implement the EOS Plugin for Unity into your game.
This will walk you through the steps to set up your project, supplemented by links to more detailed documentation should you want to explore things in greater detail.

## Prerequisites

This guide can be followed from a blank Unity project, or from an already implemented game. There are no steps required before you can start following this guide.

See [the Supported Version of Unity Documentation](/com.playeveryware.eos/Documentation~/supported_versions_of_unity.md) to see if your version of Unity is supported.

Your development team and the users of your game need to be able to access certain endpoints for the EOS Plugin for Unity to function. See [Epic's documentation on Firewall Considerations](https://dev.epicgames.com/docs/epic-online-services/eos-get-started/firewall-considerations) for a list of addresses.


# Signing Up for Epic Online Services

The EOS Plugin for Unity requires you to have a configured account and game on the Epic Game Store portal.
This leads to [the Developer Portal landing page](https://dev.epicgames.com/portal/en-US/).
Pressing Sign In will present you with a screen where you can sign up for a new account.
If you already have an Epic Games Account, perhaps because you've played games on the Epic Games Store, you can sign in with that existing account.

<p align="center" width="100%"><a href="https://www.epicgames.com/id/login?"><img src="/com.playeveryware.eos/Documentation~/getting_started_guide/epic_developerportal_signin_or_createaccount.png" alt="You can either sign in to an existing account, or create a new account, on the Epic Developer Portal."/></a><br/>
<em>You can either sign in to an existing account, or create a new account, on the Epic Developer Portal.</em></p>

After acquiring an account, if you are not using an existing Organization, you will be required to create an Organization.
An Organization is a holder for all of the Products you make.

See [Epic's documentation on Organizations](https://dev.epicgames.com/docs/dev-portal/organization-management) for more information.

## Working With an Existing Organization

If you or a team member has already created an Organization, you can add Epic Game Store accounts to that Organization.
While signed in to the `Epic Games Developer Portal`, navigate to `Organization`.
Under the `Members` tab, you can use the `Invite New` button to send an invitation email to the target user.
That email will contain a link that will help the user sign up for an Epic Account if necessary, and then subsequently they will be added to your Organization.

Everyone who will be testing your game should be a part of your Organization, or part of a Player Group. If they aren't, then whenever the EOS SDK requires the user to login, that user will be forbidden from accessing your application.

For more information on Player Groups, see [Epic's documentation on Player Groups](https://dev.epicgames.com/docs/dev-portal/product-management#player-groups).

# Developer Portal Configuration for Your Game

Before bringing the EOS Plugin for Unity into your project, you must first configure your game in the Epic Developer Portal.
This section will cover the creation and configuration of a `Product`, `Client Policy`, `Client`, and `Application`.
During this process, the Developer Portal will create a set of `Sandbox` and `Deployment` environments for your game to use.

> [!NOTE]
> While navigating the Epic Developer Portal to certain features, there will be dialogues to review and accept Epic terms and services.
> There are several of these throughout the onboarding process, with specific domains such as Epic Account Services. You will need to agree to the terms and services to use features that are part of those domains.
> This will bring you to a Terms of Service page with detailed information about the agreement.
> 
> In order to utilize many features of Epic Online Services, you will need to review and agree to these conditions.
> In order to use any features under `Epic Games Store`, there will be an agreement process that includes a possible submission fee.
> While this agreement is required to perform Epic Game Store operations, including publishing and selling your game, the EOS Plugin for Unity can be configured and utilized before this is agreed to.
>
> For more information on the onboarding process, see [Epic's documentation on Get Started Overview](https://dev.epicgames.com/docs/epic-games-store/get-started/get-started-overview#onboarding-process).

## 1) Create a Product

By default, your Organization will not contain any Products.
While signed in to the `Epic Games Developer Portal` with your Organization selected, click the `Create Product` button underneath the `Your products` header to start the process.

After naming your Product and creating it, there will be a brief wait as Epic's back-end servers process the creation. This is represented by a spinning loading animation.
When it's done, you can navigate to your Product to begin configuring it.

For more information on Products, see [Epic's documentation on Products](https://dev.epicgames.com/docs/dev-portal/product-management#whats-a-product).

## Client Policy and Client

On the `Product Settings` page for your Product within the Epic Developer Portal, you will see sections for your Product, Clients, Sandboxes, and Deployments.

<p align="center" width="100%"><img src="/com.playeveryware.eos/Documentation~/getting_started_guide/epic_developerportal_productlanding.png" alt="The first thing on the Product landing page is your Product, Clients, Sandboxes, and Deployments. Other than Product, these likely haven't been configured yet."/><br/>
<em>The first thing on the Product landing page is your Product, Clients, Sandboxes, and Deployments. Other than Product, these likely haven't been configured yet.</em></p>

In order to get the most out of the EOS Plugin for Unity, you'll need to create and configure a Product, a Client, Sandboxes, and Deployments in the developer portal.
Navigate to the `Clients` section. There you'll see your list of Clients and Client policies, which will be empty.

Clients are essentially types of contexts for your game. For example, all users running your game from the store will be assigned a Client that represents their abilities.
Each Client is assigned to one Client Policy.
Client Policies are configurable sets of rules for what permissions a Client can perform. For example, a Client Policy determines the EOS SDK's ability to look up information about the friend list for a particular player.

## 2) Create a Client Policy

The first thing to configure on the `Clients` screen is a Client Policy. To create a Client Policy, press the `Add new policy` button to bring up a dialogue.
The Client Policy needs a name, as well as select the details of a policy.

If you select a preset, the permissions for that policy preset will be displayed in the dialogue.
The most permissive default policy is `GameClient /w UnlockAchievements`, which will give the Client Policy all of the permissions necessary to perform most actions. Consider using this `GameClient /w UnlockAchievements` policy setting if you don't have other specific needs.

Custom policies can be defined here, and changed at any time. You don't need to commit to the final details at this time. Only Custom policies can have their details changed.

## 3) Create a Client

After the Client Policy is created, create a client. Press `Add new client` to bring up the Add new client dialogue.
This will require a name and a Client Policy. Use the previously created Client Policy.
At this time, it is not required that you fill the fields for either the `Trusted Server IP allow list` or the `Epic Account Service Redirect URL`. For information on Trusted Servers see [Epic's documentation on Linked Clients](https://dev.epicgames.com/docs/epic-account-services/getting-started#linked-clients). For information on the Redirect URL see [Epic's documentation on Web Applications](https://dev.epicgames.com/docs/web-api-ref/authentication#web-applications).

For more information on Clients and Client Policies, see [Epic's Client Credential and Client Policy Management documentation](https://dev.epicgames.com/docs/dev-portal/client-credentials).

## 4) Agreement of Epic Game Services Terms, Which Creates Sandboxes and Deployments

The `Game Services` section of the portal covers features that are available to all games using the plugin, even if they don't require Epic Accounts to function.
At this time, you should navigate to the `Game Services` section. This will likely prompt you with a user agreement.

Once accepted, `Sandboxes` and `Deployments` will be generated for `Dev` and `Stage`. These can be viewed in `Product Settings`.

While not required for the plugin to function, many of the features of the EOS For Unity Plugin use Epic Game Services.
The `Dev` and `Stage` environment configurations will also help with developing your game by isolating the development environments from the `Live` environment.

For more information on Sandboxes and Deployments, see [Epic's documentation on Sandboxes and Deployments](https://dev.epicgames.com/docs/dev-portal/product-management#sandboxes-and-deployments).

## 5) Create an Application

Navigate to the `Epic Account Services` page. The landing will show you all configured Applications, which are empty at first. 
Use `Create Application` to get started. Immediately when you press this button, an Application will be created for you.
An Application is a collection of information about your game, which is later used to provide the Epic Game Store with essential information and permissions.
The `Create Application` button will bring up a dialogue with three major steps; `Brand Settings`, `Permissions`, and `Linked Clients`.

`Brand Settings` contains information about your game such as the Application's website.
You do not need to fill out this page in order to start using the EOS Plugin for Unity; this information can be provided later.

`Permissions` will set the essential information that each user needs to consent to in order to use your application.
This can be adjusted later. For full use of the EOS Plugin for Unity, consider marking `Online Presence` and `Friends` as Required.

`Linked Clients` matches an Application to the Client that dictates its policies and further permissions.
Assign the Client created earlier to this.

When you exit this dialogue, the created Application will appear in the Applications page.

For more information on Applications, see [Epic's documentation on Applications in the Developer Portal Introduction](https://dev.epicgames.com/docs/dev-portal/dev-portal-intro#application).

# Add the EOS Plugin for Unity to Your Game

With a Client Policy, Client, and Application created, you're ready to start adding the EOS Plugin for Unity to your game.
Follow [our documentation for adding the plugin to your project](/com.playeveryware.eos/Documentation~/add_plugin.md).
Once successfully included, follow [the guidance on Importing the Samples](com.playeveryware.eos/Documentation~/samples.md).
In the EOS Plugin for Unity project, the provided code with the plugin will give you the ability to use EOS SDK's interfaces using a convenient C# wrapper.
The Samples come with scenes that demonstrate the usage of the EOS Plugin for Unity's Manager classes for each Sample's domain.

Next, follow [our documentation regarding how to configure the plugin](/com.playeveryware.eos/Documentation~/configure_plugin.md).
Inside of Unity (using the menu bar), navigate to `EOS Plugin` -> `EOS Configuration`.

<p align="center" width="100%"><img src="/com.playeveryware.eos/Documentation~/getting_started_guide/plugin_eosconfiguration.png" alt="The EOS Configuration popup can be filled out using information in the Product Settings page."/><br/>
<em>The EOS Configuration popup can be filled out using information in the Product Settings page.</em></p>

All of these values can be pulled from the `Product Settings` page and inner dialogues within the EOS Developer Portal.
One of the fields is your `Encryption Key`. This can remain its default value for now. This value will not be found in the EOS Developer Portal.
Essentially this value is used to encrypt Player Data Storage and Title Data Storage uploads, and needs to be consistent in order to decrypt those values when they are later downloaded.

It is recommended that you use the `Dev` Sandbox and `Dev Deployment` Sandbox while configuring the plugin.
By using the Dev environment information, the EOS Plugin for Unity will use this environment when no other environment is assumed.
If your game is deployed through the Epic Game Store, the game will be launched with `-epicsandboxid` and `epicdeploymentid` arguments, which the EOS Plugin for Unity will use instead of your configured Sandbox and Deployment.

For more information on the use of the Encryption Key, see [Epic's documentation on Title Storage Interface](https://dev.epicgames.com/docs/game-services/title-storage).

## Starting to Use the Samples

At this moment your game is set up to utilize the EOS Plugin for Unity.
The samples are documented [in the EOS Plugin for Unity Walkthrough documentation](https://github.com/PlayEveryWare/eos_plugin_for_unity/blob/stable/com.playeveryware.eos/Documentation~/Walkthrough.md), which leads to individual Scene walk throughs.
Assuming your Client Policy is set up to be permissive, consider validating the plugin inclusion by using [the Lobbies Sample](/com.playeveryware.eos/Documentation~/scene_walkthrough/lobbies_walkthrough.md).
Open the Lobbies sample in the scene, and start running the game. Note if there are any errors in the logs from the EOS SDK Plugin.

The Lobbies sample is chosen because, assuming a permissive Client Policy, there's no needed further setup in the Epic Developer Portal.
If you successfully log in to the Lobbies Sample, create a Lobby to demonstrate that everything is connected appropriately.

> [!NOTE]
> The samples include useful Managers and Services that will provide an easy way to use the EOS SDK.
> The scenes that are in the sample show how to utilize the plugin; even without needing those, it is still useful to bring in the samples.

> [!NOTE]
> Look to the Unity logs for messages that relate to the configuration of the plugin.
> If there is an error relating to "Invalid Parameters" during initialization, view the EOS Configuration screen again.
> Ensure all fields have values, including `Product Version`, and that the values match what you have in the Epic Developer Portal's `Product Settings` page.

> [!NOTE]
> When testing the Samples, ensure the account being logged in has access to your game.
> To start, use the same account that created the game, and either owns or is a member of the Organization.
> Only members of your Organization or Player Group with appropriate permissions are able to login to EOS from your game.
> 
> For more information on Player Groups, see [Epic's documentation on Player Groups](https://dev.epicgames.com/docs/dev-portal/product-management#player-groups).

# Utilizing the EOS Plugin for Unity

The core of the EOS Plugin for Unity is the `EOSManager` class. This is a `MonoBehaviour` that manages the state of the EOS SDK.
To add it to your game, either use the `Singletons` prefab included with the Samples, or create a new GameObject and attach the `EOSManager.cs` script to it.
The `EOSManager` is designed to set the GameObject it is on as `DontDestroyOnLoad`, so it will persist between scenes.
If another `EOSManager` is created, perhaps because of a scene navigation, the original `EOSManager` persist, while the new instance will disable its own `MonoBehaviour`.

`EOSManager` holds a static reference to an inner class `EOSSingleton`, which is where most of the plugin's state is held. This can be accessed with the static accessor `EOSManager.Instance`.

## Logging In

Before the plugin can be utilized, the user of your game needs to log in to EOS.
The samples include a `UILoginMenu` component in each of the scenes, which demonstrates handling logging in.
The kind of login your users should use is based on your game's platform, and the needs of your game.
See [our documentation on Login Types by Platform](/com.playeveryware.eos/Documentation~/login_type_by_platform.md) to determine the valid options for your platform.
Then follow [our documentation on Authenticating Players](/com.playeveryware.eos/Documentation~/player_authentication.md) to understand the login workflow.

The most common use case is to follow the below process:

- Use the Auth Interface to authenticate the user. This can be one of several methods, depending on your needs. For example `EOSManager.Instance.StartLoginWithLoginTypeAndToken`.
  - If the authentication results in `Result.InvalidUser`, this indicates the user doesn't yet have a link from this Identity Provider to an Epic Account. This is not an error; any user who has never logged in with the provided credentials into an Epic service will return this result. Handle this by calling `EOSManager.Instance.AuthLinkExternalAccountWithContinuanceToken`, passing in the ContinuanceToken from the callback's `Epic.OnlineServices.Auth.LoginCallbackInfo` parameter. If that succeeds, the login flow can continue to the next step. The user will be authenticated by this function call.
- Use the Connect Interface to continue the login flow. Use `EOSManager.Instance.StartConnectLoginWithEpicAccount`.
  - This can also return `Result.InvalidUser`, which indicates a Connect User needs to be created for this Epic Account to your title. This is also not an error; any user who is logging in for the first time to your game will encounter this. Use `EOSManager.Instance.CreateConnectUserWithContinuanceToken`, passing in the ContinuanceToken from the callback's `Epic.OnlineServices.Connect.LoginCallbackInfo` parameter. If this succeeds, then continue the login flow by calling `EOSManager.Instance.StartConnectLoginWithEpicAccount` again.

If the above all succeeds, the plugin has logged the user in entirely, and the plugin is ready for use.
This is all demonstrated in the `UILoginMenu`'s handling of result codes.

> [!NOTE]
> Many features in the EOS Plugin for Unity use a callback pattern for handling results.
> For example, `EOSManager.Instance.StartLoginWithLoginTypeAndToken` contains an `OnAuthLoginCallback` callback argument.
> 
> These are usually delegates, with one parameter that describes the return type. For example, `OnAuthLoginCallback` has a `Epic.OnlineServices.Auth.LoginCallbackInfo` parameter.
> This contains information about whether the operation succeeded or failed through a `Result` enum.
>
> `UILoginMenu` demonstrates this in `StartConnectLoginWithEpicAccount`. It calls a function, providing a delegate which responds to the result of the operation.

## Utilizing Managers and Interfaces

All of the functionality for the EOS SDK is separated into interfaces.
These can be accessed, for example, through `EOSManager.Instance.GetEOSAchievementInterface()`.
This returns a C# wrapper around the EOS SDK's operations.

When the Samples are included into your project, all of these interfaces are accessible through Manager and Service classes.
These Managers and Services are designed to make using the EOS SDK's functions feel "more Unity-like" and be more convenient than accessing the Interfaces directly.

For example, the functionality of `FriendsInterface` is made into convenient functions inside `EOSFriendsManager`.
To get a `Manager` class, for example the `EOSFriendsManager`, use `EOSManager.Instance.GetOrCreateManager<EOSFriendsManager>()`.

Some of the interfaces are wrapped in `Service`s. For example, `AchievementsInterface` has its functionality inside `AchievementsService`.
These can be accessed through their lazy-loaded Singleton reference. For example, `AchievementsService.Instance`.

The Managers and Services in the plugin manage their own states. Other than the `EOSManager`, none of them are MonoBehaviours, and will clean up their data on login and logout operations automatically.