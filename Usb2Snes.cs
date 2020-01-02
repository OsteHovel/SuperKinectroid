using System;
using WebSocketSharp;

namespace Ostsoft.Games.SuperKinectroid
{
    public class Usb2Snes
    {
        private WebSocket ws;

        public Usb2Snes()
        {
            Console.WriteLine("Connecting to Usb2Snes");
            ws = new WebSocket("ws://localhost:8080");
            ws.Connect();
            Console.WriteLine("Connected to Usb2Snes");


            Console.WriteLine("Attaching to SNES");

            var port = "SD2SNES COM1";

            ws.Send($"{{\"Opcode\":\"Attach\",\"Space\":\"SNES\",\"Operands\":[\"{port}\"]}}");
            Console.WriteLine("Attached to SNES");

            Console.WriteLine("Patching controller input");
            /*
                val hijackCode = JSR(joypadCodeLocation) + NOP() + NOP()
                val joypadCode =
                LDAValue(0x0000) +
                        ORA(0x4218) + // Load Auto-Joypad 1 into A
                        ORA(controller1InputLocation) + // OR with the controller input from PC
                        STA(0x008B) + // Store to normal ram location for joypad input
                        RTS()
             */

            var controller1InputLocation = 0x701CFA;
            var joypadCodeLocation = 0x80D000;
            var hijackCodeLocation = 0x809465;

            Write(snesToSd2Snes(controller1InputLocation), new byte[] {0x00, 0x00});
            Write(snesToSd2Snes(joypadCodeLocation),
                new byte[] {0xA9, 0x0, 0x0, 0xD, 0x18, 0x42, 0xF, 0xFA, 0x1C, 0x70, 0x8D, 0x8B, 0x0, 0x60});
            Write(snesToSd2Snes(hijackCodeLocation), new byte[] {0x20, 0x0, 0xD0, 0xEA, 0xEA});
        }

        public void Write(int address, byte[] bytes)
        {
            ws.Send("{\"Opcode\":\"PutAddress\",\"Space\":\"SNES\",\"Operands\":[\"" + address.ToString("X") +
                    "\",\"" + bytes.Length + "\"]}");
            ws.Send(bytes);
        }

        public int snesToSd2Snes(int offset)
        {
            if (0x7E0000 <= offset && offset <= 0x7FFFFF)
            {
                return offset - 0x7E0000 + 0xF50000;
            }
            else if (0x700000 <= offset && offset <= 0x7DFFFF)
            {
                return offset - 0x700000 + 0xE00000;
            }
            else
            {
                return snesToPc(offset);
            }
        }

        public int snesToPc(int snesAddress)
        {
            var b1 = snesAddress & 0xFF;
            var b2 = (snesAddress >> 8) & 0xFF;
            var b3 = snesAddress >> 16;

            if (b3 >= 0x80)
            {
                b3 = (b3 - 0x80);
            }

            var tmp = (b2 << 8) | b1;
            if (tmp < 0x8000)
            {
                tmp += 0x8000;
            }

            tmp = tmp + (b3 * 0x8000) - 0x8000;

            return tmp;
        }
    }
}