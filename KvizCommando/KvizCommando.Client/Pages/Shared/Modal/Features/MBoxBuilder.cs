using KvizCommando.Client.Pages.Shared.Modal.ViewModels;
using KvizCommando.Client.Services.Visual.UiService.Language;

namespace KvizCommando.Client.Pages.Shared.Modal.Features
{
    public static class MBoxBuilder
    {
        private const string BUTTON_STYLE = "background-color: ";
        public static ModalBoxVm BuildParam(ModalTypes type, ILanguageService lang)
        {
            if ((int)type == 0)
                return new ModalBoxVm();

            var specs = ModalBox.Specs[type];

            return new ModalBoxVm
            {
                Mode = type,
                Title = lang[specs.TitleKey],
                ActionText1 = lang[specs.TextKey1],
                ActionText2 = lang[specs.TextKey2],
                ActionStyle1 = string.IsNullOrEmpty(specs.Style1) ? string.Empty : $"{BUTTON_STYLE}{specs.Style1}",
                ActionStyle2 = string.IsNullOrEmpty(specs.Style2) ? string.Empty : $"{BUTTON_STYLE}{specs.Style2}",
                CloseText = lang[specs.CloseTextKey],
                Size = $"modal-{specs.Size}",
                CheckBoxText = lang[specs.CheckBoxTextKey],
                CheckBoxKey = specs.CheckBoxKey,
                CheckBottom = specs.CheckBottom,
                BodyComponent = specs.BodyComponent
            };
        }
    }
}
