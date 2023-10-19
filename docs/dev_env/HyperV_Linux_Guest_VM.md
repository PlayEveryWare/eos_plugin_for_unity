# Hyper-V Linux Guest VM

## Prerequisites:

*   Turn on Hyper-V. Hit the windows key and type in "Turn windows features on or off."

Note that the follow screenshots are taken in Windows 11, so if you are running Windows 10 yours might look a little differently.

*   ![](https://t1243816.p.clickup-attachments.com/t1243816/9ab74d30-ebd9-48e5-8e6a-895fa5860033/image.png)
*   Make sure you select all of the options below.
*   ![](https://t1243816.p.clickup-attachments.com/t1243816/f42cf14e-61e4-4faf-b1a7-4c4432c198ad/image.png)

If these options are greyed out, you may need to enter your bios settings to enable virtualization. See [here](https://www.bleepingcomputer.com/tutorials/how-to-enable-cpu-virtualization-in-your-computer-bios/) for general instructions on how to accomplish this.

  

You can also tell if Virtualization is enabled in your BIOS by looking at the CPU Performance tab in Task Manager

![](https://t1243816.p.clickup-attachments.com/t1243816/b4a7036d-b45f-4a09-ac22-5de9a0c87353/image.png)

If it says "Disabled" like it does here, you will need to enable it in your BIOS

*   You will be prompted to restart your computer, go ahead and do so.

## Set up Hyper-V Network Switch:

Setting up network configuration can be a little bit confusing if you're doing it for the first time. From a high-level view, consider that a virtual machine needs a way to connect to the internet, and this is the process that facilitates that capability. It does so by taking your default internet connection and pretending that it's actually two connections - one that your host computer gets to use, and one that your virtual machine gets to use.

*   After restarting your computer, open the Hyper-V Manager, and configure the network by opening the "Virtual Switch Manager..." in the right-hand "Actions" bar.
*   ![](https://t1243816.p.clickup-attachments.com/t1243816/0329171b-59b9-4e81-8c6f-0744844a3689/image.png)
*   If there isn't already a listed "Default Switch" listed in the Virtual Switch Manager, go ahead and select "External," and "Create Virtual Switch," naming the switch "Default Switch."
*   In the subsequent part of the creation process, make sure to check "Allow management operating system to share this network adapter."
*   ![](https://t1243816.p.clickup-attachments.com/t1243816/962888aa-481c-42ce-9d57-915ca889d5e7/image.png)
*   When it's done it should look like this:
*   ![](https://t1243816.p.clickup-attachments.com/t1243816/8048194f-79dc-4735-b904-1ba9bff5ad1d/image.png)

## Disable Enhanced Session

On Windows using Hyper-V the "Enhanced Session" feature doesn't play very nicely with Ubuntu.

*   Go to Hyper-V Manager Actions bar and select "Hyper-V Settings..." and disable the "Use enhanced session mode."
*   ![](https://t1243816.p.clickup-attachments.com/t1243816/f2146df0-ade2-4aa9-af8a-b169dd347965/image.png)

## Adjust Disk Space

By default, Hyper-V will create a very small disk. After you are done setting up the VM, return to this section in order to expand the disk size.

  

First expand the virtual hard disk via Hyper-V disk manager. Your VM will need to be off and have no checkpoints saved in order to make this change. Once that is done, restart the VM and proceed to the following steps.

  

To expand the disk size, first install this package:

  

```plain
sudo apt install cloud-guest-utils
```

Then run:

```plain
sudo growpart /dev/sda 1
sudo resize2fs /dev/sda1 
```

## Creating a virtual machine to run Ubuntu 18.04:

Ubuntu 18.04 is the official version of Linux that is supported by both EOS, the EOS Plugin, and the versions of unity the plugin supports. It might not be limited to this version, but it is certainly the most widely used, so it is best to perform testing on at least this platform.

*   Open Hyper-V Manager and select "Quick Create..." from the actions bar on the right-hand side.
*   ![](https://t1243816.p.clickup-attachments.com/t1243816/af8f2cea-e2b0-46b6-9fa4-ea678718531e/image.png)
*   Select "Ubuntu 18.04 LTS" and make sure that the Network switch selected is set to the virtual switch you created earlier.
*   ![](https://t1243816.p.clickup-attachments.com/t1243816/49dc4e68-4aab-48f9-af3a-46bdc3f64788/image.png)
*   Click "Create Virtual Machine" and wait for the process to complete.
*   Once the Virtual Machine is created, connect to the virtual machine to complete the installation of Ubuntu.
*   When you get to the user creation portion of installation, make sure you do NOT enable "Log in automatically," as this will disable some of the capabilities that make using Hyper-V nice.