﻿using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBook.Presentation.Views;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;

namespace algoBhaiya.ReportBook.Presentation.ViewModels
{
    public class MonthlySummaryViewModel : BaseViewModel
    {
        private readonly IAppNavigator _appNavigator;
        private readonly IServiceProvider _serviceProvider;

        public ObservableCollection<MonthlySummaryItem> MonthlySummaries { get; } = new();

        private string _currentMonthLabel;
        public string CurrentMonthLabel
        {
            get => _currentMonthLabel;
            set
            {
                if (_currentMonthLabel != value)
                {
                    _currentMonthLabel = value;
                    OnPropertyChanged(nameof(CurrentMonthLabel));
                }
            }
        }

        private bool _isNavigating = false;
        public ICommand ShowCalendarCommand { get; }

        public MonthlySummaryViewModel(
            IAppNavigator appNavigator, 
            IServiceProvider serviceProvider)
        {
            _appNavigator = appNavigator;
            _serviceProvider = serviceProvider;

            ShowCalendarCommand = new Command<MonthlySummaryItem>(async (selectedItem) =>
            {
                if (selectedItem == null || _isNavigating)
                    return;

                try
                {
                    _isNavigating = true;

                    await ShowCalendar(selectedItem);
                }
                finally
                {
                    _isNavigating = false;
                }
            });

            LoadDataAsync(DateTime.Today.Year, DateTime.Today.Month);            
        }

        public async Task LoadDataAsync(int year, int month)
        {
            MonthlySummaries.Clear();

            byte userId = (byte)Preferences.Get("CurrentUserId", 0);
            if (userId == 0) return;

            var records = await _serviceProvider
                .GetRequiredService<IDailyEntryRepository>()
                .GetMonthlySummaryReportAsync(userId, year, month);

            foreach( var record in records)
            {
                MonthlySummaries.Add(record);
            }
            
            CurrentMonthLabel = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)} {year}";
        }

        private async Task ShowCalendar(MonthlySummaryItem item)
        {
            await _appNavigator.PushModalAsync(() =>
            {
                var page = new FilledDatesCalendarPage(item.FilledDates);
                return page;
            });
        }
    }

}
