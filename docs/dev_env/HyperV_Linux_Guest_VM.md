<a href="/README.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="README.md" width="5%"/></a>

# <div align="center">Setting up a Hyper-V Linux Guest VM</div>
---

## Overview

This document describes how to create a Ubuntu Virtual Machine on Hyper-V suitable for making contributions to the EOS Plugin for Unity project on Linux.

## Step 1: Enabling Virtualization

1. Turn on Hyper-V. Hit the windows key and type in "Turn windows features on or off."

    > [!NOTE] 
    > The follow screenshots are taken in Windows 11, so if you are running Windows 10 yours might look a little differently.

    <img src="/docs/images/hyperv_linux_guest_vm/windows-features-search.png" width="350" />

2. Make sure you select all of the options below.

    <img src="/docs/images/hyperv_linux_guest_vm/windows-features-on-off.png" width="400" />

    If these options are greyed out, you may need to enter your bios settings to enable virtualization. See [here](https://www.bleepingcomputer.com/tutorials/how-to-enable-cpu-virtualization-in-your-computer-bios/) for general instructions on how to accomplish this.

    You can also tell if Virtualization is enabled in your BIOS by looking at the CPU Performance tab in Task Manager.

    <img src="/docs/images/hyperv_linux_guest_vm/task-manager-virtualization-check.png" width="500" />

    If it says "Disabled" instead of "Enabled," (like it does here) you will need to enable it in your BIOS.

3. You will be prompted to restart your computer, go ahead and do so.

## Step 2: Set up Hyper-V Network Switch

Setting up network configuration can be a little bit confusing if you're doing it for the first time. From a high-level view, consider that a virtual machine needs a way to connect to the internet, and this is the process that facilitates that capability. It does so by taking your default internet connection and pretending that it's actually two connections - one that your host computer gets to use, and one that your virtual machine gets to use.

1. After restarting your computer, open the Hyper-V Manager, and configure the network by opening the "Virtual Switch Manager..." in the right-hand "Actions" bar.

    <img src="/docs/images/hyperv_linux_guest_vm/virtual-switch-manager.png" width="150" />

2. If there _isn't_ **already** a "Default Switch" listed in the Virtual Switch Manager, go ahead and select "External," and "Create Virtual Switch," naming the switch "Default Switch."

    > [!NOTE]
    > Make sure to check "Allow management operating system to share this network adapter."

    <img src="/docs/images/hyperv_linux_guest_vm/external-network.png" width="175" />

    When it's done it should look like this:

    ![](/docs/images/hyperv_linux_guest_vm/finished-virtual-switch.png)

## Step 3: Disable Enhanced Session

The "Enhanced Session" feature of Hyper-V doesn't play very nicely with Ubuntu, you will want to disable it:

Go to Hyper-V Manager Actions bar and select "Hyper-V Settings..." and disable the "Use enhanced session mode."

<img src="/docs/images/hyperv_linux_guest_vm/enhanced-session.png" width="520" />

## Step 4: Creating a virtual machine to run Ubuntu 18.04

Ubuntu 18.04 is the official version of Linux that is supported by both EOS, the EOS Plugin, and the versions of unity the plugin supports. It might not be limited to this version, but it is certainly the most widely used, so it is best to perform testing on at least this platform.

1. Open Hyper-V Manager and select "Quick Create..." from the actions bar on the right-hand side.

    <img src="/docs/images/hyperv_linux_guest_vm/quick-create.png" width="537" />

2. Select "Ubuntu 18.04 LTS" and make sure that the Network switch selected is set to the virtual switch you created earlier.

    <img src="/docs/images/hyperv_linux_guest_vm/ubuntu-18.04%20LTS.png" width="600" />

3. Click "Create Virtual Machine" and wait for the process to complete.
   
4.  Click `Edit Settings`

    <img src="/docs/images/hyperv_linux_guest_vm/edit-settings.png" width="450" />

5. Select the hard drive in the left panel, and click `Edit`

    <img src="/docs/images/hyperv_linux_guest_vm/vm-settings.png" width="450" />
    
6. Expand the drive to _at least_ 40GB

    <img src="/docs/images/hyperv_linux_guest_vm/expand.png" width="450" />

7. Once the Virtual Machine is created, connect to the virtual machine to complete the installation of Ubuntu.
    * When you get to the user creation portion of installation, make sure you do NOT enable "Log in automatically," as this will disable some of the capabilities that make using Hyper-V nice.

## Optional:

Set up a shared folder between Linux and Windows. Follow these instructions: [https://linuxhint.com/shared\_folders\_hypver-v\_ubuntu\_guest/](https://linuxhint.com/shared_folders_hypver-v_ubuntu_guest/)

If you would like to increase the Hyper-V display resolution, see [here](https://superuser.com/questions/518484/how-can-i-increase-the-hyper-v-display-resolution#:~:text=1%20Install%20linux-image-extras%20%28hyperv-drivers%29%3A%20sudo%20apt-get%20install%20linux-image-extra-virtual,%28restarting%20Ubuntu%20%28Linux%29%20might%20be%20enough%29%20More%20).

## Next Steps

Move on to the [instructions](/docs/dev_env/Ubuntu_Development_Environment.md) to setup Ubuntu.
