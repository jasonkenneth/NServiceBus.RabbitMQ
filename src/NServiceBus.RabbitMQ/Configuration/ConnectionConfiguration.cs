﻿namespace NServiceBus.Transport.RabbitMQ
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Logging;
    using Support;

    class ConnectionConfiguration
    {
        static readonly ILog Logger = LogManager.GetLogger(typeof(ConnectionConfiguration));

        public string Host { get; set; }

        public int Port { get; set; }

        public string VirtualHost { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public ushort RequestedHeartbeat { get; set; }

        public TimeSpan RetryDelay { get; set; }

        public bool UseTls { get; set; }

        public string CertPath { get; set; }

        public string CertPassphrase { get; set; }

        public Dictionary<string, object> ClientProperties { get; } = new Dictionary<string, object>();

        ConnectionConfiguration(string endpointName)
        {
            // set default values
            VirtualHost = "/";
            UserName = "guest";
            Password = "guest";
            CertPath = "";
            CertPassphrase = null;

            SetDefaultClientProperties(endpointName);
        }

        public static ConnectionConfiguration Create(string connectionString, string endpointName)
        {
            var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };

            var invalidOptionsMessage = new StringBuilder();

            object value;

            var useTls = false;
            if (builder.TryGetValue("useTls", out value))
            {
                if (!bool.TryParse(value.ToString(), out useTls))
                {
                    invalidOptionsMessage.AppendLine($"'{value}' is not a valid Boolean value for the 'useTls' connection string option.");
                }
            }

            var port = useTls ? 5671 : 5672;
            if (builder.TryGetValue("port", out value))
            {
                if (!int.TryParse(value.ToString(), out port))
                {
                    invalidOptionsMessage.AppendLine($"'{value}' is not a valid Int32 value for the 'port' connection string option.");
                }
            }

            var host = default(string);
            if (builder.TryGetValue("host", out value))
            {
                var hostsAndPorts = value.ToString().Split(',');

                if (hostsAndPorts.Length > 1)
                {
                    invalidOptionsMessage.AppendLine("Multiple hosts are no longer supported. If using RabbitMQ in a cluster, consider using a load balancer to represent the nodes as a single host.");
                }

                var parts = hostsAndPorts[0].Split(':');
                host = parts.ElementAt(0);

                if (host.Length == 0)
                {
                    invalidOptionsMessage.AppendLine("Empty host name in 'host' connection string option.");
                }

                if (parts.Length > 1 && !int.TryParse(parts[1], out port))
                {
                    invalidOptionsMessage.AppendLine($"'{parts[1]}' is not a valid Int32 value for the port in the 'host' connection string option.");
                }
            }
            else
            {
                invalidOptionsMessage.AppendLine("Invalid connection string. 'host' value must be supplied. e.g: \"host=myServer\"");
            }

            ushort requestedHeartbeat = 5;
            if (builder.TryGetValue("requestedHeartbeat", out value))
            {
                if (!ushort.TryParse(value.ToString(), out requestedHeartbeat))
                {
                    invalidOptionsMessage.AppendLine($"'{value}' is not a valid UInt16 value for the 'requestedHeartbeat' connection string option.");
                }
            }

            var retryDelay = TimeSpan.FromSeconds(10);
            if (builder.TryGetValue("retryDelay", out value))
            {
                if (!TimeSpan.TryParse(value.ToString(), out retryDelay))
                {
                    invalidOptionsMessage.AppendLine($"'{value}' is not a valid TimeSpan value for the 'retryDelay' connection string option.");
                }
            }

            if (builder.ContainsKey("dequeuetimeout"))
            {
                invalidOptionsMessage.AppendLine("The 'DequeueTimeout' connection string option has been removed. Consult the documentation for further information.");
            }

            if (builder.ContainsKey("maxwaittimeforconfirms"))
            {
                invalidOptionsMessage.AppendLine("The 'MaxWaitTimeForConfirms' connection string option has been removed. Consult the documentation for further information");
            }

            if (builder.ContainsKey("prefetchcount"))
            {
                invalidOptionsMessage.AppendLine("The 'PrefetchCount' connection string option has been removed. Use 'EndpointConfiguration.UseTransport<RabbitMQTransport>().PrefetchCount' instead.");
            }

            if (builder.ContainsKey("usepublisherconfirms"))
            {
                invalidOptionsMessage.AppendLine("The 'UsePublisherConfirms' connection string option has been removed. Use 'EndpointConfiguration.UseTransport<RabbitMQTransport>().UsePublisherConfirms' instead.");
            }

            if (invalidOptionsMessage.Length > 0)
            {
                var message = invalidOptionsMessage.ToString().TrimEnd('\r', '\n');

                Logger.Error(message);

                throw new NotSupportedException(message);
            }

            var connectionConfiguration = new ConnectionConfiguration(endpointName);
            var connectionConfigurationType = typeof(ConnectionConfiguration);
            foreach (var key in builder.Keys.Cast<string>())
            {
                var property = connectionConfigurationType.GetProperty(key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                property?.SetValue(connectionConfiguration, TypeDescriptor.GetConverter(property.PropertyType).ConvertFrom(builder[key]));
            }

            connectionConfiguration.UseTls = useTls;
            connectionConfiguration.Port = port;
            connectionConfiguration.Host = host;
            connectionConfiguration.RequestedHeartbeat = requestedHeartbeat;
            connectionConfiguration.RetryDelay = retryDelay;

            return connectionConfiguration;
        }

        void SetDefaultClientProperties(string endpointName)
        {
            var nsb = typeof(Endpoint).Assembly.Location;
            var nsbVersion = FileVersionInfo.GetVersionInfo(nsb);
            var nsbFileVersion = $"{nsbVersion.FileMajorPart}.{nsbVersion.FileMinorPart}.{nsbVersion.FileBuildPart}";

            var rabbitMQ = typeof(ConnectionConfiguration).Assembly.Location;
            var rabbitMQVersion = FileVersionInfo.GetVersionInfo(rabbitMQ);
            var rabbitMQFileVersion = $"{rabbitMQVersion.FileMajorPart}.{rabbitMQVersion.FileMinorPart}.{rabbitMQVersion.FileBuildPart}";

            var applicationNameAndPath = Environment.GetCommandLineArgs()[0];
            var applicationName = Path.GetFileName(applicationNameAndPath);
            var applicationPath = Path.GetDirectoryName(applicationNameAndPath);

            var hostname = RuntimeEnvironment.MachineName;

            ClientProperties.Add("client_api", "NServiceBus");
            ClientProperties.Add("nservicebus_version", nsbFileVersion);
            ClientProperties.Add("nservicebus.rabbitmq_version", rabbitMQFileVersion);
            ClientProperties.Add("application", applicationName);
            ClientProperties.Add("application_location", applicationPath);
            ClientProperties.Add("machine_name", hostname);
            ClientProperties.Add("user", UserName);
            ClientProperties.Add("endpoint_name", endpointName);
        }
    }
}
