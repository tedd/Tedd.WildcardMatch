## .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3 (Job: Job-GPHEUC(Affinity=01000000000000000000000000000000, IterationCount=10, IterationTime=100ms, LaunchCount=3, WarmupCount=3))

```assembly
; GlobAcceptance.Match()
       push      rbp
       sub       rsp,90
       lea       rbp,[rsp+90]
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rbp-70],ymm4
       vmovdqa   xmmword ptr [rbp-50],xmm4
       xor       eax,eax
       mov       [rbp-40],rax
       mov       [rbp+10],rcx
       mov       dword ptr [rbp-68],3E8
       xor       eax,eax
       mov       [rbp-3C],eax
       mov       rax,[rbp+10]
       mov       rax,[rax+18]
       mov       [rbp-48],rax
       xor       eax,eax
       mov       [rbp-4C],eax
       jmp       near ptr M00_L02
M00_L00:
       mov       rax,[rbp-48]
       mov       ecx,[rbp-4C]
       cmp       ecx,[rax+8]
       jae       near ptr M00_L04
       mov       edx,ecx
       lea       rax,[rax+rdx*8+10]
       mov       rax,[rax]
       mov       [rbp-58],rax
       mov       rax,[rbp+10]
       mov       rax,[rax+20]
       mov       [rbp-60],rax
       mov       rcx,[rbp-60]
       mov       rdx,7FFCCB75B350
       call      CORINFO_HELP_DELEGATEPROFILE32
       mov       rax,[rbp-60]
       mov       [rbp-70],rax
       mov       rax,[rbp-70]
       mov       rdx,[rbp-58]
       mov       rcx,[rax+8]
       mov       rax,[rbp-70]
       call      qword ptr [rax+18]
       test      eax,eax
       je        short M00_L01
       mov       rcx,7FFCCB75B458
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-3C]
       inc       eax
       mov       [rbp-3C],eax
M00_L01:
       mov       rcx,7FFCCB75B45C
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-4C]
       inc       eax
       mov       [rbp-4C],eax
M00_L02:
       mov       eax,[rbp-68]
       dec       eax
       mov       [rbp-68],eax
       cmp       dword ptr [rbp-68],0
       jg        short M00_L03
       lea       rcx,[rbp-68]
       mov       edx,27
       call      CORINFO_HELP_PATCHPOINT
M00_L03:
       mov       rax,[rbp-48]
       mov       eax,[rax+8]
       cmp       eax,[rbp-4C]
       jg        near ptr M00_L00
       mov       rcx,7FFCCB75B460
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-3C]
       add       rsp,90
       pop       rbp
       ret
M00_L04:
       call      CORINFO_HELP_RNGCHKFAIL
       int       3
; Total bytes of code 289
```

## .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3 (Job: Job-GPHEUC(Affinity=01000000000000000000000000000000, IterationCount=10, IterationTime=100ms, LaunchCount=3, WarmupCount=3))

