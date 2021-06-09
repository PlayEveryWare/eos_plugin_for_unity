# For more information about using CMake with Android Studio, read the
# documentation: https://d.android.com/studio/projects/add-native-code.html

# Sets the minimum version of CMake required to build the native library.

cmake_minimum_required(VERSION 3.10.2)

# Declares and names the project.

project("UnityHelpers_Android")

# Creates and names a library, sets it as either STATIC
# or SHARED, and provides the relative paths to its source code.
# You can define multiple libraries, and CMake builds them for you.
# Gradle automatically packages shared libraries with your APK.

#set(src_files native-lib.cpp)
set(src_files JNIHelpers.cpp)
#list(APPEND src_files JNIHelpers.cpp)

add_library( # Sets the name of the library.
            UnityHelpers_Android

             # Sets the library as a shared library.
             SHARED

             # Provides a relative path to your source file(s).
             ${src_files} )

# Searches for a specified prebuilt library and stores the path as a
# variable. Because CMake includes system libraries in the search path by
# default, you only need to specify the name of the public NDK library
# you want to add. CMake verifies that the library exists before
# completing its build.

find_library( # Sets the name of the path variable.
              log-lib

              # Specifies the name of the NDK library that
              # you want CMake to locate.
              log )

set(libraries_to_link ${log-lib})

find_library(jvm-lib jvm)
list(APPEND livraries_to_link ${jvm-lib})
# Specifies libraries CMake should link to your target library. You
# can link multiple libraries, such as libraries you define in this
# build script, prebuilt third-party libraries, or system libraries.

target_link_libraries( # Specifies the target library.
                        UnityHelpers_Android

                       # Links the target library to the log library
                       # included in the NDK.
                       ${log-lib} )