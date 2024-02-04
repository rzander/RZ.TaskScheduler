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

### manually trigger a task but only run one instance (skip is task is already running)
```c#
Scheduler.Run("Task1", singleinstance: true);
```
