using RZ.TaskScheduler;
namespace RZTask_Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void CreateTask_do_not_start()
        {
            RZSched.Add("TEST1", (e) => { (e as RZTask).Result = "myResult"; });

            var T1 = RZSched.Get("TEST1");
            if (T1 != null)
            {
                if (T1.IsRunning || T1.IsCompleted || T1.Result != null || T1.LastRun >= DateTime.Now.AddMinutes(-5))
                    Assert.Fail();
                else
                    Assert.Pass();
            }
            else
                Assert.Fail();
        }

        [Test]
        public void GetExistingTask()
        {
            if (RZSched.Get("TEST1") != null)
                Assert.Pass();
            else
                Assert.Fail();
        }

        [Test]
        public void RunExistingTask()
        {
            var T1 = RZSched.Get("TEST1");

            if (T1 != null)
            {
                T1.Run();
                Thread.Sleep(100);
                if (T1.Result == "myResult" && T1.IsCompleted && !T1.IsRunning && T1.LastRun > DateTime.Now.AddMinutes(-5))
                    Assert.Pass();
                else
                    Assert.Fail();
            }
            else
                Assert.Fail();
        }

        [Test]
        public void RunTask_Background()
        {
            var T1 = RZSched.Add("TEST1", (e) => 
            {
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} Timer {(e as RZTask)?.Name} was started...");
                Thread.Sleep(2000);
                if ((e as RZTask)?.CancellationToken.IsCancellationRequested == true)
                {
                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} Timer {(e as RZTask)?.Name} was cancelled.");
                    return;
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} Timer {(e as RZTask)?.Name} completed.");
                    (e as RZTask).Result = "myResult";
                }
            });

            T1.Run(singleinstance: true, wait: false);

            Thread.Sleep(500);

            if (!T1.IsRunning || T1.IsCompleted || T1.Result != null)
            {
                Assert.Fail("Part1");
                return;
            }

            Thread.Sleep(3000);

            if (!T1.IsRunning && T1.IsCompleted && T1.Result == "myResult")
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail("Part2");
            }
        }

        [Test]
        public void RunTask_Cancel()
        {
            var T1 = RZSched.Add("TEST1", (e) =>
            {
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} Timer {(e as RZTask)?.Name} was started...");
                Thread.Sleep(2000);
                if ((e as RZTask)?.CancellationToken.IsCancellationRequested == true)
                {
                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} Timer {(e as RZTask)?.Name} was cancelled.");
                    return;
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} Timer {(e as RZTask)?.Name} completed.");
                    (e as RZTask).Result = "myResult";
                }
            });

            T1.Run(singleinstance: true, wait: false, cancellationTokenSource: new CancellationTokenSource(500));

            Thread.Sleep(500);

            if (!T1.IsRunning || T1.IsCompleted || T1.Result != null)
            {
                Assert.Fail("Part1");
                return;
            }

            Thread.Sleep(3000);

            if (!T1.IsRunning && !T1.IsCompleted && T1.Result == null)
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail("Part2");
            }
        }

        [Test]
        public void RunTasks_Queue()
        {
            var T1 = RZSched.Add("TEST1", (e) =>
            {
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} Timer {(e as RZTask)?.Name} was started...");
                Thread.Sleep(2000);
                if ((e as RZTask)?.CancellationToken.IsCancellationRequested == true)
                {
                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} Timer {(e as RZTask)?.Name} was cancelled.");
                    return;
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} Timer {(e as RZTask)?.Name} completed.");
                    (e as RZTask).Result = "myResult";
                }
            });

            var T2 = RZSched.Add("TEST2", (e) =>
            {
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} Timer {(e as RZTask)?.Name} was started...");
                Thread.Sleep(2000);
                if ((e as RZTask)?.CancellationToken.IsCancellationRequested == true)
                {
                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} Timer {(e as RZTask)?.Name} was cancelled.");
                    return;
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} Timer {(e as RZTask)?.Name} completed.");
                    (e as RZTask).Result = "myResult";
                }
            });

            RZSched.Queue(T1);
            RZSched.Queue(T2);  

            Thread.Sleep(500);

            if (!T1.IsRunning && !T2.IsRunning)
            {
                Assert.Fail("Part1");
                return;
            }

            if (T1.IsCompleted || T2.IsCompleted)
            {
                Assert.Fail("Part2");
                return;
            }

            Thread.Sleep(2000);

            //one of the tasks should be completed
            if((T1.IsCompleted && !T2.IsCompleted) || (!T1.IsCompleted && T2.IsCompleted))
            {
            }
            else
            {
                Assert.Fail("Part3");
                return;
            }

            Thread.Sleep(2000);

            if(T1.IsCompleted && T2.IsCompleted)
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail("Part4");
            }
        }
    }
}