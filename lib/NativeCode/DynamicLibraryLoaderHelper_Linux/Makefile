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

