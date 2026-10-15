; Assembly listing for method Tedd.WildcardMatch:IsMatch(System.String,System.String,int):bool (FullOpts)
; Emitting BLENDED_CODE for generic X64 + VEX on Windows
; FullOpts code
; optimized code
; rbp based frame
; partially interruptible
; No PGO data
; 5 inlinees with PGO data; 20 single block inlinees; 4 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rbp
       push     r15
       push     r14
       push     r13
       push     rdi
       push     rsi
       push     rbx
       sub      rsp, 112
       lea      rbp, [rsp+0xA0]
       vxorps   xmm4, xmm4, xmm4
       vmovdqu  ymmword ptr [rbp-0x50], ymm4
       mov      rdi, rcx
       mov      rsi, rdx
       mov      ebx, r8d
 
G_M000_IG02:                ;; offset=0x0028
       test     rsi, rsi
       je       G_M000_IG19
       mov      ecx, ebx
       call     [Tedd.WildcardEngine:ValidateOptions(int)]
       mov      rcx, rsi
       mov      edx, ebx
       call     [Tedd.WildcardEngine:GetMatchingPattern(System.String,int):System.String]
       mov      r14, rax
       test     ebx, 0x803
       je       G_M000_IG06
 
G_M000_IG03:                ;; offset=0x0053
       mov      ecx, ebx
       call     [Tedd.WildcardEngine:GetCaseBehavior(int):int]
       mov      r15d, eax
       test     byte  ptr [(reloc 0x7ffc6583e6f0)], 1
       je       G_M000_IG23
 
G_M000_IG04:                ;; offset=0x006B
       mov      rdx, 0x1D7B6000C60
       mov      rdx, gword ptr [rdx]
       mov      rcx, r14
       cmp      dword ptr [rcx], ecx
       call     [System.String:IndexOfAny(char[]):int:this]
       mov      ecx, eax
       shr      ecx, 31
       mov      r9, 0x1D7802D6DC8
       mov      rdx, qword ptr [r9]
       mov      gword ptr [rsp+0x30], rsi
       mov      dword ptr [rsp+0x38], -1
       mov      dword ptr [rsp+0x40], -1
       mov      dword ptr [rsp+0x48], -1
       mov      r9d, r15d
       mov      dword ptr [rsp+0x20], ecx
       mov      qword ptr [rsp+0x28], rdx
       mov      rcx, rdi
       mov      rdx, r14
       mov      r8d, ebx
       call     [Tedd.WildcardEngine:IsMatch(System.String,System.String,int,int,bool,System.TimeSpan,System.String,int,int,int):bool]
       nop      
 
G_M000_IG05:                ;; offset=0x00CE
       add      rsp, 112
       pop      rbx
       pop      rsi
       pop      rdi
       pop      r13
       pop      r14
       pop      r15
       pop      rbp
       ret      
 
G_M000_IG06:                ;; offset=0x00DD
       test     rdi, rdi
       je       G_M000_IG20
       lea      rsi, bword ptr [rdi+0x0C]
       mov      edi, dword ptr [rdi+0x08]
       test     r14, r14
       je       SHORT G_M000_IG07
       lea      r15, bword ptr [r14+0x0C]
       mov      r13d, dword ptr [r14+0x08]
       jmp      SHORT G_M000_IG08
 
G_M000_IG07:                ;; offset=0x00FC
       xor      r15, r15
       xor      r13d, r13d
 
G_M000_IG08:                ;; offset=0x0102
       test     bl, 16
       setne    bl
       movzx    rbx, bl
       test     byte  ptr [(reloc 0x7ffc6583e6f0)], 1
       je       G_M000_IG21
 
