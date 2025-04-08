using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Exam.ViewModels;

namespace Exam.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                InputMethod.SetPreferredImeConversionMode(textBox, ImeConversionModeValues.Native);
                InputMethod.SetPreferredImeSentenceMode(textBox, ImeSentenceModeValues.None);
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                InputMethod.SetPreferredImeConversionMode(textBox, ImeConversionModeValues.Native);
                InputMethod.SetPreferredImeSentenceMode(textBox, ImeSentenceModeValues.None);
            }
        }
    }
} 