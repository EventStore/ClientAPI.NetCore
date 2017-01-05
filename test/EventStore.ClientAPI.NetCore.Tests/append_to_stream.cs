using System;
using System.Linq;
using Eventstore.ClientAPI.Tests.Helpers;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using NUnit.Framework;

namespace Eventstore.ClientAPI.Tests
{
    [TestFixture]
    public class append_to_stream 
    {
        private readonly TcpType _tcpType = TcpType.Normal;
        

        protected virtual IEventStoreConnection BuildConnection()
        {
            return TestConnection.Create(_tcpType);
        }

        [Test, Category("Network")]
        public void should_allow_appending_zero_events_to_stream_with_no_problems()
        {
            const string stream1 = "should_allow_appending_zero_events_to_stream_with_no_problems1";
            const string stream2 = "should_allow_appending_zero_events_to_stream_with_no_problems2";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();

                Assert.AreEqual(-1, store.AppendToStreamAsync(stream1, ExpectedVersion.Any).Result.NextExpectedVersion);
                Assert.AreEqual(-1, store.AppendToStreamAsync(stream1, ExpectedVersion.NoStream).Result.NextExpectedVersion);
                Assert.AreEqual(-1, store.AppendToStreamAsync(stream1, ExpectedVersion.Any).Result.NextExpectedVersion);
                Assert.AreEqual(-1, store.AppendToStreamAsync(stream1, ExpectedVersion.NoStream).Result.NextExpectedVersion);

                var read1 = store.ReadStreamEventsForwardAsync(stream1, 0, 2, resolveLinkTos: false).Result;
                Assert.That(read1.Events.Length, Is.EqualTo(0));

                Assert.AreEqual(-1, store.AppendToStreamAsync(stream2, ExpectedVersion.NoStream).Result.NextExpectedVersion);
                Assert.AreEqual(-1, store.AppendToStreamAsync(stream2, ExpectedVersion.Any).Result.NextExpectedVersion);
                Assert.AreEqual(-1, store.AppendToStreamAsync(stream2, ExpectedVersion.NoStream).Result.NextExpectedVersion);
                Assert.AreEqual(-1, store.AppendToStreamAsync(stream2, ExpectedVersion.Any).Result.NextExpectedVersion);

                var read2 = store.ReadStreamEventsForwardAsync(stream2, 0, 2, resolveLinkTos: false).Result;
                Assert.That(read2.Events.Length, Is.EqualTo(0));
            }
        }

