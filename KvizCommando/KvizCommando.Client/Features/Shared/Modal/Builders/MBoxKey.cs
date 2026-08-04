namespace KvizCommando.Client.Features.Shared.Modal.Builders
{
    public static class ModalConst
    {
        public const string LOCAL_NOT_SHOW_NEW = "notShowNew";
        public const string LOCAL_NOT_SHOW_DEL = "notShowDel";
    }
    public enum ModalTypes
    {
        None = 0,
        Terms = 1,
        LangConfirm = 2,
        DialogConfirm = 3,
        QUsrDelet = 101,
        QPendHandle = 102,
        QNewRules = 103,
        QCheckQuestion = 104,
        THire = 201,
        TPromoteMember = 202,
        TRetire = 203,
        THandle = 204,
        TPromoteTeam = 205
    }

}
