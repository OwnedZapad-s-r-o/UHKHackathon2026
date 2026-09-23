namespace SmartAudit.Api.Services
{
    // Sdílený stav startu — simulace čeká, až import doběhne, /health ukazuje fázi
    public interface IPipelineStateService
    {
        string Phase { get; }
        bool IsReady { get; }
        void SetPhase(string phase);
        void MarkReady();
        Task WaitUntilReadyAsync(CancellationToken ct);
    }
    public class PipelineStateService : IPipelineStateService
    {
        private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public string Phase { get; private set; } = "Spouštění";
        public bool IsReady => _ready.Task.IsCompleted;

        public void SetPhase(string phase)
        {
            Phase = phase;
        }

        public void MarkReady()
        {
            Phase = "Připraveno";
            _ready.TrySetResult();
        }

        public Task WaitUntilReadyAsync(CancellationToken ct)
        {
            return _ready.Task.WaitAsync(ct);
        }
    }
}
