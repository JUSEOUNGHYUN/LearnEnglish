using System.Windows;
using System.Windows.Input;
using Exam.ViewModels;

namespace Exam.Views
{
    public partial class AnswerDialog : Window
    {
        public bool DialogResult { get; private set; }

        public AnswerDialog(string message)
        {
            InitializeComponent();
            MessageText.Text = message;
            
            // A 키 입력을 위한 커맨드 설정
            var noCommand = new RelayCommand(() => 
            {
                DialogResult = false;
                Close();
            });
            
            DataContext = new { NoCommand = noCommand };
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 