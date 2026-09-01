using System.Text.Json;
using Terminal.Gui;

namespace KvizCommando.Admin;

internal sealed class AdminMainWindow : Window
{
    private readonly AdminDatabase _database;

    public AdminMainWindow(AdminDatabase database)
        : base("KvizCommando Admin")
    {
        _database = database;
        X = 0;
        Y = 0;
        Width = Dim.Fill();
        Height = Dim.Fill();

        var environment = new Label($"Környezet: {_database.EnvironmentLabel}   Adatbázis: {_database.ProviderLabel}")
        {
            X = 2,
            Y = 1
        };

        var users = new Button("Users") { X = 4, Y = 4, Width = 24 };
        var pending = new Button("Pending questions") { X = 4, Y = 7, Width = 24 };
        var userQuestions = new Button("User questions") { X = 4, Y = 10, Width = 24 };
        var operations = new Button("Operations") { X = 4, Y = 13, Width = 24 };
        var quit = new Button("Kilépés") { X = 4, Y = 17, Width = 24 };

        users.Clicked += OpenUsers;
        pending.Clicked += OpenPendingQuestions;
        userQuestions.Clicked += OpenUserQuestions;
        operations.Clicked += OpenOperations;
        quit.Clicked += () => Application.RequestStop();

        Add(environment, users, pending, userQuestions, operations, quit);
    }

