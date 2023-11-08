<a href="/readme.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="Lobby Screenshot" width="5%"/></a>

<div align="center"> <img src="/docs/images/EOSPluginLogo.png" alt="PlayEveryWare EOS Plugin for Unity" /> </div>

# Overview

The PlayEveryWare EOS Plugin for Unity brings the free services from Epic that connect players across all platforms and all stores to Unity in an easy-to-use package. Find more information on EOS [here](https://dev.epicgames.com/en-US/services) and read the Epic docs on the services [here](https://dev.epicgames.com/docs/epic-online-services).

This repository contains the source code for development and serves as a destination for support for the [PlayEveryWare EOS Plugin for Unity (UPM Package)](https://github.com/PlayEveryWare/eos_plugin_for_unity_upm).

Out of the box, this project demonstrates (through a collection of sample scenes) each feature of the Epic Online Services SDK[^1]. The sample scenes (coupled with accompanying documentation) can be used to get an idea of how you can implement all the online features you want in your game!

See [this](/docs/plugin_advantages.md) for a more complete overview of the advantages of using EOS with Unity.

[^1]: See [here](#supported-eos-sdk-features) for which SDK features specifically are demonstrated.

> [!NOTE]
> If you are **not** interested in the _development_ of the EOS Plugin project (and instead just want to get to using it) you can follow the instructions [here](#importing-the-plugin) on how to start using the most recently released version of the EOS Plugin.

# Getting Started

## Prerequisites

* An Epic Games Account, you can sign up for [here](https://www.epicgames.com/id/register) (_although, most [features](#supported-eos-sdk-features) do not require an Epic Games Account_).
* A product configured on the [Epic Games Developer Portal](https://dev.epicgames.com/portal/).
* A Unity project to integrate the plugin into.

> [!NOTE]
> Your system should also satisfy [Unity's system requirements](https://docs.unity3d.com/2021.3/Documentation/Manual/system-requirements.html) as well as the [EOS system requirements](https://dev.epicgames.com/docs/epic-online-services/eos-get-started/system-requirements)

## Supported Platforms

For an overview of supported platforms and targetted versions of both Unity and the EOS SDK, see our [Supported Platforms](/docs/supported_platforms.md) document.

## Importing the Plugin Package

There are two options to install the package:
* Via a [tarball](/docs/add_plugin.md#adding-the-package-from-a-tarball) _(easiest to get started quickly)_.
* From a [git url](/docs/add_plugin.md#adding-the-package-from-a-git-url) _(this method has the possible advantage of keeping the plugin up-to-date, if that's something that you would prefer)_.

## Exploring Supported EOS Features

### [Supported Epic Online Services Features](/docs/eos_features.md)
### [How to import sample scenes into your project](/docs/samples.md)

## Contributor Notes

This is an open source project! We welcome you to make contributions. See our [Contributions](/docs/contributions.md) document for more information.

# Support / Contact

PlayEveryWare EOS Plugin for Unity API Documentation can be found at [here](https://eospluginforunity.playeveryware.com).

For issues related to integration or usage of the EOS Unity plugin, please create a `New Issue` under the [Issues](https://github.com/PlayEveryWare/eos_plugin_for_unity/issues) tab.

For issues related to Epic Online Services SDK, Epic Dev Portal or for general EOS SDK information, please go to [Epic Online Services Community Support](https://eoshelp.epicgames.com/).

Detailed descriptions and usage for EOS SDK Interfaces can be found at [here](https://dev.epicgames.com/docs/services/en-US/GameServices/index.html).

# FAQ

To disable the plugin for specific platforms, see [this](/docs/disable_plugin_per_platform.md) (which also explains why you might want to do this).

See [here](/docs/command_line_export.md) for a guide on how to export the plugin from the command line. 

For issues of API Level compatibility, please read our [document](/docs/dotnet_quirks.md) on .NET Quirks and Unity compatibility.

For more FAQs see [here](/docs/frequently_asked_questions.md).

If you have any outstanding questions, please bring them up in the [Discussions](https://github.com/PlayEveryWare/eos_plugin_for_unity/discussions) tab.