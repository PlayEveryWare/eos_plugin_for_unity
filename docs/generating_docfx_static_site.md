<a href="/readme.md"><img src="/docs/images/PlayEveryWareLogo.gif" alt="README.md" width="5%"/></a>

# How to update the DocFX static site
*Overview*
1) Install docfx.
2) Go to the `etc/docfx/` dir in the repository.
3) Run docfx.
4) Run a test server to see the results: `docfx serve \_site`.
5) Copy the files to the github static site.

## Installing docfx

One way to install it is with Chocolatey:
```
choco install docfx
```

## Go to the docfx_project
In a terminal of your choice that has docfx in the PATH, go the the repo, and then to the 
docfx_project. One may do so via something like this:
```
cd ${THE_PATH_OF_YOUR_CHECKOUT_OF_THE_PLUGIN}
cd etc/docfx
```

## Run docfx
In aforementioned terminal, run the following:
```
docfx
```
This will product a lot of output, some of which might be warnings that comments were made in an unspecified format.
If those comments are supposed to be valid, clean them up based on the directions of the warnings.

## Running a test server 
In that same directory, run 
```
docfx serve _site
```
This should cause a server to run the static site on port 8080.
One may access this by going to [localhost:8080](http://localhost:8080).

## Copy the files to correct location

If everything looks good, copy the results of the static site generation process from `\_site` to the repo that houses 
the static site, then commit / push.
