using System;
using Unity.Profiling;

namespace Fives.Runtime.Tests
{
    /// <summary>
    /// Counts the managed allocations the test thread makes while an action runs.
    /// Unity's Is.Not.AllocatingGCMemory() is not used: its recorder starts on every thread and is only then narrowed to
    /// the test thread, and an allocation another thread makes at that moment is sometimes counted as the test's.
    /// This recorder listens to the test thread from the start.
    /// </summary>
    internal static class Allocations
    {
        public static int During(Action action)
        {
            using var recorder = new ProfilerRecorder("GC.Alloc", capacity: 64, ProfilerRecorderOptions.CollectOnlyOnCurrentThread);
            recorder.Start();
            action();
            recorder.Stop();
            return recorder.Count;
        }
    }
}
