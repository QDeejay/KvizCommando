using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Terminal.Gui;

namespace KvizCommando.Admin;

internal sealed class AdminMainWindow : Window
{
    private readonly AdminDatabase _database;
    private readonly bool _isProduction;
    private readonly AuditLogOperations _auditLogs;
    private readonly DeploymentOperations? _deployments;

    public AdminMainWindow(
        AdminDatabase database,
        bool isProduction,
        string auditOutputRoot)
        : base("KvizCommando Admin")
    {
        _database = database;
        _isProduction = isProduction;
        _auditLogs = new AuditLogOperations(auditOutputRoot);
        _deployments = isProduction ? new DeploymentOperations(database) : null;
        X = 0;
        Y = 0;
        Width = Dim.Fill();
        Height = Dim.Fill();

        var environment = new Label($"Környezet: {_database.EnvironmentLabel}   Adatbázis: {_database.ProviderLabel}")
        {
            X = 2,
            Y = 1
        };

        var users = new Button("_Users") { X = 4, Y = 4, Width = 24 };
        var pending = new Button("_Pending questions") { X = 4, Y = 7, Width = 24 };
        var userQuestions = new Button("User _questions") { X = 4, Y = 10, Width = 24 };
        var quit = new Button("_Kilépés") { X = 4, Y = _isProduction ? 23 : 17, Width = 24 };

        users.Clicked += OpenUsers;
        pending.Clicked += OpenPendingQuestions;
        userQuestions.Clicked += OpenUserQuestions;
        quit.Clicked += () => Application.RequestStop();

        Add(environment, users, pending, userQuestions);

        if (_isProduction)
        {
            var operations = new Button("_Operations") { X = 4, Y = 13, Width = 24 };
            var logs = new Button("_Logs") { X = 4, Y = 16, Width = 24 };
            var deploy = new Button("_Deploy") { X = 4, Y = 19, Width = 24 };
            operations.Clicked += OpenOperations;
            logs.Clicked += OpenLogs;
            deploy.Clicked += OpenDeployments;
            Add(operations, logs, deploy);
        }
        else
        {
            var logs = new Button("_Logs") { X = 4, Y = 13, Width = 24 };
            logs.Clicked += OpenLogs;
            Add(logs);
        }
        Add(quit);
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

        var refresh = new Button("_Frissítés") { X = 1, Y = Pos.Bottom(list) + 1 };
        var open = new Button("_Megnyitás") { X = Pos.Right(refresh) + 2, Y = Pos.Top(refresh) };
        var audit = new Button("_Log") { X = Pos.Right(open) + 2, Y = Pos.Top(refresh) };
        var create = new Button("_Új user") { X = Pos.Right(audit) + 2, Y = Pos.Top(refresh) };
        var close = new Button("_Vissza") { X = Pos.Right(create) + 2, Y = Pos.Top(refresh) };

        refresh.Clicked += Refresh;
        void OpenSelected()
        {
            if (rows.Count == 0 || list.SelectedItem < 0 || list.SelectedItem >= rows.Count)
                return;
            OpenUserEditor(rows[list.SelectedItem]);
            Refresh();
        }
        open.Clicked += OpenSelected;
        audit.Clicked += () =>
        {
            if (rows.Count == 0 || list.SelectedItem < 0 || list.SelectedItem >= rows.Count)
                return;

            var row = rows[list.SelectedItem];
            OpenAuditFiles(row.Id, row.Email);
        };
        create.Clicked += () =>
        {
            OpenCreateUser();
            Refresh();
        };
        close.Clicked += () => Application.RequestStop();
        BindListAction(list, OpenSelected, 'm');

        dialog.Add(searchLabel, search, list, refresh, open, audit, create, close);
        Refresh();
        Application.Run(dialog);
    }

