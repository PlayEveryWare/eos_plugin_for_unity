<a href="/readme.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="README.md" width="5%"/></a>

# <div align="center">Setting up a Hyper-V Linux Guest VM</div>

## Prerequisites:

*   Turn on Hyper-V. Hit the windows key and type in "Turn windows features on or off."

Note that the follow screenshots are taken in Windows 11, so if you are running Windows 10 yours might look a little differently.

*   ![](/docs/images/hyperv_linux_guest_vm/windows-features-search.png)
*   Make sure you select all of the options below.
*   ![](/docs/images/hyperv_linux_guest_vm/windows-features-on-off.png)

If these options are greyed out, you may need to enter your bios settings to enable virtualization. See [here](https://www.bleepingcomputer.com/tutorials/how-to-enable-cpu-virtualization-in-your-computer-bios/) for general instructions on how to accomplish this.

You can also tell if Virtualization is enabled in your BIOS by looking at the CPU Performance tab in Task Manager

![](/docs/images/hyperv_linux_guest_vm/task-manager-virtualization-check.png)

If it says "Disabled" like it does here, you will need to enable it in your BIOS

*   You will be prompted to restart your computer, go ahead and do so.

## Set up Hyper-V Network Switch:

Setting up network configuration can be a little bit confusing if you're doing it for the first time. From a high-level view, consider that a virtual machine needs a way to connect to the internet, and this is the process that facilitates that capability. It does so by taking your default internet connection and pretending that it's actually two connections - one that your host computer gets to use, and one that your virtual machine gets to use.

*   After restarting your computer, open the Hyper-V Manager, and configure the network by opening the "Virtual Switch Manager..." in the right-hand "Actions" bar.
*   ![](/docs/images/hyperv_linux_guest_vm/virtual-switch-manager.png)
*   If there isn't already a listed "Default Switch" listed in the Virtual Switch Manager, go ahead and select "External," and "Create Virtual Switch," naming the switch "Default Switch."
*   In the subsequent part of the creation process, make sure to check "Allow management operating system to share this network adapter."
*   ![](/docs/images/hyperv_linux_guest_vm/external-network.png)
*   When it's done it should look like this:
*   ![](/docs/images/hyperv_linux_guest_vm/finished-virtual-switch.png)

## Disable Enhanced Session

On Windows using Hyper-V the "Enhanced Session" feature doesn't play very nicely with Ubuntu.

*   Go to Hyper-V Manager Actions bar and select "Hyper-V Settings..." and disable the "Use enhanced session mode."
*   ![](/docs/images/hyperv_linux_guest_vm/enhanced-session.png)

## Creating a virtual machine to run Ubuntu 18.04:

Ubuntu 18.04 is the official version of Linux that is supported by both EOS, the EOS Plugin, and the versions of unity the plugin supports. It might not be limited to this version, but it is certainly the most widely used, so it is best to perform testing on at least this platform.

*   Open Hyper-V Manager and select "Quick Create..." from the actions bar on the right-hand side.
*   ![](/docs/images/hyperv_linux_guest_vm/quick-create.png)
*   Select "Ubuntu 18.04 LTS" and make sure that the Network switch selected is set to the virtual switch you created earlier.
*   ![](/docs/images/hyperv_linux_guest_vm/ubuntu-18.04%20LTS.png)
*   Click "Create Virtual Machine" and wait for the process to complete.
*   Once the Virtual Machine is created, connect to the virtual machine to complete the installation of Ubuntu.
*   When you get to the user creation portion of installation, make sure you do NOT enable "Log in automatically," as this will disable some of the capabilities that make using Hyper-V nice.
*   Move on to the [instructions](/docs/dev_env/Ubuntu_Development_Environment.md) to setup Ubuntu.