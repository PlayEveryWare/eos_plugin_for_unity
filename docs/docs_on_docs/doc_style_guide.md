<a href="/README.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="README.md" width="5%"/></a>

# <div align="center">Documentation Style Guide</div>
---

# Overview

The purpose of this document is to describe a standard which all other documents should follow. Use and follow this guide when creating new documentation for the project.

While this document does give examples for some of the most frequently utilized components of markdown, the intent is to describe *how* those components should be utilized in this project, not necessarily how to *implement* them.

As with most things in life, this document serves as a guide, not a rule book. For the most part it should be strictly followed, but a reasonable amount of deviation is permissable so long as the goal of proper communication is accomplished.

# Getting Started

GitHub is the primary place where documentation for this project will be consumed. Therefore be sure to write your documentation using [GFM (GitHub Flavored Markdown)](https://github.github.com/gfm/).  

Please be aware that *not all Markdown engines work the same*. This style guide considers the rendering of the GitHub markdown renderer to be the standard, so before submitting documentation please make sure it conforms to the style guide _as viewed through GitHub_.

You may find a [GFM Cheat Sheet](https://gist.github.com/roshith-balendran/d50b32f8f7d900c34a7dc00766bcfb9c) to be helpful.

Understanding how to use GFM should be considered a prerequisite to contributing documentation to the project.

# Organizational Structure

Fundamentally (and very broadly speaking) every document should contain two or optionally three components:

| Component | Purpose |
| -: | :- |
| Header | _Title, logo, and overview_ |
| Body | _This should contain the meat of the document wherein each concept is appropriately boxed into sections._ |
| "See also" | _(Optional). This should contain links to other documents that are somewhat related in topic, or (as in this document) links to supplementary resources._ |

# Header 

## Logo:

Each document should start (before the document title) with the PlayEveryWare, Inc. logo. The image should be surrounded by a link (`<a> </a>`) tag with the `href` set to the main [README.md document](/README.md), and with the width of the image set to 10%.

Markdown:
```markdown
<a href="/README.md">
    <img src="/docs/images/PlayEveryWareLogo.gif" alt="PlayEveryWare, Inc. Logo" width="10%"/>
</a>
```

What it looks like:

<a href="/README.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="PlayEveryWare, Inc. Logo" width="10%"/></a>

## Title:

Following the PlayEveryWare, Inc. logo should be the title of the document centered, followed immediately by a horizontal rule. 

As an example of how to properly add a title to the document, below is the markdown used to create the header for _this_ document.

Markdown:
```markdown
# <div align="center">Documentation Style Guide</div>
---
```

# Body

## Sections:

Document sections allow for clear organization of thought within a document.

Each main section of the document should be denoted by the following markdown. Please note that it is important to have your first main section be an "Overview" section that gives the reader a _summary of the purpose of the document_. 

The document section headers share the same format as the document title, but without the horizontal bar.

Example markdown for creating a document section:

```markdown
# Section Title
```

## Subsections:

Subsections of a document allow for clear organization of thought within a section. As an example, in this document (yes, the one you are reading right now) the section "Body" has subsections "Document Sections", "Document Subsections", "Document Sub-Subsections", and "Section Summary".

As it becomes appropriate, further break down the document sections into subsections. Note for instance that this is itself a subsection. As an example, the markdown for the subsection title above this text is as follows:

```markdown
## Subsections
```

## Sub-Subsections:

Should a subsection of the document need to be further broken up into discrete sections, it can be so divided by putting the components beneath a header prefaced with two pound signs.
For example:

```markdown
## Sub-Subsection:
```

> [!NOTE]
> Just to be as clear as possible: note that the "Sub-Subsections" title above is itself a _sub section_, **not** a _**sub** subsection_.

> [!IMPORTANT] 
> Ideally, documents should only ever have a maximum section depth of 2. If you feel greater depth is needed, it is a sign that you need to rethink the structure of your document, or break it up into separate files.

## Section Summary:

The difference between the different section depths is below, followed by the markdown that generates it:

# <div align="center">Document Title Example </div>
# Section Example
## Subsection Example
### Sub-Subsection Example 

```markdown
# <div align="center">Document Title Example </div><a name="section-example" />
# Section Example
## Subsection Example
### Sub-Subsection Example
```

> [!IMPORTANT]
> No section or subsection should have within it only one "child" section. If you find yourself in this situation, consider rewrite your section title.

## Images:

Images are to be utilized in specific circumstances:
* Instructional references, like images of dropdown menus or highlighted parts of windows.
* Introductory images, to make it clear or preview what what is being referenced in a section.

An image can be displayed with the web link format, prefacing with an exclamation mark. While the text is generally hidden we want it to be informative in case the image doesn't load.

Markdown example:

```markdown
![unity tools package manager](/docs/images/unity_tools_package_manager.gif)
```

What it looks like:

![unity tools package manager](/docs/images/unity_tools_package_manager.gif)

Always place images in the `/docs/images/` directory of the repository. In most cases, it is also appropriate to add a subdirectory to the images directory in order to keep related images organized. For instance, when creating a new document that contains a variety of images, it would be wise to create a subdirectory in the images folder to contain all the images for that new document.

> [!IMPORTANT] 
> Never use an image in place of text (for instance do not take a screenshot of documentation from one area to include it in another). Aside from the issue of maintainability, the problem with this is that GitHub allows for dark and light modes, and the image will look wrong in one mode or the other. 

> [!IMPORTANT]
> Because of how we utilize the documentation, it is **very important** that all your links be absolute instead of relative. If you fail to do this your links might be broken when the documentation is added to places like [eospluginforunity.playeveryware.com](https://eospluginforunity.playeveryware.com).

## Links:

Web links can be written by surrounding the text you want as the link text in brackets, followed by the URL in parentheses. 

When linking to a header within the same document, the link can consist of just the pound sign followed by the header name. 

When linking to another document, the base folder can be the start of the link, so `'/docs/android/README_Android.md'` would be an acceptable link. Additionally you can link to a specific area in another document by adding the pound sign and name at the end of the link, `'/README.md#prerequisites'`. when ending a sentence with a link, make sure the period is not accidentally included in the url portion of the link.

Example markdown linking to a specific section within a document:

```markdown  
[Getting Started](#getting-started)
```

Example markdown linking to another document:

```markdown
[Android README](/docs/android/README_Android.md)
```

Example markdown linking to a specific section of another document:

```markdown
[Android Prerequisites](/docs/android/README_Android.md#prerequisites)
```

> [!IMPORTANT]
> Periodically, a script should be run against all the documentation to check that all of the links (internal and external) are still valid. It's important to make sure links do not become stale or broken. Because of this, external links should be used sparingly.

> [!IMPORTANT]
> When renaming a document or any of the sections, make sure to search for (and update) any references to that document from others.
> A handy tool for doing this is `grep`, and you could run a command in git bash like the following from the root of the repository to find all documents that link to a specific document you might want to rename:
>
> ```bash
> grep -irnl "login_type_by_platform.md" ./docs
> ```
>
> Output:
> ```bash
> $ grep -irnl "login_type_by_platform.md" ./docs
> ./docs/frequently_asked_questions.md
> ./docs/player_authentication.md
> ```
> 
> Based on the output of the command above, were we to rename the document `login_type_by_platform.md`, we would need to update references to the document in both `./docs/frequently_asked_questions.md`, and `./docs/player_authentication.md`.

> [!IMPORTANT]
> Because of how we utilize the documentation, it is **very important** that (as with images) all your links be absolute instead of relative. If you fail to do this, your links might be broken when the documentation is added to places like [eospluginforunity.playeveryware.com](https://eospluginforunity.playeveryware.com) or GitHub Pages.

## Code:

### Block

For inline code formatting, use single ticks. This is useful to highlight certain words to indicate that they are variables, or to clearly identify things like menu paths to follow.

In order to display code blocks, put the code you wish to display between two lines containing only three ticks. For code blocks, make sure to add to the first set of three ticks the language that the code snippet is in, so that syntax highlighting is accomplished (for instance you can use `cs` to indicate that the block is C#, or `markdown` to indicate that it's a code snippet in markdown). See [here](https://github.com/highlightjs/highlight.js/blob/main/SUPPORTED_LANGUAGES.md) for a list of all the languages that GitHub Flavored Markdown supports.

> [!IMPORTANT]
> When you are providing a code example, it may be necessary to break coding standards for the sake of readability. One circumstance where this is particularly true is with code that would normally require horizontal scrolling to fully view. If a line of code within the codeblock exceeds 130 characters, be sure to add line breaks following  [this](https://se-education.org/guides/conventions/csharp.html#2-maximum-line-length-is-130-characters) guide.

### Inline

When writing inline instructions (such as menu navigation) use the inline code block, and separate action names by an arrow ` -> ` (spaces included for increased legibility). Inline code blocks are also appropriate to use when (in normal a sentence) you are referencing a variable.

Examples of inline code markdown:

```markdown
To create a new C# script in Unity, navigate the menus through `Assets -> Create -> C# Script`.
```

```markdown
The variable `foo` is much better than the variable `bar`.
```

What it looks like:

To create a new C# script in Unity, navigate the menus through `Assets -> Create -> C# Script`.

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

> [!IMPORTANT]
> If the list *can* be unordered, then it *should* be unordered.

> [!WARNING]
> When organizing a list, no item should ever have a single child, if you find yourself doing that rethink the organizational structure of the process you are describing.

## Grids:

Often, larger amounts of information need to be illustrated in the documentation. Information like this might be well suited to a table format. See the below example of markdown for an example of how to create a grid in markdown:

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

When there is a large amount of information that may or may not be immediately pertinent to the documentation, it may be wise to include it in a collapsed section of the document, making it clear that the information exists, albeit in an abbreviated manner. This helps avoid a situation where too much information of variable utility is displayed on the screen, while still making the information accessible if needed.

Markdown for collapsing content:

```markdown
### Collapsed stuff:
<details>
  <summary><b>Find a surprise hidden here</b></summary>
Surprise!
</details>
```

What it looks like:

### Collapsed stuff:
<details>
  <summary><b>Find a surprise hidden here</b></summary>
Surprise!
</details>

> [!WARNING] 
> Collapsed sections have a tendency to be overlooked by readers; be cautious about what you choose to put within them. In many cases, it might be more effective to break up your documentation into more than one file.

## Mermaid:

Mermaid is the formatting we use for displaying flowcharts. From the perspective of style guidelines, these flowcharts can be thought of as images, with the added functionality that they are interactive. See [here](https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/creating-diagrams) for a guide on creating diagrams using Mermaid.

## Alerts:

This section is a copy of [a discussion](https://github.com/orgs/community/discussions/16925) of these features when they were first introduced to GitHub Flavored Markdown (GFM).

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