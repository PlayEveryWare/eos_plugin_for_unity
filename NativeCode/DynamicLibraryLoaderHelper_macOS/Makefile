CC=clang
CXX=clang++

# clang -dynamiclib MicrophoneUtility_macOS.mm -arch x86_64 --std=c++11 -o MicrophoneUtility_macos.dylib

CXXFLAGS = --stdc++11


MicrophoneUtility_mac_x86: MicrophoneUtility_mac.mm
	$(CXX) -dynamiclib $? --arch x86_64 $(CXXFLAGS) -o $@

MicrophoneUtility_mac_arm: MicrophoneUtility_mac.mm
	$(CXX) -dynamiclib $? --arch arm64 $(CXXFLAGS) -o $@

MicrophoneUtility_mac: MicrophoneUtility_mac_arm MicrophoneUtility_mac_x86
	lipo -create -output MicrophoneUtility_mac.dylib MicrophoneUtility_mac_arm MicrophoneUtility_mac_x86 
