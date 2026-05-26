namespace Gudel.GLogWare.Services.Garbage
{
    public partial class Garbage : IHostedService, IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
