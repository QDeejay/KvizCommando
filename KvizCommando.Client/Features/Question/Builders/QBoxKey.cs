using KvizCommando.Client.Features.Home.Builders;

namespace KvizCommando.Client.Features.Question.Builders;

internal enum QBoxKeyRoot
{
    Factory = (int)HomeBoxKey.Question * 100 + 1,
    Usr = (int)HomeBoxKey.Question * 100 + 2,
    Pending = (int)HomeBoxKey.Question * 100 + 3,
    New = (int)HomeBoxKey.Question * 100 + 4
}

internal enum QBoxKeyContent
{
    FactSlots,
    UsrSlots,
    PendigSlots,
    NewSlot
}
