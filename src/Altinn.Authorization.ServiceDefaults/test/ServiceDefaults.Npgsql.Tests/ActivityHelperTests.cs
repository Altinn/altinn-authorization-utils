using Altinn.Authorization.ServiceDefaults.Npgsql.Yuniql;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Tests;

public class ActivityHelperTests
{
    [Fact]
    public void ThreadLocalState_Works_InMultithreaded()
    {
        var count = 100;
        using var ready = new ManualResetEvent(false);
        var threads = new List<Thread>(count);
        var threadMap = new ConcurrentDictionary<int, ThreadInfo>();
        var startedActivities = new ConcurrentBag<Activity>();
        var stoppedActivities = 0;
        using var source = new ActivitySource("test");
        using var listener = new ActivityListener
        {
            ShouldListenTo = activitySource => ReferenceEquals(activitySource, source),
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
            ActivityStarted = startedActivities.Add,
            ActivityStopped = (_) => Interlocked.Increment(ref stoppedActivities),
        };

        ActivitySource.AddActivityListener(listener);

        for (var i = 0; i < count; i++)
        {
            var index = i;
            var thread = new Thread(() =>
            {
                threadMap[index] = new ThreadInfo(Environment.CurrentManagedThreadId);
                Span<KeyValuePair<string, object?>> tags = [
                    new("thread.id", Environment.CurrentManagedThreadId),
                    new("test.iteration", index),
                ];

                ready.WaitOne();

                RunOuterActivity($"part1-{index}", source, tags);
                RunOuterActivity($"part2-{index}", source, tags);
            });
            threads.Add(thread);
        }

        threads.ForEach(static t => t.Start());
        ready.Set();

        // Wait for all threads to finish
        threads.ForEach(static t => t.Join());

        var started = startedActivities.ToList();
        var stopped = Volatile.Read(ref stoppedActivities);

        threadMap.Should().HaveCount(count);
        started.Should().HaveCount(count * 4);
        stopped.Should().Be(started.Count);

        foreach (var activity in started)
        {
            activity.Duration.Should().BeGreaterThan(TimeSpan.Zero);
            var testIteration = (int)activity.TagObjects.Single(tag => tag.Key == "test.iteration").Value!;
            var threadId = (int)activity.TagObjects.Single(tag => tag.Key == "thread.id").Value!;
            threadMap[testIteration].ThreadId.Should().Be(threadId);
        }

        static void RunOuterActivity(string name, ActivitySource source, ReadOnlySpan<KeyValuePair<string, object?>> tags)
        {
            using var activity = StartActivity($"{name}-outer", source, tags);
            Thread.Sleep(25); // Simulate tiny work
            RunInnerActivity(name, source, tags);
            Thread.Sleep(25); // Simulate tiny work
        }

        static void RunInnerActivity(string name, ActivitySource source, ReadOnlySpan<KeyValuePair<string, object?>> tags)
        {
            using var activity = StartActivity($"{name}-inner", source, tags);
            Thread.Sleep(50); // Simulate small work
        }

        static Activity? StartActivity(string name, ActivitySource source, ReadOnlySpan<KeyValuePair<string, object?>> tags)
        {
            using var state = ActivityHelper.ThreadLocalState;
            state.AddTags(tags);

            return source.StartActivity(ActivityKind.Internal, tags: state.Tags, name: name);
        }
    }

    [Fact]
    public void UnsetTagValue_IsNotNull()
    {
        ActivityHelper.UnsetTagValue.Should().NotBeNull();
    }

    [Fact]
    public void UnsetTagValue_IsFilteredOut()
    {
        using var source = new ActivitySource("test");
        using var listener = new ActivityListener
        {
            ShouldListenTo = activitySource => ReferenceEquals(activitySource, source),
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
            ActivityStarted = static _ => { },
            ActivityStopped = static _ => { },
        };

        ActivitySource.AddActivityListener(listener);

        using var state = ActivityHelper.ThreadLocalState;
        state.AddTags([
            new("tag.before", "before"),
            new("tag.unset", ActivityHelper.UnsetTagValue),
            new("tag.after", "after"),
        ]);

        using var activity = source.StartActivity(ActivityKind.Internal, tags: state.Tags, name: "test");

        Assert.NotNull(activity);
        activity.TagObjects.Should().HaveCount(2);
        activity.TagObjects.Should().Contain(new KeyValuePair<string, object?>("tag.before", "before"));
        activity.TagObjects.Should().Contain(new KeyValuePair<string, object?>("tag.after", "after"));
    }

    private sealed record ThreadInfo(int ThreadId);
}
