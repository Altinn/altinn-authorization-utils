using Altinn.Authorization.ServiceDefaults.Telemetry;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Altinn.Authorization.ServiceDefaults.Tests;

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

        var linkContext = new ActivityContext(ActivityTraceId.CreateRandom(), ActivitySpanId.CreateRandom(), ActivityTraceFlags.None);
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
                Span<ActivityLink> links = [
                    new(linkContext),
                ];

                ready.WaitOne();

                RunOuterActivity($"part1-{index}", source, tags, links);
                RunOuterActivity($"part2-{index}", source, tags, links);
            });
            threads.Add(thread);
        }

        threads.ForEach(static t => t.Start());
        ready.Set();

        // Wait for all threads to finish
        threads.ForEach(static t => t.Join());

        var started = startedActivities.ToList();
        var stopped = Volatile.Read(ref stoppedActivities);

        threadMap.Count.ShouldBe(count);
        started.Count.ShouldBe(count * 4);
        stopped.ShouldBe(started.Count);

        foreach (var activity in started)
        {
            activity.Duration.ShouldBeGreaterThan(TimeSpan.Zero);
            var testIteration = (int)activity.TagObjects.Single(tag => tag.Key == "test.iteration").Value!;
            var threadId = (int)activity.TagObjects.Single(tag => tag.Key == "thread.id").Value!;

            if (activity.OperationName.EndsWith("-outer"))
            {
                activity.Links.Count().ShouldBe(1);
                activity.Links.First().Context.ShouldBe(linkContext);
            }

            threadMap[testIteration].ThreadId.ShouldBe(threadId);
        }

        static void RunOuterActivity(string name, ActivitySource source, ReadOnlySpan<KeyValuePair<string, object?>> tags, ReadOnlySpan<ActivityLink> links)
        {
            using var activity = StartActivity($"{name}-outer", source, tags, links);
            Thread.Sleep(25); // Simulate tiny work
            RunInnerActivity(name, source, tags);
            Thread.Sleep(25); // Simulate tiny work
        }

        static void RunInnerActivity(string name, ActivitySource source, ReadOnlySpan<KeyValuePair<string, object?>> tags)
        {
            using var activity = StartActivity($"{name}-inner", source, tags);
            Thread.Sleep(50); // Simulate small work
        }

        static Activity? StartActivity(string name, ActivitySource source, ReadOnlySpan<KeyValuePair<string, object?>> tags, ReadOnlySpan<ActivityLink> links = default)
        {
            using var state = ActivityHelper.ThreadLocalState;
            state.AddTags(tags);
            state.AddLinks(links);

            return source.StartActivity(ActivityKind.Internal, tags: state.Tags, links: state.Links, name: name);
        }
    }

    [Fact]
    public void UnsetTagValue_IsNotNull()
    {
        ActivityHelper.UnsetTagValue.ShouldNotBeNull();
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
        activity.TagObjects.Count().ShouldBe(2);
        activity.TagObjects.ShouldContain(new KeyValuePair<string, object?>("tag.before", "before"));
        activity.TagObjects.ShouldContain(new KeyValuePair<string, object?>("tag.after", "after"));
    }

    [Fact]
    public void TagList_CanBeUsed()
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

        var list = new TagList([
            new("tag.foo", "foo"),
            new("tag.bar", "bar"),
        ]);

        using var activity = source.StartActivity(
            "test",
            ActivityKind.Internal,
            tags: in list);

        Assert.NotNull(activity);
        activity.TagObjects.Count().ShouldBe(2);
        activity.TagObjects.ShouldContain(new KeyValuePair<string, object?>("tag.foo", "foo"));
        activity.TagObjects.ShouldContain(new KeyValuePair<string, object?>("tag.bar", "bar"));
    }

    private sealed record ThreadInfo(int ThreadId);
}
