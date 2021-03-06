// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

#ifndef _TRAMPOLINE_UNITY_EAGLCONTEXTHELPER_H_
#define _TRAMPOLINE_UNITY_EAGLCONTEXTHELPER_H_

extern "C" bool AllocateRenderBufferStorageFromEAGLLayer(void* eaglContext, void* eaglLayer);
extern "C" void DeallocateRenderBufferStorageFromEAGLLayer(void* eaglContext);

#if __OBJC__

    @class EAGLContext;
    EAGLContext*    CreateContext(EAGLContext* parent);

    struct
    EAGLContextSetCurrentAutoRestore
    {
        EAGLContext* old;
        EAGLContext* cur;

        EAGLContextSetCurrentAutoRestore(EAGLContext* cur);
        ~EAGLContextSetCurrentAutoRestore();
    };

#endif // __OBJC__

#endif // _TRAMPOLINE_UNITY_EAGLCONTEXTHELPER_H_
