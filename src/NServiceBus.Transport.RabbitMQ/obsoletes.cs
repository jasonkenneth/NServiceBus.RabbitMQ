﻿#pragma warning disable 1591

namespace NServiceBus
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using Transport.RabbitMQ;

    public partial class RabbitMQTransportSettingsExtensions
    {
        [ObsoleteEx(RemoveInVersion = "7.0", TreatAsErrorFromVersion = "6.0", ReplacementTypeOrMember = "RabbitMQTransportSettingsExtensions.UseCustomRoutingTopology(TransportExtensions<RabbitMQTransport> transportExtensions, Func<bool, IRoutingTopology>)")]
        public static TransportExtensions<RabbitMQTransport> UseRoutingTopology(this TransportExtensions<RabbitMQTransport> transportExtensions, Func<bool, IRoutingTopology> topologyFactory)
        {
            throw new NotImplementedException();
        }

        [ObsoleteEx(RemoveInVersion = "7.0", TreatAsErrorFromVersion = "6.0", ReplacementTypeOrMember = "RabbitMQTransportSettingsExtensions.DisableDurableExchangesAndQueues(TransportExtensions<RabbitMQTransport> transportExtensions)")]
        public static TransportExtensions<RabbitMQTransport> UseDurableExchangesAndQueues(this TransportExtensions<RabbitMQTransport> transportExtensions, bool useDurableExchangesAndQueues)
        {
            throw new NotImplementedException();
        }

        [ObsoleteEx(RemoveInVersion = "7.0", TreatAsErrorFromVersion = "6.0", ReplacementTypeOrMember = "RabbitMQTransportSettingsExtensions.SetClientCertificate(TransportExtensions<RabbitMQTransport> transportExtensions, X509Certificate2 clientCertificate)")]
        public static TransportExtensions<RabbitMQTransport> SetClientCertificates(this TransportExtensions<RabbitMQTransport> transportExtensions, X509CertificateCollection clientCertificates)
        {
            throw new NotImplementedException();
        }

        [ObsoleteEx(RemoveInVersion = "7.0", TreatAsErrorFromVersion = "6.0", Message ="Publisher confirms are always enabled, so this method is no longer available.")]
        public static TransportExtensions<RabbitMQTransport> UsePublisherConfirms(this TransportExtensions<RabbitMQTransport> transportExtensions, bool usePublisherConfirms)
        {
            throw new NotImplementedException();
        }
    }
}

#pragma warning restore 1591