```assembly
; GlobAcceptance.Match()
       push      rbp
       sub       rsp,90
       lea       rbp,[rsp+90]
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rbp-70],ymm4
       vmovdqa   xmmword ptr [rbp-50],xmm4
       xor       eax,eax
       mov       [rbp-40],rax
       mov       [rbp+10],rcx
       mov       dword ptr [rbp-68],3E8
       xor       eax,eax
       mov       [rbp-3C],eax
       mov       rax,[rbp+10]
       mov       rax,[rax+18]
       mov       [rbp-48],rax
       xor       eax,eax
       mov       [rbp-4C],eax
       jmp       near ptr M00_L02
M00_L00:
       mov       rax,[rbp-48]
       mov       ecx,[rbp-4C]
       cmp       ecx,[rax+8]
       jae       near ptr M00_L04
       mov       edx,ecx
       lea       rax,[rax+rdx*8+10]
       mov       rax,[rax]
       mov       [rbp-58],rax
       mov       rax,[rbp+10]
       mov       rax,[rax+20]
       mov       [rbp-60],rax
       mov       rcx,[rbp-60]
       mov       rdx,7FFCCB737438
       call      CORINFO_HELP_DELEGATEPROFILE32
       mov       rax,[rbp-60]
       mov       [rbp-70],rax
       mov       rax,[rbp-70]
       mov       rdx,[rbp-58]
       mov       rcx,[rax+8]
       mov       rax,[rbp-70]
       call      qword ptr [rax+18]
       test      eax,eax
       je        short M00_L01
       mov       rcx,7FFCCB737540
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-3C]
       inc       eax
       mov       [rbp-3C],eax
M00_L01:
       mov       rcx,7FFCCB737544
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-4C]
       inc       eax
       mov       [rbp-4C],eax
M00_L02:
       mov       eax,[rbp-68]
       dec       eax
       mov       [rbp-68],eax
       cmp       dword ptr [rbp-68],0
       jg        short M00_L03
       lea       rcx,[rbp-68]
       mov       edx,27
       call      CORINFO_HELP_PATCHPOINT
M00_L03:
       mov       rax,[rbp-48]
       mov       eax,[rax+8]
       cmp       eax,[rbp-4C]
       jg        near ptr M00_L00
       mov       rcx,7FFCCB737548
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-3C]
       add       rsp,90
       pop       rbp
       ret
M00_L04:
       call      CORINFO_HELP_RNGCHKFAIL
       int       3
; Total bytes of code 289
```

## .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3 (Job: Job-GPHEUC(Affinity=01000000000000000000000000000000, IterationCount=10, IterationTime=100ms, LaunchCount=3, WarmupCount=3))

```assembly
; GlobAcceptance.Match()
       push      rbp
       sub       rsp,90
       lea       rbp,[rsp+90]
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rbp-70],ymm4
       vmovdqa   xmmword ptr [rbp-50],xmm4
       xor       eax,eax
       mov       [rbp-40],rax
       mov       [rbp+10],rcx
       mov       dword ptr [rbp-68],3E8
       xor       eax,eax
       mov       [rbp-3C],eax
       mov       rax,[rbp+10]
       mov       rax,[rax+18]
       mov       [rbp-48],rax
       xor       eax,eax
       mov       [rbp-4C],eax
       jmp       near ptr M00_L02
M00_L00:
       mov       rax,[rbp-48]
       mov       ecx,[rbp-4C]
       cmp       ecx,[rax+8]
       jae       near ptr M00_L04
       mov       edx,ecx
       lea       rax,[rax+rdx*8+10]
       mov       rax,[rax]
       mov       [rbp-58],rax
       mov       rax,[rbp+10]
       mov       rax,[rax+20]
       mov       [rbp-60],rax
       mov       rcx,[rbp-60]
       mov       rdx,7FFCCB736CC8
       call      CORINFO_HELP_DELEGATEPROFILE32
       mov       rax,[rbp-60]
       mov       [rbp-70],rax
       mov       rax,[rbp-70]
       mov       rdx,[rbp-58]
       mov       rcx,[rax+8]
       mov       rax,[rbp-70]
       call      qword ptr [rax+18]
       test      eax,eax
       je        short M00_L01
       mov       rcx,7FFCCB736DD0
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-3C]
       inc       eax
       mov       [rbp-3C],eax
M00_L01:
       mov       rcx,7FFCCB736DD4
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-4C]
       inc       eax
       mov       [rbp-4C],eax
M00_L02:
       mov       eax,[rbp-68]
       dec       eax
       mov       [rbp-68],eax
       cmp       dword ptr [rbp-68],0
       jg        short M00_L03
       lea       rcx,[rbp-68]
       mov       edx,27
       call      CORINFO_HELP_PATCHPOINT
M00_L03:
       mov       rax,[rbp-48]
       mov       eax,[rax+8]
       cmp       eax,[rbp-4C]
       jg        near ptr M00_L00
       mov       rcx,7FFCCB736DD8
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-3C]
       add       rsp,90
       pop       rbp
       ret
M00_L04:
       call      CORINFO_HELP_RNGCHKFAIL
       int       3
; Total bytes of code 289
```