G_M000_IG09:                ;; offset=0x0118
       mov      rdx, 0x1D7B6000C60
       mov      rdx, gword ptr [rdx]
       mov      rcx, r14
       cmp      dword ptr [rcx], ecx
       call     [System.String:IndexOfAny(char[]):int:this]
       test     eax, eax
       jl       G_M000_IG12
       mov      rcx, r15
       mov      r8d, r13d
       mov      edx, 42
       call     [System.PackedSpanHelpers:IndexOf[System.SpanHelpers+DontNegate`1[short],System.PackedSpanHelpers+NopTransform](byref,short,int):int]
       test     eax, eax
       jl       G_M000_IG11
       mov      bword ptr [rbp-0x40], rsi
       mov      dword ptr [rbp-0x38], edi
       mov      bword ptr [rbp-0x50], r15
       mov      dword ptr [rbp-0x48], r13d
       mov      dword ptr [rsp+0x20], -1
       lea      rcx, [rbp-0x40]
       lea      rdx, [rbp-0x50]
       mov      r8d, ebx
       mov      r9d, -1
       call     [Tedd.DefaultWildcardEngine:Window(System.ReadOnlySpan`1[char],System.ReadOnlySpan`1[char],bool,int,int):bool]
       test     eax, eax
       je       SHORT G_M000_IG10
       mov      eax, 1
       jmp      G_M000_IG18
 
G_M000_IG10:                ;; offset=0x018D
       test     edi, edi
       jle      G_M000_IG13
       lea      r14d, [rdi-0x01]
       mov      ecx, r14d
       cmp      word  ptr [rsi+2*rcx], 10
       jne      G_M000_IG13
       cmp      r14d, edi
       ja       G_M000_IG22
       mov      bword ptr [rbp-0x40], rsi
       mov      dword ptr [rbp-0x38], r14d
       mov      bword ptr [rbp-0x50], r15
       mov      dword ptr [rbp-0x48], r13d
       mov      dword ptr [rsp+0x20], -1
       lea      rcx, [rbp-0x40]
       lea      rdx, [rbp-0x50]
       mov      r8d, ebx
       mov      r9d, -1
       call     [Tedd.DefaultWildcardEngine:Window(System.ReadOnlySpan`1[char],System.ReadOnlySpan`1[char],bool,int,int):bool]
       jmp      G_M000_IG18
 
G_M000_IG11:                ;; offset=0x01E4
       mov      bword ptr [rbp-0x40], rsi
       mov      dword ptr [rbp-0x38], edi
       mov      bword ptr [rbp-0x50], r15
       mov      dword ptr [rbp-0x48], r13d
       lea      rcx, [rbp-0x40]
       lea      rdx, [rbp-0x50]
       mov      r8d, ebx
       call     [Tedd.DefaultWildcardEngine:Fixed(System.ReadOnlySpan`1[char],System.ReadOnlySpan`1[char],bool):bool]
       jmp      SHORT G_M000_IG18
 
G_M000_IG12:                ;; offset=0x0206
       cmp      edi, r13d
       je       SHORT G_M000_IG14
       test     edi, edi
       jle      SHORT G_M000_IG13
       lea      r14d, [rdi-0x01]
       cmp      r14d, r13d
       jne      SHORT G_M000_IG13
       mov      eax, r14d
       mov      ecx, eax
       cmp      word  ptr [rsi+2*rcx], 10
       je       SHORT G_M000_IG14
 
G_M000_IG13:                ;; offset=0x0224
       xor      eax, eax
       jmp      SHORT G_M000_IG18
 
G_M000_IG14:                ;; offset=0x0228
       cmp      r13d, edi
       ja       G_M000_IG22
 
G_M000_IG15:                ;; offset=0x0231
       mov      r8d, r13d
       add      r8, r8
       cmp      r8, 10
       je       SHORT G_M000_IG17
 
G_M000_IG16:                ;; offset=0x023D
       mov      rcx, rsi
       mov      rdx, r15
       call     [System.SpanHelpers:SequenceEqual(byref,byref,nuint):bool]
       jmp      SHORT G_M000_IG18
 
G_M000_IG17:                ;; offset=0x024B
       mov      rax, qword ptr [rsi]
       mov      rcx, qword ptr [rsi+0x02]
       mov      rdx, qword ptr [r15]
       xor      rax, rdx
       xor      rcx, qword ptr [r15+0x02]
       or       rax, rcx
       sete     al
       movzx    rax, al
 
G_M000_IG18:                ;; offset=0x0265
       add      rsp, 112
       pop      rbx
       pop      rsi
       pop      rdi
       pop      r13
       pop      r14
       pop      r15
       pop      rbp
       ret      
 
G_M000_IG19:                ;; offset=0x0274
       mov      rcx, 0x7FFC656684E0
       call     CORINFO_HELP_NEWSFAST
       mov      rbx, rax
       mov      ecx, 35
       mov      rdx, 0x7FFC6583D248
       call     [CORINFO_HELP_STRCNS]
       mov      rdx, rax
       mov      rcx, rbx
       call     [System.ArgumentNullException:.ctor(System.String):this]
       mov      rcx, rbx
       call     CORINFO_HELP_THROW
       int3     
 
G_M000_IG20:                ;; offset=0x02B0
       mov      rcx, 0x7FFC656684E0
       call     CORINFO_HELP_NEWSFAST
       mov      rbx, rax
       mov      ecx, 155
       mov      rdx, 0x7FFC6583D248
       call     [CORINFO_HELP_STRCNS]
       mov      rdx, rax
       mov      rcx, rbx
       call     [System.ArgumentNullException:.ctor(System.String):this]
       mov      rcx, rbx
       call     CORINFO_HELP_THROW
       int3     
 
G_M000_IG21:                ;; offset=0x02EC
       mov      rcx, 0x7FFC6583E688
       call     CORINFO_HELP_GET_GCSTATIC_BASE
       jmp      G_M000_IG09
 
G_M000_IG22:                ;; offset=0x0300
       call     [System.ThrowHelper:ThrowArgumentOutOfRangeException()]
       int3     
 
G_M000_IG23:                ;; offset=0x0307
       mov      rcx, 0x7FFC6583E688
       call     CORINFO_HELP_GET_GCSTATIC_BASE
       jmp      G_M000_IG04
 
; Total bytes of code 795

; Assembly listing for method Tedd.WildcardMatch:IsMatch(System.String):bool:this (FullOpts)
; Emitting BLENDED_CODE for generic X64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; fully interruptible
; No PGO data
; 6 inlinees with PGO data; 32 single block inlinees; 7 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     r15
       push     r14
       push     rdi
       push     rsi
       push     rbp
       push     rbx
       sub      rsp, 56
       xor      eax, eax
       mov      qword ptr [rsp+0x28], rax
       mov      qword ptr [rsp+0x30], rax
 
G_M000_IG02:                ;; offset=0x0018
       test     rdx, rdx
       je       G_M000_IG46
       cmp      byte  ptr [rcx+0x5C], 0
       je       SHORT G_M000_IG04
 
G_M000_IG03:                ;; offset=0x0027
       cmp      byte  ptr [rcx+0x5D], 0
       jne      G_M000_IG24
       mov      rbx, gword ptr [rcx+0x10]
       test     rbx, rbx
       jne      G_M000_IG15
       cmp      dword ptr [rdx+0x08], 32
       jg       SHORT G_M000_IG04
       mov      rbx, gword ptr [rcx+0x08]
       test     rbx, rbx
       jne      SHORT G_M000_IG06
 
G_M000_IG04:                ;; offset=0x004D
       mov      rax, qword ptr [rcx+0x40]
 
G_M000_IG05:                ;; offset=0x0051
       vzeroupper 
       add      rsp, 56
       pop      rbx
       pop      rbp
       pop      rsi
       pop      rdi
       pop      r14
       pop      r15
       tail.jmp rax
 
G_M000_IG06:                ;; offset=0x0063
       mov      rcx, rbx
       lea      r8, bword ptr [rdx+0x0C]
       mov      edx, dword ptr [rdx+0x08]
       mov      rax, r8
       mov      r10d, edx
       movzx    rbx, byte  ptr [rcx+0x3E]
       test     ebx, ebx
       jne      SHORT G_M000_IG07
       test     r10d, r10d
       jle      SHORT G_M000_IG07
       lea      esi, [r10-0x01]
       mov      r9d, esi
       cmp      word  ptr [rax+2*r9], 10
       cmove    r10d, esi
 
G_M000_IG07:                ;; offset=0x0091
       lea      r9d, [r10-0x04]
       cmp      r9d, 4
       jb       SHORT G_M000_IG08
       mov      bword ptr [rsp+0x28], r8
       mov      dword ptr [rsp+0x30], edx
       lea      rdx, [rsp+0x28]
       mov      r8d, r10d
       call     [Tedd.ShortPattern:MatchPackedScalar(System.ReadOnlySpan`1[char],int):bool:this]
       jmp      G_M000_IG14
       align    [3 bytes for IG10]
 
G_M000_IG08:                ;; offset=0x00BA
       mov      r8, qword ptr [rax]
       lea      edx, [r10-0x04]
       vmovq    xmm0, r8
       vpinsrq  xmm0, xmm0, qword ptr [rax+2*rdx], 1
       vpbroadcastw xmm1, word  ptr [rcx+0x3C]
       vpcmpeqw xmm1, xmm1, xmm0
       vpmovmskb r8d, xmm1
       lzcnt    r8d, r8d
       neg      r8d
       add      r8d, 31
       sar      r8d, 1
       cmp      r8d, 4
       jl       SHORT G_M000_IG09
       lea      r8d, [r8+r10-0x08]
 
G_M000_IG09:                ;; offset=0x00F5
       sub      r8d, dword ptr [rcx+0x38]
       xor      edx, edx
       test     r8d, r8d
       jl       SHORT G_M000_IG11
 
G_M000_IG10:                ;; offset=0x0100
       movsxd   r10, edx
       mov      r10, qword ptr [rax+2*r10]
       and      r10, qword ptr [rcx+0x30]
       cmp      r10, qword ptr [rcx+0x28]
       je       SHORT G_M000_IG12
       inc      edx
       cmp      edx, r8d
       jle      SHORT G_M000_IG10
 
G_M000_IG11:                ;; offset=0x0118
       xor      eax, eax
       jmp      SHORT G_M000_IG14
 
