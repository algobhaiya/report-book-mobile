using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace algoBhaiya.ReportBook.Presentation.ViewModels
{
    public class DailyEntryFieldCollection : ObservableCollection<DailyEntryFieldViewModel>
    {
        private readonly HashSet<DailyEntryFieldViewModel> _dirtyItems = new();

        public int DirtyCount => _dirtyItems.Count;

        public event EventHandler? DirtyCountChanged;

        protected override void InsertItem(int index, DailyEntryFieldViewModel item)
        {
            base.InsertItem(index, item);
            item.PropertyChanged += OnItemPropertyChanged;
            UpdateDirtyState(item);
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];
            item.PropertyChanged -= OnItemPropertyChanged;
            if (_dirtyItems.Remove(item))
            {
                DirtyCountChanged?.Invoke(this, EventArgs.Empty);
            }

            base.RemoveItem(index);
        }

        protected override void SetItem(int index, DailyEntryFieldViewModel item)
        {
            var oldItem = this[index];
            oldItem.PropertyChanged -= OnItemPropertyChanged;
            if (_dirtyItems.Remove(oldItem))
            {
                DirtyCountChanged?.Invoke(this, EventArgs.Empty);
            }

            base.SetItem(index, item);
            item.PropertyChanged += OnItemPropertyChanged;
            UpdateDirtyState(item);
        }

        protected override void ClearItems()
        {
            foreach (var item in this)
            {
                item.PropertyChanged -= OnItemPropertyChanged;
            }

            base.ClearItems();
            if (_dirtyItems.Count > 0)
            {
                _dirtyItems.Clear();
                DirtyCountChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not DailyEntryFieldViewModel item)
            {
                return;
            }

            if (e.PropertyName != nameof(DailyEntryFieldViewModel.Value) &&
                e.PropertyName != nameof(DailyEntryFieldViewModel.OriginalValue))
            {
                return;
            }

            UpdateDirtyState(item);
        }

        private void UpdateDirtyState(DailyEntryFieldViewModel item)
        {
            var isDirty = item.IsDirty;
            var dirtyBefore = _dirtyItems.Contains(item);

            if (dirtyBefore == isDirty)
            {
                return;
            }

            if (isDirty)
            {
                _dirtyItems.Add(item);
            }
            else
            {
                _dirtyItems.Remove(item);
            }

            DirtyCountChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