## .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3 (Job: Job-GPHEUC(Affinity=01000000000000000000000000000000, IterationCount=10, IterationTime=100ms, LaunchCount=3, WarmupCount=3))

```assembly
; GlobAcceptance.Match()
       push      rbp
       sub       rsp,90
       lea       rbp,[rsp+90]
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rbp-70],ymm4
       vmovdqa   xmmword ptr [rbp-50],xmm4
       xor       eax,eax
       mov       [rbp-40],rax
       mov       [rbp+10],rcx
       mov       dword ptr [rbp-68],3E8
       xor       eax,eax
       mov       [rbp-3C],eax
       mov       rax,[rbp+10]
       mov       rax,[rax+18]
       mov       [rbp-48],rax
       xor       eax,eax
       mov       [rbp-4C],eax
       jmp       near ptr M00_L02
M00_L00:
       mov       rax,[rbp-48]
       mov       ecx,[rbp-4C]
       cmp       ecx,[rax+8]
       jae       near ptr M00_L04
       mov       edx,ecx
       lea       rax,[rax+rdx*8+10]
       mov       rax,[rax]
       mov       [rbp-58],rax
       mov       rax,[rbp+10]
       mov       rax,[rax+20]
       mov       [rbp-60],rax
       mov       rcx,[rbp-60]
       mov       rdx,7FFCCB736D00
       call      CORINFO_HELP_DELEGATEPROFILE32
       mov       rax,[rbp-60]
       mov       [rbp-70],rax
       mov       rax,[rbp-70]
       mov       rdx,[rbp-58]
       mov       rcx,[rax+8]
       mov       rax,[rbp-70]
       call      qword ptr [rax+18]
       test      eax,eax
       je        short M00_L01
       mov       rcx,7FFCCB736E08
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-3C]
       inc       eax
       mov       [rbp-3C],eax
M00_L01:
       mov       rcx,7FFCCB736E0C
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-4C]
       inc       eax
       mov       [rbp-4C],eax
M00_L02:
       mov       eax,[rbp-68]
       dec       eax
       mov       [rbp-68],eax
       cmp       dword ptr [rbp-68],0
       jg        short M00_L03
       lea       rcx,[rbp-68]
       mov       edx,27
       call      CORINFO_HELP_PATCHPOINT
M00_L03:
       mov       rax,[rbp-48]
       mov       eax,[rax+8]
       cmp       eax,[rbp-4C]
       jg        near ptr M00_L00
       mov       rcx,7FFCCB736E10
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-3C]
       add       rsp,90
       pop       rbp
       ret
M00_L04:
       call      CORINFO_HELP_RNGCHKFAIL
       int       3
; Total bytes of code 289
```

## .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3 (Job: Job-GPHEUC(Affinity=01000000000000000000000000000000, IterationCount=10, IterationTime=100ms, LaunchCount=3, WarmupCount=3))

