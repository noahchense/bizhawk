﻿using System;

namespace BizHawk.Emulation.CPUs.M68K
{
    partial class MC68000
    {
        void ADD0()
        {
            int Dreg = (op >> 9) & 7;
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;

            switch (size)
            {
                case 0: // byte
                {
                    sbyte value = ReadValueB(mode, reg);
                    int sResult = D[Dreg].s8 + value;
                    int uResult = D[Dreg].u8 + (byte)value;
                    X = C = (uResult & 0x100) != 0;
                    V = sResult > sbyte.MaxValue || sResult < sbyte.MinValue;
                    N = (sResult & 0x80) != 0;
                    Z = sResult == 0;
                    D[Dreg].s8 = (sbyte) sResult;
                    PendingCycles -= 4 + EACyclesBW[mode, reg];
                    return;
                }
                case 1: // word
                {
                    short value = ReadValueW(mode, reg);
                    int sResult = D[Dreg].s16 + value;
                    int uResult = D[Dreg].u16 + (ushort)value;
                    X = C = (uResult & 0x10000) != 0;
                    V = sResult > short.MaxValue || sResult < short.MinValue;
                    N = (sResult & 0x8000) != 0;
                    Z = sResult == 0;
                    D[Dreg].s16 = (short)sResult;
                    PendingCycles -= 4 + EACyclesBW[mode, reg];
                    return;
                }
                case 2: // long
                {
                    int value = ReadValueL(mode, reg);
                    long sResult = D[Dreg].s32 + value;
                    long uResult = D[Dreg].u32 + (uint)value;
                    X = C = (uResult & 0x100000000) != 0;
                    V = sResult > int.MaxValue || sResult < int.MinValue;
                    N = (sResult & 0x80000000) != 0;
                    Z = sResult == 0;
                    D[Dreg].s32 = (int)sResult;
                    PendingCycles -= 6 + EACyclesL[mode, reg];
                    return;
                }
            }
        }

        void ADD1()
        {
            int Dreg = (op >> 9) & 7;
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;

            switch (size)
            {
                case 0: // byte
                {
                    sbyte value = PeekValueB(mode, reg);
                    int sResult = value + D[Dreg].s8;
                    int uResult = (byte)value + D[Dreg].u8;
                    X = C = (uResult & 0x100) != 0;
                    V = sResult > sbyte.MaxValue || sResult < sbyte.MinValue;
                    N = (sResult & 0x80) != 0;
                    Z = sResult == 0;
                    WriteValueB(mode, reg, (sbyte)sResult);
                    PendingCycles -= 8 + EACyclesBW[mode, reg];
                    return;
                }
                case 1: // word
                {
                    short value = PeekValueW(mode, reg);
                    int sResult = value + D[Dreg].s16;
                    int uResult = (ushort)value + D[Dreg].u16;
                    Log.Note("CPU", "ADD1.W. value={0}, reg={1}, signed result = {2}, unsigned result = {3}", value, D[Dreg].s16, sResult, uResult);
                    X = C = (uResult & 0x10000) != 0;
                    V = sResult > short.MaxValue || sResult < short.MinValue;
                    N = (sResult & 0x8000) != 0;
                    Z = sResult == 0;
                    WriteValueW(mode, reg, (short)sResult);
                    PendingCycles -= 8 + EACyclesBW[mode, reg];
                    return;
                }
                case 2: // long
                {
                    int value = PeekValueL(mode, reg);
                    long sResult = value + D[Dreg].s32;
                    long uResult = (uint)value + D[Dreg].u32;
                    X = C = (uResult & 0x100000000) != 0;
                    V = sResult > int.MaxValue || sResult < int.MinValue;
                    N = (sResult & 0x80000000) != 0;
                    Z = sResult == 0;
                    WriteValueL(mode, reg, (int)sResult);
                    PendingCycles -= 12 + EACyclesL[mode, reg];
                    return;
                }
            }
        }