G_M000_IG12:                ;; offset=0x011C
       test     ebx, ebx
       je       SHORT G_M000_IG13
       mov      eax, 1
       jmp      SHORT G_M000_IG14
 
G_M000_IG13:                ;; offset=0x0127
       vpcmpeqw xmm0, xmm0, xmmword ptr [reloc @RWD00]
       vptest   xmm0, xmm0
       sete     al
       movzx    rax, al
 
G_M000_IG14:                ;; offset=0x013A
       vzeroupper 
       add      rsp, 56
       pop      rbx
       pop      rbp
       pop      rsi
       pop      rdi
       pop      r14
       pop      r15
       ret      
 
G_M000_IG15:                ;; offset=0x014A
       lea      rsi, bword ptr [rdx+0x0C]
       mov      edi, dword ptr [rdx+0x08]
       mov      rbp, rsi
       mov      rcx, gword ptr [rbx+0x08]
       mov      r14d, dword ptr [rcx+0x08]
       cmp      edi, r14d
       je       SHORT G_M000_IG16
       lea      ecx, [r14+0x01]
       cmp      edi, ecx
       jne      G_M000_IG22
       cmp      r14d, edi
       jae      G_M000_IG48
       mov      ecx, r14d
       cmp      word  ptr [rbp+2*rcx], 10
       jne      G_M000_IG22
 
