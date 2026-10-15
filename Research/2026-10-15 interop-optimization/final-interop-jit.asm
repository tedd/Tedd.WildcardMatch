; Assembly listing for method Tedd.WildcardMatchBenchmark.NativeMatcher:MatchEach():int:this (FullOpts)
; Emitting BLENDED_CODE for generic X64 + VEX on Windows
; FullOpts code
; optimized code
; rbp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rbp
       push     r15
       push     r14
       push     r13
       push     r12
       push     rdi
       push     rsi
       push     rbx
       sub      rsp, 88
       lea      rbp, [rsp+0x90]
       mov      rbx, rcx
 
G_M000_IG02:                ;; offset=0x001B
       lea      rcx, [rbp-0x70]
       call     CORINFO_HELP_INIT_PINVOKE_FRAME
       mov      rsi, rax
       mov      rcx, rsp
       mov      qword ptr [rbp-0x58], rcx
       mov      rcx, rbp
       mov      qword ptr [rbp-0x48], rcx
       xor      edi, edi
       xor      r14d, r14d
       cmp      dword ptr [rbx+0x38], 0
       jle      SHORT G_M000_IG07
 
G_M000_IG03:                ;; offset=0x0040
       mov      rax, qword ptr [rbx+0x20]
       mov      gword ptr [rbp+0x10], rbx
       mov      rcx, qword ptr [rbx+0x10]
       mov      edx, r14d
       lea      r8, G_M000_IG05
       mov      qword ptr [rbp-0x50], r8
       lea      r8, [rbp-0x70]
       mov      qword ptr [rsi+0x08], r8
       mov      byte  ptr [rsi+0x04], 0
 
G_M000_IG04:                ;; offset=0x0066
       call     rax
 
G_M000_IG05:                ;; offset=0x0068
       mov      byte  ptr [rsi+0x04], 1
       cmp      dword ptr [(reloc 0x7ffcc5463a90)], 0
       je       SHORT G_M000_IG06
       call     [CORINFO_HELP_STOP_FOR_GC]
 
G_M000_IG06:                ;; offset=0x007B
       mov      rcx, qword ptr [rbp-0x68]
       mov      qword ptr [rsi+0x08], rcx
       add      edi, eax
       inc      r14d
       mov      rbx, gword ptr [rbp+0x10]
       cmp      r14d, dword ptr [rbx+0x38]
       jl       SHORT G_M000_IG03
 
G_M000_IG07:                ;; offset=0x0092
       mov      eax, edi
 
G_M000_IG08:                ;; offset=0x0094
       add      rsp, 88
       pop      rbx
       pop      rsi
       pop      rdi
       pop      r12
       pop      r13
       pop      r14
       pop      r15
       pop      rbp
       ret      
 
; Total bytes of code 165

; Assembly listing for method Tedd.WildcardMatchBenchmark.NativeMatcher:MatchEachSuppressed():int:this (FullOpts)
; Emitting BLENDED_CODE for generic X64 + VEX on Windows
; FullOpts code
; optimized code
; rbp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rbp
       push     rdi
       push     rsi
       push     rbx
       sub      rsp, 40
       lea      rbp, [rsp+0x40]
       mov      rbx, rcx
 
G_M000_IG02:                ;; offset=0x0010
       cmp      byte  ptr [rbx+0x3C], 0
       je       SHORT G_M000_IG08
       xor      esi, esi
       xor      edi, edi
       cmp      dword ptr [rbx+0x38], 0
       jle      SHORT G_M000_IG06
 
G_M000_IG03:                ;; offset=0x0020
       mov      rax, qword ptr [rbx+0x28]
       mov      gword ptr [rbp+0x10], rbx
       mov      rcx, qword ptr [rbx+0x10]
       mov      edx, edi
 
G_M000_IG04:                ;; offset=0x002E
       call     rax
       add      esi, eax
       inc      edi
       cmp      dword ptr [(reloc 0x7ffcc5463a90)], 0
       jne      SHORT G_M000_IG09
 
G_M000_IG05:                ;; offset=0x003D
       mov      rbx, gword ptr [rbp+0x10]
       cmp      edi, dword ptr [rbx+0x38]
       jl       SHORT G_M000_IG03
 
G_M000_IG06:                ;; offset=0x0046
       mov      eax, esi
 
G_M000_IG07:                ;; offset=0x0048
       add      rsp, 40
       pop      rbx
       pop      rsi
       pop      rdi
       pop      rbp
       ret      
 
G_M000_IG08:                ;; offset=0x0051
       mov      rcx, 0x7FFC6566AED0
       call     CORINFO_HELP_NEWSFAST
       mov      rbx, rax
       mov      ecx, 0x92C
       mov      rdx, 0x7FFC6572EC88
       call     [CORINFO_HELP_STRCNS]
       mov      rdx, rax
       mov      rcx, rbx
       call     [System.InvalidOperationException:.ctor(System.String):this]
       mov      rcx, rbx
       call     CORINFO_HELP_THROW
       int3     
 
G_M000_IG09:                ;; offset=0x008D
       call     CORINFO_HELP_POLL_GC
       jmp      SHORT G_M000_IG05
 
; Total bytes of code 148

