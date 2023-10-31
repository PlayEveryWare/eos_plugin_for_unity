<a href="/readme.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="README.md" width="5%"/></a>

# <div align="center">How to debug the native code on Windows</div>
---

1. Get code for the eos samples.
2. Set build configuration to `Debug`
3. Set `SHOW_DIALOG_BOX_ON_WARN` to `1`.
2. Build the Visual Studio project.
3. Copy the DLL to a version of the exported project to debug.
4. After launch, attach to the project after the dialog box appears.
