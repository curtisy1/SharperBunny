namespace SharperBunny.Serializer {
  using System;
  using System.Text.Json;

  public class Serializable<TResponse> {
    public static string ContentEncoding => "utf-8";
    public static string ContentType => "application/json";

    protected virtual byte[] InternalSerialize<TRequest>(TRequest msg) {
      return JsonSerializer.SerializeToUtf8Bytes(msg);
    }

    protected virtual TResponse InternalDeserialize(ReadOnlyMemory<byte> arg) {
      return JsonSerializer.Deserialize<TResponse>(arg.Span);
    }
  }
}