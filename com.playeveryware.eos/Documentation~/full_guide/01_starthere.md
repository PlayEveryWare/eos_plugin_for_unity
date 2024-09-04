<a href="/README.md"><img src="/com.playeveryware.eos/Documentation~/images/PlayEveryWareLogo.gif" alt="README.md" width="5%"/></a>

# Full Guide to Implementing the EOS Plugin for Unity

This document serves as the starting point for a beginning-to-end guide on how to implement the EOS Plugin for Unity into your game.
The only prerequisite to following this guide is having a game you want to implement the plugin into. This will walk you through
the steps to set up your project, with links to documentation for more details and information.

# Signing Up for Epic Online Services

The EOS Plugin for Unity requires you to have a configured account and game on the Epic Game Store portal.
For information about the benefits of publishing through Epic, see [Epic's Store Distribution portal](https://store.epicgames.com/en-US/distribution).
This leads to [the Developer Portal landing page](https://dev.epicgames.com/portal/en-US/).
Pressing Sign In will present you with a screen that you can sign up for a new account.
If you already have an Epic Games Account, perhaps because you've played games on the Epic Games Store, you can sign in with that existing account.

<a href="https://www.epicgames.com/id/login?"><img src="/com.playeveryware.eos/Documentation~/full_guide/epic_developerportal_signin_or_createaccount.png" alt="You can 'create account' on the Epic Developer Portal."/></a>

Upon signing up or creating an account, you'll be required to create an Organization.
An Organization is a holder for all of the Products you make. See [Epic's documentation on Organizations](https://dev.epicgames.com/docs/dev-portal/organization-management) for more information.

## Working With an Existing Organization

If you or a team member has already created an Organization, you can add Epic Game Store accounts to that organization.
While signed in to the `Epic Games Developer Portal`, navigate to `Organization`.
Under the `Members` tab, you can use the `Invite New` button to send an invitation email to the target user.
That email will contain a link that will help the user sign up for an Epic Account if necessary, and then they'll join your organization.

Everyone who will be testing your game should be a part of your organization, or part of a Player Group. If they aren't, then whenever the EOS SDK requires the user to login, that user will be forbidden from accessing your application.
For more information on Player Groups, see [Epic's documentation on Player Groups](https://dev.epicgames.com/docs/dev-portal/product-management#player-groups).

# Creating a Product

By default your Organization will not contain any Products.
While signed in to the `Epic Games Developer Portal`, and with your Organization selected, the `Create Product` button underneath the `Your products` header will start the process.
After naming your Product, there will be a brief wait as Epic's back-end servers process the creation.
When it's done, you can navigate to your Product to begin configuring it.

## Additional Agreements Required

At some point while navigating around your Product, there will be dialogues for Epic terms and services.
There are several of these throughout the onboarding process, with specific domains.
This will bring you to a Terms of Service page with detailed information about the agreement.
In order to utilize many features of Epic Online Services, you will need to review and agree to these conditions.
In order to use any features under `Epic Games Store`, there will be an agreement process that includes a possible submission fee.
See [Epic's documentation on Get Started Overview](https://dev.epicgames.com/docs/epic-games-store/get-started/get-started-overview#onboarding-process) for more information.
While this agreement is required to perform Epic Game Store operations, including publishing and selling your game, the EOS Plugin for Unity can be configured and utilized before this is agreed to.

# Creating a Client Policy and Client

The landing page for a Product will note your existing Product, Clients, Sandboxes, and Deployments.

<img src="/com.playeveryware.eos/Documentation~/full_guide/epic_developerportal_productlanding.png" alt="The first thing in the Product landing page is your Product, Clients, Sandboxes, and Deployments. Other than Product, these likely haven't been configured yet."/>

In order to get the most out of the EOS Plugin for Unity, you'll need to create several of these.
Start by creating a Client by navigating to the `Clients` section.
There you'll see your list of Clients and Client policies, which will be empty.

For more information on Clients and Client Policies, see [Epic's Client Credential and Client Policy Management documentation](https://dev.epicgames.com/docs/dev-portal/client-credentials).
Clients are essentially types of contexts for your game. For example, all users running your game from the store will be assigned a Client that represents their abilities.
Each Client is assigned to one Client Policy.
Client Policies are configurable sets of rules for what permissions a Client can perform. For example, a Client Policy determines the EOS SDK's ability to look up information about a player's friend list.

First, create a Client policy. Press the `Add new policy` button to bring up the dialogue.
The Client Policy needs a name, as well as select the details of a policy.
If you select a preset, the permissions for that policy preset will be displayed in the dialogue.
Custom policies can be defined here, and changed at any time. You don't need to commit to the final details at this time.
The most permissive default policy is `GameClient /w UnlockAchievements`, which will give the Client Policy all of the permissions necessary to perform most actions.

When the Client Policy is created, next create a client. Press `Add new client` to bring the Add new client dialogue.
This will require a name and a Client Policy. Use the previously created Client Policy.
`Trusted Server IP allow list` and `Epic Account Service Redirect URL` are not required to be filled at this time.

# Creating an Application

Navigate to the `Epic Account Services` page. The landing will show you all configured Applications, which is empty at first.
See [Epic's documentation on Applications in the Developer Portal Introduction](https://dev.epicgames.com/docs/dev-portal/dev-portal-intro#application) for more information on Applications.
An Application is collection of information about your game, which is later used to provide the Epic Game Store with essential information and permissions.
Use `Create Application` to get started. Immediately when you press this button, an Application will be created for you.
This will bring up a dialogue with three major steps; `Brand Settings`, `Permissions`, and `Linked Clients`.

`Brand Settings` contains information about your game such as the Application's website.
To utilize the EOS Plugin for Unity, you do not need to fill this page out at this time.

`Permissions` will set the essential information that each user needs to consent to in order to use your application.
This can be adjusted later. For full use of the EOS Plugin for Unity, consider marking `Online Presence` and `Friends` as Required.

`Linked Clients` matches an Application to the Client that dictates its policies and further permissions.
Assign the Client Policy created earlier to this.

When you exit this dialogue, the created Application will appear in the Applications page.
Navigating back to the `Product Settings` page, `Sandboxes` and `Deployments` will automatically be made for Dev, Stage, and Live.

# Add the EOS Plugin for Unity to Your Game

With a Client Policy, Client, and Application created, you're ready to start adding the EOS Plugin for Unity to your game.
Follow [the Add Plugin documentation](/com.playeveryware.eos/Documentation~/add_plugin.md) to add the Plugin to the project.
Once successfully included, follow [the guidance on Importing the Samples](com.playeveryware.eos/Documentation~/samples.md).
In the EOS Plugin for Unity project, the provided code with the plugin will give you the ability to use EOS SDK's interfaces using convenient C# wrapper.
Inside the Samples are a set of `Manager`s and `Service`s that wrap up the EOS SDK in easy-to-use functions.
The Samples come with scenes that demonstrate the usage of the EOS Plugin for Unity's Manager classes for each Sample's domain.

Next follow [the Configuring the Plugin](/com.playeveryware.eos/Documentation~/configure_plugin.md) guidance.
Inside of Unity using the top bar, navigate to `Tools` -> `EOS Plugin` -> `EOS Configuration`.

<img src="/com.playeveryware.eos/Documentation~/full_guide/plugin_eosconfiguration.png" alt="The EOS Configuration popup can be filled out using information in the Product Settings page."/>

All of theses values can be pulled from the `Product Settings` page and inner dialogues within the EOS Developer Portal.
One of the fields is your `Encryption Key`. This can remain its default value for now. This value will not be found in the EOS Developer Portal.
For more information on the use of the Encryption Key, see [Epic's documentation on Title Storage Interface](https://dev.epicgames.com/docs/game-services/title-storage).
Essentially this value is used to encrypt Player Data Storage and Title Data Storage uploads, and needs to be consistent in order to decrypt those values when they are later downloaded.

It is recommended that you use the `Dev` Sandbox and `Dev Deployment` Sandbox while configuring the plugin.
See [Epic's documentation on Sandboxes and Deployments](https://dev.epicgames.com/docs/dev-portal/product-management#sandboxes-and-deployments) for more information on Sandboxes and Deployments.
By using the Dev environment information, the EOS Plugin for Unity will use this environment when no other environment is assumed.
If your game is deployed through the Epic Game Store, the game will be launched with `-epicsandboxid` and `epicdeploymentid` arguments, which the EOS Plugin for Unity will use instead of your configured Sandbox and Deployment.

# Starting to Use the Samples

At this moment your game is set up to utilize the EOS Plugin for Unity.
The samples are documented [in the EOS Plugin for Unity Walkthrough documentation](https://github.com/PlayEveryWare/eos_plugin_for_unity/blob/stable/com.playeveryware.eos/Documentation~/Walkthrough.md), which leads to individual Scene walk throughs.
Assuming your Client Policy is set up to be permissive, consider validating the plugin inclusion by using [the Lobbies Sample](/com.playeveryware.eos/Documentation~/scene_walkthrough/lobbies_walkthrough.md).
Open the Lobbies sample in the scene, and start running the game. Note if there are any errors in the logs from the EOS SDK Plugin.

The Lobbies sample is chosen because, assuming a permissive Client Policy, there's no needed further set up in the Epic Developer Portal.
If you successfully log in to the Lobbies Sample, create a Lobby to demonstrate that everything is connected appropriately.