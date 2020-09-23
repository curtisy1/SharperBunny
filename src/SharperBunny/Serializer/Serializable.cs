namespace SharperBunny.Serializer {
  using System;
  using System.Text.Json;

  public class Serializable<TResponse> {
    protected virtual byte[] InternalSerialize<TRequest>(TRequest msg) => JsonSerializer.SerializeToUtf8Bytes(msg);

    protected virtual TResponse InternalDeserialize(ReadOnlyMemory<byte> arg) => JsonSerializer.Deserialize<TResponse>(arg.Span);
  }
}