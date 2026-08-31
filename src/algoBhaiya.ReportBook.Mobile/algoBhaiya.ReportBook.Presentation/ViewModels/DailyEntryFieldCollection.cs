using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace algoBhaiya.ReportBook.Presentation.ViewModels
{
    public class DailyEntryFieldCollection : ObservableCollection<DailyEntryFieldViewModel>
    {
        private readonly HashSet<DailyEntryFieldViewModel> _dirtyItems = new();
        private int _dirtyTrackingSuspensionCount;

        public int DirtyCount => _dirtyItems.Count;

        public event EventHandler? DirtyCountChanged;

        public IDisposable SuspendDirtyTracking()
        {
            _dirtyTrackingSuspensionCount++;
            return new DirtyTrackingScope(this);
        }

        public void RecalculateDirtyState()
        {
            _dirtyItems.Clear();
            foreach (var item in this)
            {
                if (item.IsDirty)
                {
                    _dirtyItems.Add(item);
                }
            }

            DirtyCountChanged?.Invoke(this, EventArgs.Empty);
        }

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
            if (_dirtyTrackingSuspensionCount > 0)
            {
                return;
            }

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

        private sealed class DirtyTrackingScope : IDisposable
        {
            private readonly DailyEntryFieldCollection _owner;
            private bool _disposed;

            public DirtyTrackingScope(DailyEntryFieldCollection owner)
            {
                _owner = owner;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                _owner._dirtyTrackingSuspensionCount--;
                if (_owner._dirtyTrackingSuspensionCount == 0)
                {
                    _owner.RecalculateDirtyState();
                }
            }
        }
    }
}
