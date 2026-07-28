using System.Threading;

namespace algoBhaiya.ReportBook.Presentation.Services
{
    internal static class FieldUnitSeedGate
    {
        public static readonly SemaphoreSlim Gate = new(1, 1);
    }
}
