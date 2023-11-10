CC=clang
CXX=clang++

# clang -dynamiclib MicrophoneUtility_macOS.mm -arch x86_64 --std=c++11 -o MicrophoneUtility_macos.dylib

CXXFLAGS = --std=c++11
DYLIBS = build/libDynamicLibraryLoaderHelper.dylib build/MicrophoneUtility_macos.dylib
UNITY_META_FILES = libDynamicLibraryLoaderHelper.dylib.meta

#-----------------------------------------------------------------------
# all comes first so that it will be the default 
all : $(DYLIBS)

install : all
	cp $(DYLIBS) ../../../Assets/Plugins/macOS/
	cp $(UNITY_META_FILES) ../../../Assets/Plugins/macOS/

clean : DynamicLibraryLoaderHelper_clean MicrophoneUtility_macos_clean
#-----------------------------------------------------------------------
#-----------------------------------------------------------------------

build:
	test -d build || mkdir build

#-----------------------------------------------------------------------
MICROPHONE_SRC = MicrophoneUtility_macos.mm
build/MicrophoneUtility_mac_x86: build $(MICROPHONE_SRC)
	$(CXX) -dynamiclib $(MICROPHONE_SRC) -framework Foundation -framework AVFoundation -arch x86_64 $(CXXFLAGS) -o $@

build/MicrophoneUtility_mac_arm: build $(MICROPHONE_SRC)
	$(CXX) -dynamiclib $(MICROPHONE_SRC) -framework Foundation -framework AVFoundation -arch arm64 $(CXXFLAGS) -o $@

build/MicrophoneUtility_macos.dylib: build/MicrophoneUtility_mac_arm build/MicrophoneUtility_mac_x86
	lipo -create -output $@ build/MicrophoneUtility_mac_arm build/MicrophoneUtility_mac_x86 

MicrophoneUtility_macos_clean:
	test -f build/MicrophoneUtility_macos.dylib && rm build/MicrophoneUtility_macos.dylib || true
	test -f build/MicrophoneUtility_mac_x86 && rm build/MicrophoneUtility_mac_x86 || true
	test -f build/MicrophoneUtility_macarm && rm build/MicrophoneUtility_mac_arm || true
#-----------------------------------------------------------------------

#-----------------------------------------------------------------------
DLLH_SRC = DynamicLibraryLoaderHelper_macos.cpp
build/DynamicLibraryLoaderHelper_mac_x86: build $(DLLH_SRC)
	$(CXX) -dynamiclib $(DLLH_SRC) -arch x86_64 $(CXXFLAGS) -o $@

build/DynamicLibraryLoaderHelper_mac_arm: build $(DLLH_SRC)
	$(CXX) -dynamiclib $(DLLH_SRC) -arch arm64 $(CXXFLAGS) -o $@

build/libDynamicLibraryLoaderHelper.dylib: build/DynamicLibraryLoaderHelper_mac_arm build/DynamicLibraryLoaderHelper_mac_x86
	lipo -create -output build/libDynamicLibraryLoaderHelper.dylib $?

DynamicLibraryLoaderHelper_clean:
	test -f libDynamicLibraryLoaderHelper.dylib && rm libDynamicLibraryLoaderHelper.dylib || true
	test -f DynamicLibraryLoaderHelper_mac_x86 && rm DynamicLibraryLoaderHelper_mac_x86 || true
	test -f DynamicLibraryLoaderHelper_mac_arm && rm DynamicLibraryLoaderHelper_mac_arm || true
#-----------------------------------------------------------------------

