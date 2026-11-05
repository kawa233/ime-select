using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace ImeSelectApp
{
    internal static class ImeSelect
    {
        private static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                return ImeStatus.Execute();
            }

            if (args.Length != 1 || (args[0] != "0" && args[0] != "1"))
            {
                Console.Error.WriteLine("Usage: ime-select.exe [0|1] (no argument: status, 0: OFF, 1: ON)");
                return 1;
            }

            if (args[0] == "0")
            {
                ImeOff.Execute();
            }
            else
            {
                ImeOn.Execute();
            }

            return 0;
        }
    }

    internal static class ImeStatus
    {
        private const uint WmImeControl = 0x0283;
        private const uint ImcGetOpenStatus = 0x0005;
        private const uint AbortIfHung = 0x0002;

        [StructLayout(LayoutKind.Sequential)]
        private struct GuiThreadInfo
        {
            public uint Size;
            public uint Flags;
            public IntPtr Active;
            public IntPtr Focus;
            public IntPtr Capture;
            public IntPtr MenuOwner;
            public IntPtr MoveSize;
            public IntPtr Caret;
            public int CaretLeft;
            public int CaretTop;
            public int CaretRight;
            public int CaretBottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetGUIThreadInfo(uint threadId, ref GuiThreadInfo info);

        [DllImport("imm32.dll")]
        private static extern IntPtr ImmGetDefaultIMEWnd(IntPtr window);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr SendMessageTimeoutW(
            IntPtr window, uint message, UIntPtr command, IntPtr value,
            uint flags, uint timeout, out UIntPtr result);

        private static UIntPtr SendImeCommand(IntPtr window, uint command, int value)
        {
            UIntPtr result;
            if (SendMessageTimeoutW(window, WmImeControl, new UIntPtr(command),
                    new IntPtr(value), AbortIfHung, 200, out result) == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(),
                    "Could not send an IME command to the focused window.");
            }
            return result;
        }

        internal static int Execute()
        {
            try
            {
                var info = new GuiThreadInfo();
                info.Size = (uint)Marshal.SizeOf(typeof(GuiThreadInfo));
                if (!GetGUIThreadInfo(0, ref info) || info.Focus == IntPtr.Zero)
                    throw new InvalidOperationException("No focused window was found.");

                IntPtr imeWindow = ImmGetDefaultIMEWnd(info.Focus);
                if (imeWindow == IntPtr.Zero)
                    throw new InvalidOperationException("No IME window was found.");

                UIntPtr status = SendImeCommand(imeWindow, ImcGetOpenStatus, 0);
                Console.WriteLine(status == UIntPtr.Zero ? "0" : "1");
                return 0;
            }
            catch (Exception error)
            {
                Console.Error.WriteLine(error.Message);
                return 1;
            }
        }
    }

    internal static class ImeOff
    {
        private const byte VkImeOff = 0x1A;
        private const uint KeyUp = 0x0002;

        [DllImport("user32.dll")]
        private static extern void keybd_event(
            byte virtualKey, byte scanCode, uint flags, UIntPtr extraInfo);

        internal static void Execute()
        {
            keybd_event(VkImeOff, 0, 0, UIntPtr.Zero);
            keybd_event(VkImeOff, 0, KeyUp, UIntPtr.Zero);
        }
    }

    internal static class ImeOn
    {
        private const byte VkImeOn = 0x16;
        private const uint KeyUp = 0x0002;

        [DllImport("user32.dll")]
        private static extern void keybd_event(
            byte virtualKey, byte scanCode, uint flags, UIntPtr extraInfo);

        internal static void Execute()
        {
            keybd_event(VkImeOn, 0, 0, UIntPtr.Zero);
            keybd_event(VkImeOn, 0, KeyUp, UIntPtr.Zero);
        }
    }
}
