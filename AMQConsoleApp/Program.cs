using Apache.NMS;
using Microsoft.Extensions.Configuration;
class Program
{
    static void Main(string[] args)
    {

        var builder = new ConfigurationBuilder();
        builder.SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        IConfiguration config = builder.Build();

        string? brokerUri = config.GetSection("ActiveMQ").GetSection("BrokerUri").Value;
        string? requestQueueName = config.GetSection("ActiveMQ").GetSection("RequestQueueName").Value;
        Dictionary<string, string>? requestFilePath = config.GetSection("Path:RequestFilePath").Get<Dictionary<string, string>>();

        string? xmlResponseContent = string.Empty;

        // Loop through the ResponseFilePath entries
        foreach (var keyValuePair in requestFilePath)
        {
            string fileType = keyValuePair.Key;
            string filePath = keyValuePair.Value.ToString();
            xmlResponseContent = System.IO.File.ReadAllText(filePath);
            SendMessage(brokerUri, requestQueueName, xmlResponseContent);
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static void SendMessage(string brokerUri, string queueName, string message)
    {
        Apache.NMS.IConnectionFactory factory = new NMSConnectionFactory(brokerUri);

        using (IConnection connection = factory.CreateConnection())
        {
            connection.Start();

            using (ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
            {
                IDestination destination = session.GetQueue(queueName);
                using (IMessageProducer producer = session.CreateProducer(destination))
                {
                    ITextMessage textMessage = producer.CreateTextMessage(message);
                    producer.Send(textMessage);
                    Console.WriteLine("Message sent: " + textMessage.Text);
                }
            }

            connection.Close();
        }
    }

}