G_M000_IG16:                ;; offset=0x0185
       mov      r15, gword ptr [rbx+0x10]
       test     r15, r15
       jne      G_M000_IG39
       mov      ecx, r14d
       sub      ecx, 8
       cmp      ecx, 8
       jbe      G_M000_IG37
       cmp      byte  ptr [rbx+0x1C], 0
       jne      SHORT G_M000_IG17
       mov      r8d, dword ptr [rbx+0x18]
       cmp      r8d, edi
       ja       G_M000_IG47
       mov      rcx, rbp
       mov      edx, 10
       call     [System.PackedSpanHelpers:IndexOf[System.SpanHelpers+DontNegate`1[short],System.PackedSpanHelpers+NopTransform](byref,short,int):int]
       test     eax, eax
       jge      SHORT G_M000_IG22
 
G_M000_IG17:                ;; offset=0x01C6
       mov      eax, dword ptr [rbx+0x18]
       cmp      eax, r14d
       jge      G_M000_IG44
       test     eax, eax
       jl       G_M000_IG35
       cmp      r14d, edi
       jg       G_M000_IG35
       mov      rcx, gword ptr [rbx+0x08]
       jmp      SHORT G_M000_IG20
 
G_M000_IG18:                ;; offset=0x01E9
       mov      edx, eax
       movzx    rdx, word  ptr [rbp+2*rdx]
       cmp      edx, r8d
       jne      SHORT G_M000_IG22
 
G_M000_IG19:                ;; offset=0x01F5
       inc      eax
       cmp      eax, r14d
       jge      G_M000_IG44
 
G_M000_IG20:                ;; offset=0x0200
       mov      rdx, rcx
       cmp      eax, dword ptr [rdx+0x08]
       jae      G_M000_IG48
       mov      r8d, eax
       movzx    r8, word  ptr [rdx+2*r8+0x0C]
       cmp      r8d, 63
       jne      SHORT G_M000_IG18
 
G_M000_IG21:                ;; offset=0x021B
       cmp      byte  ptr [rbx+0x1C], 0
       jne      SHORT G_M000_IG19
       mov      r8d, eax
       cmp      word  ptr [rbp+2*r8], 10
       jne      SHORT G_M000_IG19
 
G_M000_IG22:                ;; offset=0x022D
       xor      eax, eax
 
G_M000_IG23:                ;; offset=0x022F
       vzeroupper 
       add      rsp, 56
       pop      rbx
       pop      rbp
       pop      rsi
       pop      rdi
       pop      r14
       pop      r15
       ret      
 
G_M000_IG24:                ;; offset=0x023F
       lea      rax, bword ptr [rdx+0x0C]
       mov      edx, dword ptr [rdx+0x08]
       mov      rcx, gword ptr [rcx+0x28]
       test     rcx, rcx
       je       SHORT G_M000_IG25
       lea      r8, bword ptr [rcx+0x0C]
       mov      ecx, dword ptr [rcx+0x08]
       jmp      SHORT G_M000_IG26
 
G_M000_IG25:                ;; offset=0x0258
       xor      r8, r8
       xor      ecx, ecx
 
G_M000_IG26:                ;; offset=0x025D
       cmp      edx, ecx
       je       SHORT G_M000_IG28
       test     edx, edx
       jle      SHORT G_M000_IG27
       lea      esi, [rdx-0x01]
       cmp      esi, ecx
       jne      SHORT G_M000_IG27
       mov      r10d, esi
       cmp      word  ptr [rax+2*r10], 10
       je       SHORT G_M000_IG28
 
G_M000_IG27:                ;; offset=0x0277
       xor      eax, eax
       jmp      SHORT G_M000_IG32
 
G_M000_IG28:                ;; offset=0x027B
       cmp      ecx, edx
       ja       G_M000_IG47
 
G_M000_IG29:                ;; offset=0x0283
       mov      ecx, ecx
       add      rcx, rcx
       mov      r10, rcx
       cmp      r10, 10
       je       SHORT G_M000_IG31
 
G_M000_IG30:                ;; offset=0x0291
       mov      rcx, rax
       mov      rdx, r8
       mov      r8, r10
       call     [System.SpanHelpers:SequenceEqual(byref,byref,nuint):bool]
       jmp      SHORT G_M000_IG32
       align    [0 bytes for IG40]
 
G_M000_IG31:                ;; offset=0x02A2
       mov      rcx, qword ptr [rax]
       mov      rax, qword ptr [rax+0x02]
       mov      rdx, qword ptr [r8]
       xor      rcx, rdx
       xor      rax, qword ptr [r8+0x02]
       or       rax, rcx
       sete     al
       movzx    rax, al
 
G_M000_IG32:                ;; offset=0x02BC
       vzeroupper 
       add      rsp, 56
       pop      rbx
       pop      rbp
       pop      rsi
       pop      rdi
       pop      r14
       pop      r15
       ret      
 
G_M000_IG33:                ;; offset=0x02CC
       cmp      eax, edi
       jae      G_M000_IG48
       mov      ecx, eax
       movzx    rcx, word  ptr [rbp+2*rcx]
       cmp      ecx, r8d
       jne      G_M000_IG22
 
G_M000_IG34:                ;; offset=0x02E4
       inc      eax
       cmp      eax, r14d
       jge      G_M000_IG44
 
G_M000_IG35:                ;; offset=0x02EF
       mov      rcx, gword ptr [rbx+0x08]
       mov      rdx, rcx
       cmp      eax, dword ptr [rdx+0x08]
       jae      G_M000_IG48
       mov      r8d, eax
       movzx    r8, word  ptr [rdx+2*r8+0x0C]
       cmp      r8d, 63
       jne      SHORT G_M000_IG33
 
G_M000_IG36:                ;; offset=0x030E
       cmp      byte  ptr [rbx+0x1C], 0
       jne      SHORT G_M000_IG34
       cmp      eax, edi
       jae      G_M000_IG48
       mov      r8d, eax
       cmp      word  ptr [rbp+2*r8], 10
       je       G_M000_IG22
       jmp      SHORT G_M000_IG34
 
G_M000_IG37:                ;; offset=0x032E
       vmovups  xmm0, xmmword ptr [rsi]
       add      r14d, -8
       movsxd   rax, r14d
       vmovups  xmm1, xmmword ptr [rsi+2*rax]
       vpxor    xmm2, xmm0, xmmword ptr [rbx+0x28]
       vmovups  xmm3, xmmword ptr [rbx+0x48]
       vpand    xmm2, xmm2, xmm3
       vpxor    xmm4, xmm1, xmmword ptr [rbx+0x38]
       vmovups  xmm5, xmmword ptr [rbx+0x58]
       vpand    xmm4, xmm4, xmm5
       vpor     xmm2, xmm4, xmm2
       cmp      byte  ptr [rbx+0x1C], 0
       jne      SHORT G_M000_IG38
       vbroadcastss xmm4, dword ptr [reloc @RWD00]
       vpcmpeqw xmm0, xmm0, xmm4
       vpandn   xmm0, xmm3, xmm0
       vpcmpeqw xmm1, xmm1, xmm4
       vpandn   xmm1, xmm5, xmm1
       vpor     xmm0, xmm1, xmm0
       vpor     xmm2, xmm0, xmm2
 
G_M000_IG38:                ;; offset=0x0385
       vptest   xmm2, xmm2
       sete     al
       movzx    rax, al
       jmp      G_M000_IG23
 
G_M000_IG39:                ;; offset=0x0395
       xor      eax, eax
       mov      rcx, gword ptr [rbx+0x10]
       cmp      dword ptr [rcx+0x08], 0
       jle      SHORT G_M000_IG44
       lea      ecx, [r14-0x10]
 
G_M000_IG40:                ;; offset=0x03A5
       mov      edx, eax
       shl      edx, 4
       mov      r8d, ecx
       cmp      edx, r8d
       jg       SHORT G_M000_IG45
 
G_M000_IG41:                ;; offset=0x03B2
       movsxd   rdx, edx
       vmovups  ymm0, ymmword ptr [rsi+2*rdx]
       mov      rdx, r15
       cmp      eax, dword ptr [rdx+0x08]
       jae      G_M000_IG48
       mov      r8, rax
       shl      r8, 6
       lea      rdx, bword ptr [rdx+r8+0x10]
       vmovups  ymm1, ymmword ptr [rdx+0x20]
       vpxor    ymm2, ymm0, ymmword ptr [rdx]
       vpand    ymm2, ymm2, ymm1
       cmp      byte  ptr [rbx+0x1C], 0
       jne      SHORT G_M000_IG43
 
G_M000_IG42:                ;; offset=0x03E5
       vpcmpeqw ymm0, ymm0, ymmword ptr [reloc @RWD32]
       vpandn   ymm0, ymm1, ymm0
       vpor     ymm2, ymm0, ymm2
 
G_M000_IG43:                ;; offset=0x03F5
       vptest   ymm2, ymm2
       jne      G_M000_IG22
       inc      eax
       mov      rdx, gword ptr [rbx+0x10]
       cmp      dword ptr [rdx+0x08], eax
       jg       SHORT G_M000_IG40
 
G_M000_IG44:                ;; offset=0x040B
       mov      eax, 1
       jmp      G_M000_IG23
 
G_M000_IG45:                ;; offset=0x0415
       mov      edx, r8d
       jmp      SHORT G_M000_IG41
 
G_M000_IG46:                ;; offset=0x041A
       mov      rcx, 0x7FFC656684E0
       call     CORINFO_HELP_NEWSFAST
       mov      rbx, rax
       mov      ecx, 155
       mov      rdx, 0x7FFC6583D248
       call     [CORINFO_HELP_STRCNS]
       mov      rdx, rax
       mov      rcx, rbx
       call     [System.ArgumentNullException:.ctor(System.String):this]
       mov      rcx, rbx
       call     CORINFO_HELP_THROW
       int3     
 
G_M000_IG47:                ;; offset=0x0456
       call     [System.ThrowHelper:ThrowArgumentOutOfRangeException()]
       int3     
 
G_M000_IG48:                ;; offset=0x045D
       call     CORINFO_HELP_RNGCHKFAIL
       int3     
 
RWD00  	dq	000A000A000A000Ah, 000A000A000A000Ah
RWD16  	dd	00000000h, 00000000h, 00000000h, 00000000h
RWD32  	dq	000A000A000A000Ah, 000A000A000A000Ah, 000A000A000A000Ah, 000A000A000A000Ah

; Total bytes of code 1123

; Assembly listing for method Tedd.ShortPattern:MatchPackedScalar(System.ReadOnlySpan`1[char],int):bool:this (FullOpts)
; Emitting BLENDED_CODE for generic X64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; fully interruptible
; No PGO data
; 5 inlinees with PGO data; 15 single block inlinees; 0 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rdi
       push     rsi
       push     rbx
       sub      rsp, 32
       mov      rbx, rcx
       mov      esi, r8d
 
G_M000_IG02:                ;; offset=0x000D
       mov      rdi, bword ptr [rdx]
       mov      edx, dword ptr [rdx+0x08]
       cmp      esi, dword ptr [rbx+0x38]
       jle      SHORT G_M000_IG05
 
G_M000_IG03:                ;; offset=0x0018
       cmp      esi, edx
       ja       SHORT G_M000_IG11
       movzx    rdx, word  ptr [rbx+0x3C]
       movsx    rdx, dx
       mov      rcx, rdi
       mov      r8d, esi
       call     [System.SpanHelpers:LastIndexOfValueType[short,System.SpanHelpers+DontNegate`1[short]](byref,short,int):int]
       sub      eax, dword ptr [rbx+0x38]
       xor      ecx, ecx
       test     eax, eax
       jl       SHORT G_M000_IG05
       align    [3 bytes for IG04]
 
G_M000_IG04:                ;; offset=0x0040
       movsxd   r8, ecx
       mov      r8, qword ptr [rdi+2*r8]
       and      r8, qword ptr [rbx+0x30]
       cmp      r8, qword ptr [rbx+0x28]
       je       SHORT G_M000_IG07
       inc      ecx
       cmp      ecx, eax
       jle      SHORT G_M000_IG04
 
G_M000_IG05:                ;; offset=0x0057
       xor      eax, eax
 
G_M000_IG06:                ;; offset=0x0059
       add      rsp, 32
       pop      rbx
       pop      rsi
       pop      rdi
       ret      
 
G_M000_IG07:                ;; offset=0x0061
       cmp      byte  ptr [rbx+0x3E], 0
       je       SHORT G_M000_IG09
       mov      eax, 1
 
G_M000_IG08:                ;; offset=0x006C
       add      rsp, 32
       pop      rbx
       pop      rsi
       pop      rdi
       ret      
 
G_M000_IG09:                ;; offset=0x0074
       mov      rcx, rdi
       mov      r8d, esi
       mov      edx, 10
       call     [System.PackedSpanHelpers:IndexOf[System.SpanHelpers+DontNegate`1[short],System.PackedSpanHelpers+NopTransform](byref,short,int):int]
       shr      eax, 31
 
G_M000_IG10:                ;; offset=0x0088
       add      rsp, 32
       pop      rbx
       pop      rsi
       pop      rdi
       ret      
 
G_M000_IG11:                ;; offset=0x0090
       call     [System.ThrowHelper:ThrowArgumentOutOfRangeException()]
       int3     
 
; Total bytes of code 151

; Assembly listing for method Tedd.CompiledSegments:Window(System.ReadOnlySpan`1[char]):bool:this (FullOpts)
; Emitting BLENDED_CODE for generic X64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; fully interruptible
; No PGO data
; 30 inlinees with PGO data; 81 single block inlinees; 4 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     r15
       push     r14
       push     r13
       push     r12
       push     rdi
       push     rsi
       push     rbp
       push     rbx
       sub      rsp, 104
       mov      rbx, rcx
 
G_M000_IG02:                ;; offset=0x0013
       mov      rsi, bword ptr [rdx]
       mov      edi, dword ptr [rdx+0x08]
       cmp      edi, dword ptr [rbx+0x18]
       jl       G_M000_IG65
 
G_M000_IG03:                ;; offset=0x0022
       xor      ebp, ebp
       mov      r14, gword ptr [rbx+0x10]
       mov      r15d, dword ptr [r14+0x08]
       xor      r13d, r13d
       mov      r12d, edi
       cmp      byte  ptr [rbx+0x1D], 0
       jne      G_M000_IG70
       mov      ebp, 1
       cmp      dword ptr [r14+0x08], 0
       jbe      G_M000_IG84
       add      r14, 16
       cmp      dword ptr [r14+0x08], 0
       jl       G_M000_IG09
       mov      ecx, dword ptr [r14+0x0C]
       mov      r13d, dword ptr [r14+0x04]
       cmp      ecx, r13d
       je       G_M000_IG71
       xor      ecx, ecx
       cmp      dword ptr [r14+0x04], 0
       jle      G_M000_IG69
       align    [0 bytes for IG04]
 
G_M000_IG04:                ;; offset=0x0079
       mov      rdx, gword ptr [rbx+0x08]
       mov      r8d, ecx
       add      r8d, dword ptr [r14]
       cmp      r8d, dword ptr [rdx+0x08]
       jae      G_M000_IG84
       movzx    rdx, word  ptr [rdx+2*r8+0x0C]
       cmp      ecx, r12d
       jae      G_M000_IG84
       movzx    r8, word  ptr [rsi+2*rcx]
       cmp      edx, 63
       jne      G_M000_IG67
 
G_M000_IG05:                ;; offset=0x00AA
       cmp      byte  ptr [rbx+0x1C], 0
       jne      G_M000_IG68
       cmp      r8d, 10
       jne      G_M000_IG68
       jmp      G_M000_IG65
 
G_M000_IG06:                ;; offset=0x00C3
       mov      rax, rsi
       mov      rcx, qword ptr [rax]
       mov      r8, qword ptr [rax+0x02]
       mov      rax, qword ptr [rdx]
       xor      rcx, rax
       xor      r8, qword ptr [rdx+0x02]
       or       rcx, r8
       sete     al
       movzx    rax, al
 
G_M000_IG07:                ;; offset=0x00E0
       test     eax, eax
       je       G_M000_IG65
       jmp      G_M000_IG69
 
G_M000_IG08:                ;; offset=0x00ED
       xor      eax, eax
       jmp      SHORT G_M000_IG07
       align    [0 bytes for IG11]
 
G_M000_IG09:                ;; offset=0x00F1
       cmp      byte  ptr [rbx+0x1C], 0
       jne      G_M000_IG69
       mov      r13d, dword ptr [r14+0x04]
       mov      r8d, r13d
       cmp      r8d, r12d
       ja       G_M000_IG83
       mov      rcx, rsi
       mov      edx, 10
       call     [System.PackedSpanHelpers:IndexOf[System.SpanHelpers+DontNegate`1[short],System.PackedSpanHelpers+NopTransform](byref,short,int):int]
       test     eax, eax
       jge      G_M000_IG65
       jmp      G_M000_IG69
 
