# Creating a UPM package

## Overview
This tool, while a bit rough around the edges, allows for the generation of custom UPM packages of various components of the project.
By using the tool, it lets one generate packages which one may configure to only have "public" parts of the project. Alternatively, one may
generate versions of the package which include "Restricted" parts as well. 

## Steps to generate a UPM package
1) Open up Unity project in this repo

2) Go to Tools -> Create Package

3) Fill out the "JSON Description Path"
This should by default be defined as ${WHATEVER_THE_GIT_REPO_IS_CALLED}/Assets/../PackageDescriptionConfigs/eos_package_description.json .
This file defines what will be exported into the final UPM package. The documentation for this is currently in the same directory as the 
package files are.


4) Fill out the "Output path"
This is where the generated UPM package will be saved to. 

5) Custom Build Directory
This lets one define where the files will be copied to before the final UPM is created. If left blank, a temp directory will be used.
This setting is useful, as it allows for the user to also export the project to another directory, using the same filtering specification as
the package does. In fact, this is how the github project for the UPM package is setup. 

6) hit the "Create UPM package" to create a UPM package.