```assembly
; GlobAcceptance.Match()
       push      rbp
       sub       rsp,90
       lea       rbp,[rsp+90]
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rbp-70],ymm4
       vmovdqa   xmmword ptr [rbp-50],xmm4
       xor       eax,eax
       mov       [rbp-40],rax
       mov       [rbp+10],rcx
       mov       dword ptr [rbp-68],3E8
       xor       eax,eax
       mov       [rbp-3C],eax
       mov       rax,[rbp+10]
       mov       rax,[rax+18]
       mov       [rbp-48],rax
       xor       eax,eax
       mov       [rbp-4C],eax
       jmp       near ptr M00_L02
M00_L00:
       mov       rax,[rbp-48]
       mov       ecx,[rbp-4C]
       cmp       ecx,[rax+8]
       jae       near ptr M00_L04
       mov       edx,ecx
       lea       rax,[rax+rdx*8+10]
       mov       rax,[rax]
       mov       [rbp-58],rax
       mov       rax,[rbp+10]
       mov       rax,[rax+20]
       mov       [rbp-60],rax
       mov       rcx,[rbp-60]
       mov       rdx,7FFCCB729658
       call      CORINFO_HELP_DELEGATEPROFILE32
       mov       rax,[rbp-60]
       mov       [rbp-70],rax
       mov       rax,[rbp-70]
       mov       rdx,[rbp-58]
       mov       rcx,[rax+8]
       mov       rax,[rbp-70]
       call      qword ptr [rax+18]
       test      eax,eax
       je        short M00_L01
       mov       rcx,7FFCCB729760
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-3C]
       inc       eax
       mov       [rbp-3C],eax
M00_L01:
       mov       rcx,7FFCCB729764
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-4C]
       inc       eax
       mov       [rbp-4C],eax
M00_L02:
       mov       eax,[rbp-68]
       dec       eax
       mov       [rbp-68],eax
       cmp       dword ptr [rbp-68],0
       jg        short M00_L03
       lea       rcx,[rbp-68]
       mov       edx,27
       call      CORINFO_HELP_PATCHPOINT
M00_L03:
       mov       rax,[rbp-48]
       mov       eax,[rax+8]
       cmp       eax,[rbp-4C]
       jg        near ptr M00_L00
       mov       rcx,7FFCCB729768
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-3C]
       add       rsp,90
       pop       rbp
       ret
M00_L04:
       call      CORINFO_HELP_RNGCHKFAIL
       int       3
; Total bytes of code 289
```

## .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3 (Job: Job-GPHEUC(Affinity=01000000000000000000000000000000, IterationCount=10, IterationTime=100ms, LaunchCount=3, WarmupCount=3))

```assembly
; GlobAcceptance.Match()
       push      rbp
       sub       rsp,90
       lea       rbp,[rsp+90]
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rbp-70],ymm4
       vmovdqa   xmmword ptr [rbp-50],xmm4
       xor       eax,eax
       mov       [rbp-40],rax
       mov       [rbp+10],rcx
       mov       dword ptr [rbp-68],3E8
       xor       eax,eax
       mov       [rbp-3C],eax
       mov       rax,[rbp+10]
       mov       rax,[rax+18]
       mov       [rbp-48],rax
       xor       eax,eax
       mov       [rbp-4C],eax
       jmp       near ptr M00_L02
M00_L00:
       mov       rax,[rbp-48]
       mov       ecx,[rbp-4C]
       cmp       ecx,[rax+8]
       jae       near ptr M00_L04
       mov       edx,ecx
       lea       rax,[rax+rdx*8+10]
       mov       rax,[rax]
       mov       [rbp-58],rax
       mov       rax,[rbp+10]
       mov       rax,[rax+20]
       mov       [rbp-60],rax
       mov       rcx,[rbp-60]
       mov       rdx,7FFCCB727090
       call      CORINFO_HELP_DELEGATEPROFILE32
       mov       rax,[rbp-60]
       mov       [rbp-70],rax
       mov       rax,[rbp-70]
       mov       rdx,[rbp-58]
       mov       rcx,[rax+8]
       mov       rax,[rbp-70]
       call      qword ptr [rax+18]
       test      eax,eax
       je        short M00_L01
       mov       rcx,7FFCCB727198
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-3C]
       inc       eax
       mov       [rbp-3C],eax
M00_L01:
       mov       rcx,7FFCCB72719C
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-4C]
       inc       eax
       mov       [rbp-4C],eax
M00_L02:
       mov       eax,[rbp-68]
       dec       eax
       mov       [rbp-68],eax
       cmp       dword ptr [rbp-68],0
       jg        short M00_L03
       lea       rcx,[rbp-68]
       mov       edx,27
       call      CORINFO_HELP_PATCHPOINT
M00_L03:
       mov       rax,[rbp-48]
       mov       eax,[rax+8]
       cmp       eax,[rbp-4C]
       jg        near ptr M00_L00
       mov       rcx,7FFCCB7271A0
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-3C]
       add       rsp,90
       pop       rbp
       ret
M00_L04:
       call      CORINFO_HELP_RNGCHKFAIL
       int       3
; Total bytes of code 289
```