        void ADD_Disasm(DisassemblyInfo info)
        {
            int pc = info.PC + 2;

            int Dreg = (op >> 9) & 7;
            int dir = (op >> 8) & 1;
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;

            string op1 = "D" + Dreg;
            string op2;

            switch (size)
            {
                case 0:  info.Mnemonic = "add.b"; op2 = DisassembleValue(mode, reg, 1, ref pc); break;
                case 1:  info.Mnemonic = "add.w"; op2 = DisassembleValue(mode, reg, 2, ref pc); break;
                default: info.Mnemonic = "add.l"; op2 = DisassembleValue(mode, reg, 4, ref pc); break;
            }
            info.Args = dir == 0 ? (op2 + ", " + op1) : (op1 + ", " + op2);
            info.Length = pc - info.PC;
        }
        
        void ADDI()
        {
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg  = (op >> 0) & 7;
    Log.Error("CPU", "ADDI: note, flags probably calculated wrong. I suck.");
            switch (size)
            {
                case 0: // byte
                {
                    int immed = (sbyte) ReadWord(PC); PC += 2;
                    int result = PeekValueB(mode, reg) + immed;
                    X = C = (result & 0x100) != 0;
                    V = result > sbyte.MaxValue || result < sbyte.MinValue;
                    N = result < 0;
                    Z = result == 0;
                    WriteValueB(mode, reg, (sbyte)result);
                    if (mode == 0) PendingCycles -= 8;
                    else PendingCycles -= 12 + EACyclesBW[mode, reg];
                    return;
                }
                case 1: // word
                {
                    int immed = ReadWord(PC); PC += 2;
                    int result = PeekValueW(mode, reg) + immed;
                    X = C = (result & 0x10000) != 0;
                    V = result > short.MaxValue || result < short.MinValue;
                    N = result < 0;
                    Z = result == 0;
                    WriteValueW(mode, reg, (short)result);
                    if (mode == 0) PendingCycles -= 8;
                    else PendingCycles -= 12 + EACyclesBW[mode, reg];
                    return;
                }
                case 2: // long
                {
                    int immed = ReadLong(PC); PC += 2;
                    long result = PeekValueL(mode, reg) + immed;
                    X = C = (result & 0x100000000) != 0;
                    V = result > int.MaxValue || result < int.MinValue;
                    N = result < 0;
                    Z = result == 0;
                    WriteValueL(mode, reg, (int)result);
                    if (mode == 0) PendingCycles -= 16;
                    else PendingCycles -= 20 + EACyclesBW[mode, reg];
                    return;
                }
            }
        }

        void ADDI_Disasm(DisassemblyInfo info)
        {
            int pc = info.PC + 2;

            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg  = (op >> 0) & 3;

            switch (size)
            {
                case 0:
                    info.Mnemonic = "addi.b";
                    info.Args = DisassembleImmediate(1, ref pc) + ", " + DisassembleValue(mode, reg, 1, ref pc);
                    break;
                case 1:
                    info.Mnemonic = "addi.w";
                    info.Args = DisassembleImmediate(2, ref pc) + ", " + DisassembleValue(mode, reg, 2, ref pc);
                    break;
                case 2:
                    info.Mnemonic = "addi.l";
                    info.Args = DisassembleImmediate(4, ref pc) + ", " + DisassembleValue(mode, reg, 4, ref pc);
                    break;
            }
            info.Length = pc - info.PC;
        }

