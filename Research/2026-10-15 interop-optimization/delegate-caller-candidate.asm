; Assembly listing for method MatchScenario:<.ctor>b__10_0(System.String):bool:this (FullOpts)
; Emitting BLENDED_CODE for generic X64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; fully interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000
       mov      rax, rdx
 
G_M000_IG02:                ;; offset=0x0003
       mov      rdx, gword ptr [rcx+0x10]
       mov      rcx, rax
 
G_M000_IG03:                ;; offset=0x000A
       tail.jmp [MatchScenario:Oracle(System.String,System.String):bool]
 
; Total bytes of code 16

; Assembly listing for method MatchScenario:ExecuteBatch(int):long:this (FullOpts)
; Emitting BLENDED_CODE for generic X64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data
; 2 inlinees with PGO data; 1 single block inlinees; 0 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     r14
       push     rdi
       push     rsi
       push     rbp
       push     rbx
       sub      rsp, 32
       mov      rbx, rcx
       mov      esi, edx
 
G_M000_IG02:                ;; offset=0x000F
       xor      edi, edi
       test     esi, esi
       jle      SHORT G_M000_IG07
 
G_M000_IG03:                ;; offset=0x0015
       test     edi, 0xFFF
       jne      SHORT G_M000_IG06
 
G_M000_IG04:                ;; offset=0x001D
       lea      rcx, bword ptr [rbx+0x50]
       cmp      gword ptr [rcx], 0
       je       SHORT G_M000_IG06
 
G_M000_IG05:                ;; offset=0x0027
       mov      rax, gword ptr [rcx]
       cmp      dword ptr [rax+0x20], 0
       jne      SHORT G_M000_IG09
 
G_M000_IG06:                ;; offset=0x0030
       mov      rcx, gword ptr [rbx+0x18]
       mov      eax, edi
       cdq      
       idiv     edx:eax, dword ptr [rcx+0x08]
       mov      ebp, edx
       mov      r14, qword ptr [rbx+0x38]
       mov      rax, gword ptr [rbx+0x28]
       cmp      ebp, dword ptr [rcx+0x08]
       jae      SHORT G_M000_IG10
       mov      edx, ebp
       mov      rdx, gword ptr [rcx+8*rdx+0x10]
       mov      rcx, gword ptr [rax+0x08]
       call     [rax+0x18]System.Func`2[System.__Canon,bool]:Invoke(System.__Canon):bool:this
       test     eax, eax
       setne    al
       movzx    rax, al
       add      rax, r14
       mov      qword ptr [rbx+0x38], rax
       mov      rax, gword ptr [rbx+0x20]
       cmp      ebp, dword ptr [rax+0x08]
       jae      SHORT G_M000_IG10
       mov      ecx, ebp
       cmp      byte  ptr [rax+rcx+0x10], 0
       setne    al
       movzx    rax, al
       add      qword ptr [rbx+0x40], rax
       inc      edi
       cmp      edi, esi
       jl       SHORT G_M000_IG03
 
G_M000_IG07:                ;; offset=0x0086
       movsxd   rax, esi
 
G_M000_IG08:                ;; offset=0x0089
       add      rsp, 32
       pop      rbx
       pop      rbp
       pop      rsi
       pop      rdi
       pop      r14
       ret      
 
G_M000_IG09:                ;; offset=0x0094
       call     [System.Threading.CancellationToken:ThrowOperationCanceledException():this]
       int3     
 
G_M000_IG10:                ;; offset=0x009B
       call     CORINFO_HELP_RNGCHKFAIL
       int3     
 
; Total bytes of code 161

; Assembly listing for method MatchScenario:<.ctor>b__10_1(System.String):bool:this (FullOpts)
; Emitting BLENDED_CODE for generic X64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 0 single block inlinees; 1 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       mov      r8, rdx
 
G_M000_IG02:                ;; offset=0x0003
       mov      rdx, gword ptr [rcx+0x10]
       mov      rcx, r8
       xor      r8d, r8d
 
G_M000_IG03:                ;; offset=0x000D
       tail.jmp [Tedd.WildcardMatch:IsMatch(System.String,System.String,int):bool]
 
; Total bytes of code 19

