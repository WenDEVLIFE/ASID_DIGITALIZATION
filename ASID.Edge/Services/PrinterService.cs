using System.Windows;

namespace ASID.Edge.Services
{
    public class PrinterService
    {
        private const string PrinterName = "ZDesigner GK420t";

        public bool Print(string zpl)
        {
            try
            {
                return RawPrinterHelper.SendStringToPrinter(
                    PrinterName,
                    zpl);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.ToString(),
                    "Printer Error");

                return false;
            }
        }
    }
}