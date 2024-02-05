# RZ.TaskScheduler
a simple TaskScheduler library for .NET

## Examples

### Create a Task but do not run
```c#
Scheduler.Add("Task1", (e) =>
      {
          Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} Timer {(e as ScheduledTask)?.Name} was started...");
          Thread.Sleep(2000);
          Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} Timer {(e as ScheduledTask)?.Name} completed.");
      });
```

### Create a Task to run every 10s
```c#
Scheduler.Add("Task2", (e) =>
      {
          Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} Timer {(e as ScheduledTask)?.Name} was started...");
          Thread.Sleep(2000);
          Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} Timer {(e as ScheduledTask)?.Name} completed.");
      }).Every(new TimeSpan(0,0,10));
```

### Create a Task to run every 10s, but start first run in 5s
```c#
Scheduler.Add("Task2", (e) =>
      {
          Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} Timer {(e as ScheduledTask)?.Name} was started...");
          Thread.Sleep(2000);
          Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} Timer {(e as ScheduledTask)?.Name} completed.");
      }).Every(new TimeSpan(0,0,10), new TimeSpan(0,0,5));
```

### manually trigger a Task
```c#
Scheduler.Run("Task1");
```

### manually trigger a task but only run one instance (skip if task is already running)
```c#
Scheduler.Run("Task1", singleinstance: true);
```

### Create a Task and trigger code OnError and OnComplete
```c#
Scheduler.Add("Task1", (e) =>
            {
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} Timer {(e as ScheduledTask)?.Name} was started...");
                Thread.Sleep(5000);
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} Timer {(e as ScheduledTask)?.Name} completed.");
                (e as ScheduledTask).Result = "myResult";
            }).OnError((e) =>
            {
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} Timer {(e as ScheduledTaskException)?.ScheduledTask.Name} was failed with Error '{(e as ScheduledTaskException)?.Exception.Message}'.");
            }).OnComplete((e) =>
            {
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} Timer {(e as ScheduledTask)?.Name} was completed with Result: {(e as ScheduledTask)?.Result}");
            }).Run(cancellationTokenSource: new CancellationTokenSource(new TimeSpan(0,5,0)));
```
