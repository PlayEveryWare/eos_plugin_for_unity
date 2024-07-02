# Copyright (c) 2021 PlayEveryWare
#
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included in 
# all copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
# SOFTWARE.

CC=clang
CXX=clang++

CXXFLAGS = --std=c++11 -fPIC
SOLIBS = build/libDynamicLibraryLoaderHelper.so
UNITY_META_FILES = libDynamicLibraryLoaderHelper.so.meta

#-----------------------------------------------------------------------
# all comes first so that it will be the default 
all : $(SOLIBS)

install : all
	cp $(SOLIBS) ../../../Assets/Plugins/Linux/
	cp $(UNITY_META_FILES) ../../../Assets/Plugins/Linux/

clean : DynamicLibraryLoaderHelper_clean
#-----------------------------------------------------------------------
#-----------------------------------------------------------------------

build:
	test -d build || mkdir build

#-----------------------------------------------------------------------
DLLH_SRC = DynamicLibraryLoaderHelper_Linux.cpp
build/libDynamicLibraryLoaderHelper.so: build $(DLLH_SRC)
	$(CXX) -shared $(DLLH_SRC) -march=x86-64 $(CXXFLAGS) -o $@

#build/libDynamicLibraryLoaderHelper.so: build/DynamicLibraryLoaderHelper_Linux_x86
#	lipo -create -output build/libDynamicLibraryLoaderHelper.so $?

DynamicLibraryLoaderHelper_clean:
	test -d build && rm -r build || true
	test -f libDynamicLibraryLoaderHelper.so && rm libDynamicLibraryLoaderHelper.so || true
#	test -f DynamicLibraryLoaderHelper_Linux_x86 && rm DynamicLibraryLoaderHelper_Linux_x86 || true
#-----------------------------------------------------------------------

