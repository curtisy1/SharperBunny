namespace SharperBunny.Interfaces {
  using System;
  using RabbitMQ.Client;

  public interface IPermanentChannel : IDisposable {
    public IModel Channel { get; }

    public void StartConfirmMode();
  }
}