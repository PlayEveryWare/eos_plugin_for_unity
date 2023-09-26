# Creating a UPM package

## Overview
This tool, while a bit rough around the edges, allows for the generation of custom UPM packages of various components of the project.
By using the tool, it lets one generate packages which one may configure to only have "public" parts of the project. Alternatively, one may
generate versions of the package which include "Restricted" parts as well. 

## Steps to generate a UPM package
1) Open up Unity project in this repo.

2) Go to `Tools -> EOS Plugin -> Create Package`

3) Fill out "JSON Description Path".
This should by default be defined as `${WHATEVER_THE_GIT_REPO_IS_CALLED}/Assets/../PackageDescriptionConfigs/eos_package_description.json`.
This file defines what will be exported into the final UPM package, the documentation of which currently reside in the same directory as the package files.

3) Fill out "Output path".
This is where the generated UPM package will be saved to. 

4) Select button for the package you want to create:
  - "Create UPM Package" will create a tarball `.tgz` file in the output directory indicated that contains the plugin.
  - "Create .unitypackage" will create a `.unitypackage` file containing the plugin.
  - "Export to Directory" will do the same as "Create UPM Package," but will not compress the output, so you'll get a directory of the exported files.

*Advanced:*
If you are familiar with the structure of the package `.json` file format, you can expand the advanced carrot and use a different `.json` for the package creation.