    private void OpenCreateUser()
    {
        var dialog = new Dialog("Új felhasználó", 78, 16);
        var emailLabel = new Label("E-mail:") { X = 2, Y = 2 };
        var email = new TextField(string.Empty) { X = 20, Y = 2, Width = 50 };
        var confirmed = new CheckBox("E-mail confirmed", true) { X = 20, Y = 5 };
        var save = new Button("_Létrehozás") { X = 20, Y = 9 };
        var cancel = new Button("_Mégse") { X = Pos.Right(save) + 3, Y = 9 };

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

        var save = new Button("_Mentés") { X = 2, Y = 20 };
        var reset = new Button("_Forgot password e-mail") { X = Pos.Right(save) + 2, Y = 20 };
        var delete = new Button("_Törlés") { X = Pos.Right(reset) + 2, Y = 20 };
        var close = new Button("_Vissza") { X = Pos.Right(delete) + 2, Y = 20 };

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

        var refresh = new Button("_Frissítés") { X = 1, Y = Pos.Bottom(list) + 1 };
        var open = new Button("_Szerkesztés") { X = Pos.Right(refresh) + 2, Y = Pos.Top(refresh) };
        var close = new Button("_Vissza") { X = Pos.Right(open) + 2, Y = Pos.Top(refresh) };

        refresh.Clicked += Refresh;
        void OpenSelected()
        {
            if (rows.Count == 0 || list.SelectedItem < 0 || list.SelectedItem >= rows.Count)
                return;
            OpenPendingEditor(rows[list.SelectedItem]);
            Refresh();
        }
        open.Clicked += OpenSelected;
        close.Clicked += () => Application.RequestStop();
        BindListAction(list, OpenSelected, 's');

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
        var save = new Button("_Mentés") { X = 2, Y = 25 };
        var approve = new Button("_Jóváhagyás") { X = Pos.Right(save) + 2, Y = 25 };
        var reject = new Button("_Elutasítás") { X = Pos.Right(approve) + 2, Y = 25 };
        var close = new Button("_Vissza") { X = Pos.Right(reject) + 2, Y = 25 };

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

        var refresh = new Button("_Frissítés") { X = 1, Y = Pos.Bottom(list) + 1 };
        var open = new Button("_Szerkesztés") { X = Pos.Right(refresh) + 2, Y = Pos.Top(refresh) };
        var close = new Button("_Vissza") { X = Pos.Right(open) + 2, Y = Pos.Top(refresh) };

        refresh.Clicked += Refresh;
        void OpenSelected()
        {
            if (rows.Count == 0 || list.SelectedItem < 0 || list.SelectedItem >= rows.Count)
                return;
            OpenUserQuestionEditor(rows[list.SelectedItem]);
            Refresh();
        }
        open.Clicked += OpenSelected;
        close.Clicked += () => Application.RequestStop();
        BindListAction(list, OpenSelected, 's');

        dialog.Add(list, refresh, open, close);
        Refresh();
        Application.Run(dialog);
    }

