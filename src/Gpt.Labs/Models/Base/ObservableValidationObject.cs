using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Gpt.Labs.Models.Base
{
    public class ObservableValidationObject : ObservableObject, INotifyDataErrorInfo
    {
        #region Fields

        private readonly Dictionary<string, List<string>> errors = new();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        #endregion

        #region Properties

        [JsonIgnore]
        [NotMapped]
        public bool HasErrors
        {
            get
            {
                return errors.Any(p => p.Value != null && p.Value.Count > 0);
            }
        }

        [JsonIgnore]
        [NotMapped]
        public bool HasPropertyErrors
        {
            get
            {
                return errors.Any(p => !string.IsNullOrEmpty(p.Key) && p.Value != null && p.Value.Count > 0);
            }
        }

        [JsonIgnore]
        [NotMapped]
        public bool HasGeneralErrors
        {
            get
            {
                if (!errors.ContainsKey(string.Empty))
                {
                    return false;
                }

                return errors[string.Empty] != null && errors.Count > 0;
            }
        }

        [JsonIgnore]
        [NotMapped]
        public List<string> Errors => errors.Count > 0 ? errors.SelectMany(p => p.Value).ToList() : new List<string>();

        [JsonIgnore]
        [NotMapped]
        public List<string> GeneralErrors => errors.ContainsKey(string.Empty) ? errors[string.Empty] : null;

        #endregion

        #region Public Methods

        public void ClearErrors()
        {
            errors.Clear();

            RaisePropertyChanged(nameof(HasErrors));
            RaisePropertyChanged(nameof(Errors));

            RaisePropertyChanged(nameof(HasPropertyErrors));

            RaisePropertyChanged(nameof(HasGeneralErrors));
            RaisePropertyChanged(nameof(GeneralErrors));
        }

        public void ClearGeneralErrors()
        {
            if (errors.ContainsKey(string.Empty))
            {
                errors.Remove(string.Empty);

                RaisePropertyChanged(nameof(HasGeneralErrors));
                RaisePropertyChanged(nameof(GeneralErrors));
                RaisePropertyChanged(nameof(HasErrors));
                RaisePropertyChanged(nameof(Errors));
            }
        }

        public void AddError(string key, string message)
        {
            ClearGeneralErrors();

            AddError(new KeyValuePair<string, List<string>>(key, new List<string> { message }));

            RaisePropertyChanged(nameof(HasPropertyErrors));
            RaisePropertyChanged(nameof(HasErrors));
            RaisePropertyChanged(nameof(Errors));
        }

        private void AddError(KeyValuePair<string, List<string>> item)
        {
            if (!errors.ContainsKey(item.Key))
            {
                errors.Add(item.Key, new List<string>());
            }

            errors[item.Key].AddRange(item.Value);

            if (item.Key == string.Empty)
            {
                RaisePropertyChanged(nameof(HasGeneralErrors));
                RaisePropertyChanged(nameof(GeneralErrors));
            }
            else
            {
                RaiseErrorsChanged(item.Key);
            }
        }

        public IEnumerable GetErrors(string propertyName)
        {
            return GetAllErrors(propertyName);
        }

        public override bool Set<T>(string propertyName, ref T oldValue, T newValue)
        {
            var result = base.Set(propertyName, ref oldValue, newValue);
            if (result)
            {
                Validate(newValue, propertyName);
            }

            return result;
        }

        public void Validate()
        {
            Validate(this, string.Empty);
        }

        public void RaiseErrorsChanged([CallerMemberName] string propertyName = null)
        {
            var copy = ErrorsChanged;
            copy?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        #endregion

        #region Private Methods

        private void Validate(object value, [CallerMemberName] string propertyName = null)
        {
            ClearGeneralErrors();

            var validationContext = new ValidationContext(this, null, null);

            var validationResults = new List<ValidationResult>();

            if (!string.IsNullOrEmpty(propertyName))
            {
                validationContext.MemberName = propertyName;
                Validator.TryValidateProperty(value, validationContext, validationResults);

                if (errors.ContainsKey(propertyName))
                {
                    errors.Remove(propertyName);
                    RaiseErrorsChanged(propertyName);

                    RaisePropertyChanged(nameof(HasPropertyErrors));
                    RaisePropertyChanged(nameof(HasErrors));
                    RaisePropertyChanged(nameof(Errors));
                }
            }
            else
            {
                Validator.TryValidateObject(value, validationContext, validationResults, true);

                var errorsKeys = errors.Keys.ToList();

                if (errorsKeys.Count > 0)
                {
                    foreach (var item in errorsKeys)
                    {
                        errors.Remove(item);

                        if (item == string.Empty)
                        {
                            RaisePropertyChanged(nameof(HasGeneralErrors));
                            RaisePropertyChanged(nameof(GeneralErrors));
                        }
                        else
                        {
                            RaiseErrorsChanged(item);
                        }
                    }

                    RaisePropertyChanged(nameof(HasPropertyErrors));
                    RaisePropertyChanged(nameof(HasErrors));
                    RaisePropertyChanged(nameof(Errors));
                }
            }

            if (validationResults.Count > 0)
            {
                var validatedProperties = new HashSet<string>();

                foreach (var validationResult in validationResults)
                {
                    var members = validationResult.MemberNames?.ToList();

                    if (members != null && members.Count > 0)
                    {
                        foreach (var memberName in members)
                        {
                            validatedProperties.Add(memberName);

                            if (!errors.ContainsKey(memberName))
                            {
                                errors.Add(memberName, new List<string>());
                            }

                            errors[memberName].Add(validationResult.ErrorMessage);
                        }
                    }
                    else
                    {
                        if (!errors.ContainsKey(string.Empty))
                        {
                            errors.Add(string.Empty, new List<string>());
                        }

                        errors[string.Empty].Add(validationResult.ErrorMessage);
                    }
                }

                foreach (var memberName in validatedProperties)
                {
                    RaiseErrorsChanged(memberName);
                }

                RaisePropertyChanged(nameof(HasPropertyErrors));
                RaisePropertyChanged(nameof(HasErrors));
                RaisePropertyChanged(nameof(Errors));
            }
        }

        private IEnumerable<object> GetAllErrors(string propertyName)
        {
            if (propertyName != string.Empty)
            {
                if (errors.ContainsKey(propertyName) && errors[propertyName] != null
                                                          && errors[propertyName].Count > 0)
                {
                    return errors[propertyName];
                }

                return new List<string>();
            }

            if (errors.ContainsKey(string.Empty) && errors[string.Empty] != null
                                                      && errors[string.Empty].Count > 0)
            {
                return errors[string.Empty];
            }

            return new List<string>();
        }

        #endregion
    }
}