## .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3 (Job: Job-GPHEUC(Affinity=01000000000000000000000000000000, IterationCount=10, IterationTime=100ms, LaunchCount=3, WarmupCount=3))

```assembly
; GlobAcceptance.Match()
       push      rbp
       sub       rsp,90
       lea       rbp,[rsp+90]
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rbp-70],ymm4
       vmovdqa   xmmword ptr [rbp-50],xmm4
       xor       eax,eax
       mov       [rbp-40],rax
       mov       [rbp+10],rcx
       mov       dword ptr [rbp-68],3E8
       xor       eax,eax
       mov       [rbp-3C],eax
       mov       rax,[rbp+10]
       mov       rax,[rax+18]
       mov       [rbp-48],rax
       xor       eax,eax
       mov       [rbp-4C],eax
       jmp       near ptr M00_L02
M00_L00:
       mov       rax,[rbp-48]
       mov       ecx,[rbp-4C]
       cmp       ecx,[rax+8]
       jae       near ptr M00_L04
       mov       edx,ecx
       lea       rax,[rax+rdx*8+10]
       mov       rax,[rax]
       mov       [rbp-58],rax
       mov       rax,[rbp+10]
       mov       rax,[rax+20]
       mov       [rbp-60],rax
       mov       rcx,[rbp-60]
       mov       rdx,7FFCCB74B9B8
       call      CORINFO_HELP_DELEGATEPROFILE32
       mov       rax,[rbp-60]
       mov       [rbp-70],rax
       mov       rax,[rbp-70]
       mov       rdx,[rbp-58]
       mov       rcx,[rax+8]
       mov       rax,[rbp-70]
       call      qword ptr [rax+18]
       test      eax,eax
       je        short M00_L01
       mov       rcx,7FFCCB74BAC0
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-3C]
       inc       eax
       mov       [rbp-3C],eax
M00_L01:
       mov       rcx,7FFCCB74BAC4
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-4C]
       inc       eax
       mov       [rbp-4C],eax
M00_L02:
       mov       eax,[rbp-68]
       dec       eax
       mov       [rbp-68],eax
       cmp       dword ptr [rbp-68],0
       jg        short M00_L03
       lea       rcx,[rbp-68]
       mov       edx,27
       call      CORINFO_HELP_PATCHPOINT
M00_L03:
       mov       rax,[rbp-48]
       mov       eax,[rax+8]
       cmp       eax,[rbp-4C]
       jg        near ptr M00_L00
       mov       rcx,7FFCCB74BAC8
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-3C]
       add       rsp,90
       pop       rbp
       ret
M00_L04:
       call      CORINFO_HELP_RNGCHKFAIL
       int       3
; Total bytes of code 289
```

## .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3 (Job: Job-GPHEUC(Affinity=01000000000000000000000000000000, IterationCount=10, IterationTime=100ms, LaunchCount=3, WarmupCount=3))

