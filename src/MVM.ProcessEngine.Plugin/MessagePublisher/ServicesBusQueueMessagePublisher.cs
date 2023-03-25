using Microsoft.Azure.ServiceBus;
using MVM.ProcessEngine.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Plugin
{
    public class ServicesBusQueueMessagePublisher : IMessagePublisher
    {
        public Guid ConversationHandleId { get; set; }

        public async Task PublishAsync(string tenant, object message)
        {
            var connectionString = Environment.GetEnvironmentVariable("ServicesBusConnectionString", EnvironmentVariableTarget.Process);
            var queueName = Environment.GetEnvironmentVariable("ServicesBusQueueName", EnvironmentVariableTarget.Process);
            var queueClient = new QueueClient(connectionString, queueName);
            var messageToSend = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)));
            System.Threading.Thread.Sleep(2000);
            await queueClient.SendAsync(messageToSend);

        }

        public void Publish(string tenant, object message)
        {
            var connectionString = Environment.GetEnvironmentVariable("ServicesBusConnectionString");
            var queueName = Environment.GetEnvironmentVariable("ServicesBusQueueName");
            var queueClient = new QueueClient(connectionString, queueName);
            var messageToSend = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)));
            System.Threading.Thread.Sleep(2000);
            queueClient.SendAsync(messageToSend);
        }
    }
}
