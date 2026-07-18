using KvizCommando.Client.Services.Dto;
using KvizCommando.Shared.Models.Dtos;
using System.Threading;

namespace KvizCommando.Client.Services.ClientCache
{
    public sealed class QuestionState : IQuestionState
    {
        private readonly ICacheApiService _api;

        private QuestionDtos? _snapshot;
        private bool _dirty = true;
        private readonly SemaphoreSlim _gate = new(1, 1);

        public QuestionState(ICacheApiService api) => _api = api;

        public bool IsLoaded => _snapshot is not null && !_dirty;

        public QuestionDtos? Snapshot => _snapshot;
        public int[]? FactorySlots => _snapshot?.FactorySlots;
        public UserSlot[]? Userlots => _snapshot?.Userlots;
        public PendingSlot[]? PendingSlots => _snapshot?.PendingSlots;
        public QuestionExtendedInfo? ExtendedInfo => _snapshot?.ExtendedInfo;
        //public QuestionButtons? QuestionButtons => _snapshot?.QuestionButtons;

        public async Task EnsureLoadedAsync()
        {
            if (IsLoaded) return;
            await _gate.WaitAsync();
            try
            {
                if (IsLoaded) return; // double-check
                _snapshot = await _api.GetQuestionAsync();
                _dirty = false;
            }
            finally { _gate.Release(); }
        }

        public async Task RefreshAsync()
        {
            await _gate.WaitAsync();
            try
            {
                _snapshot = await _api.GetQuestionAsync();
                _dirty = false;
            }
            finally { _gate.Release(); }
        }

        public void Invalidate() => _dirty = true;

        public void Clear()
        {
            _snapshot = null;
            _dirty = true;
        }
    }
}