G_M000_IG10:                ;; offset=0x0126
       cmp      dword ptr [rcx+0x0C], r14d
       je       G_M000_IG20
       xor      edx, edx
       test     r14d, r14d
       jle      SHORT G_M000_IG15
 
G_M000_IG11:                ;; offset=0x0137
       mov      r8, gword ptr [rbx+0x08]
       mov      eax, edx
       add      eax, dword ptr [rcx]
       cmp      eax, dword ptr [r8+0x08]
       jae      G_M000_IG84
       movzx    r8, word  ptr [r8+2*rax+0x0C]
       lea      eax, [r12+rdx]
       cmp      eax, edi
       jae      G_M000_IG84
       lea      eax, [r12+rdx]
       movzx    rax, word  ptr [rsi+2*rax]
       cmp      r8d, 63
       jne      SHORT G_M000_IG13
 
G_M000_IG12:                ;; offset=0x0169
       cmp      byte  ptr [rbx+0x1C], 0
       jne      SHORT G_M000_IG14
       cmp      eax, 10
       jne      SHORT G_M000_IG14
       jmp      G_M000_IG65
 
G_M000_IG13:                ;; offset=0x0179
       cmp      r8d, eax
       jne      G_M000_IG65
 
G_M000_IG14:                ;; offset=0x0182
       inc      edx
       cmp      edx, r14d
       jl       SHORT G_M000_IG11
 
G_M000_IG15:                ;; offset=0x0189
       mov      ebp, ebp
       jmp      SHORT G_M000_IG17
 
G_M000_IG16:                ;; offset=0x018D
       add      r13d, dword ptr [r14+0x04]
       mov      rbp, qword ptr [rsp+0x38]
       inc      ebp
       mov      r15d, dword ptr [rsp+0x5C]
 
G_M000_IG17:                ;; offset=0x019D
       mov      dword ptr [rsp+0x5C], r15d
       cmp      ebp, r15d
       jl       G_M000_IG28
 
