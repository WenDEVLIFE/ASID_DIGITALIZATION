using System;
using System.Runtime.InteropServices;

namespace ASID.Edge.Services
{
    public static class RawPrinterHelper
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDocName = "";

            [MarshalAs(UnmanagedType.LPStr)]
            public string pOutputFile = "";

            [MarshalAs(UnmanagedType.LPStr)]
            public string pDataType = "RAW";
        }

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA",
            SetLastError = true,
            CharSet = CharSet.Ansi)]
        static extern bool OpenPrinter(
            string szPrinter,
            out IntPtr hPrinter,
            IntPtr pd);

        [DllImport("winspool.Drv", SetLastError = true)]
        static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA",
            SetLastError = true,
            CharSet = CharSet.Ansi)]
        static extern bool StartDocPrinter(
            IntPtr hPrinter,
            int level,
            [In] DOCINFOA di);

        [DllImport("winspool.Drv", SetLastError = true)]
        static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", SetLastError = true)]
        static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", SetLastError = true)]
        static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", SetLastError = true)]
        static extern bool WritePrinter(
            IntPtr hPrinter,
            IntPtr pBytes,
            int dwCount,
            out int dwWritten);

        public static bool SendStringToPrinter(
            string printerName,
            string zpl)
        {
            IntPtr pPrinter = IntPtr.Zero;
            IntPtr pBytes = IntPtr.Zero;

            try
            {
                if (!OpenPrinter(printerName, out pPrinter, IntPtr.Zero))
                    throw new Exception($"OpenPrinter failed. Win32={Marshal.GetLastWin32Error()}");

                var doc = new DOCINFOA
                {
                    pDocName = "ASID Label",
                    pDataType = "RAW"
                };

                if (!StartDocPrinter(pPrinter, 1, doc))
                    throw new Exception($"StartDocPrinter failed. Win32={Marshal.GetLastWin32Error()}");

                if (!StartPagePrinter(pPrinter))
                    throw new Exception($"StartPagePrinter failed. Win32={Marshal.GetLastWin32Error()}");

                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(zpl);

                pBytes = Marshal.AllocHGlobal(bytes.Length);

                Marshal.Copy(bytes, 0, pBytes, bytes.Length);

                if (!WritePrinter(
                    pPrinter,
                    pBytes,
                    bytes.Length,
                    out int written))
                {
                    throw new Exception($"WritePrinter failed. Win32={Marshal.GetLastWin32Error()}");
                }

                EndPagePrinter(pPrinter);
                EndDocPrinter(pPrinter);

                return true;
            }
            finally
            {
                if (pBytes != IntPtr.Zero)
                    Marshal.FreeHGlobal(pBytes);

                if (pPrinter != IntPtr.Zero)
                    ClosePrinter(pPrinter);
            }
        }
    }
}