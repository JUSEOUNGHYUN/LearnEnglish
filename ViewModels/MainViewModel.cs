using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Exam.Services;
using Exam.Views;

namespace Exam.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ExcelService _excelService;
        private readonly Random _random;
        private ObservableCollection<ExcelService.Question> _questions;
        private string _selectedSheet;
        private int _currentQuestionIndex;
        private string _userAnswer;
        private int _score;
        private string _message;
        private bool _isMessageVisible;
        private string _correctAnswerText;

        public event PropertyChangedEventHandler PropertyChanged;

        public string QuestionProgress => Questions != null && Questions.Count > 0 
            ? $"문제 {_currentQuestionIndex + 1}/{Questions.Count}"
            : "";

        public MainViewModel()
        {
            _excelService = new ExcelService();
            _random = new Random();
            LoadQuestionsCommand = new RelayCommand(LoadQuestions);
            NextQuestionCommand = new RelayCommand(NextQuestion, CanNextQuestion);
            CheckAnswerCommand = new RelayCommand(CheckAnswer);
            CloseCommand = new RelayCommand(() => Application.Current.MainWindow.Close());
            
            SheetNames = new[] { "Idiom", "Word", "Sentence" };
            SelectedSheet = SheetNames[0];
        }

        public string[] SheetNames { get; }
        
        public string SelectedSheet
        {
            get => _selectedSheet;
            set
            {
                _selectedSheet = value;
                OnPropertyChanged(nameof(SelectedSheet));
            }
        }

        public ObservableCollection<ExcelService.Question> Questions
        {
            get => _questions;
            set
            {
                _questions = value;
                OnPropertyChanged(nameof(Questions));
            }
        }

        public ExcelService.Question CurrentQuestion => 
            Questions != null && Questions.Count > 0 ? Questions[_currentQuestionIndex] : null;

        public string UserAnswer
        {
            get => _userAnswer;
            set
            {
                // 빈 문자열이거나 한글, 자음, 모음, 물음표, 느낌표가 포함된 경우에만 값을 설정
                if (string.IsNullOrEmpty(value) || Regex.IsMatch(value, @"^[ㄱ-ㅎㅏ-ㅣ가-힣\s!?~]*$"))
                {
                    _userAnswer = value;
                    OnPropertyChanged(nameof(UserAnswer));
                }
            }
        }

        public int Score
        {
            get => _score;
            set
            {
                _score = value;
                OnPropertyChanged(nameof(Score));
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        public bool IsMessageVisible
        {
            get => _isMessageVisible;
            set
            {
                _isMessageVisible = value;
                OnPropertyChanged(nameof(IsMessageVisible));
            }
        }

        public string CorrectAnswerText
        {
            get => _correctAnswerText;
            set
            {
                _correctAnswerText = value;
                OnPropertyChanged(nameof(CorrectAnswerText));
            }
        }

        public ICommand LoadQuestionsCommand { get; }
        public ICommand NextQuestionCommand { get; }
        public ICommand CheckAnswerCommand { get; }
        public ICommand CloseCommand { get; }

        private string NormalizeAnswer(string answer)
        {
            if (string.IsNullOrEmpty(answer)) return "";
            
            // 괄호와 그 안의 내용 제거 (예: "할지도 모른다 (약한 추측)" -> "할지도 모른다")
            answer = Regex.Replace(answer, @"\s*\([^)]*\)", "");
            
            // 남은 모든 공백 제거
            return Regex.Replace(answer, @"\s+", "");
        }

        private void LoadQuestions()
        {
            // 문제를 로드하고 랜덤으로 섞기
            var questionsList = _excelService.LoadQuestions(SelectedSheet);
            var shuffledQuestions = questionsList.OrderBy(x => _random.Next()).ToList();
            Questions = new ObservableCollection<ExcelService.Question>(shuffledQuestions);
            
            _currentQuestionIndex = 0;
            Score = 0;
            UserAnswer = "";
            Message = "";
            CorrectAnswerText = "";
            IsMessageVisible = false;
            OnPropertyChanged(nameof(CurrentQuestion));
            OnPropertyChanged(nameof(QuestionProgress));
        }

        private void NextQuestion()
        {
            if (_currentQuestionIndex < Questions.Count - 1)
            {
                _currentQuestionIndex++;
                UserAnswer = "";
                Message = "";
                CorrectAnswerText = "";
                IsMessageVisible = false;
                OnPropertyChanged(nameof(CurrentQuestion));
                OnPropertyChanged(nameof(QuestionProgress));
            }
        }

        private bool CanNextQuestion()
        {
            return Questions != null && _currentQuestionIndex < Questions.Count - 1;
        }

        private async void CheckAnswer()
        {
            if (CurrentQuestion != null)
            {
                string userAnswer = NormalizeAnswer(UserAnswer);
                
                bool isCorrect = CurrentQuestion.CorrectAnswers.Any(correctAnswer => 
                {
                    string normalizedCorrectAnswer = NormalizeAnswer(correctAnswer);
                    return userAnswer.Equals(normalizedCorrectAnswer, StringComparison.OrdinalIgnoreCase);
                });

                if (isCorrect)
                {
                    Score++;
                    Message = "정답입니다!";
                    IsMessageVisible = true;
                    await Task.Delay(1000); // 1초 대기
                    NextQuestion();
                }
                else
                {
                    // 정답 텍스트 생성 (원본 정답 표시)
                    string correctAnswers = string.Join(" 또는 ", CurrentQuestion.CorrectAnswers);
                    CorrectAnswerText = $"정답: {correctAnswers}";
                    Message = "틀렸습니다.";
                    IsMessageVisible = true;
                    UserAnswer = ""; // 오답일 때 입력창 비우기

                    var dialog = new AnswerDialog($"{CorrectAnswerText}\n\n다음 문제로 넘어가시겠습니까?");
                    dialog.Owner = Application.Current.MainWindow;
                    dialog.ShowDialog();

                    if (dialog.DialogResult)
                    {
                        NextQuestion();
                    }
                    else
                    {
                        Message = "";
                        CorrectAnswerText = "";
                        IsMessageVisible = false;
                    }
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object parameter) => _execute();
    }
} 