        void ADDQ()
        {
            int data = (op >> 9) & 7;
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg  = (op >> 0) & 7;
Log.Error("CPU", "ADDQ: note, flags probably calculated wrong. I suck.");
            data = data == 0 ? 8 : data; // range is 1-8; 0 represents 8

            switch (size)
            {
                case 0: // byte
                {
                    if (mode == 1) throw new Exception("ADDQ.B on address reg is invalid");
                    int result = PeekValueB(mode, reg) + data;
                    N = result < 0;
                    Z = result == 0;
                    V = result > sbyte.MaxValue || result < sbyte.MinValue;
                    C = X = (result & 0x100) != 0;
                    WriteValueB(mode, reg, (sbyte) result);
                    if (mode == 0) PendingCycles -= 4;
                    else PendingCycles -= 8 + EACyclesBW[mode, reg];
                    return;
                }
                case 1: // word
                {
                    int result;
                    if (mode == 1)
                    {
                        result = PeekValueL(mode, reg) + data;
                        WriteValueL(mode, reg, (short) result);
                    } else {
                        result = PeekValueW(mode, reg) + data;
                        N = result < 0;
                        Z = result == 0;
                        V = result > short.MaxValue || result < short.MinValue;
                        C = X = (result & 0x10000) != 0;
                        WriteValueW(mode, reg, (short)result);
                    }
                    if (mode <= 1) PendingCycles -= 4;
                    else PendingCycles -= 8 + EACyclesBW[mode, reg];
                    return;
                }
                default: // long
                {
                    long result = PeekValueL(mode, reg) + data;
                    if (mode != 1)
                    {
                        N = result < 0;
                        Z = result == 0;
                        V = result > int.MaxValue || result < int.MinValue;
                        C = X = (result & 0x100000000) != 0;
                    }
                    WriteValueL(mode, reg, (int)result);
                    if (mode <= 1) PendingCycles -= 8;
                    else PendingCycles -= 12 + EACyclesL[mode, reg];
                    return;
                }
            }
        }

        void ADDQ_Disasm(DisassemblyInfo info)
        {
            int pc = info.PC + 2;
            int data = (op >> 9) & 7;
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg  = (op >> 0) & 7;

            data = data == 0 ? 8 : data; // range is 1-8; 0 represents 8

            switch (size)
            {
                case 0: info.Mnemonic = "addq.b"; info.Args = data+", "+DisassembleValue(mode, reg, 1, ref pc); break;
                case 1: info.Mnemonic = "addq.w"; info.Args = data+", "+DisassembleValue(mode, reg, 2, ref pc); break;
                case 2: info.Mnemonic = "addq.l"; info.Args = data+", "+DisassembleValue(mode, reg, 4, ref pc); break;
            }
            info.Length = pc - info.PC;
        }

        void ADDA()
        {
            int aReg = (op >> 9) & 7;
            int size = (op >> 8) & 1;
            int mode = (op >> 3) & 7;
            int reg  = (op >> 0) & 7;

            if (size == 0) // word
            {
                int value = ReadValueW(mode, reg);
                A[aReg].s32 += value;
                PendingCycles -= 8 + EACyclesBW[mode, reg];
            } else { // long
                int value = ReadValueL(mode, reg);
                A[aReg].s32 -= value;
                PendingCycles += 6 + EACyclesL[mode, reg];
            }
        }

        void ADDA_Disasm(DisassemblyInfo info)
        {
            int pc = info.PC + 2;

            int aReg = (op >> 9) & 7;
            int size = (op >> 8) & 1;
            int mode = (op >> 3) & 7;
            int reg  = (op >> 0) & 7;

            info.Mnemonic = (size == 0) ? "adda.w" : "adda.l";
            info.Args = DisassembleValue(mode, reg, (size == 0) ? 2 : 4, ref pc) + ", A" + aReg;

            info.Length = pc - info.PC;
        }

        void SUB0()
        {
            int Dreg = (op >> 9) & 7;
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;

            switch (size)
            {
                case 0: // byte
                {
                    sbyte value = ReadValueB(mode, reg);
                    int sResult = D[Dreg].s8 - value;
                    int uResult = D[Dreg].u8 - (byte)value;
                    X = C = (uResult & 0x100) != 0;
                    V = sResult > sbyte.MaxValue || sResult < sbyte.MinValue;
                    N = (sResult & 0x80) != 0;
                    Z = sResult == 0;
                    D[Dreg].s8 = (sbyte) sResult;
                    PendingCycles -= 4 + EACyclesBW[mode, reg];
                    return;
                }
                case 1: // word
                {
                    short value = ReadValueW(mode, reg);
                    int sResult = D[Dreg].s16 - value;
                    int uResult = D[Dreg].u16 - (ushort)value;
                    X = C = (uResult & 0x10000) != 0;
                    V = sResult > short.MaxValue || sResult < short.MinValue;
                    N = (sResult & 0x8000) != 0;
                    Z = sResult == 0;
                    D[Dreg].s16 = (short) sResult;
                    PendingCycles -= 4 + EACyclesBW[mode, reg];
                    return;
                }
                case 2: // long
                {
                    int value = ReadValueL(mode, reg);
                    long sResult = D[Dreg].s32 - value;
                    long uResult = D[Dreg].u32 - (uint)value;
                    X = C = (uResult & 0x100000000) != 0;
                    V = sResult > int.MaxValue || sResult < int.MinValue;
                    N = (sResult & 0x80000000) != 0;
                    Z = sResult == 0;
                    D[Dreg].s32 = (int)sResult;
                    PendingCycles -= 6 + EACyclesL[mode, reg];
                    return;
                }
            }
        }

