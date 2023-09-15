<a href="/readme.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="Lobby Screenshot" width="5%"/></a>

<div align="center"> <img src="/docs/images/EOSPluginLogo.png" alt="PlayEveryWare EOS Plugin for Unity" /> </div>
<br /><br /><br />

---



# <div align="center">$\textcolor{deeppink}{\textsf{Linking EOS Library on Android Settings}}$</div> <a name="linking-eos-library-on-android-settings" />
---

## What is the Difference Between Linking the EOS Library Dynamically and Statically?

Static linking packs libraries into the executable, whereas dynamic linking links the function symbols to the corresponding entry point in the dynamic libraries at runtime.

The full expression of this setting is to determine how the EOS Library links `against the C++ Library`.  

If the game is using other libraries that also links to the C++ Library, the EOS Library should use the version that matches the linking type of the others, or else duplicate symbols would occur.  

<br />

## Steps to Swap Dynamically and Statically

1. Open Preferences, ```Edit -> Preferences...```.

    ![EOS Config UI](/docs/images/preferences_menu.gif)

2. Select ```EOS Plugin``` from the categories menu on the left.

    ![EOS Config UI](/docs/images/link_eos_lib_instructions.gif)

3. Under the ```Android Build Settings``` section, check the box next to ```Link EOS Library Dynamically``` to link the EOS library dynamically, and uncheck it link the EOS library statically.

4. Press ```Save All Changes```.

<br />