G_M000_IG18:                ;; offset=0x01AB
       cmp      byte  ptr [rbx+0x1C], 0
       jne      G_M000_IG76
       mov      r8d, r12d
       sub      r8d, r13d
       mov      ecx, r13d
       mov      edx, r8d
       add      rcx, rdx
       mov      edx, edi
       cmp      rcx, rdx
       ja       G_M000_IG83
       mov      ecx, r13d
       lea      rcx, bword ptr [rsi+2*rcx]
       mov      edx, 10
       call     [System.PackedSpanHelpers:IndexOf[System.SpanHelpers+DontNegate`1[short],System.PackedSpanHelpers+NopTransform](byref,short,int):int]
       shr      eax, 31
       movzx    rax, al
 
G_M000_IG19:                ;; offset=0x01E7
       add      rsp, 104
       pop      rbx
       pop      rbp
       pop      rsi
       pop      rdi
       pop      r12
       pop      r13
       pop      r14
       pop      r15
       ret      
 
G_M000_IG20:                ;; offset=0x01F8
       mov      edx, r14d
       mov      r8d, r12d
       mov      eax, edx
       add      r8, rax
       mov      eax, edi
       cmp      r8, rax
       ja       G_M000_IG83
 
G_M000_IG21:                ;; offset=0x020E
       mov      r8d, r12d
       lea      rax, bword ptr [rsi+2*r8]
       mov      r8, gword ptr [rbx+0x08]
       mov      ecx, dword ptr [rcx]
       test     r8, r8
       je       G_M000_IG79
       mov      r10d, ecx
       mov      r9d, r14d
       add      r10, r9
       mov      r9d, dword ptr [r8+0x08]
       cmp      r10, r9
       ja       G_M000_IG82
       lea      rcx, bword ptr [r8+2*rcx+0x0C]
       mov      r10, rcx
 
G_M000_IG22:                ;; offset=0x0242
       cmp      edx, r14d
       jne      SHORT G_M000_IG27
 
G_M000_IG23:                ;; offset=0x0247
       mov      r8d, r14d
       add      r8, r8
       cmp      r8, 10
       je       SHORT G_M000_IG25
 
G_M000_IG24:                ;; offset=0x0253
       mov      rcx, rax
       mov      rdx, r10
       call     [System.SpanHelpers:SequenceEqual(byref,byref,nuint):bool]
       jmp      SHORT G_M000_IG26
 
G_M000_IG25:                ;; offset=0x0261
       mov      rcx, qword ptr [rax]
       mov      r8, qword ptr [rax+0x02]
       mov      rdx, qword ptr [r10]
       xor      rcx, rdx
       xor      r8, qword ptr [r10+0x02]
       or       rcx, r8
       sete     al
       movzx    rax, al
 
G_M000_IG26:                ;; offset=0x027B
       test     eax, eax
       je       G_M000_IG65
       jmp      G_M000_IG15
 
G_M000_IG27:                ;; offset=0x0288
       xor      eax, eax
       jmp      SHORT G_M000_IG26
       align    [0 bytes for IG36]
 
G_M000_IG28:                ;; offset=0x028C
       mov      rcx, gword ptr [rbx+0x10]
       cmp      ebp, dword ptr [rcx+0x08]
       jae      G_M000_IG84
       mov      qword ptr [rsp+0x38], rbp
       lea      rdx, [rbp+2*rbp]
       lea      r14, bword ptr [rcx+8*rdx+0x10]
       mov      eax, edi
       sub      eax, dword ptr [r14+0x10]
       sub      eax, dword ptr [r14+0x04]
       mov      dword ptr [rsp+0x54], eax
       mov      dword ptr [rsp+0x50], r13d
       jmp      SHORT G_M000_IG30
 
G_M000_IG29:                ;; offset=0x02BD
       lea      r13d, [r10+0x01]
       mov      dword ptr [rsp+0x50], r10d
 
G_M000_IG30:                ;; offset=0x02C6
       cmp      r13d, eax
       jg       G_M000_IG65
       mov      ecx, dword ptr [r14+0x08]
       test     ecx, ecx
       jl       G_M000_IG41
 
G_M000_IG31:                ;; offset=0x02DB
       lea      edx, [rcx+r13]
       mov      r8d, eax
       sub      r8d, r13d
       mov      r9d, dword ptr [r14+0x0C]
       mov      dword ptr [rsp+0x40], r9d
       add      r8d, r9d
       mov      r11d, dword ptr [r14+0x14]
       test     r11d, r11d
       jl       SHORT G_M000_IG32
       mov      r15d, eax
       sub      r15d, r13d
       cmp      r15d, 7
       mov      eax, dword ptr [rsp+0x54]
       jge      SHORT G_M000_IG35
 
G_M000_IG32:                ;; offset=0x030A
       mov      r11d, edx
       mov      r15d, r8d
       add      r11, r15
       mov      r15d, edi
       cmp      r11, r15
       ja       G_M000_IG83
 
G_M000_IG33:                ;; offset=0x031F
       mov      edx, edx
       lea      rdx, bword ptr [rsi+2*rdx]
       mov      bword ptr [rsp+0x28], rdx
       mov      r15, gword ptr [rbx+0x08]
       mov      r11d, dword ptr [r14]
       add      ecx, r11d
       test     r15, r15
       je       G_M000_IG81
       mov      r11d, ecx
       mov      edx, r9d
       add      rdx, r11
       mov      r11d, dword ptr [r15+0x08]
       cmp      rdx, r11
       ja       G_M000_IG82
       lea      rcx, bword ptr [r15+2*rcx+0x0C]
       mov      r11, rcx
       mov      rdx, bword ptr [rsp+0x28]
 
G_M000_IG34:                ;; offset=0x0360
       mov      rcx, rdx
       mov      edx, r8d
       mov      r8, r11
       call     [System.SpanHelpers:IndexOf(byref,int,byref,int):int]
       mov      r15d, eax
       mov      dword ptr [rsp+0x58], r12d
       mov      r12d, r15d
       jmp      G_M000_IG40
 
G_M000_IG35:                ;; offset=0x037F
       mov      edx, r13d
       mov      dword ptr [rsp+0x4C], -1
       mov      dword ptr [rsp+0x58], r12d
 
G_M000_IG36:                ;; offset=0x038F
       lea      r15d, [rdx+rcx]
       movsxd   r15, r15d
       vmovups  xmm0, xmmword ptr [rsi+2*r15]
       lea      r15d, [rdx+r11]
       movsxd   r15, r15d
       vmovups  xmm1, xmmword ptr [rsi+2*r15]
       mov      r15, gword ptr [rbx+0x08]
       mov      r12, r15
       mov      ebp, dword ptr [r14]
       lea      r9d, [rcx+rbp]
       mov      r8d, dword ptr [r12+0x08]
       cmp      r9d, r8d
       jae      G_M000_IG84
       vpbroadcastw xmm2, word  ptr [r12+2*r9+0x0C]
       vpcmpeqw xmm0, xmm2, xmm0
       mov      r9, r15
       lea      r12d, [r11+rbp]
       cmp      r12d, r8d
       jae      G_M000_IG84
       vpbroadcastw xmm2, word  ptr [r9+2*r12+0x0C]
       vpcmpeqw xmm1, xmm2, xmm1
       vpand    xmm0, xmm1, xmm0
       vpshufb  xmm0, xmm0, xmmword ptr [reloc @RWD00]
       vpmovmskb r9d, xmm0
       test     r9d, r9d
       jne      G_M000_IG50
       add      edx, 8
       mov      r9d, eax
       sub      r9d, edx
       cmp      r9d, 7
       jge      G_M000_IG36
 
G_M000_IG37:                ;; offset=0x0418
       mov      r12d, dword ptr [rsp+0x4C]
       test     r12d, r12d
       jge      G_M000_IG51
       lea      r9d, [rdx+rcx]
       mov      dword ptr [rsp+0x44], r9d
       mov      dword ptr [rsp+0x48], edx
       mov      r11d, eax
       sub      r11d, edx
       add      r11d, dword ptr [rsp+0x40]
       mov      qword ptr [rsp+0x60], r9
       mov      r9d, r11d
       add      r9, qword ptr [rsp+0x60]
       mov      qword ptr [rsp+0x60], r9
       mov      r9d, edi
       cmp      qword ptr [rsp+0x60], r9
       ja       G_M000_IG83
 
G_M000_IG38:                ;; offset=0x045E
       mov      r9d, dword ptr [rsp+0x44]
       lea      r9, bword ptr [rsi+2*r9]
       mov      bword ptr [rsp+0x30], r9
       add      ecx, ebp
       mov      ebp, dword ptr [rsp+0x40]
       test     r15, r15
       je       G_M000_IG80
       mov      r9d, ecx
       mov      qword ptr [rsp+0x60], r9
       mov      r9d, ebp
       add      r9, qword ptr [rsp+0x60]
       mov      r8d, r8d
       cmp      r9, r8
       ja       G_M000_IG82
       lea      r8, bword ptr [r15+2*rcx+0x0C]
       mov      r9d, ebp
 
G_M000_IG39:                ;; offset=0x049F
       mov      rcx, bword ptr [rsp+0x30]
       mov      edx, r11d
       call     [System.SpanHelpers:IndexOf(byref,int,byref,int):int]
       test     eax, eax
       jl       SHORT G_M000_IG40
       mov      r12d, dword ptr [rsp+0x48]
       sub      r12d, r13d
       add      r12d, eax
 
G_M000_IG40:                ;; offset=0x04BC
       test     r12d, r12d
       jl       G_M000_IG65
       add      r13d, r12d
       mov      eax, dword ptr [rsp+0x54]
       mov      r12d, dword ptr [rsp+0x58]
 
G_M000_IG41:                ;; offset=0x04D1
       cmp      byte  ptr [rbx+0x1C], 0
       jne      SHORT G_M000_IG43
 
G_M000_IG42:                ;; offset=0x04D7
       mov      r10d, dword ptr [rsp+0x50]
       mov      r8d, r13d
       sub      r8d, r10d
       mov      ecx, r10d
       mov      edx, r8d
       add      rcx, rdx
       mov      edx, edi
       cmp      rcx, rdx
       ja       G_M000_IG83
       mov      ecx, r10d
       lea      rcx, bword ptr [rsi+2*rcx]
       mov      edx, 10
       call     [System.PackedSpanHelpers:IndexOf[System.SpanHelpers+DontNegate`1[short],System.PackedSpanHelpers+NopTransform](byref,short,int):int]
       test     eax, eax
       mov      eax, dword ptr [rsp+0x54]
       jge      G_M000_IG65
 