        void SUB1()
        {
            int Dreg = (op >> 9) & 7;
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;

            switch (size)
            {
                case 0: // byte
                {
                    sbyte value = PeekValueB(mode, reg);
                    int sResult = value - D[Dreg].s8;
                    int uResult = (byte)value - D[Dreg].u8;
                    X = C = (uResult & 0x100) != 0;
                    V = sResult > sbyte.MaxValue || sResult < sbyte.MinValue;
                    N = (sResult & 0x80) != 0;
                    Z = sResult == 0;
                    WriteValueB(mode, reg, (sbyte) sResult);
                    PendingCycles -= 8 + EACyclesBW[mode, reg];
                    return;
                }
                case 1: // word
                {
                    short value = PeekValueW(mode, reg);
                    int sResult = value - D[Dreg].s16;
                    int uResult = (ushort)value - D[Dreg].u16;
                    X = C = (uResult & 0x10000) != 0;
                    V = sResult > short.MaxValue || sResult < short.MinValue;
                    N = (sResult & 0x8000) != 0;
                    Z = sResult == 0;
                    WriteValueW(mode, reg, (short) sResult);
                    PendingCycles -= 8 + EACyclesBW[mode, reg];
                    return;
                }
                case 2: // long
                {
                    int value = PeekValueL(mode, reg);
                    long sResult = value - D[Dreg].s32;
                    long uResult = (uint)value - D[Dreg].u32;
                    X = C = (uResult & 0x100000000) != 0;
                    V = sResult > int.MaxValue || sResult < int.MinValue;
                    N = (sResult & 0x80000000) != 0;
                    Z = sResult == 0;
                    WriteValueL(mode, reg, (int) sResult);
                    PendingCycles -= 12 + EACyclesL[mode, reg];
                    return;
                }
            }
        }

        void SUB_Disasm(DisassemblyInfo info)
        {
            int pc = info.PC + 2;

            int Dreg = (op >> 9) & 7;
            int dir  = (op >> 8) & 1;
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg  = (op >> 0) & 7;

            string op1 = "D" + Dreg;
            string op2;

            switch (size)
            {
                case 0:  info.Mnemonic = "sub.b"; op2 = DisassembleValue(mode, reg, 1, ref pc); break;
                case 1:  info.Mnemonic = "sub.w"; op2 = DisassembleValue(mode, reg, 2, ref pc); break;
                default: info.Mnemonic = "sub.l"; op2 = DisassembleValue(mode, reg, 4, ref pc); break;
            }
            info.Args = dir == 0 ? (op2 + ", " + op1) : (op1 + ", " + op2);
            info.Length = pc - info.PC;
        }

