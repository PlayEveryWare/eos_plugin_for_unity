## Description of this directory
These files are for use by the Packaging tool in the PEW EOS Samples repo.
They allow a user to specify specific files to include in a UPM package, a
.unitypackage, or to export to a directory. 

Currently there are 3 in this directory:

### eos_package_description.json
This file contains all the files needed to create a UPM package with no
restricted platforms in it.

### eos_dotunitypackage_package_desc.json
This file contains all the files needed to create a .unitypackage that with
no restricted platforms in it.

### eos_export_assets_package_desc.json
This file contains all the files needed to export a version of this repo that doesn't
contain any restricted platforms in it.


## File Format
Each json file contains a simple format to specify what files to move, and where to move them to:

### "source_to_dest"
This object is an array of objects that specify what to move to where.

### "SrcDestPairs"
This is the actual object in the source_to_dest array. 
* The "src" specifies what files are to be copied. The files allow a limited
version of windows style wildcards to specify what to copy. 
* The "dest" specifies where to copy the files. 
    * If "dest" ends in a "/", it is taken to be a directory. Otherwise it's considered to be a single file, i.e. it will write the source file as the dest name on copy.
    * If "dest" is an empty string, it will copy the file to the root of the path set by the tool for output.
    * Of note, the copy currently flattens the paths, so that intermediate paths are _not_ copied over. 

