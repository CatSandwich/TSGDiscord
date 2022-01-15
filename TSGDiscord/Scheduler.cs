using System;

namespace TSGDiscord
{
    public class Scheduler
    {
        public void Schedule(string name, DateTime fireAt, Action action) => Schedule(new Task(name, fireAt, action));
        public void Schedule(Task task)
        {
            task.Schedule();
        }

        public void ScheduleRepeating(string name, DateTime fireAt, Action action, TimeSpan repeatTime) => ScheduleRepeating(new Task(name, fireAt, action), repeatTime);
        public void ScheduleRepeating(Task task, TimeSpan repeatTime)
        {
            // Store the previous action
            var temp = task.Action;

            // Overwrite the action
            task.Action = () =>
            {
                // Run the action
                temp();
                // Set the next fire time
                task.FireAt += repeatTime;
                // Reschedule
                task.Schedule();
            };
            // Set it for execution
            task.Schedule();
        }

        public class Task
        {
            public string Name;
            public DateTime FireAt;
            public Action Action;

            public Task(string name, DateTime fireAt, Action action)
            {
                Name = name;
                FireAt = fireAt;
                Action = action;
            }

            public void Schedule()
            {
                System.Threading.Tasks.Task.Run(async () =>
                {
                    await System.Threading.Tasks.Task.Delay(FireAt - DateTime.Now);
                    SafeInvoke();
                });
            }

            public void SafeInvoke()
            {
                try { Action.Invoke(); }
                catch (Exception ex) { Console.WriteLine($"Exception in scheduled task '{Name}': {ex}"); }
            }
        }
    }
}
