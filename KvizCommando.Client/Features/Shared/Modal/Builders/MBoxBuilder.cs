using KvizCommando.Client.Features.Shared.Modal.ViewModels;

namespace KvizCommando.Client.Features.Shared.Modal.Builders
{
    public static class MBoxBuilder
    {
        private const string BUTTON_STYLE = "background-color: ";
        /// <summary>
        /// Összeállítja a megadott típushoz tartozó modális ablak paramétereit.
        /// </summary>
        /// <param name="type">Az üzenet vagy megjelenítés típusa.</param>
        public static ModalBoxVm BuildParam(ModalTypes type)
        {
            if ((int)type == 0)
                return new ModalBoxVm();

            var specs = ModalBox.Specs[type];

            return new ModalBoxVm
            {
                Mode = type,
                TitleKey = specs.TitleKey,
                ActionTextKey1 = specs.TextKey1,
                ActionTextKey2 = specs.TextKey2,
                ActionStyle1 = string.IsNullOrEmpty(specs.Style1) ? string.Empty : $"{BUTTON_STYLE}{specs.Style1}",
                ActionStyle2 = string.IsNullOrEmpty(specs.Style2) ? string.Empty : $"{BUTTON_STYLE}{specs.Style2}",
                CloseTextKey = specs.CloseTextKey,
                Size = specs.Size,
                SizeLock = specs.SizeLock,
                CheckBoxTextKey = specs.CheckBoxTextKey,
                CheckBoxKey = specs.CheckBoxKey,
                CheckBottom = specs.CheckBottom,
                BodyComponent = specs.BodyComponent
            };
        }
    }
}