    private void OpenUserQuestionEditor(UserQuestionRow row)
    {
        var answers = DeserializeAnswers(row.AnswersJson);
        var dialog = CreateQuestionDialog($"User question #{row.Id}", row.CategoryNo, row.Question, answers, out var category, out var text, out var answerFields);
        dialog.Add(new Label($"Ask: {row.Ask}   OkAnswer: {row.OkAnswer}") { X = 2, Y = 20 });
        var save = new Button("_Mentés") { X = 2, Y = 24 };
        var close = new Button("_Vissza") { X = Pos.Right(save) + 3, Y = 24 };

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
        var dialog = new Dialog("Operations", 104, 36);

        var serverState = new Label(string.Empty) { X = 2, Y = 2, Width = 90 };
        var serverStart = new Button("_Start server") { X = 2, Y = 4 };
        var serverStop = new Button("Sto_p server") { X = 22, Y = 4 };
        var serverRestart = new Button("_Restart server") { X = 41, Y = 4 };

        var siteState = new Label(string.Empty) { X = 2, Y = 8, Width = 90 };
        var siteOnline = new Button("_ONLINE") { X = 2, Y = 10 };
        var siteMaintenance = new Button("_MAINTENANCE") { X = 18, Y = 10 };

        var registrationState = new Label(string.Empty) { X = 2, Y = 14, Width = 90 };
        var registrationOn = new Button("Registration _ON") { X = 2, Y = 16 };
        var registrationOff = new Button("Registration O_FF") { X = 24, Y = 16 };

        var testPeriodLabel = new Label("Tesztidőszak:") { X = 2, Y = 20 };
        var testPeriod = new TextField(string.Empty) { X = 18, Y = 20, Width = 45 };
        var saveTestPeriod = new Button("_Mentés") { X = 66, Y = 20 };

        var facebookState = new Label(string.Empty) { X = 2, Y = 24, Width = 90 };
        var facebookOn = new Button("Facebook O_N") { X = 2, Y = 26 };
        var facebookOff = new Button("Facebook OF_F") { X = 24, Y = 26 };

        var refresh = new Button("_Frissítés") { X = 2, Y = 30 };
        var close = new Button("_Vissza") { X = 20, Y = 30 };

        void RefreshState()
        {
            try
            {
                serverState.Text = $"KvizCommando.Server: {SystemOperations.GetServerState()}";
                siteState.Text = $"Public site: {ProductionOperations.GetSiteMode()}";
                var auth = ProductionOperations.GetPublicAuthState();
                registrationState.Text = $"Registration: {(auth.RegistrationEnabled ? "ON" : "OFF")}";
                facebookState.Text = $"Facebook Login: {(auth.FacebookLoginEnabled ? "ON" : "OFF")}";
                testPeriod.Text = auth.InvitationTestPeriod;
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

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

        serverStart.Clicked += () => Execute(SystemOperations.StartServer);
        serverStop.Clicked += () => Execute(SystemOperations.StopServer);
        serverRestart.Clicked += () => Execute(SystemOperations.RestartServer);
        siteOnline.Clicked += () => Execute(ProductionOperations.SetSiteOnline);
        siteMaintenance.Clicked += () => Execute(ProductionOperations.SetSiteMaintenance);
        registrationOn.Clicked += () => Execute(() => ProductionOperations.SetRegistrationEnabled(true));
        registrationOff.Clicked += () => Execute(() => ProductionOperations.SetRegistrationEnabled(false));
        saveTestPeriod.Clicked += () => Execute(() => ProductionOperations.SetInvitationTestPeriod(testPeriod.Text?.ToString() ?? string.Empty));
        facebookOn.Clicked += () => Execute(() => ProductionOperations.SetFacebookLoginEnabled(true));
        facebookOff.Clicked += () => Execute(() => ProductionOperations.SetFacebookLoginEnabled(false));
        refresh.Clicked += RefreshState;
        close.Clicked += () => Application.RequestStop();

        dialog.Add(
            serverState, serverStart, serverStop, serverRestart,
            siteState, siteOnline, siteMaintenance,
            registrationState, registrationOn, registrationOff,
            testPeriodLabel, testPeriod, saveTestPeriod,
            facebookState, facebookOn, facebookOff,
            refresh, close);
        RefreshState();
        Application.Run(dialog);
    }

    private void OpenDeployments()
    {
        if (_deployments is null)
            return;

        var dialog = new Dialog("Deploy / release-ek", 124, 39);
        var serverState = new Label(string.Empty) { X = 2, Y = 1, Width = 116 };
        var migrationState = new Label(string.Empty) { X = 2, Y = 3, Width = 116 };
        var applicationState = new Label(string.Empty) { X = 2, Y = 4, Width = 116 };
        var gameState = new Label(string.Empty) { X = 2, Y = 5, Width = 116 };
        var warning = new Label("⚠ A jelölés csak rollback-kockázatot jelez; a release-váltást nem tiltja.")
        {
            X = 2,
            Y = 7,
            Width = 116
        };
        var list = new ListView
        {
            X = 2,
            Y = 9,
            Width = Dim.Fill(2),
            Height = Dim.Fill(5)
        };

        DeploymentSnapshot? snapshot = null;

        void Refresh()
        {
            try
            {
                snapshot = _deployments.GetSnapshot();
                serverState.Text = $"KvizCommando.Server: {snapshot.ServerState}";
                if (snapshot.Migration is null)
                {
                    migrationState.Text = "Utolsó SQL migrációs feltöltés: nincs nyilvántartott manifest";
                    applicationState.Text = "Application: -";
                    gameState.Text = "Game: -";
                }
                else
                {
                    migrationState.Text =
                        $"Utolsó SQL migrációs feltöltés: {snapshot.Migration.UploadedAtUtc.ToLocalTime():yyyy-MM-dd HH:mm:ss}   " +
                        $"Állapot: {snapshot.Migration.PackageState}";
                    applicationState.Text = $"Application: {snapshot.Migration.Application.DisplayText}";
                    gameState.Text = $"Game:        {snapshot.Migration.Game.DisplayText}";
                }

                list.SetSource(snapshot.Releases.Select(release => release.ToString()).ToList());
            }
            catch (Exception exception)
            {
                ShowError(exception);
            }
        }

        ReleaseRow? GetSelected()
        {
            if (snapshot is null ||
                list.SelectedItem < 0 ||
                list.SelectedItem >= snapshot.Releases.Count)
            {
                return null;
            }

            return snapshot.Releases[list.SelectedItem];
        }

        void ActivateSelected()
        {
            var release = GetSelected();
            if (release is null)
                return;
            if (release.IsActive)
            {
                MessageBox.Query("Release", "A kiválasztott release már aktív.", "OK");
                return;
            }

            var risk = release.HasMigrationRisk
                ? "\n\nFIGYELEM: ez a release az utolsó végrehajtott migrációs feltöltés előtti."
                : string.Empty;
            if (MessageBox.Query(
                    "Release aktiválása",
                    $"Aktiválod ezt a release-t?\n{release.Id}{risk}\n\nA szerver nem indul el automatikusan.",
                    "NEM",
                    "IGEN") != 1)
            {
                return;
            }

            try
            {
                _deployments.ActivateRelease(release);
                Refresh();
            }
            catch (Exception exception)
            {
                ShowError(exception);
            }
        }

        void DeleteSelected()
        {
            var release = GetSelected();
            if (release is null)
                return;
            if (release.IsActive)
            {
                MessageBox.Query("Release törlése", "Az aktív release nem törölhető.", "OK");
                return;
            }
            if (MessageBox.Query(
                    "Release törlése",
                    $"Végleg törlöd ezt az inaktív release-t?\n{release.Id}",
                    "NEM",
                    "IGEN") != 1)
            {
                return;
            }

            try
            {
                _deployments.DeleteRelease(release);
                Refresh();
            }
            catch (Exception exception)
            {
                ShowError(exception);
            }
        }

        var refresh = new Button("_Frissítés") { X = 2, Y = Pos.Bottom(list) + 1 };
        var activate = new Button("_Aktiválás") { X = Pos.Right(refresh) + 2, Y = Pos.Top(refresh) };
        var delete = new Button("_Törlés") { X = Pos.Right(activate) + 2, Y = Pos.Top(refresh) };
        var log = new Button("Deploy _log") { X = Pos.Right(delete) + 2, Y = Pos.Top(refresh) };
        var migrationTracking = new Button("_Migráció DB") { X = Pos.Right(log) + 2, Y = Pos.Top(refresh) };
        var close = new Button("_Vissza") { X = Pos.Right(migrationTracking) + 2, Y = Pos.Top(refresh) };

        refresh.Clicked += Refresh;
        activate.Clicked += ActivateSelected;
        delete.Clicked += DeleteSelected;
        log.Clicked += OpenDeployLog;
        migrationTracking.Clicked += OpenMigrationTracking;
        close.Clicked += () => Application.RequestStop();
        BindListAction(list, ActivateSelected, 'a');

        dialog.Add(
            serverState,
            migrationState,
            applicationState,
            gameState,
            warning,
            list,
            refresh,
            activate,
            delete,
            log,
            migrationTracking,
            close);
        Refresh();
        Application.Run(dialog);
    }

    private void OpenMigrationTracking()
    {
        var dialog = new Dialog("Migrációkövetés", 108, 18);
        var hint = new Label("Az időpont helyi idő; az adatbázisban UTC-ként tárolódik.")
        {
            X = 2,
            Y = 1,
            Width = 100
        };
        var applicationState = new Label(string.Empty) { X = 2, Y = 4, Width = 100 };
        var gameState = new Label(string.Empty) { X = 2, Y = 7, Width = 100 };

        MigrationTrackingState? application = null;
        MigrationTrackingState? game = null;

        void Refresh()
        {
            try
            {
                application = _database.GetApplicationMigrationTracking();
                game = _database.GetGameMigrationTracking();
                applicationState.Text = $"Application: {application.DisplayText}";
                gameState.Text = $"Game:        {game.DisplayText}";
            }
            catch (Exception exception)
            {
                ShowError(exception);
            }
        }

        void Configure(
            string databaseName,
            MigrationTrackingState? state,
            Action<DateTimeOffset> initialize,
            Action<long, DateTimeOffset> update)
        {
            if (state is null)
                return;

            if (state.IsInitialized && !state.ExecutionId.HasValue)
            {
                MessageBox.Query(
                    databaseName,
                    "A követés inicializálva van, de még nincs rögzített migráció.",
                    "OK");
                return;
            }

            var title = state.IsInitialized
                ? $"{databaseName} időpont módosítása"
                : $"{databaseName} inicializálása";
            var initialValue = state.AppliedAtUtc?.ToLocalTime() ?? DateTimeOffset.Now;
            var entered = ReadLocalMigrationTime(title, initialValue);
            if (!entered.HasValue)
                return;

            try
            {
                if (state.IsInitialized)
                    update(state.ExecutionId!.Value, entered.Value);
                else
                    initialize(entered.Value);

                Refresh();
            }
            catch (Exception exception)
            {
                ShowError(exception);
            }
        }

        var applicationButton = new Button("_Application inicializálás / időpont")
        {
            X = 2,
            Y = 10,
            Width = 39
        };
        var gameButton = new Button("_Game inicializálás / időpont")
        {
            X = Pos.Right(applicationButton) + 2,
            Y = Pos.Top(applicationButton),
            Width = 34
        };
        var refresh = new Button("_Frissítés") { X = 2, Y = 13 };
        var close = new Button("_Vissza") { X = Pos.Right(refresh) + 2, Y = Pos.Top(refresh) };

        applicationButton.Clicked += () => Configure(
            "Application",
            application,
            _database.InitializeApplicationMigrationTracking,
            _database.UpdateApplicationMigrationExecution);
        gameButton.Clicked += () => Configure(
            "Game",
            game,
            _database.InitializeGameMigrationTracking,
            _database.UpdateGameMigrationExecution);
        refresh.Clicked += Refresh;
        close.Clicked += () => Application.RequestStop();

        dialog.Add(
            hint,
            applicationState,
            gameState,
            applicationButton,
            gameButton,
            refresh,
            close);
        Refresh();
        Application.Run(dialog);
    }

    private static DateTimeOffset? ReadLocalMigrationTime(string title, DateTimeOffset initialValue)
    {
        var dialog = new Dialog(title, 68, 12);
        var label = new Label("Helyi idő (yyyy-MM-dd HH:mm:ss):") { X = 2, Y = 2 };
        var value = new TextField(initialValue.ToString("yyyy-MM-dd HH:mm:ss"))
        {
            X = 2,
            Y = 4,
            Width = 30
        };
        var save = new Button("_Mentés") { X = 2, Y = 7 };
        var cancel = new Button("_Mégse") { X = Pos.Right(save) + 2, Y = Pos.Top(save) };
        DateTimeOffset? result = null;

        save.Clicked += () =>
        {
            if (!DateTime.TryParseExact(
                    value.Text?.ToString(),
                    "yyyy-MM-dd HH:mm:ss",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var localTime))
            {
                MessageBox.ErrorQuery("Hibás időpont", "A formátum: yyyy-MM-dd HH:mm:ss", "OK");
                return;
            }

            result = new DateTimeOffset(
                    DateTime.SpecifyKind(localTime, DateTimeKind.Local))
                .ToUniversalTime();
            Application.RequestStop();
        };
        cancel.Clicked += () => Application.RequestStop();

        dialog.Add(label, value, save, cancel);
        value.SetFocus();
        Application.Run(dialog);
        return result;
    }

    private void OpenLogs()
    {
        var dialog = new Dialog("Logs", 70, _isProduction ? 20 : 12);
        var audit = new Button("_Audit log") { X = 4, Y = 3, Width = 24 };
        var close = new Button("_Vissza") { X = 4, Y = _isProduction ? 15 : 7, Width = 24 };

        audit.Clicked += () => OpenAuditFiles();
        close.Clicked += () => Application.RequestStop();

        dialog.Add(audit);

        if (_isProduction)
        {
            var server = new Button("_Server log") { X = 4, Y = 7, Width = 24 };
            var deploy = new Button("_Deploy log") { X = 4, Y = 11, Width = 24 };
            server.Clicked += OpenServerLog;
            deploy.Clicked += OpenDeployLog;
            dialog.Add(server, deploy);
        }
        dialog.Add(close);
        Application.Run(dialog);
    }

    private void OpenAuditFiles(string? userId = null, string? userLabel = null)
    {
        var title = userId is null
            ? "Audit log"
            : $"Audit log: {userLabel}";
        var dialog = new Dialog(title, 92, 32);
        var heading = new Label("Napi auditfájlok") { X = 2, Y = 1 };
        var list = new ListView
        {
            X = 2,
            Y = 3,
            Width = Dim.Fill(2),
            Height = Dim.Fill(5)
        };

        IReadOnlyList<AuditFileRow> files = Array.Empty<AuditFileRow>();

        void Refresh()
        {
            try
            {
                files = _auditLogs.GetFiles();
                list.SetSource(files.Select(file => file.ToString()).ToList());
            }
            catch (Exception exception)
            {
                ShowError(exception);
            }
        }

        void OpenSelected()
        {
            if (files.Count == 0 || list.SelectedItem < 0 || list.SelectedItem >= files.Count)
                return;

            OpenAuditEntries(files[list.SelectedItem], userId, userLabel);
        }

        var refresh = new Button("_Frissítés") { X = 2, Y = Pos.Bottom(list) + 1 };
        var open = new Button("_Megnyitás") { X = Pos.Right(refresh) + 2, Y = Pos.Top(refresh) };
        var close = new Button("_Vissza") { X = Pos.Right(open) + 2, Y = Pos.Top(refresh) };

        refresh.Clicked += Refresh;
        open.Clicked += OpenSelected;
        close.Clicked += () => Application.RequestStop();
        BindListAction(list, OpenSelected, 'm');

        dialog.Add(heading, list, refresh, open, close);
        Refresh();
        Application.Run(dialog);
    }

    private void OpenAuditEntries(
        AuditFileRow file,
        string? userId,
        string? userLabel)
    {
        var title = userId is null
            ? file.FileName
            : $"{file.FileName}: {userLabel}";
        var dialog = new Dialog(title, 120, 38);
        var list = new ListView
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(2),
            Height = Dim.Fill(5)
        };

        IReadOnlyList<AuditEntryRow> entries;
        try
        {
            entries = _auditLogs.GetEntries(file, userId);
            list.SetSource(entries.Select(entry => entry.ToString()).ToList());
        }
        catch (Exception exception)
        {
            ShowError(exception);
            return;
        }

        var details = new Button("_Részletek") { X = 1, Y = Pos.Bottom(list) + 1 };
        var close = new Button("_Vissza") { X = Pos.Right(details) + 2, Y = Pos.Top(details) };

        void OpenSelected()
        {
            if (entries.Count == 0 || list.SelectedItem < 0 || list.SelectedItem >= entries.Count)
                return;

            OpenAuditEntryDetails(entries[list.SelectedItem]);
        }
        details.Clicked += OpenSelected;
        close.Clicked += () => Application.RequestStop();
        BindListAction(list, OpenSelected, 'r');

        dialog.Add(list, details, close);
        Application.Run(dialog);
    }

    private static void OpenAuditEntryDetails(AuditEntryRow entry)
    {
        var changedFields = entry.Details?.ChangedFields is { Length: > 0 }
            ? string.Join(", ", entry.Details.ChangedFields)
            : "-";
        var content = new StringBuilder()
            .AppendLine($"Idő (UTC):        {entry.UtcTime:yyyy-MM-dd HH:mm:ss.fff}")
            .AppendLine($"Esemény:          {entry.EventName}")
            .AppendLine($"Eredmény:         {entry.Outcome}")
            .AppendLine()
            .AppendLine($"Actor ID:         {entry.ActorId ?? "-"}")
            .AppendLine($"Subject ID:       {entry.SubjectId ?? "-"}")
            .AppendLine($"Request ID:       {entry.RequestId ?? "-"}")
            .AppendLine($"IP hash:          {entry.IpHash ?? "-"}")
            .AppendLine()
            .AppendLine($"Módosított mezők: {changedFields}")
            .AppendLine($"Dokumentumverzió: {entry.Details?.DocumentVersion ?? "-"}")
            .ToString();

        OpenStaticLog($"Audit: {entry.EventName}", content);
    }

    private void OpenServerLog()
    {
        var dialog = new Dialog("Server log", 118, 34);
        var live = new Button("_Live") { X = 2, Y = 2 };
        var last200 = new Button("Last _200") { X = 16, Y = 2 };
        var close = new Button("_Vissza") { X = 34, Y = 2 };

        live.Clicked += () => OpenLiveLog("Server log - LIVE", LogOperations.StartServerLive);
        last200.Clicked += () =>
        {
            try
            {
                OpenStaticLog("Server log - Last 200", LogOperations.GetServerLast200());
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        };
        close.Clicked += () => Application.RequestStop();

        dialog.Add(live, last200, close);
        Application.Run(dialog);
    }

    private void OpenDeployLog()
    {
        var dialog = new Dialog("Deploy log", 118, 34);
        var live = new Button("_Live") { X = 2, Y = 2 };
        var clear = new Button("_Clear") { X = 16, Y = 2 };
        var close = new Button("_Vissza") { X = 30, Y = 2 };

        live.Clicked += () => OpenLiveLog("Deploy log - LIVE", LogOperations.StartDeployLive);
        clear.Clicked += () =>
        {
            if (MessageBox.Query("Deploy log törlése", "Biztosan üríted a deploy logot?", "NEM", "IGEN") != 1)
                return;
            try
            {
                LogOperations.ClearDeployLog();
                MessageBox.Query("Kész", "Deploy log ürítve.", "OK");
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        };
        close.Clicked += () => Application.RequestStop();

        dialog.Add(live, clear, close);
        Application.Run(dialog);
    }

    private static void OpenStaticLog(string title, string content)
    {
        var dialog = new Dialog(title, 120, 38);
        var text = new TextView
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(2),
            Height = Dim.Fill(4),
            ReadOnly = true,
            WordWrap = false,
            Text = content
        };
        var close = new Button("_Vissza") { X = 1, Y = Pos.Bottom(text) + 1 };
        close.Clicked += () => Application.RequestStop();
        dialog.Add(text, close);
        Application.Run(dialog);
    }

    private static void OpenLiveLog(string title, Func<Process> processFactory)
    {
        Process? process = null;
        try
        {
            process = processFactory();
            var dialog = new Dialog(title, 120, 38);
            var text = new TextView
            {
                X = 1,
                Y = 1,
                Width = Dim.Fill(2),
                Height = Dim.Fill(4),
                ReadOnly = true,
                WordWrap = false
            };
            var close = new Button("_Vissza") { X = 1, Y = Pos.Bottom(text) + 1 };
            close.Clicked += () => Application.RequestStop();

            var buffer = new StringBuilder();
            void Append(string? line)
            {
                if (line is null)
                    return;
                Application.MainLoop.Invoke(() =>
                {
                    buffer.AppendLine(line);
                    text.Text = buffer.ToString();
                });
            }

            process.OutputDataReceived += (_, args) => Append(args.Data);
            process.ErrorDataReceived += (_, args) => Append(args.Data);
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            dialog.Add(text, close);
            Application.Run(dialog);
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
        finally
        {
            if (process is { HasExited: false })
                process.Kill(entireProcessTree: true);
            process?.Dispose();
        }
    }

    private static int? ParseNullableInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        if (!int.TryParse(value, out var result))
            throw new InvalidOperationException($"Nem egész szám: {value}");
        return result;
    }

    private static void BindListAction(ListView list, Action action, char shortcut)
    {
        list.OpenSelectedItem += _ => action();
        list.KeyPress += args =>
        {
            var key = args.KeyEvent.Key;
            if (key != (Key)char.ToLowerInvariant(shortcut) &&
                key != (Key)char.ToUpperInvariant(shortcut))
            {
                return;
            }

            args.Handled = true;
            action();
        };
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
