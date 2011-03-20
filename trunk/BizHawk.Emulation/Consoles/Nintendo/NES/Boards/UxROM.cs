using System;
using System.IO;
using System.Diagnostics;

namespace BizHawk.Emulation.Consoles.Nintendo
{
	//generally mapper2

	//Mega Man
	//Castlevania
	//Contra
	//Duck Tales
	//Metal Gear

	//TODO - look for a mirror=H UNROM--maybe there are none? this may be fixed to the board type.

	public class UxROM : NES.NESBoardBase
	{
		//configuration
		int prg_mask;
		int cram_byte_mask;

		//state
		int prg;

		public override bool Configure(NES.EDetectionOrigin origin)
		{
			//configure
			switch (Cart.board_type)
			{
				case "NES-UNROM": //mega man
				case "HVC-UNROM": 
				case "KONAMI-UNROM":
					AssertPrg(128); AssertChr(0); AssertVram(8); AssertWram(0);
					break;

				case "NES-UOROM": //paperboy 2
				case "HVC-UOROM":
					AssertPrg(256); AssertChr(0); AssertVram(8); AssertWram(0);
					break;

				default:
					return false;
			}
			//these boards always have 8KB of CRAM
			cram_byte_mask = (Cart.vram_size*1024) - 1;
			prg_mask = (Cart.prg_size / 16) - 1;
			SetMirrorType(Cart.pad_h, Cart.pad_v);

			return true;
		}

		public override byte ReadPRG(int addr)
		{
			int block = addr >> 14;
			int page = block == 1 ? prg_mask : prg;
			int ofs = addr & 0x3FFF;
			return ROM[(page << 14) | ofs];
		}
		public override void WritePRG(int addr, byte value)
		{
			prg = value & prg_mask;
		}

		public override byte ReadPPU(int addr)
		{
			if (addr < 0x2000)
			{
				return VRAM[addr & cram_byte_mask];
			}
			else return base.ReadPPU(addr);
		}

		public override void WritePPU(int addr, byte value)
		{
			if (addr < 0x2000)
			{
				VRAM[addr & cram_byte_mask] = value;
			}
			else base.WritePPU(addr,value);
		}

		public override void SaveStateBinary(BinaryWriter bw)
		{
			base.SaveStateBinary(bw);
			bw.Write(prg);
		}

		public override void LoadStateBinary(BinaryReader br)
		{
			base.LoadStateBinary(br);
			prg = br.ReadInt32();
		}
	}
}