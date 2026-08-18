using KvizCommando.Client.Services.Dto;
using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Services.ClientCache
{
    public class SoloState : ISoloState
    {
        private readonly ICacheApiService _api;

        private SoloGameDtos? _snapshot;
        private bool _dirty = true;
        private readonly SemaphoreSlim _gate = new(1, 1);
        public SoloState(ICacheApiService api) => _api = api;
        public bool IsLoaded => _snapshot is not null && !_dirty;

        public SoloGameDtos? Snapshot => _snapshot;
        public SoloEnables? Enables => _snapshot?.Enables;
        public SoloResults? REsults => _snapshot?.Results;


        /// <inheritdoc />
        public async Task EnsureLoadedAsync()
        {
            if (IsLoaded) return;
            await _gate.WaitAsync();
            try
            {
                if (IsLoaded) return; // double-check
                _snapshot = await _api.GetSoloAsync();
                _dirty = false;
            }
            finally { _gate.Release(); }
        }

        /// <inheritdoc />
        public async Task RefreshAsync()
        {
            await _gate.WaitAsync();
            try
            {
                _snapshot = await _api.GetSoloAsync();
                _dirty = false;
            }
            finally { _gate.Release(); }
        }
        /// <inheritdoc />
        public void Invalidate() => _dirty = true;
        /// <inheritdoc />
        public void Clear()
        {
            _snapshot = null;
            _dirty = true;
        }
    }

}
