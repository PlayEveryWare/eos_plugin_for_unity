<a href="http://playeveryware.com"><img src="/docs/images/PlayEveryWareLogo.gif" alt="Lobby Screenshot" width="10%"/></a>

# <div align="center">$\textcolor{deeppink}{\textsf{Documentation Style Guide}}$</div><a name="documentation-style-guide" />
---

**Table of Contents**
1. [Overview](#overview)
2. [Getting Started](#getting-started)
3. [Document Header](#header)
    * [Logo](#logo)
    * [Title](#title)
    * [Table of Contents](#table-of-contents)
4. [Document Body](#body)
    * [Sections](#sections)
    * [Subsections](#subsections)
    * [Sub-Subsections](#sub-subsections)
5. [See also](#see-also)
    * [Templates](#templates)
    * [Internal Markdown Reference](#internal-markdown-reference)
    * [Contributor Notes](#contributor-notes)

# <div align="center">$\textcolor{deeppink}{\textsf{Overview}}$</div> </div><a name="overview" />

There are two main types of documentation for this project: normal documentation (like this document) and README documentation. README documentation style guide is a sub-set of the documentation style guide. Read more about that [here](readme-style.md).

The purpose of this document is to describe a standard which all other documents should follow. Use and follow this guide when creating new documentation for the project.

While this document does give examples for some of the most frequently utilized components of markdown, the intent is to describe *how* those components should be utilized in this project, not necessarily how to *implement* them.

As with most things in life, this document serves as a guide, not a rule book. For the most part it should be strictly followed, but a reasonable amount of deviation is permissable so long as the goal of proper communication is accomplished.

# <div align="center">$\textcolor{deeppink}{\textsf{Getting Started}}$ </div><a name="getting-started" />

GitHub is the primary place where documentation for this project will be consumed. Therefore be sure to write your documentation using [GFM (GitHub Flavored Markdown)](https://github.github.com/gfm/).  

Please be aware that *not all Markdown engines work the same*. This style guide considers the rendering of the GitHub markdown renderer to be the standard, so before submitting documentation please make sure it conforms to the style guide _as viewed through GitHub_.

You may find a [GFM Cheat Sheet](https://gist.github.com/roshith-balendran/d50b32f8f7d900c34a7dc00766bcfb9c) to be helpful.

Understanding how to use GFM should be considered a prerequisite to contributing documentation to the project.

# <div align="center">$\textcolor{deeppink}{\textsf{Organizational Structure}}$ </div><a name="structure" />

Fundamentally (and very broadly speaking) every document should contain two or optionally three components:

| Component | Purpose |
| -: | :- |
| Header | _Title, logo, table of contents, and overview (table of contents only as needed)_ |
| Body | _This should contain the meat of the document wherein each concept is appropriately boxed into sections._ |
| "See also" | _(Optional). This should contain links to other documents that are somewhat related in topic, or (as in this document) links to supplementary resources._ |

# <div align="center">$\textcolor{deeppink}{\textsf{Header}}$ </div><a name="header" />

## Logo:

Each document should start (before the document title) with the PlayEveryWare, Inc. logo. The image should be surrounded by a link (`<a> </a>`) tag with the `href` set to the main [README.md document](http://github.com/PlayEveryWare/eos_plugin_for_unity/README.md), and with the width of the image set to 10%.

Markdown:
```markdown
<a href="http://github.com/PlayEveryWare/eos_plugin_for_unity/README.md">
    <img src="/docs/images/PlayEveryWareLogo.gif" alt="PlayEveryWare, Inc. Logo" width="10%"/>
</a>
```

What it looks like:

<a href="http://github.com/PlayEveryWare/eos_plugin_for_unity/README.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="PlayEveryWare, Inc. Logo" width="10%"/></a>

## Title:

Following the PlayEveryWare, Inc. logo should be the title of the document in pink text, followed immediately by a horizontal rule. To accomplish the pink text color, the typesetting system [LaTeX](http://www.latex-project.org) is used. 

As an example of how to properly add a title to the document, below is the markdown used to create the header for _this_ document.
Markdown:
```markdown
# <div align="center">$\textcolor{deeppink}{\textsf{Documentation Style Guide}}$ </div> <a name="documentation-style-guide" >
---
```

## Table of Contents:

If the document is sufficiently long as to warrant a table of contents, it should immediately follow the document title, and *precede* the "Overview" section.

As an example, the following is the markdown to create the table of contents as it exists at the top of *this* document:

Markdown:
```markdown
**Table of Contents**
1. [Overview](#overview)
2. [Getting Started](#getting-started)
3. [Document Header](#header)
    * [Logo](#logo)
    * [Title](#title)
    * [Table of Contents](#table-of-contents)
4. [Document Body](#body)
    * [Sections](#sections)
    * [Subsections](#subsections)
    * [Sub-Subsections](#sub-subsections)
5. [See also](#see-also)
    * [Templates](#templates)
    * [Internal Markdown Reference](#internal-markdown-reference)
    * [Contributor Notes](#contributor-notes)
```

Note that the `url` for an internal link is the value of the `name` attribute of the corresponding section link following a pound sign.

To see this in action, [this](#source-code-contributor-notes) is a link to the last section of this document.

# <div align="center">$\textcolor{deeppink}{\textsf{Body}}$ </div><a name="body" />

## Sections:

Document sections allow for clear organization of thought within a document.

Each main section of the document should be denoted by the following markdown. Please note that it is important to have your first main section after the table of contents be an "Overview" section that gives the reader a summary of the purpose of the document. The `name` attribute of the link should be the document title, all lowercase, with spaces replaced with dashes. This enables the section to be specifically linked to. 

The document section headers share the same format as the document title, but without the horizontal bar.

Example markdown for creating a document section:

```markdown
# <div align="center">$\textcolor{deeppink}{\textsf{Section Title}}$ </div><a name="section-title" />
```

> [!NOTE]
> Document sections should be important enough to include in a table of contents, if one exists for the document.

## Subsections:

Subsections of a document allow for clear organization of thought within a section. As an example, in this document (yes, the one you are reading right now) the section "Body" has subsections "Document Sections", "Document Subsections", "Document Sub-Subsections", and "Section Summary".

As it becomes appropriate, further break down the document sections into subsections. Note for instance that this is itself a subsection. As an example, the markdown for the subsection title above this text is as follows:

```markdown
## Subsections
```

## Sub-Subsections:

Should a subsection of the document need to be further broken up into discrete sections, it can be so divided by putting the components beneath a header prefaced with two pound signs, colored `deeppink` using the aforementioned LaTeX syntax, and with an `a` link added at the end to facilitate internal document linkages.

For example:

```markdown
## $\textcolor{deeppink}{\textsf{Sub Subsection:}}}$ <a name="sub-subsection">
```

> [!NOTE]
> Just to be as clear as possible: note that the "Sub-Subsections" title above is itself a _sub section_, **not** a _**sub** subsection_.

> [!IMPORTANT] 
> Ideally documents should only ever have a maximum depth of 2. If you feel greater depth is needed, it is a sign that you need to rethink the structure of your document, or break it up into separate files.

## Section Summary:

The difference between the different section depths is below, followed by the markdown that generates it:

# <div align="center">$\textcolor{deeppink}{\textsf{Section Example}}$ </div><a name="section-example" />
## Subsection Example
## $\textcolor{deeppink}{\textsf{Sub-Subsection Example}}$ <a name="sub-subsection-example">

```markdown
# <div align="center">$\textcolor{deeppink}{\textsf{Section Example}}$ </div><a name="section-example" />
## Subsection Example
## $\textcolor{deeppink}{\textsf{Sub-Subsection Example}}$ <a name="sub-subsection-example">
```

> [!IMPORTANT]
> No section or subsection should have within it only one "child" section. If you find yourself in this situation, rewrite your section title.

## Images:

Images are to be utilized in specific circumstances:
* Instructional references, like images of dropdown menus or highlighted parts of windows.
* Introductory images, to make it clear or preview what what is being referenced in a section.

An image can be displayed with the web link format, prefacing with an exclamation mark. While the text is generally hidden we want it to be informative in case the image doesn't load.

Markdown example:

```markdown
![unity tools package manager](docs/images/unity_tools_package_manager.gif)
```

What it looks like:

![unity tools package manager](/docs/images/unity_tools_package_manager.gif)

> [!IMPORTANT] 
> Never use an image in place of text (for instance do not take a screenshot of documentation from one area to include it in another). Aside from the issue of maintainability, the problem with this is that GitHub allows for dark and light modes, and the image will look wrong in one mode or the other. 
> 
> _One notable exception is a circumstance where a small amount of text is utilized as a frequently referenced item, and in that circumstance it's important to have the first occurrence of the information be in text format so it can be easily maintained._

## Links:

Web links can be written by surrounding the text you want as the link text in brackets, followed by the URL in parentheses. 

When linking to a header within the same document, the link can consist of just the pound sign followed by the header name. 

When linking to another document, the base folder can be the start of the link, so `'/docs/android/readme_android.md'` would be an acceptable link. Additionally you can link to a specific area in another document by adding the pound sign and name at the end of the link, `'/readme.md#prerequisites'`. when ending a sentence with a link, make sure the period is not accidentally included in the url portion of the link.

Example markdown linking to a specific section within a document:

```markdown  
[Getting Started](#getting-started)
```

Example markdown linking to another document:

```markdown
[readme_android](/docs/android/readme_android.md)
```

Example markdown linking to a specific section of another document:

```markdown
[android prerequisites](/docs/android/readme_android.md#prerequisites)
```

> [!IMPORTANT]
> Periodically, a script should be run against all the documentation to check that all of the links (internal and external) are still valid. It's important to make sure links do not become stale or broken. Because of this, external links should be used sparingly.

> [!IMPORTANT]
> When renaming a document or any of the sections, make sure to search for and update any references to that document from others.

## Code:

## $\textcolor{#2f5faf}{\textsf{Block}}$ <a name="block">

For inline code formatting, use single ticks. This is useful to highlight certain words to indicate that they are variables, or to clearly identify things like menu paths to follow.

In order to display code blocks, put the code you wish to display between two lines containing only three ticks. For code blocks, make sure to add to the first set of three ticks the language that the code snippet is in, so that syntax highlighting is accomplished (for instance you can use `cs` to indicate that the block is C#, or `markdown` to indicate that it's a code snippet in markdown). See [here](https://github.com/highlightjs/highlight.js/blob/main/SUPPORTED_LANGUAGES.md) for a list of all the languages that GitHub Flavored Markdown supports.

> [!IMPORTANT]
> When you are providing a code example, it may be necessary to break coding standards for the sake of readability. One circumstance where this is particularly true is with code that would normally require horizontal scrolling to fully view. If a line of code within the codeblock exceeds 130 characters, be sure to add line breaks following  [this](https://se-education.org/guides/conventions/csharp.html#2-maximum-line-length-is-130-characters) guide.

## $\textcolor{#2f5faf}{\textsf{Inline}}$ <a name="inline">

When writing inline instructions, such as menu navigation, use the inline code block, and separate action names by an arrow ` -> ` (spaces included for increased legibility). Inline code blocks are also appropriate to use when (in normal a normal sentence) you are referencing a variable.

Example markdown:

```markdown
To create a new c# script in unity navigate the menus through `Assets -> Create -> C# Script`.
```

```markdown
The variable `foo` is much better than the variable `bar`.
```

What it looks like:

To create a new c# script in unity navigate the menus through `Assets -> Create -> C# Script`.

The variable `foo` is much better than the variable `bar`.

## Lists:

Ordered lists (such as a set of steps to perform in a particular order) should always be numbered, whereas lists that merely enumerate a set of options or items should be bulleted.

Markdown for creating an unordered list:

```markdown
__Things:__
* something
* something else
```

Markdown for creating an ordered list:

```markdown
__How to Use EOS:__
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

> [!WARNING] 
> Use tables only when the values within the table are linear, that is, each row of the table should pertain to one item, and all other rows should pertain to items of the same type.

## Collapsed Sections:

When there is a large amount of information that may or may not be immediately pertinent to the documentation, it may be wise to include it in a collapsed section of the document, making it clear that the information exists, albeit in a collapsed manner. This helps avoid a situation where too much information of variable utility is displayed on the screen, while still making the information accessible if needed.

Markdown for collapsing content:

```markdown
#### Collapsed stuff:
<details>
  <summary><b>Find a surprise hidden here</b></summary>
Surprise!
</details>
```

What it looks like:

#### Collapsed stuff:
<details>
  <summary><b>Find a surprise hidden here</b></summary>
Surprise!
</details>

> [!WARNING] 
> Collapsed sections have a tendency to be overlooked by readers, so be cautions about what you choose to put within them, and in many cases it might be more effective to break up your documentation into more than one file.

## Mermaid:

Mermaid is the formatting we use for displaying flowcharts about the plugin. From the perspective of style guidelines, these flowcharts can be thought of as images, with the added functionality that they are interactive. [here](https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/creating-diagrams).

## Alerts:

This section is a copy of [a discussion](https://github.com/orgs/community/discussions/16925) of these features when they were first introduced to GFM.

Alerts are an extension of Markdown used to emphasize critical information. On GitHub, they are displayed with distinctive colors and icons to indicate the importance of the content. Use the "note" alert for important details that might escape observation if someone is skimming the document, use the "important" alert to describe things that _must_ or must _not_ be done in order to succeed in the documented task, and use the "warning" alert to highlight potential risks.

**An example of all three types:**

```markdown
> [!NOTE]
> Highlights information that users should take into account, even when skimming.

> [!IMPORTANT]
> Crucial information necessary for users to succeed.

> [!WARNING]
> Critical content demanding immediate user attention due to potential risks.
```

**Here is how they are displayed:**

> [!NOTE]
> Highlights information that users should take into account, even when skimming.

> [!IMPORTANT]
> Crucial information necessary for users to succeed.

> [!WARNING]
> Critical content demanding immediate user attention due to potential risks.

# <div align="center">$\textcolor{deeppink}{\textsf{See also}}$ </div><a name="see-also" />

## Templates

You can use the templates here by copying them and replacing the content within the `[ ]` including the brackets themselves, with the the appropriate content. 

They are mainly blank space holders, so you can decide whats needed, be it a list, an alert, an image, or whatever. 

[Basic Template](/docs/docs_on_docs/template_basic.md)

[ReadMe Template](/docs/docs_on_docs/template_readme.md)

## Internal Markdown reference

You can find our internal Markdown reference [here](/docs/docs_on_docs/md_reference.md) for quick access, though it is still expected you use these syntax examples inline with this style guide.

## Contributor Notes

Find info on source code (and doc) contributions [here](/readme.md#configuring-the-plugin).