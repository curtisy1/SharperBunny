namespace SharperBunny.Configuration {
  using System;
  using System.Text.Json;

  public static class Config {
    public static string ContentEncoding => "utf-8";
    public static string ContentType => "application/json";

    public static byte[] Serialize<T>(T msg) => JsonSerializer.SerializeToUtf8Bytes(msg);

    internal static T Deserialize<T>(ReadOnlyMemory<byte> arg) => JsonSerializer.Deserialize<T>(arg.Span);
  }
}