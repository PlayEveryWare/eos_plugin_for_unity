# Standards

### Language Style Guides

#### For C
Function names are prefixed, and lowercase snake case.
The Epic Online Services SDK has it's one standards and style [guide](https://docs.unrealengine.com/4.26/en-US/ProductionPipelines/DevelopmentSetup/CodingStandard/).

#### For C++

#### For C#
This follows, more or less, Microsoft's C# style [guide](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).

### Commit Message Style
While not strictly enforced, the current idea for commit messages is to use something like the following:

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```  

e.g.:
```
fix(dll_loading)!: Change how DLLs are loaded to fix big bad bug.

There was a nasty bug. It's gone now.

BREAKING CHANGE:
```

Here is an incomplete list of 'types'
```
fix: Fixes a bug.
feat: Adds a new feature.
docs: Changes to the documentation, not code.
refactor: Code change that does not directly fix a bug.
perf: A code change aimed at increasing performance
chore: Small changes that are needed to maintain something. Examples of chores are: Updating Keys, adding comments, or style changes.
revert: used to mark a reverted commit. The footer should have the sha
upgrade: Upgrade a third-party dependency. These often are a combonation of a chore, fix, refactor, and feat, and thus deserve their own type
```

Scopes are somewhat more free form in nature. It is preferred that they are nouns, and required that they are single words.
If a given commit has multiple scopes it affects, then commas may be used

Examples:
```
(style): Changes to a either code or documentation that are style only.
(comments): Used for changes that only add comments.

```

More details here
https://www.conventionalcommits.org/en/v1.0.0/


### Changelist Style Guide
TODO: There isn't a changelist yet, but it will probably follow https://keepachangelog.com/en/1.0.0/

### Release Scheme
TODO: Expand this with an inline defintion. Probably going to follow follow https://semver.org/ .