```assembly
; GlobAcceptance.Match()
       push      rbp
       sub       rsp,90
       lea       rbp,[rsp+90]
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rbp-70],ymm4
       vmovdqa   xmmword ptr [rbp-50],xmm4
       xor       eax,eax
       mov       [rbp-40],rax
       mov       [rbp+10],rcx
       mov       dword ptr [rbp-68],3E8
       xor       eax,eax
       mov       [rbp-3C],eax
       mov       rax,[rbp+10]
       mov       rax,[rax+18]
       mov       [rbp-48],rax
       xor       eax,eax
       mov       [rbp-4C],eax
       jmp       near ptr M00_L02
M00_L00:
       mov       rax,[rbp-48]
       mov       ecx,[rbp-4C]
       cmp       ecx,[rax+8]
       jae       near ptr M00_L04
       mov       edx,ecx
       lea       rax,[rax+rdx*8+10]
       mov       rax,[rax]
       mov       [rbp-58],rax
       mov       rax,[rbp+10]
       mov       rax,[rax+20]
       mov       [rbp-60],rax
       mov       rcx,[rbp-60]
       mov       rdx,7FFCCB738E58
       call      CORINFO_HELP_DELEGATEPROFILE32
       mov       rax,[rbp-60]
       mov       [rbp-70],rax
       mov       rax,[rbp-70]
       mov       rdx,[rbp-58]
       mov       rcx,[rax+8]
       mov       rax,[rbp-70]
       call      qword ptr [rax+18]
       test      eax,eax
       je        short M00_L01
       mov       rcx,7FFCCB738F60
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-3C]
       inc       eax
       mov       [rbp-3C],eax
M00_L01:
       mov       rcx,7FFCCB738F64
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-4C]
       inc       eax
       mov       [rbp-4C],eax
M00_L02:
       mov       eax,[rbp-68]
       dec       eax
       mov       [rbp-68],eax
       cmp       dword ptr [rbp-68],0
       jg        short M00_L03
       lea       rcx,[rbp-68]
       mov       edx,27
       call      CORINFO_HELP_PATCHPOINT
M00_L03:
       mov       rax,[rbp-48]
       mov       eax,[rax+8]
       cmp       eax,[rbp-4C]
       jg        near ptr M00_L00
       mov       rcx,7FFCCB738F68
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-3C]
       add       rsp,90
       pop       rbp
       ret
M00_L04:
       call      CORINFO_HELP_RNGCHKFAIL
       int       3
; Total bytes of code 289
```

## .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3 (Job: Job-GPHEUC(Affinity=01000000000000000000000000000000, IterationCount=10, IterationTime=100ms, LaunchCount=3, WarmupCount=3))

```assembly
; GlobAcceptance.Match()
       push      rbp
       sub       rsp,90
       lea       rbp,[rsp+90]
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rbp-70],ymm4
       vmovdqa   xmmword ptr [rbp-50],xmm4
       xor       eax,eax
       mov       [rbp-40],rax
       mov       [rbp+10],rcx
       mov       dword ptr [rbp-68],3E8
       xor       eax,eax
       mov       [rbp-3C],eax
       mov       rax,[rbp+10]
       mov       rax,[rax+18]
       mov       [rbp-48],rax
       xor       eax,eax
       mov       [rbp-4C],eax
       jmp       near ptr M00_L02
M00_L00:
       mov       rax,[rbp-48]
       mov       ecx,[rbp-4C]
       cmp       ecx,[rax+8]
       jae       near ptr M00_L04
       mov       edx,ecx
       lea       rax,[rax+rdx*8+10]
       mov       rax,[rax]
       mov       [rbp-58],rax
       mov       rax,[rbp+10]
       mov       rax,[rax+20]
       mov       [rbp-60],rax
       mov       rcx,[rbp-60]
       mov       rdx,7FFCCB7370A0
       call      CORINFO_HELP_DELEGATEPROFILE32
       mov       rax,[rbp-60]
       mov       [rbp-70],rax
       mov       rax,[rbp-70]
       mov       rdx,[rbp-58]
       mov       rcx,[rax+8]
       mov       rax,[rbp-70]
       call      qword ptr [rax+18]
       test      eax,eax
       je        short M00_L01
       mov       rcx,7FFCCB7371A8
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-3C]
       inc       eax
       mov       [rbp-3C],eax
M00_L01:
       mov       rcx,7FFCCB7371AC
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-4C]
       inc       eax
       mov       [rbp-4C],eax
M00_L02:
       mov       eax,[rbp-68]
       dec       eax
       mov       [rbp-68],eax
       cmp       dword ptr [rbp-68],0
       jg        short M00_L03
       lea       rcx,[rbp-68]
       mov       edx,27
       call      CORINFO_HELP_PATCHPOINT
M00_L03:
       mov       rax,[rbp-48]
       mov       eax,[rax+8]
       cmp       eax,[rbp-4C]
       jg        near ptr M00_L00
       mov       rcx,7FFCCB7371B0
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-3C]
       add       rsp,90
       pop       rbp
       ret
M00_L04:
       call      CORINFO_HELP_RNGCHKFAIL
       int       3
; Total bytes of code 289
```

