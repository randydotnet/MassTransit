// Copyright 2007-2014 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Transports
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Pipeline;


    /// <summary>
    /// A receive endpoint is called by the receive transport to push messages to consumers.
    /// The receive endpoint is where the initial deserialization occurs, as well as any additional
    /// filters on the receive context. 
    /// </summary>
    public class ReceiveEndpoint :
        IReceiveEndpoint
    {
        readonly IConsumePipe _consumePipe;
        readonly IPipe<ReceiveContext> _receivePipe;
        readonly IReceiveTransport _receiveTransport;

        public ReceiveEndpoint(IReceiveTransport receiveTransport, IPipe<ReceiveContext> receivePipe, IConsumePipe consumePipe)
        {
            _receiveTransport = receiveTransport;
            _receivePipe = receivePipe;
            _consumePipe = consumePipe;
        }

        Uri IReceiveEndpoint.InputAddress
        {
            get { return _receiveTransport.InputAddress; }
        }

        IConsumePipe IReceiveEndpoint.ConsumePipe
        {
            get { return _consumePipe; }
        }

        ReceiveEndpointHandle IReceiveEndpoint.Start()
        {
            ReceiveTransportHandle transportHandle = _receiveTransport.Start(_receivePipe);

            return new Handle(this, transportHandle);
        }


        class Handle :
            ReceiveEndpointHandle
        {
            readonly IReceiveEndpoint _endpoint;
            readonly CancellationTokenSource _stop;
            readonly ReceiveTransportHandle _transportHandle;

            public Handle(IReceiveEndpoint endpoint, ReceiveTransportHandle transportHandle)
            {
                _endpoint = endpoint;
                _transportHandle = transportHandle;
                _stop = new CancellationTokenSource();
            }

            IReceiveEndpoint ReceiveEndpointHandle.Endpoint
            {
                get { return _endpoint; }
            }

            void IDisposable.Dispose()
            {
                _stop.Cancel();

                _transportHandle.Dispose();
            }

            async Task ReceiveEndpointHandle.Stop(CancellationToken cancellationToken)
            {
                _stop.Cancel();

                await _transportHandle.Stop(cancellationToken);
            }
        }
    }
}