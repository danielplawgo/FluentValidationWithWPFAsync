using FluentValidation.Results;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FluentValidationWithWPF
{
    public class ViewModel : IViewModel
    {
        public ViewModel(IView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            View = view;
            View.ViewModel = this;
        }

        private IView _view;
        public IView View
        {
            get { return _view; }
            set
            {
                if (_view != value)
                {
                    _view = value;
                    OnPropertyChanged(() => this.View);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void OnPropertyChanged(string propertyName)
        {
            var propertyChanged = PropertyChanged;

            if (propertyChanged == null)
            {
                return;
            }

            propertyChanged(this, new PropertyChangedEventArgs(propertyName));

            if (propertyName != nameof(IsValid) && propertyName != nameof(IsValidating))
            {
                await Validate();

                if(ValidationResult.Errors.Any(e => e.PropertyName == propertyName) == false)
                {
                    OnErrorsChanged(propertyName);
                }
            }
        }
        
        protected void OnPropertyChanged<TProperty>(Expression<Func<TProperty>> propertyExpresion)
        {
            var property = propertyExpresion.Body as MemberExpression;
            if (property == null || !(property.Member is PropertyInfo) ||
                !IsPropertyOfThis(property))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    "Expression must be of the form 'this.PropertyName'. Invalid expression '{0}'.",
                    propertyExpresion), "propertyExpression");
            }

            this.OnPropertyChanged(property.Member.Name);
        }

        private bool IsPropertyOfThis(MemberExpression property)
        {
            var constant = RemoveCast(property.Expression) as ConstantExpression;
            return constant != null && constant.Value == this;
        }

        private Expression RemoveCast(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Convert ||
                expression.NodeType == ExpressionType.ConvertChecked)
                return ((UnaryExpression)expression).Operand;

            return expression;
        }

        public virtual Task<ValidationResult> SelfValidate() 
        {
            return Task.FromResult(new ValidationResult());
        }

        private bool _isValid;
        public bool IsValid
        {
            get { return _isValid; }
            set
            {
                if (_isValid != value)
                {
                    _isValid = value;
                    OnPropertyChanged(() => this.IsValid);
                }
            }
        }

        private bool _isValidating;
        public bool IsValidating
        {
            get { return _isValidating; }
            set
            {
                if (_isValidating != value)
                {
                    _isValidating = value;
                    OnPropertyChanged(() => this.IsValidating);
                }
            }
        }

        private ValidationResult ValidationResult { get; set; }

        protected async Task<bool> Validate()
        {
            IsValidating = true;

            ValidationResult = await SelfValidate();

            IsValidating = false;

            IsValid = ValidationResult.IsValid;

            if (IsValid == false)
            {
                foreach(var propertyName in ValidationResult.Errors.Select(e => e.PropertyName).Distinct())
                {
                    OnErrorsChanged(propertyName);
                }
            }               

            return IsValid;
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private void OnErrorsChanged(string propertyName)
        {
            if (ErrorsChanged == null)
            {
                return;
            }

            ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public bool HasErrors => IsValid == false;

        public IEnumerable GetErrors(string propertyName)
        {
            if (ValidationResult == null)
            {
                return new List<string>();
            }

            return ValidationResult.Errors.Where(e => e.PropertyName == propertyName).Select(e => e.ErrorMessage).ToList();
        }
    }
}