        void SUBI()
        {
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg  = (op >> 0) & 7;
Log.Error("CPU", "SUBI, bad flag calculations, I = lame");
            switch (size)
            {
                case 0: // byte
                {
                    int immed = (sbyte) ReadWord(PC); PC += 2;
                    int result = PeekValueB(mode, reg) - immed;
                    X = C = (result & 0x100) != 0;
                    V = result > sbyte.MaxValue || result < sbyte.MinValue;
                    N = result < 0;
                    Z = result == 0;
                    WriteValueB(mode, reg, (sbyte)result);
                    if (mode == 0) PendingCycles -= 8;
                    else PendingCycles -= 12 + EACyclesBW[mode, reg];
                    return;
                }
                case 1: // word
                {
                    int immed = ReadWord(PC); PC += 2;
                    int result = PeekValueW(mode, reg) - immed;
                    X = C = (result & 0x10000) != 0;
                    V = result > short.MaxValue || result < short.MinValue;
                    N = result < 0;
                    Z = result == 0;
                    WriteValueW(mode, reg, (short)result);
                    if (mode == 0) PendingCycles -= 8;
                    else PendingCycles -= 12 + EACyclesBW[mode, reg];
                    return;
                }
                case 2: // long
                {
                    int immed = ReadLong(PC); PC += 2;
                    long result = PeekValueL(mode, reg) - immed;
                    X = C = (result & 0x100000000) != 0;
                    V = result > int.MaxValue || result < int.MinValue;
                    N = result < 0;
                    Z = result == 0;
                    WriteValueL(mode, reg, (int)result);
                    if (mode == 0) PendingCycles -= 16;
                    else PendingCycles -= 20 + EACyclesBW[mode, reg];
                    return;
                }
            }
        }

        void SUBI_Disasm(DisassemblyInfo info)
        {
            int pc = info.PC + 2;

            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg  = (op >> 0) & 3;

            switch (size)
            {
                case 0:
                    info.Mnemonic = "subi.b";
                    info.Args = DisassembleImmediate(1, ref pc) + ", " + DisassembleValue(mode, reg, 1, ref pc);
                    break;
                case 1:
                    info.Mnemonic = "subi.w";
                    info.Args = DisassembleImmediate(2, ref pc) + ", " + DisassembleValue(mode, reg, 2, ref pc);
                    break;
                case 2:
                    info.Mnemonic = "subi.l";
                    info.Args = DisassembleImmediate(4, ref pc) + ", " + DisassembleValue(mode, reg, 4, ref pc);
                    break;
            }
            info.Length = pc - info.PC;
        }

        void SUBQ()
        {
            int data = (op >> 9) & 7;
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;
Log.Error("CPU", "SUBQ, bad flag calculations, I = lame");
            data = data == 0 ? 8 : data; // range is 1-8; 0 represents 8

            switch (size)
            {
                case 0: // byte
                {
                    if (mode == 1) throw new Exception("SUBQ.B on address reg is invalid");
                    int result = PeekValueB(mode, reg) - data;
                    N = result < 0;
                    Z = result == 0;
                    V = result > sbyte.MaxValue || result < sbyte.MinValue;
                    C = X = (result & 0x100) != 0;
                    WriteValueB(mode, reg, (sbyte) result);
                    if (mode == 0) PendingCycles -= 4;
                    else PendingCycles -= 8 + EACyclesBW[mode, reg];
                    return;
                }
                case 1: // word
                {
                    int result = PeekValueW(mode, reg) - data;
                    if (mode != 1)
                    {
                        N = result < 0;
                        Z = result == 0;
                        V = result > short.MaxValue || result < short.MinValue;
                        C = X = (result & 0x10000) != 0;
                    }
                    WriteValueW(mode, reg, (short)result);
                    if (mode <= 1) PendingCycles -= 4;
                    else PendingCycles -= 8 + EACyclesBW[mode, reg];
                    return;
                }
                default: // long
                {
                    long result = PeekValueL(mode, reg) - data;
                    if (mode != 1)
                    {
                        N = result < 0;
                        Z = result == 0;
                        V = result > int.MaxValue || result < int.MinValue;
                        C = X = (result & 0x100000000) != 0;
                    }
                    WriteValueL(mode, reg, (int)result);
                    if (mode <= 1) PendingCycles -= 8;
                    else PendingCycles -= 12 + EACyclesL[mode, reg];
                    return;
                }
            }
        }