G_M000_IG43:                ;; offset=0x0514
       mov      r10d, r13d
       mov      ecx, dword ptr [r14+0x0C]
       mov      r8d, dword ptr [r14+0x04]
       cmp      ecx, r8d
       je       G_M000_IG16
 
G_M000_IG44:                ;; offset=0x0528
       mov      ecx, dword ptr [r14+0x14]
       test     ecx, ecx
       jl       SHORT G_M000_IG46
 
G_M000_IG45:                ;; offset=0x0530
       lea      edx, [r10+rcx]
       cmp      edx, edi
       jae      G_M000_IG84
       movzx    rdx, word  ptr [rsi+2*rdx]
       mov      r9, gword ptr [rbx+0x08]
       add      ecx, dword ptr [r14]
       cmp      ecx, dword ptr [r9+0x08]
       jae      G_M000_IG84
       cmp      dx, word  ptr [r9+2*rcx+0x0C]
       jne      G_M000_IG64
 
G_M000_IG46:                ;; offset=0x055D
       mov      r9d, dword ptr [r14+0x08]
       test     r9d, r9d
       jl       SHORT G_M000_IG52
       xor      r11d, r11d
       cmp      dword ptr [r14+0x08], 0
       jle      G_M000_IG57
       align    [0 bytes for IG47]
 
G_M000_IG47:                ;; offset=0x0574
       mov      rcx, gword ptr [rbx+0x08]
       mov      edx, r11d
       add      edx, dword ptr [r14]
       cmp      edx, dword ptr [rcx+0x08]
       jae      G_M000_IG84
       movzx    rcx, word  ptr [rcx+2*rdx+0x0C]
       lea      edx, [r10+r11]
       cmp      edx, edi
       jae      G_M000_IG84
       movzx    rdx, word  ptr [rsi+2*rdx]
       cmp      ecx, 63
       je       G_M000_IG55
 
G_M000_IG48:                ;; offset=0x05A5
       cmp      ecx, edx
       je       G_M000_IG56
 
G_M000_IG49:                ;; offset=0x05AD
       xor      r11d, r11d
       jmp      G_M000_IG63
 
G_M000_IG50:                ;; offset=0x05B5
       mov      r11d, edx
       sub      r11d, r13d
       tzcnt    r9d, r9d
       add      r9d, r11d
       mov      r12d, r9d
       mov      dword ptr [rsp+0x4C], r12d
       jmp      G_M000_IG37
 
G_M000_IG51:                ;; offset=0x05D0
       jmp      G_M000_IG40
       align    [0 bytes for IG58]
 
G_M000_IG52:                ;; offset=0x05D5
       cmp      byte  ptr [rbx+0x1C], 0
       je       SHORT G_M000_IG53
       mov      r11d, 1
       mov      dword ptr [rsp+0x50], r10d
       jmp      SHORT G_M000_IG54
 
