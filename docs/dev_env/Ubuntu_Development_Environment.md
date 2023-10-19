# Configuring Ubuntu 18.04

# Setting up your environment:

This is the second part of the guide for using Hyper-V to set up a Linux environment on your Windows PC that can be utilized for testing and developing the EOS Plugin. This guide can however be used on its own as a guide to setting up a new Ubuntu 18.04 operating system.

## Setup your Linux Guest VM in Hyper-V

*   Hyper-V Linux Guest VM ([https://doc.clickup.com/d/h/15yn8-1036/fc00a7ef1268280/15yn8-40524](https://doc.clickup.com/d/h/15yn8-1036/fc00a7ef1268280/15yn8-40524))

## Setting up your Linux Environment:

After setting up the Linux Virtual Machine (outlined in the section above), there are some standard things that you can do right out of the gate that will cover a lot of the bases we will need, so take the following preliminary steps:

Change the `sudo` password:

```bash
sudo passwd sudo
```

Next, update and upgrade the packages already installed:

```bash
sudo apt-get update
sudo apt-get upgrade -y
```

Then install the `build-essential` package - it's a collection of tools that are commonly used in software development. It includes `git`, but does not install `git-lfs`, which is why that is included:

```bash
sudo apt-get install build-essential -y
```

#### Optional:

Set up a shared folder between Linux and Windows. Follow these instructions: [https://linuxhint.com/shared\_folders\_hypver-v\_ubuntu\_guest/](https://linuxhint.com/shared_folders_hypver-v_ubuntu_guest/)

If you would like to increase the Hyper-V display resolution, see [here](https://superuser.com/questions/518484/how-can-i-increase-the-hyper-v-display-resolution#:~:text=1%20Install%20linux-image-extras%20%28hyperv-drivers%29%3A%20sudo%20apt-get%20install%20linux-image-extra-virtual,%28restarting%20Ubuntu%20%28Linux%29%20might%20be%20enough%29%20More%20).

### Install the Unity Hub:

Follow [these instructions](https://docs.unity3d.com/hub/manual/InstallHub.html#install-hub-linux) for how to install the Unity Hub on Ubuntu. For convenience the instructions are replicated below:

```bash
wget -qO - https://hub.unity3d.com/linux/keys/public | gpg --dearmor | sudo tee /usr/share/keyrings/Unity_Technologies_ApS.gpg > /dev/null

sudo sh -c 'echo "deb [signed-by=/usr/share/keyrings/Unity_Technologies_ApS.gpg] https://hub.unity3d.com/linux/repos/deb stable main" > /etc/apt/sources.list.d/unityhub.list'

sudo apt update
sudo apt-get install unityhub
```

### Configure SSH Access to GitHub (Required):

First follow [this](https://docs.github.com/en/authentication/connecting-to-github-with-ssh/generating-a-new-ssh-key-and-adding-it-to-the-ssh-agent) guide for creating a new SSH key.

Then, follow [this](https://docs.github.com/en/authentication/connecting-to-github-with-ssh/adding-a-new-ssh-key-to-your-github-account) guide for adding that SSH key to GitHub.

Test that the connection works by running the following command:

```bash
sudo ssh -T git@github.com 
```

You'll be prompted to trust the key (type yes and hit enter). This command should congratulate you on successfully connecting, list your username, and tell you that command-line access is not enabled.

### Clone the EOS Plugin Repository

It is important that you have the latest version of `git` installed, so that you can use `git lfs`. See [here](https://itsfoss.com/install-git-ubuntu/) for an excellent guide an installing the latest version of `git`

> [!NOTE]
> Simply installing `git` via apt is not sufficient.

```bash
git clone git@github.com:PlayEveryWare/eos_plugin_for_unity_restricted
```

It is important that you follow the command format above instead of using something like [`https://github.com`](https://github.com), because it will otherwise fail. The format guarantees that the SSH protocol is used.

Once the repository is cloned, pull the lfs files with the following command from inside the repository:

```bash
git lfs pull
```

Create a directory in the root of the project called `Builds`, and inside that directory create two directories: `Server` and `Normal`. These will be utilized later.

## Configuring Unity Project for Linux

Once Unity is open:

Go to File ➝ Build Settings

Switch the platform to "Dedicated Server."

Click on the button labeled "Player Settings..." at the bottom left.

Go to Tools ➝ EOS Automated Test Settings
Select the previously created `Builds/Server` directory for the "Test Server Directory" field.

## How to expand Hyper-V disk space for Ubuntu

This is for after you have Ubuntu installed. It is recommended to set your disk space to an appropriate size before you install the OS.

1. In Hyper-V, edit the virtual machine and navigate to "Hard Drive"![](https://t1243816.p.clickup-attachments.com/t1243816/85c20f15-b004-430f-91d0-ff246f9d783d/image.png)
2. Select "Edit"

![](https://t1243816.p.clickup-attachments.com/t1243816/3ff2332d-5b0d-4b6a-8761-3b458d549472/image.png)

1. If "Edit" is greyed out, you will need to disable Checkpoints. Navigate to Checkpoints and unselect the checkbox that says "Enable checkpoints"

![](https://t1243816.p.clickup-attachments.com/t1243816/585bc3d7-b2fd-4ce6-9b4b-b5bb28cf8912/image.png)

3. The edit window will ask you to locate the disk. This should already be filled out. Select "Next"
4. Choose "Expand"
5. Enter in the desired size. 40 GB is a recommended minimum for this project. Select "Next"
6. Verify the information is correct in the Summary and select "Finish"
7. Run the VM and open the Terminal. 
8. Run the following command to find the name of your partition, the right one will be the biggest partition. Mine is `/dev/sda1`, replace that in the commands with yours.

```bash
sudo fdisk -l
```

9. To expand the partition and the file system to use the new space, run the following commands:

```bash
sudo apt install cloud-guest-utils

sudo growpart /dev/sda 1
# Note the space between `sda` and `1` that is important

sudo resize2fs /dev/sda1
# Note no space this time!
```

10. Now you should have the extra space available to you. If this doesn't work, you can find some other suggestions [here](https://superuser.com/questions/1716141/how-to-expand-ubuntu-20-04-lts-filesystem-volume-on-hyper-v).
