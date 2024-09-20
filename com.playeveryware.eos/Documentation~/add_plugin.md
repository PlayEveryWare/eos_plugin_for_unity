<a href="/com.playeveryware.eos/README.md"><img src="/com.playeveryware.eos/Documentation~/images/PlayEveryWareLogo.gif" alt="Readme" width="5%"/></a>

# Adding via Package Manager

The following document outlines the two methods with which you can add the plugin release via the package manager.

## Adding the package from a git URL

1. Install [git](https://docs.unity3d.com/2021.3/Documentation/Manual/upm-git.html#req) and [git-lfs](https://docs.unity3d.com/2021.3/Documentation/Manual/upm-git.html#req).
2.  From the Unity Editor, open the Package Manager. `Window -> Package Manager`.

    ![unity tools package manager](/com.playeveryware.eos/Documentation~/images/unity_tools_package_manager.gif)

3. Click the `+` button in the top left of the window.

    ![Unity Add Git Package](/com.playeveryware.eos/Documentation~/images/unity_package_git.gif)

4. Select `Add Package from Git URL`.
6. Paste in `git@github.com:PlayEveryWare/eos_plugin_for_unity_upm.git`.
7. After the package has finished installing, [import the samples](/com.playeveryware.eos/Documentation~/samples.md).
8. Finally, [Configure the Plugin](/com.playeveryware.eos/Documentation~/configure_plugin.md).

> [!NOTE]
> For additional details, see the [Unity Documentation for adding a UPM via Git URL](https://docs.unity3d.com/2021.3/Documentation/Manual/upm-ui-giturl.html).

## Adding the package from a tarball

1. Download the latest release UPM tarball, `"com.playeveryware.eos-[version].tgz"` ([Releases](https://github.com/PlayEveryWare/eos_plugin_for_unity/releases)).

    > [!WARNING]
    > Do *not* attempt to create a tarball yourself from the source, unless you know what you are doing with respect to [Git LFS](https://docs.github.com/en/repositories/working-with-files/managing-large-files/configuring-git-large-file-storage).

2. Move the downloaded tarball into your project folder, but outside of the `Assets` folder.

3. From the Unity Editor, open the Package Manager via `Window -> Package Manager`.

      ![unity tools package manager](/com.playeveryware.eos/Documentation~/images/unity_tools_package_manager.gif)

4. Click the `+` button in the top left of the window.

    ![Unity Add Tarball Package](/com.playeveryware.eos/Documentation~/images/unity_package_tarball.gif)

5. Select `Add package from tarball`.
6. Navigate to the directory containing the tarball, select and `Open` the tarball.
7. After the package has finished installing, [import the samples](/com.playeveryware.eos/Documentation~/samples.md).
8. Finally, <a href="#configuring-the-plugin">configure the plugin</a>.

> [!NOTE]
> For additional details, see the [Unity Documentation for adding a UPM via Tarball](https://docs.unity3d.com/2021.3/Documentation/Manual/upm-ui-tarball.html).

