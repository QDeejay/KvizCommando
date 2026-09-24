using KvizCommando.Client.Features.Home.Builders;

namespace KvizCommando.Client.Features.Rankings.Builders;

internal enum RankingBoxKey
{
    Solo = (int)HomeBoxKey.Rankings * 100 + 1,
    Vs = (int)HomeBoxKey.Rankings * 100 + 2,
    Memorial = (int)HomeBoxKey.Rankings * 100 + 3,
    SoloList = (int)HomeBoxKey.Rankings * 100 + 4,
    VsList = (int)HomeBoxKey.Rankings * 100 + 5
}
