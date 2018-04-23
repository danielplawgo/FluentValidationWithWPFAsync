using FluentValidation.Results;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentValidationWithWPF
{
    public class MainWindowViewModel : ViewModel, IMainWindowViewModel
    {
        protected MainWindowViewModelValidator Validator { get; set; }

        public MainWindowViewModel(IMainWindow view)
            : base(view)
        {
            Validator = new MainWindowViewModelValidator();

            Validate();
        }

        private string _email;
        public string Email
        {
            get { return _email; }
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged(() => this.Email);
                    SaveCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private DelegateCommand _saveCommand;
        public DelegateCommand SaveCommand
        {
            get
            {
                if(_saveCommand == null)
                {
                    _saveCommand = new DelegateCommand(Save, () => IsValid);
                }
                return _saveCommand;
            }
        }

        public void Save()
        {

        }

        public override Task<ValidationResult> SelfValidate()
        {
            return Validator.ValidateAsync(this);
        }
    }

    public interface IMainWindowViewModel : IViewModel
    {

    }
}
