using System.Threading.Tasks;
using RabbitMQ.Client;
using SharperBunny.Connect;

namespace SharperBunny.Consume {
    public class Carrot<TMsg> : ICarrot<TMsg> {
        private readonly PermanentChannel _thisChannel;

        public Carrot (TMsg message, ulong deliveryTag, PermanentChannel thisChannel) {
            Message = message;
            DeliveryTag = deliveryTag;
            _thisChannel = thisChannel;
        }

        public TMsg Message { get; }

        public ulong DeliveryTag { get; }

        public IBasicProperties MessageProperties { get; set; }

        public async Task<OperationResult<TMsg>> SendAckAsync (bool multiple = false) {
            var result = new OperationResult<TMsg> ();
            try {
                await Task.Run (() =>
                    _thisChannel.Channel.BasicAck (DeliveryTag, multiple : multiple)
                );
                result.IsSuccess = true;
                result.State = OperationState.Acked;
                return result;
            } catch (System.Exception ex) {
                result.Error = ex;
                result.IsSuccess = false;
                result.State = OperationState.Failed;
            }

            return result;
        }

        public async Task<OperationResult<TMsg>> SendNackAsync (bool multiple = false, bool withRequeue = true) {
            var result = new OperationResult<TMsg> ();
            try {
                await Task.Run (() =>
                    _thisChannel.Channel.BasicNack (DeliveryTag, multiple : multiple, requeue : withRequeue)
                );
                result.IsSuccess = true;
                result.State = OperationState.Nacked;

                return result;
            } catch (System.Exception ex) {
                result.IsSuccess = false;
                result.Error = ex;
                result.State = OperationState.Failed;
            }
            return result;
        }
    }
}