## .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3 (Job: Job-GPHEUC(Affinity=01000000000000000000000000000000, IterationCount=10, IterationTime=100ms, LaunchCount=3, WarmupCount=3))

```assembly
; GlobAcceptance.Match()
       push      rbp
       sub       rsp,90
       lea       rbp,[rsp+90]
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rbp-70],ymm4
       vmovdqa   xmmword ptr [rbp-50],xmm4
       xor       eax,eax
       mov       [rbp-40],rax
       mov       [rbp+10],rcx
       mov       dword ptr [rbp-68],3E8
       xor       eax,eax
       mov       [rbp-3C],eax
       mov       rax,[rbp+10]
       mov       rax,[rax+18]
       mov       [rbp-48],rax
       xor       eax,eax
       mov       [rbp-4C],eax
       jmp       near ptr M00_L02
M00_L00:
       mov       rax,[rbp-48]
       mov       ecx,[rbp-4C]
       cmp       ecx,[rax+8]
       jae       near ptr M00_L04
       mov       edx,ecx
       lea       rax,[rax+rdx*8+10]
       mov       rax,[rax]
       mov       [rbp-58],rax
       mov       rax,[rbp+10]
       mov       rax,[rax+20]
       mov       [rbp-60],rax
       mov       rcx,[rbp-60]
       mov       rdx,7FFCCB74B5B0
       call      CORINFO_HELP_DELEGATEPROFILE32
       mov       rax,[rbp-60]
       mov       [rbp-70],rax
       mov       rax,[rbp-70]
       mov       rdx,[rbp-58]
       mov       rcx,[rax+8]
       mov       rax,[rbp-70]
       call      qword ptr [rax+18]
       test      eax,eax
       je        short M00_L01
       mov       rcx,7FFCCB74B6B8
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-3C]
       inc       eax
       mov       [rbp-3C],eax
M00_L01:
       mov       rcx,7FFCCB74B6BC
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-4C]
       inc       eax
       mov       [rbp-4C],eax
M00_L02:
       mov       eax,[rbp-68]
       dec       eax
       mov       [rbp-68],eax
       cmp       dword ptr [rbp-68],0
       jg        short M00_L03
       lea       rcx,[rbp-68]
       mov       edx,27
       call      CORINFO_HELP_PATCHPOINT
M00_L03:
       mov       rax,[rbp-48]
       mov       eax,[rax+8]
       cmp       eax,[rbp-4C]
       jg        near ptr M00_L00
       mov       rcx,7FFCCB74B6C0
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-3C]
       add       rsp,90
       pop       rbp
       ret
M00_L04:
       call      CORINFO_HELP_RNGCHKFAIL
       int       3
; Total bytes of code 289
```

## .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3 (Job: Job-GPHEUC(Affinity=01000000000000000000000000000000, IterationCount=10, IterationTime=100ms, LaunchCount=3, WarmupCount=3))

