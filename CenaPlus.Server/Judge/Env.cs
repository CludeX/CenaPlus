﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using CenaPlus.Network;

namespace CenaPlus.Server.Judge
{
    public static class Env
    {
        public static List<Core> Cores = new List<Core>();
        public static event EventHandler CoreStatusUpdated;

        private static Core GetFreeCore()
        {
            for (; ; )
            {
                lock (Cores)
                {
                    var freeCore = Cores.Where(c => c.Status == CoreStatus.Free).FirstOrDefault();
                    if (freeCore != null)
                    {
                        freeCore.Status = CoreStatus.Working;
                        return freeCore;
                    }
                }
                Thread.Sleep(500);
            }
        }

        public static Entity.TaskFeedback Run(Entity.Task task, IJudgeNodeCallback callback)
        {
            var freeCore = GetFreeCore();
            
            if (CoreStatusUpdated != null)
                CoreStatusUpdated(null, null);

            freeCore.CurrentTask = task;
            TaskHelper helper = new TaskHelper()
            {
                Task = task,
                CenterServer = callback
            };
            helper.Start();
            freeCore.Status = CoreStatus.Free;

            if (CoreStatusUpdated != null)
                CoreStatusUpdated(null, null);
            return helper.Feedback;
        }

        public static int GetFreeCoreCount()
        {
            lock (Cores)
            {
                return Cores.Where(c => c.Status == CoreStatus.Free).Count();
            }
        }
    }

    public class Core
    {
        public Entity.Task CurrentTask { get; set; }

        public CoreStatus Status { get; set; }
        public int Index { get; set; }
        //display only
        public string Title
        {
            get
            {
                return String.Format("Core #{0}", Index);
            }
        }
        public string StatusStr
        {
            get
            {
                return "Status: " + Status.ToString();
            }
        }
        public string TaskType
        {
            get
            {
                if (CurrentTask == null) return "Task: N/A";
                else
                {
                    switch (CurrentTask.Type)
                    {
                        case Entity.TaskType.Compile:
                            return "Task: Compile R" + CurrentTask.Record.ID;
                        case Entity.TaskType.Run:
                            return "Task: Run R" + CurrentTask.Record.ID;
                        case Entity.TaskType.Hack:
                            return "Task: Hack R" + CurrentTask.Hack.ID;
                        default:
                            return "Task: N/A";
                    }
                }
            }
        }
    }
    public enum CoreStatus { Free, Working };
}