        [Test, Category("Network")]
        public void should_create_stream_with_no_stream_exp_ver_on_first_write_if_does_not_exist()
        {
            const string stream = "should_create_stream_with_no_stream_exp_ver_on_first_write_if_does_not_exist";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();

                Assert.AreEqual(0, store.AppendToStreamAsync(stream, ExpectedVersion.NoStream, TestEvent.NewTestEvent()).Result.NextExpectedVersion);

                var read = store.ReadStreamEventsForwardAsync(stream, 0, 2, resolveLinkTos: false);
                Assert.DoesNotThrow(read.Wait);
                Assert.That(read.Result.Events.Length, Is.EqualTo(1));
            }
        }

        [Test]
        [Category("Network")]
        public void should_create_stream_with_any_exp_ver_on_first_write_if_does_not_exist()
        {
            const string stream = "should_create_stream_with_any_exp_ver_on_first_write_if_does_not_exist";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();
                Assert.AreEqual(0, store.AppendToStreamAsync(stream, ExpectedVersion.Any, TestEvent.NewTestEvent()).Result.NextExpectedVersion);

                var read = store.ReadStreamEventsForwardAsync(stream, 0, 2, resolveLinkTos: false);
                Assert.DoesNotThrow(read.Wait);
                Assert.That(read.Result.Events.Length, Is.EqualTo(1));
            }
        }

        [Test]
        [Category("Network")]
        public void multiple_idempotent_writes()
        {
            const string stream = "multiple_idempotent_writes";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();

                var events = new[] { TestEvent.NewTestEvent(), TestEvent.NewTestEvent(), TestEvent.NewTestEvent(), TestEvent.NewTestEvent() };
                Assert.AreEqual(3, store.AppendToStreamAsync(stream, ExpectedVersion.Any, events).Result.NextExpectedVersion);
                Assert.AreEqual(3, store.AppendToStreamAsync(stream, ExpectedVersion.Any, events).Result.NextExpectedVersion);
            }
        }

        [Test]
        [Category("Network")]
        public void multiple_idempotent_writes_with_same_id_bug_case()
        {
            const string stream = "multiple_idempotent_writes_with_same_id_bug_case";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();
                var x = TestEvent.NewTestEvent();
                var events = new[] { x,x,x,x,x,x};
                Assert.AreEqual(5,store.AppendToStreamAsync(stream, ExpectedVersion.Any, events).Result.NextExpectedVersion);
            }
        }

        [Test]
        [Category("Network")]
        public void in_wtf_multiple_case_of_multiple_writes_expected_version_any_per_all_same_id()
        {
            const string stream = "in_wtf_multiple_case_of_multiple_writes_expected_version_any_per_all_same_id";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();
                var x = TestEvent.NewTestEvent();
                var events = new[] { x, x, x, x, x, x };
                Assert.AreEqual(5, store.AppendToStreamAsync(stream, ExpectedVersion.Any, events).Result.NextExpectedVersion);
                var f = store.AppendToStreamAsync(stream, ExpectedVersion.Any, events).Result;
                Assert.AreEqual(0, f.NextExpectedVersion);
            }
        }

        [Test]
        [Category("Network")]
        public void in_slightly_reasonable_multiple_case_of_multiple_writes_with_expected_version_per_all_same_id()
        {
            const string stream = "in_slightly_reasonable_multiple_case_of_multiple_writes_with_expected_version_per_all_same_id";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();
                var x = TestEvent.NewTestEvent();
                var events = new[] { x, x, x, x, x, x };
                Assert.AreEqual(5, store.AppendToStreamAsync(stream, ExpectedVersion.EmptyStream, events).Result.NextExpectedVersion);
                var f = store.AppendToStreamAsync(stream, ExpectedVersion.EmptyStream, events).Result;
                Assert.AreEqual(5, f.NextExpectedVersion);
            }
        }

        [Test]
        [Category("Network")]
        public void should_fail_writing_with_correct_exp_ver_to_deleted_stream()
        {
            const string stream = "should_fail_writing_with_correct_exp_ver_to_deleted_stream";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();

                var delete = store.DeleteStreamAsync(stream, ExpectedVersion.EmptyStream, hardDelete: true);
                Assert.DoesNotThrow(delete.Wait);

                var append = store.AppendToStreamAsync(stream, ExpectedVersion.NoStream, new[] { TestEvent.NewTestEvent() });
                Assert.That(() => append.Wait(), Throws.Exception.TypeOf<AggregateException>().With.InnerException.TypeOf<StreamDeletedException>());
            }
        }

        [Test]
        [Category("Network")]
        public void should_return_log_position_when_writing()
        {
            const string stream = "should_return_log_position_when_writing";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();
                var result = store.AppendToStreamAsync(stream, ExpectedVersion.EmptyStream, TestEvent.NewTestEvent()).Result;
                Assert.IsTrue(0 < result.LogPosition.PreparePosition);
                Assert.IsTrue(0 < result.LogPosition.CommitPosition);
            }
        }

        [Test]
        [Category("Network")]
        public void should_fail_writing_with_any_exp_ver_to_deleted_stream()
        {
            const string stream = "should_fail_writing_with_any_exp_ver_to_deleted_stream";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();

                try
                {
                    store.DeleteStreamAsync(stream, ExpectedVersion.EmptyStream, hardDelete: true).Wait();
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc);
                    Assert.Fail();
                }

                var append = store.AppendToStreamAsync(stream, ExpectedVersion.Any, new[] { TestEvent.NewTestEvent() });
                Assert.That(() => append.Wait(), Throws.Exception.TypeOf<AggregateException>().With.InnerException.TypeOf<StreamDeletedException>());
            }
        }

        [Test]
        [Category("Network")]
        public void should_fail_writing_with_invalid_exp_ver_to_deleted_stream()
        {
            const string stream = "should_fail_writing_with_invalid_exp_ver_to_deleted_stream";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();

                var delete = store.DeleteStreamAsync(stream, ExpectedVersion.EmptyStream, hardDelete: true);
                Assert.DoesNotThrow(delete.Wait);

                var append = store.AppendToStreamAsync(stream, 5, new[] { TestEvent.NewTestEvent() });
                Assert.That(() => append.Wait(), Throws.Exception.TypeOf<AggregateException>().With.InnerException.TypeOf<StreamDeletedException>());
            }
        }

        [Test]
        [Category("Network")]
        public void should_append_with_correct_exp_ver_to_existing_stream()
        {
            const string stream = "should_append_with_correct_exp_ver_to_existing_stream";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();
                store.AppendToStreamAsync(stream, ExpectedVersion.EmptyStream, TestEvent.NewTestEvent()).Wait();

                var append = store.AppendToStreamAsync(stream, 0, new[] { TestEvent.NewTestEvent() });
                Assert.DoesNotThrow(append.Wait);
            }
        }

        [Test]
        [Category("Network")]
        public void should_append_with_any_exp_ver_to_existing_stream()
        {
            const string stream = "should_append_with_any_exp_ver_to_existing_stream";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();
                Assert.AreEqual(0, store.AppendToStreamAsync(stream, ExpectedVersion.EmptyStream, TestEvent.NewTestEvent()).Result.NextExpectedVersion);
                Assert.AreEqual(1, store.AppendToStreamAsync(stream, ExpectedVersion.Any, TestEvent.NewTestEvent()).Result.NextExpectedVersion);
            }
        }

        [Test]
        [Category("Network")]
        public void should_fail_appending_with_wrong_exp_ver_to_existing_stream()
        {
            const string stream = "should_fail_appending_with_wrong_exp_ver_to_existing_stream";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();
 
                var append = store.AppendToStreamAsync(stream, 1, new[] { TestEvent.NewTestEvent() });
                Assert.That(() => append.Wait(), Throws.Exception.TypeOf<AggregateException>().With.InnerException.TypeOf<WrongExpectedVersionException>());
            }
        }

        [Test]
        [Category("Network")]
        public void should_append_with_stream_exists_exp_ver_to_existing_stream()
        {
            const string stream = "should_append_with_stream_exists_exp_ver_to_existing_stream";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();
                store.AppendToStreamAsync(stream, ExpectedVersion.EmptyStream, TestEvent.NewTestEvent()).Wait();
 
                var append = store.AppendToStreamAsync(stream, ExpectedVersion.StreamExists, new[] { TestEvent.NewTestEvent() });
                Assert.DoesNotThrow(append.Wait);
            }
        }

        [Test]
        [Category("Network")]
        public void should_append_with_stream_exists_exp_ver_to_stream_with_multiple_events()
        {
            const string stream = "should_append_with_stream_exists_exp_ver_to_stream_with_multiple_events";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();
                for(var i = 0; i < 5; i++) {
                    store.AppendToStreamAsync(stream, ExpectedVersion.Any, TestEvent.NewTestEvent()).Wait();
                }
 
                var append = store.AppendToStreamAsync(stream, ExpectedVersion.StreamExists, new[] { TestEvent.NewTestEvent() });
                Assert.DoesNotThrow(append.Wait);
            }
        }

        [Test]
        [Category("Network")]
        public void should_append_with_stream_exists_exp_ver_if_metadata_stream_exists()
        {
            const string stream = "should_append_with_stream_exists_exp_ver_if_metadata_stream_exists";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();
                store.SetStreamMetadataAsync(stream, ExpectedVersion.Any, new StreamMetadata(10, null, null, null, null)).Wait();
 
                var append = store.AppendToStreamAsync(stream, ExpectedVersion.StreamExists, new[] { TestEvent.NewTestEvent() });
                Assert.DoesNotThrow(append.Wait);
            }
        }

        [Test]
        [Category("Network")]
        public void should_fail_appending_with_stream_exists_exp_ver_and_stream_does_not_exist()
        {
            const string stream = "should_fail_appending_with_stream_exists_exp_ver_and_stream_does_not_exist";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();
 
                var append = store.AppendToStreamAsync(stream, ExpectedVersion.StreamExists, new[] { TestEvent.NewTestEvent() });
                Assert.That(() => append.Wait(), Throws.Exception.TypeOf<AggregateException>().With.InnerException.TypeOf<WrongExpectedVersionException>());
            }
        }

        [Test]
        [Category("Network")]
        public void should_fail_appending_with_stream_exists_exp_ver_to_hard_deleted_stream()
        {
            const string stream = "should_fail_appending_with_stream_exists_exp_ver_to_hard_deleted_stream";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();

                var delete = store.DeleteStreamAsync(stream, ExpectedVersion.EmptyStream, hardDelete: true);
                Assert.DoesNotThrow(delete.Wait);

                var append = store.AppendToStreamAsync(stream, ExpectedVersion.StreamExists, new[] { TestEvent.NewTestEvent() });
                Assert.That(() => append.Wait(), Throws.Exception.TypeOf<AggregateException>().With.InnerException.TypeOf<StreamDeletedException>());
            }
        }

        [Test]
        [Category("Network")]
        public void should_fail_appending_with_stream_exists_exp_ver_to_soft_deleted_stream()
        {
            const string stream = "should_fail_appending_with_stream_exists_exp_ver_to_soft_deleted_stream";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();

                var delete = store.DeleteStreamAsync(stream, ExpectedVersion.EmptyStream, hardDelete: false);
                Assert.DoesNotThrow(delete.Wait);

                var append = store.AppendToStreamAsync(stream, ExpectedVersion.StreamExists, new[] { TestEvent.NewTestEvent() });
                Assert.That(() => append.Wait(), Throws.Exception.TypeOf<AggregateException>().With.InnerException.TypeOf<StreamDeletedException>());
            }
        }

        [Test, Category("Network")]
        public void can_append_multiple_events_at_once()
        {
            const string stream = "can_append_multiple_events_at_once";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();

                var events = Enumerable.Range(0, 100).Select(i => TestEvent.NewTestEvent(i.ToString(), i.ToString()));
                Assert.AreEqual(99, store.AppendToStreamAsync(stream, ExpectedVersion.EmptyStream, events).Result.NextExpectedVersion);
            }
        }

        [Test, Category("Network")]
        public void returns_failure_status_when_conditionally_appending_with_version_mismatch()
        {
            const string stream = "returns_failure_status_when_conditionally_appending_with_version_mismatch";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();

                var result = store.ConditionalAppendToStreamAsync(stream, 7, new[] {TestEvent.NewTestEvent()}).Result;

                Assert.AreEqual(ConditionalWriteStatus.VersionMismatch, result.Status);
            }
        }

        [Test, Category("Network")]
        public void returns_success_status_when_conditionally_appending_with_matching_version()
        {
            const string stream = "returns_success_status_when_conditionally_appending_with_matching_version";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();

                var result = store.ConditionalAppendToStreamAsync(stream, ExpectedVersion.Any, new[] { TestEvent.NewTestEvent() }).Result;

                Assert.AreEqual(ConditionalWriteStatus.Succeeded, result.Status);
                Assert.IsNotNull(result.LogPosition);
                Assert.IsNotNull(result.NextExpectedVersion);
            }
        }

        [Test, Category("Network")]
        public void returns_failure_status_when_conditionally_appending_to_a_deleted_stream()
        {
            const string stream = "returns_failure_status_when_conditionally_appending_to_a_deleted_stream";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();

                store.AppendToStreamAsync(stream, ExpectedVersion.Any, TestEvent.NewTestEvent()).Wait();
                store.DeleteStreamAsync(stream, ExpectedVersion.Any, true).Wait();

                var result = store.ConditionalAppendToStreamAsync(stream, ExpectedVersion.Any, new[] { TestEvent.NewTestEvent() }).Result;

                Assert.AreEqual(ConditionalWriteStatus.StreamDeleted, result.Status);
            }
        }
    }
}