```assembly
; GlobAcceptance.Match()
       push      rbp
       sub       rsp,90
       lea       rbp,[rsp+90]
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rbp-70],ymm4
       vmovdqa   xmmword ptr [rbp-50],xmm4
       xor       eax,eax
       mov       [rbp-40],rax
       mov       [rbp+10],rcx
       mov       dword ptr [rbp-68],3E8
       xor       eax,eax
       mov       [rbp-3C],eax
       mov       rax,[rbp+10]
       mov       rax,[rax+18]
       mov       [rbp-48],rax
       xor       eax,eax
       mov       [rbp-4C],eax
       jmp       near ptr M00_L02
M00_L00:
       mov       rax,[rbp-48]
       mov       ecx,[rbp-4C]
       cmp       ecx,[rax+8]
       jae       near ptr M00_L04
       mov       edx,ecx
       lea       rax,[rax+rdx*8+10]
       mov       rax,[rax]
       mov       [rbp-58],rax
       mov       rax,[rbp+10]
       mov       rax,[rax+20]
       mov       [rbp-60],rax
       mov       rcx,[rbp-60]
       mov       rdx,7FFCCB71C640
       call      CORINFO_HELP_DELEGATEPROFILE32
       mov       rax,[rbp-60]
       mov       [rbp-70],rax
       mov       rax,[rbp-70]
       mov       rdx,[rbp-58]
       mov       rcx,[rax+8]
       mov       rax,[rbp-70]
       call      qword ptr [rax+18]
       test      eax,eax
       je        short M00_L01
       mov       rcx,7FFCCB71C748
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-3C]
       inc       eax
       mov       [rbp-3C],eax
M00_L01:
       mov       rcx,7FFCCB71C74C
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-4C]
       inc       eax
       mov       [rbp-4C],eax
M00_L02:
       mov       eax,[rbp-68]
       dec       eax
       mov       [rbp-68],eax
       cmp       dword ptr [rbp-68],0
       jg        short M00_L03
       lea       rcx,[rbp-68]
       mov       edx,27
       call      CORINFO_HELP_PATCHPOINT
M00_L03:
       mov       rax,[rbp-48]
       mov       eax,[rax+8]
       cmp       eax,[rbp-4C]
       jg        near ptr M00_L00
       mov       rcx,7FFCCB71C750
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-3C]
       add       rsp,90
       pop       rbp
       ret
M00_L04:
       call      CORINFO_HELP_RNGCHKFAIL
       int       3
; Total bytes of code 289
```

## .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3 (Job: Job-GPHEUC(Affinity=01000000000000000000000000000000, IterationCount=10, IterationTime=100ms, LaunchCount=3, WarmupCount=3))

```assembly
; GlobAcceptance.Match()
       push      rbp
       sub       rsp,90
       lea       rbp,[rsp+90]
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rbp-70],ymm4
       vmovdqa   xmmword ptr [rbp-50],xmm4
       xor       eax,eax
       mov       [rbp-40],rax
       mov       [rbp+10],rcx
       mov       dword ptr [rbp-68],3E8
       xor       eax,eax
       mov       [rbp-3C],eax
       mov       rax,[rbp+10]
       mov       rax,[rax+18]
       mov       [rbp-48],rax
       xor       eax,eax
       mov       [rbp-4C],eax
       jmp       near ptr M00_L02
M00_L00:
       mov       rax,[rbp-48]
       mov       ecx,[rbp-4C]
       cmp       ecx,[rax+8]
       jae       near ptr M00_L04
       mov       edx,ecx
       lea       rax,[rax+rdx*8+10]
       mov       rax,[rax]
       mov       [rbp-58],rax
       mov       rax,[rbp+10]
       mov       rax,[rax+20]
       mov       [rbp-60],rax
       mov       rcx,[rbp-60]
       mov       rdx,7FFCCB72B6C8
       call      CORINFO_HELP_DELEGATEPROFILE32
       mov       rax,[rbp-60]
       mov       [rbp-70],rax
       mov       rax,[rbp-70]
       mov       rdx,[rbp-58]
       mov       rcx,[rax+8]
       mov       rax,[rbp-70]
       call      qword ptr [rax+18]
       test      eax,eax
       je        short M00_L01
       mov       rcx,7FFCCB72B7D0
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-3C]
       inc       eax
       mov       [rbp-3C],eax
M00_L01:
       mov       rcx,7FFCCB72B7D4
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-4C]
       inc       eax
       mov       [rbp-4C],eax
M00_L02:
       mov       eax,[rbp-68]
       dec       eax
       mov       [rbp-68],eax
       cmp       dword ptr [rbp-68],0
       jg        short M00_L03
       lea       rcx,[rbp-68]
       mov       edx,27
       call      CORINFO_HELP_PATCHPOINT
M00_L03:
       mov       rax,[rbp-48]
       mov       eax,[rax+8]
       cmp       eax,[rbp-4C]
       jg        near ptr M00_L00
       mov       rcx,7FFCCB72B7D8
       call      CORINFO_HELP_COUNTPROFILE32
       mov       eax,[rbp-3C]
       add       rsp,90
       pop       rbp
       ret
M00_L04:
       call      CORINFO_HELP_RNGCHKFAIL
       int       3
; Total bytes of code 289
```