        void SUBQ_Disasm(DisassemblyInfo info)
        {
            int pc = info.PC + 2;
            int data = (op >> 9) & 7;
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;

            data = data == 0 ? 8 : data; // range is 1-8; 0 represents 8

            switch (size)
            {
                case 0: info.Mnemonic = "subq.b"; info.Args = data+", "+DisassembleValue(mode, reg, 1, ref pc); break;
                case 1: info.Mnemonic = "subq.w"; info.Args = data+", "+DisassembleValue(mode, reg, 2, ref pc); break;
                case 2: info.Mnemonic = "subq.l"; info.Args = data+", "+DisassembleValue(mode, reg, 4, ref pc); break;
            }
            info.Length = pc - info.PC;   
        }

        void SUBA()
        {
            int aReg = (op >> 9) & 7;
            int size = (op >> 8) & 1;
            int mode = (op >> 3) & 7;
            int reg  = (op >> 0) & 7;

            if (size == 0) // word
            {
                int value = ReadValueW(mode, reg);
                A[aReg].s32 -= value;
                PendingCycles -= 8 + EACyclesBW[mode, reg];
            } else { // long
                int value = ReadValueL(mode, reg);
                A[aReg].s32 -= value;
                PendingCycles -= 6 + EACyclesL[mode, reg];
            }
        }

        void SUBA_Disasm(DisassemblyInfo info)
        {
            int pc = info.PC + 2;

            int aReg = (op >> 9) & 7;
            int size = (op >> 8) & 1;
            int mode = (op >> 3) & 7;
            int reg  = (op >> 0) & 7;

            info.Mnemonic = (size == 0) ? "suba.w" : "suba.l";
            info.Args = DisassembleValue(mode, reg, (size == 0) ? 2 : 4, ref pc) + ", A"+aReg;

            info.Length = pc - info.PC;
        }

        void CMP()
        {
            int dReg = (op >> 9) & 7;
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg  = (op >> 0) & 7;
Log.Error("CPU", "CMP, very possibly bad flag calculations, I = lame");
            switch (size)
            {
                case 0: // byte
                {
                    int result = ReadValueB(mode, reg) - D[dReg].s8;
                    N = result < 0;
                    Z = result == 0;
                    V = result > sbyte.MaxValue || result < sbyte.MinValue;
                    C = (result & 0x100) != 0;
                    if (mode == 0) PendingCycles -= 8;
                    PendingCycles -= 4 + EACyclesBW[mode, reg];
                    return;
                }
                case 1: // word
                {
                    int result = ReadValueW(mode, reg) - D[dReg].s16;
                    N = result < 0;
                    Z = result == 0;
                    V = result > short.MaxValue || result < short.MinValue;
                    C = (result & 0x10000) != 0;
                    if (mode == 0) PendingCycles -= 8;
                    PendingCycles -= 4 + EACyclesBW[mode, reg];
                    return;
                }
                case 2: // long
                {
                    long result = ReadValueL(mode, reg) - D[dReg].s32;
                    N = result < 0;
                    Z = result == 0;
                    V = result > int.MaxValue || result < int.MinValue;
                    C = (result & 0x100000000) != 0;
                    PendingCycles -= 6 + EACyclesL[mode, reg];
                    return;
                }
            }
        }

        void CMP_Disasm(DisassemblyInfo info)
        {
            int pc = info.PC + 2;

            int dReg = (op >> 9) & 7;
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg  = (op >> 0) & 7;

            switch (size)
            {
                case 0:
                    info.Mnemonic = "cmp.b";
                    info.Args = DisassembleValue(mode, reg, 1, ref pc) + ", D" + dReg;
                    break;
                case 1:
                    info.Mnemonic = "cmp.w";
                    info.Args = DisassembleValue(mode, reg, 2, ref pc) + ", D" + dReg;
                    break;
                case 2:
                    info.Mnemonic = "cmp.l";
                    info.Args = DisassembleValue(mode, reg, 4, ref pc) + ", D" + dReg;
                    break;
            }
            info.Length = pc - info.PC;
        }

