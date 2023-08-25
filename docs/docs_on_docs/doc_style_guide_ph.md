<a href="/readme.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="Lobby Screenshot" width="5%"/></a>

# $\textcolor{deeppink}{\textsf{Documentation Style Guide}}$</p> <a name="documentation-style-guide" />


Table of Contents
1. [Overview](#overview)
2. [Getting Started](#getting-started)
    * [Prerequisites](#prerequisites)
3. [Templates](#templates)
4. [Internal Markdown Reference](#internal-markdown-reference)
5. [Source Code Contributor Notes](#source-code-contributor-notes)

# $\textcolor{deeppink}{\textsf{Overview}}$ <a name="overview" />
---

The purpose of this document is to describe a standard which all other documents should follow. Use and follow this guide when creating new documentation for the project.

<br />

# $\textcolor{deeppink}{\textsf{Getting Started}}$ <a name="getting-started" />
---

## Prerequisites

GitHub is the primary place where documentation for this project will be consumed. In order to contribute to documentation you must have access to GitHub, and (optionally) an offline markdown editor so you can see a visual representation of the document that you are writing. 

Please be aware that *not all Markdown engines work the same*. This style guide considers the rendering of the GitHub markdown renderer to be the standard, so before submitting documentation please make sure it conforms to the style guide _as viewed through GitHub_.

See [here](https://github.github.com/gfm/) for documentation on GitHub flavored markdown.

# $\textcolor{deeppink}{\textsf{Document Header}}$ <a name="doc-header" />
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
# $\textcolor{deeppink}{\textsf{Documentation Style Guide}}$ <a name="documentation-style-guide" />
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
# $\textcolor{deeppink}{\textsf{Section Title}}$ <a name="section-title" />
---
```

Note that the markdown used for the main sections of the document is the same as the markdown for the main document title, with the addition of a line beneath the text.

Major sections should have headers to break up the important pieces of it, as an example, in the main readme, the getting started section includes sub headers Prerequisites, Importing the Plugin, Samples, Configuring the Plugin, and Disable on selected platforms, these are denoted by using a header with two pound signs, ##, and notably goes right into the text with no following break. these should be important enough to be linked in the table of contents under their main header, if a table of contents exists

## Document Subsections:

As it becomes appropriate, break down the document sections into subsections to break up the components of the section. Note for instance that this is itself a subsection. As an example, the markdown for the subsection title above this text is as follows:

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
* Instructional references, like images of dropdown menus or highlighted parts of windows.
* Introductory images, to make it clear or preview what what is being referenced in a section.

An image can be displayed with the web link format, prefacing with an exclamation mark. While the text is generally hidden we want it to be informative in case the image doesn't load.

Markdown example:

```markdown
![unity tools package manager](docs/images/unity_tools_package_manager.gif)
```

What it looks like:

![unity tools package manager](/docs/images/unity_tools_package_manager.gif)

## Links:

Web links can be written by surrounding the text you want as the link text in brackets, followed by the URL in parentheses. 

When linking to a header within the same document, the link can consist of just the pound sign followed by the header name. 

When linking to another document, the base folder can be the start of the link, so `'/docs/android/readme_android.md'` would be an acceptable link. Additionally you can link to a specific area in another document by adding the pound sign and name at the end of the link, `'/readme.md#prerequisites'`. when ending a sentence with a link, make sure the period is not included in the link, as this will help prevent confusion about what is and is not part of the link.

Example of linking to another document:

```markdown
[readme_android](/docs/android/readme_android.md)
```

Example of linking to a specific section of another document:

```markdown
[android prerequisites](/docs/android/readme_android.md#prerequisites)
```

## Codeblocks:

For inline code formatting, use single ticks (\`\`\`). This is useful to highlight certain words to indicate that they are variables, or to clearly identify things like menu paths to follow.

In order to display code blocks, put the code you wish to display between two lines containing only three ticks ('```'). For code blocks, make sure to add to the first set of three ticks the language that the code snippet is in, so that syntax highlighting is accomplished (for instance you can use `cs` to indicate that the block is C#, or `markdown` to indicate that it's a code snippet in markdown).

When writing inline instructions, such as menu navigation, it should use the inline code block, and consist of the action names separated by a ' -> ' (spaces included).

Example markdown:

```markdown
to create a new c# script in unity navigate the menus through ```Assets -> Create -> C# Script```.
```

What it looks like:

To create a new c# script in unity navigate the menus through ```Assets -> Create -> C# Script```.

## Lists:

Ordered lists (such as a set of steps to perform in a particular order) should always be numbered, whereas lists that merely enumerate a set of options or items should be bulleted.

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

## Grids:

Often larger amounts of information need to be illustrated in the documentation. Information like this might be well suited to a table format. See the below example of markdown for an example of how to create a grid in markdown:

```markdown
| First | Second | Add more boxes to the right for more columns |
| - | - | - |
| stuff | | <- left blank |
| another | something | add more boxes bellow for more rows |
| alternates color | auto fills empty boxes -> |
```

What it looks like:

| First | Second | Add more boxes to the right for more columns |
| - | - | - |
| stuff | | <- left blank |
| another | something | add more boxes bellow for more rows |
| alternates color | auto fills empty boxes -> |

## Collapsed Sections:

When there is a large amount of information that may or may not be immediately pertinent to the documentation, it is wise to include it in a collapsed section of the document, making it clear that the information exists, albeit in a collapsed manner. This helps avoid a situation where too much information of variable utility is displayed on the screen, while still making the information accessible if needed.

Markdown for collapsing content:

```markdown
#### Collapsed stuff:
<details>
  <summary><b>Find a surprise hidden here</b></summary>
<br />
Surprise!
</details>
```

What it looks like:

#### Collapsed stuff:
<details>
  <summary><b>Find a surprise hidden here</b></summary>
<br />
Surprise!
</details>

## Mermaid:

Mermaid is the formatting we use for displaying flowcharts about the plugin. From the perspective of style guidelines, these flowcharts can be thought of as images, with the added functionality that they are interactive. [here](https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/creating-diagrams).

## Banners:

Banners (or alerts) are an extension of Markdown used to emphasize critical information. On GitHub, they are displayed with distinctive colors and icons to indicate the importance of the content.

**An example of all three types:**
```markdown
> [!NOTE]
> Highlights information that users should take into account, even when skimming.

> [!IMPORTANT]
> Crucial information necessary for users to succeed.

> [!WARNING]
> Critical content demanding immediate user attention due to potential risks.
```

*Here is how they are displayed:*

> [!NOTE]
> Highlights information that users should take into account, even when skimming.

> [!IMPORTANT]
> Crucial information necessary for users to succeed.

> [!WARNING]
> Critical content demanding immediate user attention due to potential risks.

<br />

# $\textcolor{deeppink}{\textsf{Templates}}$ <a name="templates" />
---

You can use the templates here by copying them and replacing the content within the ```[ ]``` including the brackets themselves, with the the appropriate content. 

They are mainly blank space holders, so you can decide whats needed, be it a list, a ! notice, an image, or whatever. 

Make sure to update the pink headers and table of contests if used as well. you can add and remove sections as needed, in the case of the readmes more reduction of sections than additions is expected, while the basic one is meant to  added to and removed from to whatever custom needs the particular document will have.
With the readmes, since the main one holds most of the info, it is currently our preference to write additional readmes with changes and heavy link references to the  original, as it cuts down on the amount of time and changes needed during updates.

That preference includes external docs as well, which should heavily link to first party docs related to our preferences, to keep that information as up to date as possible. such as having a link to unity's android setup steps, which we shouldn't need to rewrite, but also the link would be for the unity document with our specific unity version, not just the newest document, as that may also be different for our targeted environment.

[Basic Template](/docs/docs_on_docs/template_basic.md)

[ReadMe Template](/docs/docs_on_docs/template_readme.md)

<br />

# $\textcolor{deeppink}{\textsf{Internal Markdown Reference}}$ 
---

You can find our internal Markdown reference [here](/docs/docs_on_docs/md_reference.md) for quick access, though it is still expected you use these syntax examples inline with this style guide.

<br />

# $\textcolor{deeppink}{\textsf{Source Code Contributor Notes}}$ <a name="source-code-contributor-notes" />
---

Find info on source code (and doc) contributions [here](/readme.md#configuring-the-plugin).