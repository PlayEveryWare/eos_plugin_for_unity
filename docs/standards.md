<a href="/README.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="README.md" width="5%"/></a>

# <div align="center">Coding Standards</div>
---

## Language Style Guides

### For all source files
Each source code file needs to contain the license which it is licensed under.

### For C / C++ / C#:

### Variables
* static variables should start with `s_`, globals with `g_`.
* Objects of type `struct` should be PascalCase; Abbreviations should always be uppercase (for Example favor `URL` over `url` when as a component of a named type).
* All other variables should snake_case.
* Just as [nasa](https://ntrs.nasa.gov/api/citations/19950022400/downloads/19950022400.pdf#page=18) does, use 4 spaces to indent.
* Constants should done in SCREAMING_SNAKE_CASE.

Additionally, here are some common variable suffixes that are recommended:
`_ptr`  for pointer
`_f`    for a pointer to a function
`_ctx`  for context data i.e. data that is passed into a function

### Functions
Exported function names are prefixed, and lowercase snake case.
e.g.
`FUN_EXPORT(bool) DLLH_unload_library_at_path(void *ctx, void *library_handle)`

The Epic Online Services SDK has it's one standards and style [guide](https://docs.unrealengine.com/4.26/en-US/ProductionPipelines/DevelopmentSetup/CodingStandard/).

### For C#
This follows, more or less, Microsoft's C# style [guide](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).

### Commit Message Style
While not strictly enforced, the current idea for commit messages is to use something like the following:

```markdown
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```  

e.g.:

```markdown
fix(dll_loading)!: Change how DLLs are loaded to fix big bad bug.

There was a nasty bug. It's gone now.

BREAKING CHANGE:
```

Here is an incomplete list of 'types'

- `fix`: Fixes a bug.
- `feat`: Adds a new feature.
- `docs`: Changes to the documentation, not code.
- `refactor`: Code change that does not directly fix a bug.
- `perf`: A code change aimed at increasing performance
- `chore`: Small changes that are needed to maintain something. 

   _Examples of chores are: Updating Keys, adding comments, or style changes._

- `revert`: used to mark a reverted commit. The footer should have the sha.
- `upgrade`: Upgrade a third-party dependency. These often are a combination of a `chore`, `fix`, `refactor`, and `feat`, and thus deserve their own type.

Scopes are somewhat more free-form in nature. They should always be limited to a single word, and should preferably be a noun. _If_ a given commit has multiple scopes it affects, then commas may be used.

Examples:

`(style)`: Changes to a either code or documentation that are style only.
`(comments)`: Used for changes that only add comments.

More details [here](https://www.conventionalcommits.org/en/v1.0.0/).

### Change-list Style Guide
TODO: There isn't a change-list yet, but it will probably follow https://keepachangelog.com/en/1.0.0/.

### Release Scheme
The versioning scheme this project uses is [semver](https://semver.org/).

