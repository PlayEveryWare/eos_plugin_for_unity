<a href="http://playeveryware.com"><img src="/docs/images/PlayEveryWareLogo.gif" alt="Lobby Screenshot" width="10%"/></a>

# <div align="center">$\textcolor{deeppink}{\textsf{Documentation Style Guide}}$</div>
---

Table of Contents
1. [Overview](#overview)
2. [Getting Started](#getting-started)
3. [Document Header](#document-header)
4. [Document Body](#document-body)
5. [Templates](#templates)
6. [Internal Markdown Reference](#internal-markdown-reference)
7. [Source Code Contributor Notes](#source-code-contributor-notes)

# <div align="center">$\textcolor{deeppink}{\textsf{Overview}}$</div> </div><a name="overview" />

There are two main types of documentation for this project: normal documentation (like this document) and README documentation. README documentation style guide is a sub-set of the documentation style guide. Read more about that [here](readme-style.md).

The purpose of this document is to describe a standard which all other documents should follow. Use and follow this guide when creating new documentation for the project.

While this document does give examples for some of the most frequently utilized components of markdown, the intent is to describe *how* those components should be utilized in this project, not necessarily how to *implement* them.

As with most things in life, this document serves as a guide, not a rulebook. For the most part it should be strictly followed, but a reasonable amount of deviation is permissable so long as the goal of proper communication is accomplished.

# <div align="center">$\textcolor{deeppink}{\textsf{Getting Started}}$ </div><a name="getting-started" />

GitHub is the primary place where documentation for this project will be consumed. Therefore be sure to write your documentation using [GFM (GitHub Flavored Markdown)](https://github.github.com/gfm/).  

Please be aware that *not all Markdown engines work the same*. This style guide considers the rendering of the GitHub markdown renderer to be the standard, so before submitting documentation please make sure it conforms to the style guide _as viewed through GitHub_.

You may find a [GFM Cheat Sheet](https://gist.github.com/roshith-balendran/d50b32f8f7d900c34a7dc00766bcfb9c) to be helpful.

Understanding how to use GFM should be considered a prerequisite to contributing documentation to the project.

# <div align="center">$\textcolor{deeppink}{\textsf{Document Header}}$ </div><a name="document-header" />

## Logo:

Each document should start (before the document title) with the PlayEveryWare, Inc. logo. The image should be surrounded by a link (`<a> </a>`) tag with the `href` set to [http://playeveryware.com](http://playeveryware.com).

Markdown:
```markdown
<a href="http://playeveryware"><img src="/docs/images/PlayEveryWareLogo.gif" alt="PlayEveryWare, Inc. Logo" width="10%"/></a>
```

What it looks like:

<a href="http://playeveryware"><img src="/docs/images/PlayEveryWareLogo.gif" alt="PlayEveryWare, Inc. Logo" width="10%"/></a>

## Document Title:

Following the PlayEveryWare, Inc. logo should be the title of the document in pink text, followed immediately by a horizontal rule. To accomplish the pink text color, the typesetting system [LaTeX](http://www.latex-project.org) is used. 

As an example of how to properly add a title to the document, below is the markdown used to create the header for _this_ document.
Markdown:
```markdown
# <div align="center">$\textcolor{deeppink}{\textsf{Documentation Style Guide}}$
---
```

## Table of Contents:

If the document is sufficiently long as to warrant a table of contents, it should immediately follow the document title, and *precede* the overview section.

As an example, the following is the markdown to create the table of contents as it exists at the top of *this* document:

Markdown:
```markdown
Table of Contents
1. [Overview](#overview)
2. [Getting Started](#getting-started)
3. [Document Header](#document-header)
4. [Document Body](#document-body)
5. [Templates](#templates)
6. [Internal Markdown Reference](#internal-markdown-reference)
7. [Source Code Contributor Notes](#source-code-contributor-notes)
```

Note that the `url` for an internal link is the value of the `name` attribute of the corresponding section link following a pound sign.

To see this in action, [this](#source-code-contributor-notes) is a link to the last section of this document.

# <div align="center">$\textcolor{deeppink}{\textsf{Document Body}}$ </div><a name="document-body" />

## Document Sections:

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

Subsections of a document allow for clear organization of thought within a section. As an example, in this document (yes, the one you are reading right now) the section "Document Body" has subsections "Document Sections", "Document Subsections", "Document Sub-Subsections", and "Section Summary".

As it becomes appropriate, break down the document sections into subsections to break up the components of the section. Note for instance that this is itself a subsection. As an example, the markdown for the subsection title above this text is as follows:

```markdown
## Subsections
```

## Sub-Subsections:

Should a subsection of the document need to be further broken up into discrete sections, it can be so divided by putting the components beneath a header prefaced with three pound signs, and followed by a line-break. As an example, the markdown for _this_ sub subsection is as follows:

```markdown
### Sub Subsection:
```
In most cases, if you are dividing a document into sub-subsections, it is a sign that you need to rethink the structure of your document to make it more linear. 
> [!IMPORTANT]
> No section or subsection should have within it only one "child" section. If you find yourself in this situation, rewrite your section title.

> [!IMPORTANT] 
> Ideally documents should only ever have a maximum depth of 2. If you feel greater depth is needed, consider breaking up your document into separate files, so that each area of documentation uses the least amount of space.

1. This is a numbered thing

   > [!IMPORTANT]
   > This is a test

    > [!IMPORTANT]
    > This is a test

&nbsp;&nbsp;&nbsp;&nbsp;> [!IMPORTANT]
&nbsp;&nbsp;&nbsp;&nbsp;> This is a test

## Section Summary:

The difference between the different section depths is below, followed by the markdown that generates it:

# $\textcolor{deeppink}{\textsf{Section}}$ </div><a name="section" />
## Subsection
### Sub-Subsection

```markdown
# $\textcolor{deeppink}{\textsf{Section}}$ </div><a name="section" />
## Subsection
### Sub-Subsection
```

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

## Links:

Web links can be written by surrounding the text you want as the link text in brackets, followed by the URL in parentheses. 

When linking to a header within the same document, the link can consist of just the pound sign followed by the header name. 

When linking to another document, the base folder can be the start of the link, so `'/docs/android/readme_android.md'` would be an acceptable link. Additionally you can link to a specific area in another document by adding the pound sign and name at the end of the link, `'/readme.md#prerequisites'`. when ending a sentence with a link, make sure the period is not included in the link, as this will help prevent confusion about what is and is not part of the link.

Example markdown linking to another document:

```markdown
[readme_android](/docs/android/readme_android.md)
```

Example markdown linking to a specific section of another document:

```markdown
[android prerequisites](/docs/android/readme_android.md#prerequisites)
```

> [!IMPORTANT]
> Periodically, a script should be run against all the documentation to check that all of the links are still valid. It's important to make sure links do not become stale or broken. Because of this, external links should be used sparingly.

## Code:

### Block
For inline code formatting, use single ticks. This is useful to highlight certain words to indicate that they are variables, or to clearly identify things like menu paths to follow.

In order to display code blocks, put the code you wish to display between two lines containing only three ticks. For code blocks, make sure to add to the first set of three ticks the language that the code snippet is in, so that syntax highlighting is accomplished (for instance you can use `cs` to indicate that the block is C#, or `markdown` to indicate that it's a code snippet in markdown). See [here](https://github.com/highlightjs/highlight.js/blob/main/SUPPORTED_LANGUAGES.md) for a list of all the languages that GitHub Flavored Markdown supports.

> [!IMPORTANT]
> When you are providing a code example, it may be necessary to break coding standards for the sake of readability. One circumstance where this is particularly true is with code that would normally require horizontal scrolling to fully view. If a line of code within the codeblock exceeds 130 characters, be sure to add line breaks following  [this](https://se-education.org/guides/conventions/csharp.html#2-maximum-line-length-is-130-characters) guide.

### Inline

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
> Because collapsed sections are easy to overlook, use them sparingly. Instead of collapsing some amount of the document, it may be more wise to break up the content into discrete files.

## Mermaid:

Mermaid is the formatting we use for displaying flowcharts about the plugin. From the perspective of style guidelines, these flowcharts can be thought of as images, with the added functionality that they are interactive. [here](https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/creating-diagrams).

## Alerts:

This section is a copy of [discussion](https://github.com/orgs/community/discussions/16925) of these features when they were first introduced to GFM.

Alerts are an extension of Markdown used to emphasize critical information. On GitHub, they are displayed with distinctive colors and icons to indicate the importance of the content. 

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

# $\textcolor{deeppink}{\textsf{Templates}}$ </div><a name="templates" />

You can use the templates here by copying them and replacing the content within the `[ ]` including the brackets themselves, with the the appropriate content. 

They are mainly blank space holders, so you can decide whats needed, be it a list, an alart, an image, or whatever. 

[Basic Template](/docs/docs_on_docs/template_basic.md)

[ReadMe Template](/docs/docs_on_docs/template_readme.md)

# $\textcolor{deeppink}{\textsf{Internal Markdown Reference}}$ </div><a name="internal-markdown-reference" />

You can find our internal Markdown reference [here](/docs/docs_on_docs/md_reference.md) for quick access, though it is still expected you use these syntax examples inline with this style guide.

# $\textcolor{deeppink}{\textsf{Source Code Contributor Notes}}$ </div><a name="source-code-contributor-notes" />

Find info on source code (and doc) contributions [here](/readme.md#configuring-the-plugin).