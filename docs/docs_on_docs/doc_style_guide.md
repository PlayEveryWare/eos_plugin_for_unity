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

The purpose of this document is to describe a standard which all other documents should follow. Use and follow this guide when creating new documentation for the project.

<br />

# <div align="center">$\textcolor{deeppink}{\textsf{Getting Started}}$</div> <a name="getting-started" />
---

## Prerequisites

GitHub is the primary place where documentation for this project will be consumed. In order to contribute to documentation you must have access to GitHub, and (optionally) an offline markdown editor so you can see a visual representation of the document that you are writing. 

Please be aware that *not all Markdown engines work the same*. This style guide considers the rendering of the GitHub markdown renderer to be the standard, so before submitting documentation please make sure it conforms to the style guide _as viewed through GitHub_.

# <div align="center">$\textcolor{deeppink}{\textsf{Document Header}}$</div> <a name="doc-header" />
---

## Logo:

Each document should start (before the document title) with the PlayEveryWare, Inc. logo, doubling as a link to the main README file for the project.

Markdown:
```markdown
<a href="/readme.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="PlayEveryWare, Inc. Logo" width="5%"/></a>
```

What it looks like:

<a href="/readme.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="PlayEveryWare, Inc. Logo" width="5%"/></a>

## Document Title:

Following the PlayEveryWare, Inc. logo should be the title of the document, centered in pink text. The `name` attribute of the link should be the document title, all lowercase, with spaces replaced with dashes. This enables the section to be specifically linked to.

As an example of how to properly add a title to the document, below is the markdown used to create the header for _this_ document.
Markdown:
```markdown
# <div align="center">$\textcolor{deeppink}{\textsf{Documentation Style Guide}}$</div> <a name="documentation-style-guide" />
```

## Table of Contents:

If the document is sufficiently long as to warrant a table of contents, it should immediately follow the document title. To create a table of contents link that points to a location internal to the document, use the value illustrated above in the header (in this case `doc-title`), prefaced with a pound symbol. 

As an example, the following is the markdown to create the table of contents as it exists at the top of *this* document:

Markdown:
```markdown
Table of Contents
1. [Overview](#overview)
2. [Getting Started](#getting-started)
    * [Prerequisites](#prerequisites)
3. [Templates](#templates)
4. [Internal Markdown Reference](#internal-markdown-reference)
5. [Source Code Contributor Notes](#source-code-contributor-notes)
```

## Document Sections:

Each main section of the document should be denoted by the following markdown. Please note that it is important to have your first main section after the table of contents be an "Overview" section that gives the reader a summary of the purpose of the document.

Example markdown for creating a document section:

```markdown
# <div align="center">$\textcolor{deeppink}{\textsf{Section Title}}$</div> <a name="section-title" />
---
```

Note that the markdown used for the main sections of the document is the same as the markdown for the main document title, with the addition of a line beneath the text.

Major sections should have headers to break up the important pieces of it, as an example, in the main readme, the getting started section includes sub headers Prerequisites, Importing the Plugin, Samples, Configuring the Plugin, and Disable on selected platforms, these are denoted by using a header with two pound signs, ##, and notably goes right into the text with no following break. these should be important enough to be linked in the table of contents under their main header, if a table of contents exists

## Document Subsections:

As it becomes appropriate, break down the document sections into subsections to break up the components of the section. Note for instance that this is itself a subsection. As an example, the mardown for the subsection title above this text is as follows:

```markdown
## Document Subsections
```

### Document Sub Subsections:
<br />

Should a subsection of the document need to be further broken up into discrete sections, it can be so divided by putting the components beneath a header prefaced with three pound signs, and followed by a line-break. As an example, the markdown for _this_ sub subsection is as follows:

```markdown
### Document Sub Subsection:
<br />
```

In most cases, if you are dividing a document into sub-subsections, it is a sign that you need to rethink the structure of your document to make it more linear. 

>[!WARNING]
>**Ideally most documents should have a maximum depth of 2**

## Images:

Images are used in a few different ways,
* Instructional references, like images of dropdown menus or hilighted parts of windows
* Introductory images, to make it clear or preview what what is being referenced in a section

An image can be displayed with the web link format, prefacing with an exclamation mark. While the text is generally hidden we want it to be informative incasew the image doesnt load.

Markdown example:

```markdown
![unity tools package manager](docs/images/unity_tools_package_manager.gif)
```

What it looks like:

![unity tools package manager](/docs/images/unity_tools_package_manager.gif)


If an image needs to be formated in the document, such as the logo on readme pages, then the html format is acceptable, but preferably avoided.

## Links:

