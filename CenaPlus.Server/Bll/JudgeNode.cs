﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CenaPlus.Network;
using System.ServiceModel;
using CenaPlus.Entity;
using CenaPlus.Server.Judge;
using System.Threading;

namespace CenaPlus.Server.Bll
{
    [ServiceBehavior(UseSynchronizationContext = false)]
    public class JudgeNode : IJudgeNode
    {
        public static string Password { get; set; }

        private bool Authenticated { get; set; }
        private IJudgeNodeCallback Callback
        {
            get
            {
                return OperationContext.Current.GetCallbackChannel<IJudgeNodeCallback>();
            }
        }

        public string GetVersion()
        {
            var version = typeof(IJudgeNode).Assembly.GetName().Version;
            return version.Major + "." + version.Minor;
        }

        public bool Authenticate(string password)
        {
            if (password == Password)
            {
                Authenticated = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        private Core GetFreeCore()
        {
            Core freeCore = null;
            while (freeCore == null)
            {
                freeCore = Env.GetFreeCore();
                if (freeCore == null) Thread.Sleep(500);
            }
            return freeCore;
        }

        public TaskFeedback_Compile Compile(Problem problem, Record record)
        {
            if (!Authenticated) throw new FaultException<AccessDeniedError>(new AccessDeniedError());
            var freeCore = GetFreeCore();
            return freeCore.Run(new Task
            {
                Problem = problem,
                Record = record,
                Type = TaskType.Compile
            },Callback) as TaskFeedback_Compile;
        }

        public TaskFeedback_Run Run(Problem problem, Record record, int testCaseID)
        {
            if (!Authenticated) throw new FaultException<AccessDeniedError>(new AccessDeniedError());
            var freeCore = GetFreeCore();
            return freeCore.Run(new Task
            {
                Problem = problem,
                Record = record,
                TestCaseID = testCaseID,
                Type = TaskType.Run
            }, Callback) as TaskFeedback_Run;
        }

        public TaskFeedback_Hack Hack(Problem problem, Record record, Hack hack)
        {
            if (!Authenticated) throw new FaultException<AccessDeniedError>(new AccessDeniedError());
            var freeCore = GetFreeCore();
            return freeCore.Run(new Task
            {
                Problem = problem,
                Record = record,
                Hack = hack,
                Type = TaskType.Run
            }, Callback) as TaskFeedback_Hack;
        }
    }
}