G_M000_IG53:                ;; offset=0x05E8
       mov      ecx, r10d
       mov      edx, r8d
       add      rcx, rdx
       mov      edx, edi
       cmp      rcx, rdx
       ja       G_M000_IG83
       mov      dword ptr [rsp+0x50], r10d
       mov      ecx, r10d
       lea      rcx, bword ptr [rsi+2*rcx]
       mov      edx, 10
       call     [System.PackedSpanHelpers:IndexOf[System.SpanHelpers+DontNegate`1[short],System.PackedSpanHelpers+NopTransform](byref,short,int):int]
       mov      r11d, eax
       shr      r11d, 31
 
G_M000_IG54:                ;; offset=0x061A
       mov      eax, dword ptr [rsp+0x54]
       mov      r10d, dword ptr [rsp+0x50]
       jmp      SHORT G_M000_IG63
 
G_M000_IG55:                ;; offset=0x0625
       cmp      byte  ptr [rbx+0x1C], 0
       jne      SHORT G_M000_IG56
       cmp      edx, 10
       je       G_M000_IG49
 
G_M000_IG56:                ;; offset=0x0634
       inc      r11d
       cmp      r11d, dword ptr [r14+0x08]
       jl       G_M000_IG47
 
G_M000_IG57:                ;; offset=0x0641
       add      r9d, dword ptr [r14+0x0C]
       cmp      r9d, r8d
       jge      SHORT G_M000_IG62
 
G_M000_IG58:                ;; offset=0x064A
       mov      rcx, gword ptr [rbx+0x08]
       mov      edx, r9d
       add      edx, dword ptr [r14]
       cmp      edx, dword ptr [rcx+0x08]
       jae      G_M000_IG84
       movzx    rcx, word  ptr [rcx+2*rdx+0x0C]
       lea      edx, [r10+r9]
       cmp      edx, edi
       jae      G_M000_IG84
       movzx    rdx, word  ptr [rsi+2*rdx]
       cmp      ecx, 63
       jne      SHORT G_M000_IG60
 
G_M000_IG59:                ;; offset=0x0677
       cmp      byte  ptr [rbx+0x1C], 0
       jne      SHORT G_M000_IG61
       cmp      edx, 10
       je       G_M000_IG49
       jmp      SHORT G_M000_IG61
 
G_M000_IG60:                ;; offset=0x0688
       cmp      ecx, edx
       jne      G_M000_IG49
 
G_M000_IG61:                ;; offset=0x0690
       inc      r9d
       cmp      r9d, r8d
       jl       SHORT G_M000_IG58
 
G_M000_IG62:                ;; offset=0x0698
       mov      r11d, 1
 
G_M000_IG63:                ;; offset=0x069E
       test     r11d, r11d
       jne      G_M000_IG16
 
G_M000_IG64:                ;; offset=0x06A7
       cmp      dword ptr [r14+0x08], 0
       jge      G_M000_IG29
 
G_M000_IG65:                ;; offset=0x06B2
       xor      eax, eax
 
G_M000_IG66:                ;; offset=0x06B4
       add      rsp, 104
       pop      rbx
       pop      rbp
       pop      rsi
       pop      rdi
       pop      r12
       pop      r13
       pop      r14
       pop      r15
       ret      
 
G_M000_IG67:                ;; offset=0x06C5
       cmp      edx, r8d
       jne      SHORT G_M000_IG65
 
G_M000_IG68:                ;; offset=0x06CA
       inc      ecx
       cmp      ecx, dword ptr [r14+0x04]
       jl       G_M000_IG04
 
G_M000_IG69:                ;; offset=0x06D6
       mov      r13d, dword ptr [r14+0x04]
 
G_M000_IG70:                ;; offset=0x06DA
       cmp      byte  ptr [rbx+0x1E], 0
       jne      G_M000_IG15
       dec      r15d
       mov      rcx, gword ptr [rbx+0x10]
       cmp      r15d, dword ptr [rcx+0x08]
       jae      G_M000_IG84
       mov      edx, r15d
       lea      rdx, [rdx+2*rdx]
       lea      rcx, bword ptr [rcx+8*rdx+0x10]
       mov      r14d, dword ptr [rcx+0x04]
       mov      r12d, edi
       sub      r12d, r14d
       cmp      r12d, r13d
       jl       SHORT G_M000_IG65
       cmp      dword ptr [rcx+0x08], 0
       jge      G_M000_IG10
       cmp      byte  ptr [rbx+0x1C], 0
       jne      G_M000_IG15
       mov      r8d, r14d
       mov      ecx, r12d
       mov      edx, r8d
       add      rcx, rdx
       mov      edx, edi
       cmp      rcx, rdx
       ja       G_M000_IG83
       mov      dword ptr [rsp+0x58], r12d
       mov      ecx, r12d
       lea      rcx, bword ptr [rsi+2*rcx]
       mov      edx, 10
       call     [System.PackedSpanHelpers:IndexOf[System.SpanHelpers+DontNegate`1[short],System.PackedSpanHelpers+NopTransform](byref,short,int):int]
       test     eax, eax
       mov      r12d, dword ptr [rsp+0x58]
       jge      G_M000_IG65
       jmp      G_M000_IG15
 
G_M000_IG71:                ;; offset=0x0764
       mov      ecx, r13d
       cmp      ecx, r12d
       ja       G_M000_IG83
 
G_M000_IG72:                ;; offset=0x0770
       mov      rdx, gword ptr [rbx+0x08]
       mov      r8d, dword ptr [r14]
       test     rdx, rdx
       je       SHORT G_M000_IG78
       mov      eax, r8d
       mov      r10d, r13d
       add      rax, r10
       mov      r10d, dword ptr [rdx+0x08]
       cmp      rax, r10
       ja       G_M000_IG82
       lea      rdx, bword ptr [rdx+2*r8+0x0C]
 
G_M000_IG73:                ;; offset=0x0797
       cmp      ecx, r13d
       jne      G_M000_IG08
 
G_M000_IG74:                ;; offset=0x07A0
       mov      r8d, r13d
       add      r8, r8
       cmp      r8, 10
       je       G_M000_IG06
 
G_M000_IG75:                ;; offset=0x07B0
       mov      rcx, rsi
       call     [System.SpanHelpers:SequenceEqual(byref,byref,nuint):bool]
       jmp      G_M000_IG07
 
G_M000_IG76:                ;; offset=0x07BE
       mov      eax, 1
 
G_M000_IG77:                ;; offset=0x07C3
       add      rsp, 104
       pop      rbx
       pop      rbp
       pop      rsi
       pop      rdi
       pop      r12
       pop      r13
       pop      r14
       pop      r15
       ret      
 
G_M000_IG78:                ;; offset=0x07D4
       or       r8d, r13d
       jne      SHORT G_M000_IG82
       xor      rdx, rdx
       xor      r13d, r13d
       jmp      SHORT G_M000_IG73
 
G_M000_IG79:                ;; offset=0x07E0
       or       ecx, r14d
       jne      SHORT G_M000_IG82
       xor      r10, r10
       xor      r14d, r14d
       jmp      G_M000_IG22
 
G_M000_IG80:                ;; offset=0x07F0
       or       ecx, ebp
       jne      SHORT G_M000_IG82
       xor      r8, r8
       xor      ecx, ecx
       xor      ebp, ebp
       mov      r9d, ebp
       jmp      G_M000_IG39
 
G_M000_IG81:                ;; offset=0x0803
       mov      rdx, bword ptr [rsp+0x28]
       or       ecx, r9d
       jne      SHORT G_M000_IG82
       xor      r11, r11
       xor      r9d, r9d
       jmp      G_M000_IG34
 
G_M000_IG82:                ;; offset=0x0818
       mov      ecx, 33
       call     [System.ThrowHelper:ThrowArgumentOutOfRangeException(int)]
       int3     
 
G_M000_IG83:                ;; offset=0x0824
       call     [System.ThrowHelper:ThrowArgumentOutOfRangeException()]
       int3     
 
G_M000_IG84:                ;; offset=0x082B
       call     CORINFO_HELP_RNGCHKFAIL
       int3     
 
RWD00  	dq	0F0D0B0907050301h, 8080808080808080h

; Total bytes of code 2097

