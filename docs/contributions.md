<a href="/README.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="Readme" width="5%"/></a>

# Contributor Notes

The following are guidelines for making contributions to this open-source project, as well as guidance on setting up your development environment to do so.

## Environment Setup

### Linux

The following two guides can help you set up your development environment on Windows using Hyper-V. If you are not using Hyper-V, the second guide can still be used to configure your environment.

  #### [Hyper-V Linux Guest VM](/docs/dev_env/HyperV_Linux_Guest_VM.md)
  #### [Configuring Ubuntu 18.04](/docs/dev_env/Ubuntu_Development_Environment.md)

### Windows

To setup your environment on windows, follow these steps (or you can run the script indicated at the end of this section):

1. Install the following:
    - [git](https://git-scm.com/downloads)
    - [Unity Hub](https://unity.com/download)
    - [Visual Studio 2019 Community Edition](https://visualstudio.microsoft.com/vs/older-downloads/)

2. Clone this repository and be sure to also run `git lfs pull` from the root of the repository.

3. Sign in to Unity Hub, and locate a project on disk by navigating to your local copy of the repository.

4. After adding the plugin project to Unity Hub, you will see a little caution sign next to the project if you do not currently have the proper version of the Unity Editor installed. This is expected. Click on the caution symbol and follow the prompts to install the appropriate version of the Unity Editor.

> [!NOTE]
> You can execute the following PowerShell command in an elevated window to run the setup script which should do everything for you:
> ```powershell
> cd [root of repository]
> Set-ExecutionPolicy RemoteSigned -Force
> .\tools\scripts\setup-windows.ps1
> ```

### macOS

See [here](/docs/macOS/README_macOS.md) to read our guide on setting up your environment on macOS.

You can run [this](/tools/scripts/setup-macos.sh) script `tools/scripts/setup-macos.sh` from a terminal to accomplish most of the setup steps, or read the aforementioned guide for details.

## Building Native Libraries

 Build the Visual Studio solutions for the native DLLs (extra platform specific instructions may be located in the docs for that platform).

1. In your local repository, navigate to the `lib/NativeCode/DynamicLibraryLoaderHelper_[PLATFORM]` folder of your platform choice in [NativeCode](/lib/NativeCode).

   > [!WARNING]
   > These files are not included with the package imported via tarball or git url.

2. Open and build the `DynamicLibraryLoaderHelper.sln` in Visual Studio.

A successful build will place the correct binaries in the proper locations for Unity to initialize the EOS SDK.

## Coding Standards

See [standards.md](/docs/standards.md).

## Core Classes

Descriptions for some of the core classes can be found [here](/docs/class_description.md).

## Create your own UPM

After making changes to the plugin, if you would like to subsequently generate your own Unity Package, read one of the following options:

### [Create UPM Package](/docs/creating_the_upm_package.md)
### [Command Line Package Creation](/docs/command_line_export.md)
