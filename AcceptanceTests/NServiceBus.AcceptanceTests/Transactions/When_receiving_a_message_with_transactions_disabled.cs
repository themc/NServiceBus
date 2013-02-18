﻿namespace NServiceBus.AcceptanceTests.Transactions
{
    using System;
    using AcceptanceTesting;
    using EndpointTemplates;
    using NUnit.Framework;
    using ScenarioDescriptors;

    public class When_receiving_a_message_with_transactions_disabled : NServiceBusIntegrationTest
    {
        [Test]
        public void Should_not_roll_the_message_back_to_the_queue_in_case_of_failure()
        {

            Scenario.Define<Context>()
                    .WithEndpoint<NonDTCEndpoint>(b => b.Given(bus => bus.SendLocal(new MyMessage())))
                    .Done(c => c.TestComplete)
                    .Repeat(r => r.For(Transports.Msmq))
                    .Should(c =>
                        {
                            Assert.AreEqual(1,c.TimesCalled,"Should not retry the message");
                        })
                    .Run();
        }

        public class Context : ScenarioContext
        {
            public bool TestComplete { get; set; }

            public int TimesCalled { get; set; }
        }

        public class NonDTCEndpoint : EndpointConfigurationBuilder
        {
            public NonDTCEndpoint()
            {
                EndpointSetup<DefaultServer>(c => Configure.Transactions.Disable());
            }

            public class MyMessageHandler : IHandleMessages<MyMessage>
            {
                public Context Context { get; set; }

                public IBus Bus { get; set; }
                public void Handle(MyMessage message)
                {
                    Context.TimesCalled++;
                    Bus.SendLocal(new CompleteTest());

                    throw new Exception("Simulated exception");
                }
            }

            public class CompleteTestHandler : IHandleMessages<CompleteTest>
            {
                public Context Context { get; set; }

                public void Handle(CompleteTest message)
                {
                    Context.TestComplete = true;
                }
            }
        }

        [Serializable]
        public class MyMessage : ICommand
        {
        }

        [Serializable]
        public class CompleteTest : ICommand
        {
        }


    }
}