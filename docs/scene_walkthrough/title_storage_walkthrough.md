<a href="/README.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="README.md" width="5%"/></a>

## **Title Storage Demo**
This demo showcases the title storage interface. This functions similar to the player data storage interface, although the data accessed is game/platform specific instead of player specific. This demo allows users to search files by tag and name and then view their contents. The demo is broken up into three parts, Tags search, File Search and the File viewer:

- Tag Search
    - ``Add Tag`` Adds the tag entered into the ``Enter tag name`` field to the list of current tags.
    - ``Add Platform Tag`` adds the tag for the current platform such as ``PLATFORM_WINDOWS``.
    - ``Clear Tags`` clears all tags from the list.
    - ``Tags`` lists the current tags.
- File Search
    - ``Find Files With Tags`` searches for all files with the current list of tags.
    - ``Files`` Displays all the files from the ``Find Files With Tags `` search result.
    - ``Enter filename from Title Storage`` and ``View Contents`` allow users to view the contents of a specific file.
- File Viewer
    - ``File Contents`` Displays the contents of the selected file.

 > [!NOTE]
 > More documentation on the Title Storage interface can be found [here](https://dev.epicgames.com/docs/game-services/title-storage)