    private void OpenUsers()
    {
        var dialog = new Dialog("Users", 118, 34);
        var searchLabel = new Label("Keresés:") { X = 1, Y = 1 };
        var search = new TextField(string.Empty) { X = 11, Y = 1, Width = 50 };
        var list = new ListView { X = 1, Y = 3, Width = Dim.Fill(2), Height = Dim.Fill(5) };

        IReadOnlyList<UserRow> rows = Array.Empty<UserRow>();
        void Refresh()
        {
            try
            {
                rows = _database.GetUsers(search.Text?.ToString());
                list.SetSource(rows.Select(x => x.ToString()).ToList());
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        var refresh = new Button("Frissítés") { X = 1, Y = Pos.Bottom(list) + 1 };
        var open = new Button("Megnyitás") { X = Pos.Right(refresh) + 2, Y = Pos.Top(refresh) };
        var create = new Button("Új user") { X = Pos.Right(open) + 2, Y = Pos.Top(refresh) };
        var close = new Button("Vissza") { X = Pos.Right(create) + 2, Y = Pos.Top(refresh) };

        refresh.Clicked += Refresh;
        open.Clicked += () =>
        {
            if (rows.Count == 0 || list.SelectedItem < 0 || list.SelectedItem >= rows.Count)
                return;
            OpenUserEditor(rows[list.SelectedItem]);
            Refresh();
        };
        create.Clicked += () =>
        {
            OpenCreateUser();
            Refresh();
        };
        close.Clicked += () => Application.RequestStop();

        dialog.Add(searchLabel, search, list, refresh, open, create, close);
        Refresh();
        Application.Run(dialog);
    }

    private void OpenCreateUser()
    {
        var dialog = new Dialog("Új felhasználó", 78, 16);
        var emailLabel = new Label("E-mail:") { X = 2, Y = 2 };
        var email = new TextField(string.Empty) { X = 20, Y = 2, Width = 50 };
        var confirmed = new CheckBox("E-mail confirmed", true) { X = 20, Y = 5 };
        var save = new Button("Létrehozás") { X = 20, Y = 9 };
        var cancel = new Button("Mégse") { X = Pos.Right(save) + 3, Y = 9 };

        save.Clicked += () =>
        {
            try
            {
                var id = _database.CreateUser(email.Text?.ToString() ?? string.Empty, confirmed.Checked);
                MessageBox.Query("Kész", $"Felhasználó létrehozva.\nID: {id}\nPasswordHash: NULL", "OK");
                Application.RequestStop();
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        };
        cancel.Clicked += () => Application.RequestStop();

        dialog.Add(emailLabel, email, confirmed, save, cancel);
        Application.Run(dialog);
    }

    private void OpenUserEditor(UserRow row)
    {
        var dialog = new Dialog($"User: {row.Email}", 92, 27);
        dialog.Add(new Label($"ID: {row.Id}") { X = 2, Y = 1 });
        dialog.Add(new Label($"E-mail: {row.Email}") { X = 2, Y = 3 });

        var displayLabel = new Label("Display name:") { X = 2, Y = 5 };
        var display = new TextField(row.DisplayName ?? string.Empty) { X = 21, Y = 5, Width = 45 };
        var confirmed = new CheckBox("E-mail confirmed", row.EmailConfirmed) { X = 21, Y = 7 };

        var rankLabel = new Label("Rank:") { X = 2, Y = 10 };
        var rank = new TextField(row.Rank?.ToString() ?? string.Empty) { X = 21, Y = 10, Width = 12, ReadOnly = !row.PlayerId.HasValue };
        var xpLabel = new Label("XP:") { X = 2, Y = 12 };
        var xp = new TextField(row.XP?.ToString() ?? string.Empty) { X = 21, Y = 12, Width = 12, ReadOnly = !row.PlayerId.HasValue };
        var creditLabel = new Label("Credit:") { X = 2, Y = 14 };
        var credit = new TextField(row.Credit?.ToString() ?? string.Empty) { X = 21, Y = 14, Width = 12, ReadOnly = !row.PlayerId.HasValue };
        var voucherLabel = new Label("Voucher:") { X = 2, Y = 16 };
        var voucher = new TextField(row.Voucher?.ToString() ?? string.Empty) { X = 21, Y = 16, Width = 12, ReadOnly = !row.PlayerId.HasValue };

        if (!row.PlayerId.HasValue)
            dialog.Add(new Label("Player rekord még nincs; Rank/XP/Credit/Voucher az első check-in után lesz módosítható.") { X = 36, Y = 11 });

        var save = new Button("Mentés") { X = 2, Y = 20 };
        var reset = new Button("Forgot password e-mail") { X = Pos.Right(save) + 2, Y = 20 };
        var delete = new Button("Törlés") { X = Pos.Right(reset) + 2, Y = 20 };
        var close = new Button("Vissza") { X = Pos.Right(delete) + 2, Y = 20 };

        save.Clicked += () =>
        {
            try
            {
                _database.UpdateUser(
                    row,
                    display.Text?.ToString(),
                    confirmed.Checked,
                    ParseNullableInt(rank.Text?.ToString()),
                    ParseNullableInt(xp.Text?.ToString()),
                    ParseNullableInt(credit.Text?.ToString()),
                    ParseNullableInt(voucher.Text?.ToString()));
                MessageBox.Query("Kész", "Felhasználó módosítva.", "OK");
                Application.RequestStop();
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        };

        reset.Clicked += () =>
        {
            try
            {
                _database.SendForgotPassword(row.Email);
                MessageBox.Query("Kész", "A meglévő forgot-password folyamat meghívva.", "OK");
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        };

        delete.Clicked += () =>
        {
            if (MessageBox.Query("Törlés", $"Biztosan törlöd?\n{row.Email}", "NEM", "IGEN") != 1)
                return;

            try
            {
                _database.DeleteUser(row);
                MessageBox.Query("Kész", "Felhasználó törölve.", "OK");
                Application.RequestStop();
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        };
        close.Clicked += () => Application.RequestStop();

        dialog.Add(displayLabel, display, confirmed, rankLabel, rank, xpLabel, xp, creditLabel, credit, voucherLabel, voucher,
            save, reset, delete, close);
        Application.Run(dialog);
    }

    private void OpenPendingQuestions()
    {
        var dialog = new Dialog("Pending questions", 118, 34);
        var list = new ListView { X = 1, Y = 1, Width = Dim.Fill(2), Height = Dim.Fill(5) };
        IReadOnlyList<PendingQuestionRow> rows = Array.Empty<PendingQuestionRow>();

        void Refresh()
        {
            try
            {
                rows = _database.GetPendingQuestions();
                list.SetSource(rows.Select(x => x.ToString()).ToList());
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        var refresh = new Button("Frissítés") { X = 1, Y = Pos.Bottom(list) + 1 };
        var open = new Button("Szerkesztés") { X = Pos.Right(refresh) + 2, Y = Pos.Top(refresh) };
        var close = new Button("Vissza") { X = Pos.Right(open) + 2, Y = Pos.Top(refresh) };

        refresh.Clicked += Refresh;
        open.Clicked += () =>
        {
            if (rows.Count == 0 || list.SelectedItem < 0 || list.SelectedItem >= rows.Count)
                return;
            OpenPendingEditor(rows[list.SelectedItem]);
            Refresh();
        };
        close.Clicked += () => Application.RequestStop();

        dialog.Add(list, refresh, open, close);
        Refresh();
        Application.Run(dialog);
    }

    private void OpenPendingEditor(PendingQuestionRow row)
    {
        var answers = DeserializeAnswers(row.AnswersJson);
        var dialog = CreateQuestionDialog($"Pending #{row.Id}", row.CategoryNo, row.Question, answers, out var category, out var text, out var answerFields);
        var statusLabel = new Label("Status:") { X = 2, Y = 20 };
        var status = new TextField(row.Status) { X = 16, Y = 20, Width = 18 };
        var remarkLabel = new Label("Remark:") { X = 2, Y = 22 };
        var remark = new TextField(row.Remark ?? string.Empty) { X = 16, Y = 22, Width = 65 };
        var save = new Button("Mentés") { X = 2, Y = 25 };
        var approve = new Button("Jóváhagyás") { X = Pos.Right(save) + 2, Y = 25 };
        var reject = new Button("Elutasítás") { X = Pos.Right(approve) + 2, Y = 25 };
        var close = new Button("Vissza") { X = Pos.Right(reject) + 2, Y = 25 };

        void Save(string requestedStatus)
        {
            try
            {
                _database.UpdatePendingQuestion(
                    row,
                    ParseCategory(category.Text?.ToString()),
                    text.Text?.ToString() ?? string.Empty,
                    answerFields.Select(x => x.Text?.ToString() ?? string.Empty).ToArray(),
                    requestedStatus,
                    remark.Text?.ToString());
                MessageBox.Query("Kész", $"Kérdés mentve. Status: {requestedStatus}", "OK");
                Application.RequestStop();
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        save.Clicked += () => Save(status.Text?.ToString() ?? row.Status);
        approve.Clicked += () => Save("Approved");
        reject.Clicked += () => Save("Rejected");
        close.Clicked += () => Application.RequestStop();

        dialog.Add(statusLabel, status, remarkLabel, remark, save, approve, reject, close);
        Application.Run(dialog);
    }

    private void OpenUserQuestions()
    {
        var dialog = new Dialog("User questions", 118, 34);
        var list = new ListView { X = 1, Y = 1, Width = Dim.Fill(2), Height = Dim.Fill(5) };
        IReadOnlyList<UserQuestionRow> rows = Array.Empty<UserQuestionRow>();

        void Refresh()
        {
            try
            {
                rows = _database.GetUserQuestions();
                list.SetSource(rows.Select(x => x.ToString()).ToList());
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        var refresh = new Button("Frissítés") { X = 1, Y = Pos.Bottom(list) + 1 };
        var open = new Button("Szerkesztés") { X = Pos.Right(refresh) + 2, Y = Pos.Top(refresh) };
        var close = new Button("Vissza") { X = Pos.Right(open) + 2, Y = Pos.Top(refresh) };

        refresh.Clicked += Refresh;
        open.Clicked += () =>
        {
            if (rows.Count == 0 || list.SelectedItem < 0 || list.SelectedItem >= rows.Count)
                return;
            OpenUserQuestionEditor(rows[list.SelectedItem]);
            Refresh();
        };
        close.Clicked += () => Application.RequestStop();

        dialog.Add(list, refresh, open, close);
        Refresh();
        Application.Run(dialog);
    }

    private void OpenUserQuestionEditor(UserQuestionRow row)
    {
        var answers = DeserializeAnswers(row.AnswersJson);
        var dialog = CreateQuestionDialog($"User question #{row.Id}", row.CategoryNo, row.Question, answers, out var category, out var text, out var answerFields);
        dialog.Add(new Label($"Ask: {row.Ask}   OkAnswer: {row.OkAnswer}") { X = 2, Y = 20 });
        var save = new Button("Mentés") { X = 2, Y = 24 };
        var close = new Button("Vissza") { X = Pos.Right(save) + 3, Y = 24 };

        save.Clicked += () =>
        {
            try
            {
                _database.UpdateUserQuestion(
                    row,
                    ParseCategory(category.Text?.ToString()),
                    text.Text?.ToString() ?? string.Empty,
                    answerFields.Select(x => x.Text?.ToString() ?? string.Empty).ToArray());
                MessageBox.Query("Kész", "User question módosítva.", "OK");
                Application.RequestStop();
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        };
        close.Clicked += () => Application.RequestStop();

        dialog.Add(save, close);
        Application.Run(dialog);
    }

    private static Dialog CreateQuestionDialog(
        string title,
        int categoryNo,
        string question,
        IReadOnlyList<string> answers,
        out TextField category,
        out TextView text,
        out TextField[] answerFields)
    {
        var dialog = new Dialog(title, 110, 31);
        var categoryLabel = new Label("Kategória:") { X = 2, Y = 1 };
        category = new TextField(categoryNo.ToString()) { X = 16, Y = 1, Width = 8 };
        var questionLabel = new Label("Kérdés:") { X = 2, Y = 3 };
        text = new TextView
        {
            X = 16,
            Y = 3,
            Width = 86,
            Height = 7,
            Text = question,
            WordWrap = true
        };

        answerFields = new TextField[4];
        for (var index = 0; index < 4; index++)
        {
            var label = new Label(index == 0 ? "1. (HELYES):" : $"{index + 1}.:") { X = 2, Y = 11 + index * 2 };
            var field = new TextField(answers.ElementAtOrDefault(index) ?? string.Empty)
            {
                X = 16,
                Y = 11 + index * 2,
                Width = 86
            };
            answerFields[index] = field;
            dialog.Add(label, field);
        }

        dialog.Add(categoryLabel, category, questionLabel, text);
        return dialog;
    }

    private void OpenOperations()
    {
        var dialog = new Dialog("Operations", 80, 20);
        var state = new Label($"KvizCommando.Server: {SystemOperations.GetServerState()}") { X = 2, Y = 2 };
        var online = new Button("ONLINE / Start") { X = 2, Y = 6 };
        var maintenance = new Button("MAINTENANCE / Stop") { X = 24, Y = 6 };
        var restart = new Button("Restart") { X = 52, Y = 6 };
        var refresh = new Button("Frissítés") { X = 2, Y = 10 };
        var close = new Button("Vissza") { X = 18, Y = 10 };

        void RefreshState() => state.Text = $"KvizCommando.Server: {SystemOperations.GetServerState()}";
        void Execute(Action operation)
        {
            try
            {
                operation();
                RefreshState();
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        online.Clicked += () => Execute(SystemOperations.StartServer);
        maintenance.Clicked += () => Execute(SystemOperations.StopServer);
        restart.Clicked += () => Execute(SystemOperations.RestartServer);
        refresh.Clicked += RefreshState;
        close.Clicked += () => Application.RequestStop();

        dialog.Add(state, online, maintenance, restart, refresh, close);
        Application.Run(dialog);
    }

    private static int? ParseNullableInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        if (!int.TryParse(value, out var result))
            throw new InvalidOperationException($"Nem egész szám: {value}");
        return result;
    }

    private static int ParseCategory(string? value)
    {
        if (!int.TryParse(value, out var result))
            throw new InvalidOperationException("Hibás kategória.");
        return result;
    }

    private static IReadOnlyList<string> DeserializeAnswers(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<string[]>(json) ?? Array.Empty<string>();
        }
        catch (JsonException)
        {
            return Array.Empty<string>();
        }
    }

    private static void ShowError(Exception exception) =>
        MessageBox.ErrorQuery("Hiba", exception.Message, "OK");
}
