<a href="/readme.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="Lobby Screenshot" width="5%"/></a>

# <div align="center">$\textcolor{deeppink}{\textsf{Documentation Style Guide}}$</div> <a name="documentation-style-guide" />


Table of Contents
1. [Overview](#overview)
2. [Getting Started](#getting-started)
    * [Prerequisites](#prerequisites)
3. [Templates](#templates)
4. [Internal Markdown Reference](#internal-markdown-reference)
5. [Source Code Contributor Notes](#source-code-contributor-notes)


# <div align="center">$\textcolor{deeppink}{\textsf{Overview}}$</div> <a name="overview" />
---

The purpose of this document is to describe a standard by which all other documents should follow. Use and follow this guide when creating new documentation for the project.

<br />

# <div align="center">$\textcolor{deeppink}{\textsf{Getting Started}}$</div> <a name="getting-started" />
---

## Prerequisites

GitHub is the primary place where documentation for this project will be consumed. In order to contribute to documentation you must have access to GitHub, and (optionally) an offline markdown editor so you can see a visual representation of the document that you are writing. 

Please be aware that not all Markdown engines work the same, and the one that this guide considers to be the golden standard is the GitHub markdown renderer, so before submitting documentation, please make sure it conforms to the style guide _as viewed through GitHub_.

<a href="/readme.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="Lobby Screenshot" width="5%"/></a>

```html
<a href="/readme.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="Lobby Screenshot" width="5%"/></a>
```

then if its a readme file it should be followed by the project logo image, three new lines, and a line.

<div align="center"> <img src="/docs/images/EOSPluginImage.gif" alt="Epic Online Services Plugin for Unity" /> </div>

<br /><br /><br />

---

```markdown
<div align="center"> <img src="docs/images/EOSPluginImage.gif" alt="Epic Online Services Plugin for Unity" /> </div>
<br /><br /><br />

---
```

README documents have other qualifications, such that they should provide prereqs for the platform, as well as getting started instructions to create foe that platform, frequently, modification steps for the getting started on the main readme is fine. and more.

If it isnt a readme, it should be the doc header in magenta and centered, with the relative accesable name. names should be the title in lower case with '-' replacing ' ', then followed by a line '---'.

# <div align="center">$\textcolor{deeppink}{\textsf{Doc Title}}$</div> <a name="doc-title" />
---

```markdown
# <div align="center">$\textcolor{deeppink}{\textsf{Doc Title}}$</div> <a name="doc-title" />
---
```

if apropriate, a table of contents should follow here. a link within the same doc to a header is created by putting a # in front of the name of the header, if onw wasnt set, the name is the header in lowercase with '-' replacing ' '. not every header needs to be included here, just the common ones work.

Table of Contents
1. [First](#first)
2. [Second](#second)
    * [Sub Second](#sub-second)
3. [Third Thing](#third-thing)
>  :heavy_exclamation_mark: The links wont appear here as they arent linked to real headers.

```markdown
Table of Contents
1. [First](#first)
2. [Second](#second)
    * [Sub Second](#sub-second)
3. [Third Thing](#third-thing)
```

Then we go into the main parts of the doc, starting with an overview of the doc, basically that describes what the doc is and why/how the doc is useful. this section should be marked with the magenta header named 'Overview' followed by an underline. images can be used through out to halp make things more clear.

# <div align="center">$\textcolor{deeppink}{\textsf{Overview}}$</div> <a name="overview" />
---

```markdown
# <div align="center">$\textcolor{deeppink}{\textsf{Overview}}$</div> <a name="overview" />
---
```

Major sections should have headers to break up the important pieces of it, as an example, in the main readme, the getting started section includes sub headers Prerequisites, Importing the Plugin, Samples, Configuring the Plugin, and Disable on selected platforms, these are denoted by using a header with two pound signs, ##, and notably goes right into the text with no following break. these should be important enough to be linked in the table of contents under their main header, if a table of contents exists

## Subheader

```markdown
## Subheader
```

For sections within the subheader that needed to be broken up in to major parts, such as how the samples section in the main readme includes the two major points Importing the samples, and Running the samples. these also use the double pound, ##, so that the stand out as important infor when skimming the doc, but include a line break after the emphasize the importance and differentuate from the sub header.

## Major Part of SubHeader
<br />

```markdown
## Major Part of Subheader
<br />
```

If you need to ephasize a portion of text, without needing a whole subsection or part of one, such as how the readme in the overview has parts for the plugin features, repo contents and plugin detalis, these arent substantial enough that we need to grab the attention of anyone skimming past the overview, but the are substantial enough for people skimming through the overview, and important enough that other places in docs may need to link to them, but not important enough to need a link from a table of contents. these areas are marked with four pound signs, ####, and the name ending with a collon, :.

#### Important Content in a Text Portion:

```markdown
#### Important Content in a Text Portion:
```
Images are used in a few different ways,
* projecting branding, such as the readme logo inclusion above
* instructional refferences, like images of dropdown menus or hilighted parts of windows
* introductory images, to make it clear or preview what what is being referenced in a section
* centralized shared info across docs, in some case we dont want to link a bunch of docs to a common reference point that wont change offten, but when it does would be extra hassle to change in multiple docs. in these cases displaying an image and changing the image may be prefered.

  an image can be displayed with the web link format, prefacing with an exclimation mark. while the text is generally hidden we want it to be informative incasew the image doesnt load.

  ![unity tools package manager](/docs/images/unity_tools_package_manager.gif)

  ```markdown
  ![unity tools package manager](docs/images/unity_tools_package_manager.gif)
  ```

  If an image needs to be formated in the doc, such as the logo on readme pages, then the html format is acceptable, but preferably avoided.

  Web links can be written but surroding the text you want to link in brackets, followed by the link in parentasies. when linking to a header within the same doc, the link can consist of just the poundsign followed by the header name, which is either specifically writtern, or just the header in lower case with '-' in place of ' '. when linking to another internal doc, the base folder can be the start of the link, so '/docs/android/readme_android.md' would be an acceptable link, a noteable exception is that the readme is located at just '/readme.me' from other docs. additionally you can link to a specifc area in another doc by adding the pundsign and name at the end of the link, '/readme.md#prerequisites'. when ending a sentance with a link, make sure the period is not included in the link, as this will help prevent confusion about what is and isnt linked.

[readme_android](/docs/android/readme_android.md)

  ```markdown
  [readme_android](/docs/android/readme_android.md)
  ```
  
[readme prerequisites](/readme.md#prerequisites)

  ```markdown
  [readme prerequisites](/readme.md#prerequisites)
  ```
  
[android prerequisites](/docs/android/readme_android.md#prerequisites)

  ```markdown
  [android prerequisites](/docs/android/readme_android.md#prerequisites)
  ```

Codeblocks and other factual information is display by being surrounded on both sides with three accent characters, '```', this is used for same line code. sometimes it is required for the accents to be on seperate lines for it to readpropperly, this will also expland it to a full code block, instead of inline. when writing a full code black, you can add 'cs' to the end of the accents to higlight and format the block to c#.

```markdown
i++;
```

```markdown
```i++;```
```
and
```cs
++i;
```

<!---
html format needed to be used to display the codeblack format, this HTML should realistically never be used.
-->
<pre class="highlight"><code>
```cs
++i;
```
</code></pre>

when writing inline instructions, such as menu navigation, it should use the inline code block, and consist of the action names seperated by a ' -> ' using the spaces.

to create a new c# script in unity navigate the menus through ```Assets -> Create -> C# Script```.

```markdown
to create a new c# script in unity navigate the menus through ```Assets -> Create -> C# Script```.
```

bullet points can be used when listing important information that has no particular order and created by adding '*' at the front of the line. these should be prefaced with either the important content, or major portion of a sub header headers depending on location and importance of list.

#### Things:
* something
* something else

```markdown
#### Things:
* something
* something else
```

while numbers should be used when for an ordered list, such as instructions, using numbers for a regular list or a list that does not specifically attach each line to a specific number. these should be prefaced with either the important content, or major portion of a sub header headers depending on location and importance of list.

#### How to Use EOS:
1. try the samples
2. integrate into your own game
3. let even more people play your game.

or
#### Leaderboard:
1. BurntPotato
2. xxx_wyld_xxx
3. Grant

```markdown
#### How to Use EOS:
1. try the samples
2. integrate into your own game
3. let even more people play your game.

or
#### Leaderboard:
1. BurntPotato
2. xxx_wyld_xxx
3. Grant
```

it is okay to have sections and subsections that entierly point to another doc. sometimes information is so big it needs its own doc, while still being important enough thatwe should point out its existence explicitly in a more important doc, such as many docs having an FAQ header.

grids can be used to organise large groups of related data in multiple groups. whileoccasionally being used to display a large amount of data in a smaller space to reduce the length of a document. created using '|' and '-' across multiple lines, with the headers being the first line, '-' in the second, and the data in the following ones. youll want to use spaces between the bars for better formatting.


| First | Second | Add mmore boxes to the right for more columns |
| - | - | - |
| stuff | | <- left blank |
| another | something | add more boxes bellow for more rows |
| alternates color | auto fills empty boxes -> |


```markdown
| First | Second | Add mmore boxes to the right for more columns |
| - | - | - |
| stuff | | <- left blank |
| another | something | add more boxes bellow for more rows |
| alternates color | auto fills empty boxes -> |
```

collapsed sections can be used to to hold relevant information that may otherwise be optional, similar inofrmation presented differnetly, conditoinally important information that might otherwise make the doc too long for for the average reader. notably, while a header can be put within a colapsed section, the results of linking to it wont open the section so visually fails for a reader. its recomended that a header be made before sections witha colapseable place. html formating is used to achieve this feature. its best to have a newline immedietly after the colapsing text.

#### Colapsed stuff:
<details>
  <summary><b>Find a surprise hidden here</b></summary>
<br />
Surprise!
</details>

```markdown
#### Colapsed stuff:
<details>
  <summary><b>Find a surprise hidden here</b></summary>
<br />
Surprise!
</details>
```

mermaid is the formatting we use for displaying flowcharts about the plugin. these will likely be used similarly to regular images. You can find info and examples [here](https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/creating-diagrams).

notes are ocasionally desired to include in a doc, they should be used for important edge cases, or reminder information, that would otherwise overbulk a doc, but is still key info for the many people outside of the core usage path. it is denoted by the '>' character followed by the :heavy_exclamation_mark: emoji, the emoji should be spelled out, and not coped in as a caracter, for better rendering across markdown viewers. additionally the note should be indented to be in line with the line that the note is related to, to have quicker readability when a note is applicable to things like the last instruction of the set, or at the end of the set of instructions.

> :heavy_exclamation_mark: there is another doc for code style

```
> :heavy_exclamation_mark: there is another doc for code style
```

grid notes are currently unused and need to be figured out how to use the effectively. they are a combination of notes and grids to make a more standout notice.

| :heavy_exclamation_mark: We love doc contributions. |
|-|

```
| :heavy_exclamation_mark: We love doc contributions. |
|-|
```

prereqs as a concept should be used to list all the needed info before starting the project. include plenty of links, for instance to getting started docs for an engine, or to the download of the engine version.

<br />

# <div align="center">$\textcolor{deeppink}{\textsf{Templates}}$</div> <a name="templates" />
---

you can use the templates here by copying them and replacing the content within the ```[ ]``` including the brackets themselves, with the the appropriate content. they are mainly blank space holders, so you can decide whats needed, be it a list, a ! notice, an image, or whatever. make sure to update the pink headers and table of contests if used as well. you can add and remove sections as needed, in the case of the readmes more reduction of sections than additions is expected, while the basic one is meant to  added to and removed from to whatever custom needs the particular doc will have.
With the readmes, since the main one holds most of the info, it is cuurently our prefernce to write additional readmes with changes and heavy link refences to the  original, as it cuts down on the amount of time and changes needed during updates.
that prefernce includes external docs as well, which should heavily link to first party docs related to our prefernces, to keep that information as up to date as possible. such as having a link to unity's android setup steps, which we shouldnt need to rewrite, but also the link would be for the unity doc with our specific unity version, not just the newest doc, as that may also be different for our targeted enviornment.

[Basic Template](/docs/docs_on_docs/template_basic.md)

[ReadMe Template](/docs/docs_on_docs/template_readme.md)

<br />

# <div align="center">$\textcolor{deeppink}{\textsf{Internal Markdown Reference}}$</div> <a name="internal-markdown-reference" />
---

You can find our internal Markdown reference [here](/docs/docs_on_docs/md_reference.md) for quick access, though it is still expected you use these syntax examples inline with this style guide.

<br />

# <div align="center">$\textcolor{deeppink}{\textsf{Source Code Contributor Notes}}$</div> <a name="source-code-contributor-notes" />
---

Find info on source code (and doc) contributions [here](/readme.md#configuring-the-plugin).

  
