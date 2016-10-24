﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xigadee;

namespace Test.Xigadee
{
    [TestClass]
    public class SimpleCommand1UnitTest: CommandUnitTestBase<SimpleCommand1>
    {
        [TestMethod]
        public void TestStandard()
        {
            DefaultTest();
        }


        [TestMethod]
        public void PipelineCommand()
        {
            try
            {
                var pipeline = Pipeline();

                pipeline.Start();

                int start = Environment.TickCount;

                var result1 = mCommandInit.Process<Blah, string>("internalIn", "simples1", "async",
                    new Blah() { Message = "hello" }, new RequestSettings() { WaitTime = TimeSpan.FromHours(1) }).Result;

                var result2 = mCommandInit.Process<Blah, string>("internalIn", "simples1", "sync",
                    new Blah() { Message = "hello" }, new RequestSettings() { WaitTime = TimeSpan.FromHours(1) }).Result;

                var result3 = mCommandInit.Process<Blah, string>("internalIn", "simples1", "sync",
                    new Blah() { Message = "hello" }, new RequestSettings() { WaitTime = TimeSpan.FromHours(1) }).Result;

                var result4 = mCommandInit.Process<Blah, string>("internalIn", "simples1", "sync",
                    new Blah() { Message = "hello" }, new RequestSettings() { WaitTime = TimeSpan.FromHours(1) }).Result;

                var result5 = mCommandInit.Process<Blah, string>("internalIn", "simples1", "sync",
                    new Blah() { Message = "hello" }, new RequestSettings() { WaitTime = TimeSpan.FromHours(1) }).Result;

                var result6 = mCommandInit.Process<Blah, string>("internalIn", "simples1", "sync",
                    new Blah() { Message = "hello" }, new RequestSettings() { WaitTime = TimeSpan.FromHours(1) }).Result;

                var end = ConversionHelper.DeltaAsTimeSpan(start);

                pipeline.Stop();
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }

    public class SimpleCommand1: CommandBase
    {
        public SimpleCommand1() : base(null){}

        [CommandContract(messageType: "simples1", actionType: "async")]
        private async Task ActionAsync(TransmissionPayload incoming, List<TransmissionPayload> outgoing)
        {
            await Task.Delay(100);
            Process(incoming, outgoing);
        }

        [CommandContract(messageType: "simples1", actionType: "sync")]
        private void ActionSync(TransmissionPayload incoming, List<TransmissionPayload> outgoing)
        {
            Process(incoming, outgoing);
        }

        private void Process(TransmissionPayload incoming, List<TransmissionPayload> outgoing)
        {
            var rs = incoming.ToResponse();
            rs.Message.Status = "204";
            outgoing.Add(rs);
        }

    }
}
