using System.Windows;
using OfficeOpenXml;

namespace Exam
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }
    }
} 