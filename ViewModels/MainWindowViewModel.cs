using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using YoutubeConverter.Enums;
using YoutubeConverter.Services;

namespace YoutubeConverter.ViewModels
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        private ConversionType _selectedConversionType;
        private IEnumerable<ConversionType> _conversionTypes;

        public IEnumerable<ConversionType> ConversionTypes
        {
            get => _conversionTypes;
            set
            {
                _conversionTypes = value;
                OnPropertyChanged();
            }
        }

        public ConversionType SelectedConversionType
        {
            get => _selectedConversionType;
            set
            {
                _selectedConversionType = value;
                OnPropertyChanged();
            }
        }

        private string _inputLink;
        private ConverterService _converterService;

        public string InputLink
        {
            get => _inputLink;
            set
            {
                _inputLink = value;
                OnPropertyChanged();
            }
        }

        public ICommand ConvertCommand { get; }

        public MainWindowViewModel(ConverterService converterService)
        {
            _converterService = converterService;

            _conversionTypes = Enum.GetValues(typeof(ConversionType)).Cast<ConversionType>();

            ConvertCommand = new RelayCommand(ExecuteConvertCommand);
        }

        /// <summary>
        /// Calls appropriate methods based on the comboBox value.
        /// </summary>
        private async void ExecuteConvertCommand()
        {
            switch (SelectedConversionType)
            {
                case ConversionType.YoutubeVideo:
                    await _converterService.LinkToMp4(_inputLink);
                    break;

                case ConversionType.YoutubeAudio:
                    await _converterService.LinkToMp3(_inputLink);
                    break;

                case ConversionType.TVP:
                    await _converterService.TVPLinkToMp4(_inputLink);
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}