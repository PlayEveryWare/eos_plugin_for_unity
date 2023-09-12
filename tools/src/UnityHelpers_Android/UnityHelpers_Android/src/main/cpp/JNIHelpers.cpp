//
// Created by test on 2021/03/04.
//
#include "JNIHelpers.h"
#include <jni.h>
#include <string>

#define EXPORT_FOR_JNI(ret_type) extern "C" JNIEXPORT ret_type JNICALL
#define NATIVE_IMPL(CLASS_NAME, METH_NAME) Java_com_pew_unity_1helpers_1android_##CLASS_NAME##_##METH_NAME

static JavaVM *s_vm;
EXPORT_FOR_JNI(jint) JNI_OnLoad(JavaVM* vm, void* reserved)
{
    s_vm = vm;

    JNIEnv *env = nullptr;
    s_vm->GetEnv((void**)&env, JNI_VERSION_1_6);

    return JNI_VERSION_1_6;
}

EXPORT_FOR_JNI(jstring) NATIVE_IMPL(MainActivity, stringFromJNI)(JNIEnv* env, jobject /* this */)
{
    std::string hello = "Hello from C++";
    return env->NewStringUTF(hello.c_str());
}

EXPORT_FOR_JNI(JavaVM*) UnityHelpers_GetJavaVM()
{
    return s_vm;
}

//extern "C" JNIEXPORT jint JNIHelper_GetCreatedJavaVMs(void ** vmBuf, jsize bufLen, jsize *nVMs)
//{
//    return JNI_GetCreatedJavaVMs((JavaVM**)vmBuf, bufLen, nVMs);
//}