Web links can be written but surroding the text you want to link in brackets, followed by the link in parentasies. when linking to a header within the same document, the link can consist of just the poundsign followed by the header name, which is either specifically writtern, or just the header in lower case with '-' in place of ' '. when linking to another internal document, the base folder can be the start of the link, so '/docs/android/readme_android.md' would be an acceptable link, a noteable exception is that the readme is located at just '/readme.me' from other docs. additionally you can link to a specifc area in another document by adding the pundsign and name at the end of the link, '/readme.md#prerequisites'. when ending a sentance with a link, make sure the period is not included in the link, as this will help prevent confusion about what is and isnt linked.

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

## Codeblocks:

For inline code formatting, use single ticks (`\```). This is useful to highlight certain words to indicate that they are variables, or to clearly identify things like menu paths to follow.

In order to display code blocks, put the code you wish to display between two lines containing only three ticks ('```'). For code blocks, make sure to add to the first set of three ticks the language that the code snippet is in, so that syntax highlighting is accomplished (for instance you can use `cs` to indicate that the block is C#, or `markdown` to indicate that it's a code snippet in markdown).

When writing inline instructions, such as menu navigation, it should use the inline code block, and consist of the action names seperated by a ' -> ' (spaces included).

Example markdown:

```markdown
to create a new c# script in unity navigate the menus through ```Assets -> Create -> C# Script```.
```

What it looks like:

To create a new c# script in unity navigate the menus through ```Assets -> Create -> C# Script```.

## Lists:

bullet points can be used when listing important information that has no particular order and created by adding '*' at the front of the line. these should be prefaced with either the important content, or major portion of a sub header headers depending on location and importance of list.

#### Things:
* something
* something else



Ordered lists (such as a set of steps to perform in a particular order) should always be numbered, whereas lists that merely enumerate a set of options or items should be bulletted. 
While numbers should be used when for an ordered list, such as instructions, using numbers for a regular list or a list that does not specifically attach each line to a specific number. these should be prefaced with either the important content, or major portion of a sub header headers depending on location and importance of list.

Markdown for creating an unordered list:

```markdown
#### Things:
* something
* something else
```

Markdown for creating an ordered list:

```markdown
#### How to Use EOS:
1. try the samples
2. integrate into your own game
3. let even more people play your game.
```

it is okay to have sections and subsections that entierly point to another document sometimes information is so big it needs its own document, while still being important enough thatwe should point out its existence explicitly in a more important document, such as many docs having an FAQ header.

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

collapsed sections can be used to to hold relevant information that may otherwise be optional, similar inofrmation presented differnetly, conditoinally important information that might otherwise make the document too long for for the average reader. notably, while a header can be put within a colapsed section, the results of linking to it wont open the section so visually fails for a reader. its recomended that a header be made before sections witha colapseable place. html formating is used to achieve this feature. its best to have a newline immedietly after the colapsing text.

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

notes are ocasionally desired to include in a document, they should be used for important edge cases, or reminder information, that would otherwise overbulk a document, but is still key info for the many people outside of the core usage path. it is denoted by the '>' character followed by the :heavy_exclamation_mark: emoji, the emoji should be spelled out, and not coped in as a caracter, for better rendering across markdown viewers. additionally the note should be indented to be in line with the line that the note is related to, to have quicker readability when a note is applicable to things like the last instruction of the set, or at the end of the set of instructions.

> :heavy_exclamation_mark: there is another document for code style

```
> :heavy_exclamation_mark: there is another document for code style
```

grid notes are currently unused and need to be figured out how to use the effectively. they are a combination of notes and grids to make a more standout notice.

| :heavy_exclamation_mark: We love document contributions. |
|-|

```
| :heavy_exclamation_mark: We love document contributions. |
|-|
```

prereqs as a concept should be used to list all the needed info before starting the project. include plenty of links, for instance to getting started docs for an engine, or to the download of the engine version.

<br />

# <div align="center">$\textcolor{deeppink}{\textsf{Templates}}$</div> <a name="templates" />
---

you can use the templates here by copying them and replacing the content within the ```[ ]``` including the brackets themselves, with the the appropriate content. they are mainly blank space holders, so you can decide whats needed, be it a list, a ! notice, an image, or whatever. make sure to update the pink headers and table of contests if used as well. you can add and remove sections as needed, in the case of the readmes more reduction of sections than additions is expected, while the basic one is meant to  added to and removed from to whatever custom needs the particular document will have.
With the readmes, since the main one holds most of the info, it is cuurently our prefernce to write additional readmes with changes and heavy link refences to the  original, as it cuts down on the amount of time and changes needed during updates.
that prefernce includes external docs as well, which should heavily link to first party docs related to our prefernces, to keep that information as up to date as possible. such as having a link to unity's android setup steps, which we shouldnt need to rewrite, but also the link would be for the unity document with our specific unity version, not just the newest document, as that may also be different for our targeted enviornment.

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

  
