using System.Collections.Concurrent;

namespace algoBhaiya.ReportBook.Presentation.Helpers
{
    public class NavigationDataService
    {
        private readonly ConcurrentDictionary<string, object> _data = new();

        // Set a value (can be object, model, delegate, etc.)
        public void Set<T>(string key, T value)
        {
            _data[key] = value!;
        }

        // Get a value with type safety
        public T? Get<T>(string key)
        {
            if (_data.TryGetValue(key, out var value) && value is T t)
                return t;

            return default;
        }

        // Remove a single key
        public void Remove(string key)
        {
            _data.TryRemove(key, out _);
        }

        // Clear all stored navigation data
        public void Clear()
        {
            _data.Clear();
        }
    }

}
