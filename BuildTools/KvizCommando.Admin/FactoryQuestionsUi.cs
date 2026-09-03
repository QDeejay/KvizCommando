using System.Globalization;
using Terminal.Gui;

namespace KvizCommando.Admin;

internal sealed partial class AdminMainWindow
{
    private void OpenFactoryQuestions()
    {
        var dialog = new Dialog("Factory questions", 142, 39);
        var categoryControls = new List<(int? CategoryNo, Label Marker, Button Button, Label Count)>();
        int? selectedCategory = null;

        var categoryLabel = new Label("Kategóriák") { X = 2, Y = 1 };
        var searchLabel = new Label("Keresés a kérdésben:") { X = 25, Y = 1 };
        var search = new TextField(string.Empty) { X = 47, Y = 1, Width = 43 };
        var reportedOnly = new CheckBox("Reported", false) { X = 94, Y = 1 };
        var playerQuestionsOnly = new CheckBox("From user", false) { X = 110, Y = 1 };
        var header = new Label("ID       Kat.  Player ID Kérdés                                                     Válaszok")
        {
            X = 25,
            Y = 3
        };
        var list = new ListView { X = 25, Y = 4, Width = Dim.Fill(2), Height = Dim.Fill(5) };
        IReadOnlyList<FactoryQuestionRow> rows = Array.Empty<FactoryQuestionRow>();

        void Refresh()
        {
            try
            {
                var counts = _database.GetFactoryQuestionCategoryCounts()
                    .ToDictionary(item => item.CategoryNo, item => item.Count);
                var total = counts.Values.Sum();
                foreach (var item in categoryControls)
                    item.Count.Text = $": {(item.CategoryNo.HasValue ? counts[item.CategoryNo.Value] : total)}";

                rows = _database.GetFactoryQuestions(
                    selectedCategory,
                    search.Text?.ToString(),
                    reportedOnly.Checked,
                    playerQuestionsOnly.Checked);
                list.SetSource(rows.Select(row => row.ToString()).ToList());
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        void SelectCategory(int? categoryNo)
        {
            selectedCategory = categoryNo;
            foreach (var item in categoryControls)
                item.Marker.Text = item.CategoryNo == selectedCategory ? ">" : " ";

            playerQuestionsOnly.Enabled = categoryNo != 99;
            if (categoryNo == 99)
                playerQuestionsOnly.Checked = false;
            Refresh();
        }

        AddCategoryControl(null, "Összes", 3);
        for (var category = 1; category <= 16; category++)
            AddCategoryControl(category, category.ToString(), category + 3);
        AddCategoryControl(99, "Tipp", 20);

        var refresh = new Button("_Frissítés") { X = 25, Y = Pos.Bottom(list) + 1 };
        var edit = new Button("_Szerkesztés") { X = Pos.Right(refresh) + 2, Y = Pos.Top(refresh) };
        var close = new Button("_Vissza") { X = Pos.Right(edit) + 2, Y = Pos.Top(refresh) };

        void OpenSelected()
        {
            if (rows.Count == 0 || list.SelectedItem < 0 || list.SelectedItem >= rows.Count)
                return;

            var row = rows[list.SelectedItem];
            if (row.IsTip)
                OpenTipQuestionEditor(row);
            else
                OpenFactoryQuestionEditor(row);
            Refresh();
        }

        refresh.Clicked += Refresh;
        edit.Clicked += OpenSelected;
        close.Clicked += () => Application.RequestStop();
        BindListAction(list, OpenSelected, 's');

        dialog.Add(categoryLabel, searchLabel, search, reportedOnly, playerQuestionsOnly, header, list, refresh, edit, close);
        foreach (var item in categoryControls)
            dialog.Add(item.Marker, item.Button, item.Count);

        Refresh();
        Application.Run(dialog);

        void AddCategoryControl(int? categoryNo, string label, int y)
        {
            var marker = new Label(categoryNo == selectedCategory ? ">" : " ") { X = 0, Y = y };
            var button = new Button(label) { X = 2, Y = y };
            var count = new Label(": 0") { X = 13, Y = y, Width = 11 };
            button.Clicked += () => SelectCategory(categoryNo);
            categoryControls.Add((categoryNo, marker, button, count));
        }
    }

    private void OpenFactoryQuestionEditor(FactoryQuestionRow row)
    {
        var answers = DeserializeAnswers(row.AnswerData);
        var dialog = CreateQuestionDialog(
            $"Factory #{row.Id}",
            row.CategoryNo,
            row.Question,
            answers,
            out var category,
            out var text,
            out var answerFields);
        dialog.Add(new Label($"Player ID: {row.PlayerId ?? 0}") { X = 2, Y = 20 });
        var reportedLabel = new Label("Reported:") { X = 2, Y = 22 };
        var reported = new TextField(row.Reported.ToString()) { X = 16, Y = 22, Width = 10 };
        var save = new Button("_Mentés") { X = 2, Y = 25 };
        var close = new Button("_Vissza") { X = Pos.Right(save) + 3, Y = 25 };

        save.Clicked += () =>
        {
            try
            {
                _database.UpdateFactoryQuestion(
                    row,
                    ParseCategory(category.Text?.ToString()),
                    text.Text?.ToString() ?? string.Empty,
                    answerFields.Select(field => field.Text?.ToString() ?? string.Empty).ToArray(),
                    ParseRequiredInt(reported.Text?.ToString(), "Reported"));
                MessageBox.Query("Kész", "Factory kérdés módosítva.", "OK");
                Application.RequestStop();
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        };
        close.Clicked += () => Application.RequestStop();
        dialog.Add(reportedLabel, reported, save, close);
        Application.Run(dialog);
    }

    private void OpenTipQuestionEditor(FactoryQuestionRow row)
    {
        var dialog = new Dialog($"Tipp #{row.Id}", 110, 24);
        dialog.Add(new Label("Kategória: 99 (Tipp)") { X = 2, Y = 1 });
        var questionLabel = new Label("Kérdés:") { X = 2, Y = 3 };
        var question = new TextView
        {
            X = 16,
            Y = 3,
            Width = 86,
            Height = 7,
            Text = row.Question,
            WordWrap = true
        };
        var answerLabel = new Label("Válasz:") { X = 2, Y = 12 };
        var answer = new TextField(row.AnswerData) { X = 16, Y = 12, Width = 30 };
        var reportedLabel = new Label("Reported:") { X = 2, Y = 14 };
        var reported = new TextField(row.Reported.ToString()) { X = 16, Y = 14, Width = 10 };
        var save = new Button("_Mentés") { X = 2, Y = 18 };
        var close = new Button("_Vissza") { X = Pos.Right(save) + 3, Y = 18 };

        save.Clicked += () =>
        {
            try
            {
                _database.UpdateTipQuestion(
                    row,
                    question.Text?.ToString() ?? string.Empty,
                    ParseRequiredDouble(answer.Text?.ToString()),
                    ParseRequiredInt(reported.Text?.ToString(), "Reported"));
                MessageBox.Query("Kész", "Tipp kérdés módosítva.", "OK");
                Application.RequestStop();
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        };
        close.Clicked += () => Application.RequestStop();
        dialog.Add(questionLabel, question, answerLabel, answer, reportedLabel, reported, save, close);
        Application.Run(dialog);
    }

    private static int ParseRequiredInt(string? value, string fieldName)
    {
        if (!int.TryParse(value, out var result))
            throw new InvalidOperationException($"A(z) {fieldName} egész szám legyen.");
        return result;
    }

    private static double ParseRequiredDouble(string? value)
    {
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out var localized))
            return localized;
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var invariant))
            return invariant;
        throw new InvalidOperationException("A válasz szám legyen.");
    }
}