        void CMPA()
        {
            int aReg = (op >> 9) & 7;
            int size = (op >> 8) & 1;
            int mode = (op >> 3) & 7;
            int reg  = (op >> 0) & 7;

            switch (size)
            {
                case 0: // word
                {
                    long result = A[aReg].s32 - ReadValueW(mode, reg);
                    N = result < 0;
                    Z = result == 0;
                    V = result > int.MaxValue || result < int.MinValue;
                    C = (result & 0x100000000) != 0;
                    PendingCycles -= 6 + EACyclesBW[mode, reg];
                    return;
                }
                case 1: // long
                {
                    long result = A[aReg].s32 - ReadValueL(mode, reg);
                    N = result < 0;
                    Z = result == 0;
                    V = result > int.MaxValue || result < int.MinValue;
                    C = (result & 0x100000000) != 0;
                    PendingCycles -= 6 + EACyclesL[mode, reg];
                    return;
                }
            }
        }

        void CMPA_Disasm(DisassemblyInfo info)
        {
            int pc = info.PC + 2;

            int aReg = (op >> 9) & 7;
            int size = (op >> 8) & 1;
            int mode = (op >> 3) & 7;
            int reg  = (op >> 0) & 7;

            switch (size)
            {
                case 0:
                    info.Mnemonic = "cmpa.w";
                    info.Args = DisassembleValue(mode, reg, 2, ref pc) + ", A" + aReg;
                    break;
                case 1:
                    info.Mnemonic = "cmpa.l";
                    info.Args = DisassembleValue(mode, reg, 4, ref pc) + ", A" + aReg;
                    break;
            }
            info.Length = pc - info.PC;
        }


        void CMPI()
        {
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;

            switch (size)
            {
                case 0: // byte
                {
                    int immed = (sbyte) ReadWord(PC); PC += 2;
                    int result = ReadValueB(mode, reg) - immed;
                    N = result < 0;
                    Z = result == 0;
                    V = result > sbyte.MaxValue || result < sbyte.MinValue;
                    C = (result & 0x100) != 0;
                    if (mode == 0) PendingCycles -= 8;
                    else PendingCycles -= 8 + EACyclesBW[mode, reg];
                    return;
                }
                case 1: // word
                {
                    int immed = ReadWord(PC); PC += 2;
                    int result = ReadValueW(mode, reg) - immed;
                    N = result < 0;
                    Z = result == 0;
                    V = result > short.MaxValue || result < short.MinValue;
                    C = (result & 0x10000) != 0;
                    if (mode == 0) PendingCycles -= 8;
                    else PendingCycles -= 8 + EACyclesBW[mode, reg];
                    return;
                }
                case 2: // long
                {
                    int immed = ReadLong(PC); PC += 4;
                    long result = ReadValueL(mode, reg) - immed;
                    N = result < 0;
                    Z = result == 0;
                    V = result > int.MaxValue || result < int.MinValue;
                    C = (result & 0x100000000) != 0;
                    if (mode == 0) PendingCycles -= 14;
                    else PendingCycles -= 12 + EACyclesL[mode, reg];
                    return;
                }
            }
        }

        void CMPI_Disasm(DisassemblyInfo info)
        {
            int pc = info.PC + 2;
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;
            int immediate;

            switch (size)
            {
                case 0:
                    immediate = (byte)ReadWord(pc); pc += 2;
                    info.Mnemonic = "cmpi.b"; 
                    info.Args = String.Format("${0:X}, {1}", immediate, DisassembleValue(mode, reg, 1, ref pc)); 
                    break;
                case 1:
                    immediate = ReadWord(pc); pc += 2;
                    info.Mnemonic = "cmpi.w";
                    info.Args = String.Format("${0:X}, {1}", immediate, DisassembleValue(mode, reg, 2, ref pc));
                    break;
                case 2:
                    immediate = ReadLong(pc); pc += 4;
                    info.Mnemonic = "cmpi.l";
                    info.Args = String.Format("${0:X}, {1}", immediate, DisassembleValue(mode, reg, 4, ref pc));
                    break;
            }
            info.Length = pc - info.PC;
        }
    }
}
