<a href="/README.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="README.md" width="5%"/></a>

# <div align="center">Configuring Ubuntu 18.04 for development</div>
---

# Overview:

This is the second part of the guide for using Hyper-V to set up a Linux environment on your Windows PC that can be utilized for testing and developing the EOS Plugin. This guide _can_ however be used on its own as a guide to setting up a new Ubuntu 18.04 operating system for making contributions to the EOS Plugin.

## Setup your Linux Guest VM in Hyper-V

If you want to set up your linux environment inside a virtual machine, and have not yet done so, follow [this](/docs/dev_env/HyperV_Linux_Guest_VM.md) guide first.

## Setting up your Linux Environment:

> [!NOTE]
> If you would like to take care of many of the following steps by simply running a script, feel free to make use of the provided [setup-linux.sh](/tools/scripts/setup-linux.sh) script. After running it, you will need to return to this guide and skip to [this](#configure-ssh-access-to-github-required) section.

After setting up the Linux Virtual Machine (outlined in the link provided in the preceding section) or (if you're not using Hyper-V) you've just set up your linux machine, there are some standard things that you can do right out of the gate that will cover a lot of the bases we will need, so take the following preliminary steps:

### Step 1: Preliminaries

1. Change the `sudo` password:

    ```bash
    sudo passwd sudo
    ```

2. Next, update and _upgrade_ the packages already installed:

    ```bash
    sudo apt-get update
    sudo apt-get upgrade -y
    ```

3. Install the `build-essential` package - it's a collection of tools that are commonly used in software development. It includes `git`, but does not install `git-lfs`, which is why that is included:

    ```bash
    sudo apt-get install build-essential -y
    ```

### Step 2: Install the Unity Hub:

Follow [these instructions](https://docs.unity3d.com/hub/manual/InstallHub.html#install-hub-linux) for how to install the Unity Hub on Ubuntu. For convenience, these instructions are replicated below:

```bash
wget -qO - https://hub.unity3d.com/linux/keys/public | gpg --dearmor | sudo tee /usr/share/keyrings/Unity_Technologies_ApS.gpg > /dev/null

sudo sh -c 'echo "deb [signed-by=/usr/share/keyrings/Unity_Technologies_ApS.gpg] https://hub.unity3d.com/linux/repos/deb stable main" > /etc/apt/sources.list.d/unityhub.list'

sudo apt update
sudo apt-get install unityhub
```

### Step 3: Configure SSH Access to GitHub (Required):

1. Follow [this](https://docs.github.com/en/authentication/connecting-to-github-with-ssh/generating-a-new-ssh-key-and-adding-it-to-the-ssh-agent) guide for creating a new SSH key.

2. Follow [this](https://docs.github.com/en/authentication/connecting-to-github-with-ssh/adding-a-new-ssh-key-to-your-github-account) guide for adding that SSH key to GitHub.

    > [!NOTE]
    > Test that the connection works by running the following command:
    >
    > ```bash
    > sudo ssh -T git@github.com 
    > ```

You'll be prompted to trust the key (type yes and hit enter). This command should congratulate you on successfully connecting, list your username, and tell you that command-line access is not enabled.

### Step 4: Clone the EOS Plugin Repository

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

### Step 5: Configuring Unity Project for Linux

Once Unity is open:

1. Go to File ➝ Build Settings
2. Switch the platform to "Dedicated Server."
3. Click on the button labeled "Player Settings..." at the bottom left.
4. Go to Tools ➝ EOS Automated Test Settings
5. Select the previously created `Builds/Server` directory for the "Test Server Directory" field.

## Help! I ran out of disk space!

If you are using Hyper-V, and failed to increase the size of the disk before installing Ubuntu, follow these steps to increase the size of the virtual disk:

1. In Hyper-V, edit the virtual machine and navigate to "Hard Drive"
    
    ![](/docs/images/ubuntu_dev_env/vm-settings.png)

2. Select "Edit"

    ![](/docs/images/ubuntu_dev_env/edit-disk.png)

1. If "Edit" is greyed out, you will need to disable Checkpoints. Navigate to Checkpoints and unselect the checkbox that says "Enable checkpoints"

    ![](/docs/images/ubuntu_dev_env/disable-checkpoints.png)

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
