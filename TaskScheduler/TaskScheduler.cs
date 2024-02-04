using System.Runtime.CompilerServices;

namespace RZ.TaskScheduler
{
    /// <summary>
    /// The scheduler is a simple class that allows you to schedule tasks to run at a specific time interval.
    /// </summary>
    public sealed class Scheduler
    {
        /// <summary>
        /// The list of scheduled tasks.
        /// </summary>
        public static List<ScheduledTask> ScheduledTasks = new List<ScheduledTask>();

        /// <summary>
        /// Adds a scheduled task.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="timerCallback"></param>
        /// <returns></returns>
        public static ScheduledTask Add(string Name, TimerCallback timerCallback)
        {
            var rt = new ScheduledTask
            {
                Name = Name,
                TimerCallback = timerCallback
            };

            if (ScheduledTasks.FirstOrDefault(x => x.Name == Name) != null)
            {
                var existing = ScheduledTasks.First(x => x.Name == Name);
                existing.Timer?.Dispose();
                ScheduledTasks.Remove(existing);
            }
            ScheduledTasks.Add(rt);

            return rt;
        }

        /// <summary>
        /// Gets a scheduled task by name.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public static ScheduledTask? Get(string Name)
        {
            return ScheduledTasks.FirstOrDefault(x => x.Name == Name);
        }

        /// <summary>
        /// Removes a scheduled task by name.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public static bool Remove(string Name)
        {
            var existing = ScheduledTasks.FirstOrDefault(x => x.Name == Name);
            if (existing != null)
            {
                existing.Timer?.Dispose();
                ScheduledTasks.Remove(existing);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Stops a scheduled task by name.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public static bool Stop(string Name)
        {
            var existing = ScheduledTasks.FirstOrDefault(x => x.Name == Name);
            if (existing != null)
            {
                existing.Timer?.Dispose();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Calls the scheduled task by name.
        /// </summary>
        /// <param name="Name"></param>
        public static bool Run(string Name)
        {
            var existing = ScheduledTasks.FirstOrDefault(x => x.Name == Name);
            if (existing != null)
            {
                new Task(() =>
                {
                    existing.Run();
                }).Start();

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Calls the scheduled task by name.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="singleinstance"></param>
        /// <param name="wait"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static bool Run(string Name, bool singleinstance, bool wait = false, TimeSpan? timeout = null)
        {
            if (singleinstance)
            {
                var existing = ScheduledTasks.FirstOrDefault(x => x.Name == Name);
                if (existing != null)
                {
                    existing.Run(singleinstance, wait, timeout);
                }

            }
            else return Run(Name);

            return false;
        }

        /// <summary>
        /// Cleans up the scheduled tasks that have already run.
        /// </summary>
        public static void Cleanup()
        {
            foreach (var oTask in ScheduledTasks.Where(x => x.NextRun < DateTime.Now).ToList())
            {
                try
                {
                    oTask.Timer?.Dispose();
                    ScheduledTasks.Remove(oTask);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }

    /// <summary>
    /// Represents a scheduled task.
    /// </summary>
    public class ScheduledTask
    {
        private DateTime _nextRun;
        internal DateTime _lastRun;
        public required TimerCallback TimerCallback { get; set; }
        public required string Name { get; set; }
        public Timer? Timer { get; set; }

        public DateTime? NextRun
        {
            get { return _nextRun; }
        }

        public DateTime? LastRun
        {
            get { return _lastRun; }
        }

        /// <summary>
        /// Schedule the task to run at a specific time interval.
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <param name="skipstartevent"></param>
        /// <param name="pause"></param>
        /// <param name="singleinstance"></param>
        /// <returns></returns>
        public ScheduledTask Every(TimeSpan timeSpan, bool skipstartevent, bool pause = false, bool singleinstance = false)
        {
            int dueTime = skipstartevent ? (int)timeSpan.TotalMilliseconds : 0;

            if (pause) dueTime = -1;

            if (Timer != null)
            {
                Timer.Dispose();
            }

            _nextRun = DateTime.Now + new TimeSpan(0, 0, 0, 0, dueTime);
            Timer = new Timer((e) => { _lastRun = DateTime.Now; Scheduler.Run(Name, singleinstance, false, timeSpan); }, this, dueTime, (int)timeSpan.TotalMilliseconds);

            return this;
        }

        /// <summary>
        /// Schedule the task to run at a specific time interval.
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public ScheduledTask Every(TimeSpan? timeSpan, TimeSpan? delay = null, bool singleinstance = false)
        {
            int dueTime = 0;
            int period = 0;
            if (delay != null) dueTime = (int)delay.Value.TotalMilliseconds;
            if (timeSpan != null) period = (int)timeSpan.Value.TotalMilliseconds; else period = -1;

            if (Timer != null)
            {
                Timer.Dispose();
            }

            _nextRun = DateTime.Now + new TimeSpan(0, 0, 0, 0, dueTime);
            Timer = new Timer((e) => { _lastRun = DateTime.Now; Scheduler.Run(Name, singleinstance, false, timeSpan); }, this, dueTime, period);

            return this;
        }

        /// <summary>
        /// Schedule the task to run once.
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        public ScheduledTask Once(TimeSpan? delay = null, bool singleinstance = false)
        {
            int dueTime = 0;
            if (delay != null) dueTime = (int)delay.Value.TotalMilliseconds;

            if(Timer != null){
                Timer.Dispose();
            }

            _nextRun = DateTime.Now + new TimeSpan(0, 0, 0, 0, dueTime);
            Timer = new Timer((e) => { _lastRun = DateTime.Now; Scheduler.Run(Name, singleinstance, false); }, this, dueTime, -1);

            return this;
        }

        /// <summary>
        /// Schedule the task to run once.
        /// </summary>
        /// <param name="startTime"></param>
        /// <returns></returns>
        public ScheduledTask Once(DateTime startTime, bool singleinstance = false)
        {
            var dueTime = (startTime - DateTime.Now);

            if (Timer != null)
            {
                Timer.Dispose();
            }

            _nextRun = DateTime.Now + dueTime;
            Timer = new Timer((e) => { _lastRun = DateTime.Now; Scheduler.Run(Name, singleinstance, false); }, this, dueTime, Timeout.InfiniteTimeSpan);
            
            return this;
        }

        public ScheduledTask Stop()
        {
            if (Timer != null)
            {
                Timer.Dispose();
            }

            _nextRun = new DateTime();
            return this;
        }

        /// <summary>
        /// Run the task.
        /// </summary>
        public bool Run()
        {
            new Task(() =>
            {
                _lastRun = DateTime.Now;
                TimerCallback(this);
            }).Start();

            return true;
        }

        /// <summary>
        /// Run the task.
        /// </summary>
        /// <param name="singleinstance"></param>
        /// <param name="wait"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool Run(bool singleinstance, bool wait = false, TimeSpan? timeout = null)
        {
            if (singleinstance)
            {
                    if (timeout == null)
                        timeout = TimeSpan.FromMilliseconds(1000);


                    Task tCall = new Task(() =>
                    {
                        //check if Task is already running
                        if (Monitor.TryEnter(this, (TimeSpan)timeout))
                        {
                            try
                            {
                                _lastRun = DateTime.Now;
                                TimerCallback(this);
                            }
                            finally
                            {
                                Monitor.Exit(this);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Task is already running...");
                        }
                    });

                    tCall.Start();

                    //wait for task to complete
                    if (wait)
                        tCall.Wait((TimeSpan)timeout);

                    return true;
            }
            else return Run();
        }
    }
}
