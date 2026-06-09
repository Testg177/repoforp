using BlazorApp.Domain.Entities;
using BlazorApp.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace BlazorApp.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles = ["SuperAdmin", "Admin", "Manager", "Supervisor",
                          "Employee", "Auditor", "Viewer"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        const string superAdminEmail = "superadmin@firma.pl";
        var existingSuperAdmin = await userManager.FindByEmailAsync(superAdminEmail);
        if (existingSuperAdmin is null)
        {
            var superAdmin = new ApplicationUser
            {
                UserName = superAdminEmail,
                Email = superAdminEmail,
                FirstName = "Super",
                LastName = "Admin",
                IsActive = true,
                ForcePasswordChange = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(superAdmin, "TymczasoweHaslo123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
        }
        else if (!await userManager.IsInRoleAsync(existingSuperAdmin, "SuperAdmin"))
        {
            await userManager.AddToRoleAsync(existingSuperAdmin, "SuperAdmin");
        }
        existingSuperAdmin ??= await userManager.FindByEmailAsync(superAdminEmail);

        await EnsureDefaultPermissionsAsync(db, roleManager);

        if (!await db.ArchiveSettings.AnyAsync())
        {
            db.ArchiveSettings.Add(new ArchiveSettings
            {
                StoragePath = "/app/archiwum",
                AllowedExtensions = [".pdf", ".jpg", ".jpeg", ".png", ".tiff"],
                MaxFileSizeMb = 50
            });
        }

        if (!await db.ArchiveCategories.AnyAsync())
        {
            db.ArchiveCategories.AddRange(
                new ArchiveCategory { Name = "BHP", Code = "BHP", IsActive = true },
                new ArchiveCategory { Name = "Produkcja", Code = "PROD", IsActive = true },
                new ArchiveCategory { Name = "HR", Code = "HR", IsActive = true },
                new ArchiveCategory { Name = "Jakość", Code = "QA", IsActive = true },
                new ArchiveCategory { Name = "Finanse", Code = "FIN", IsActive = true }
            );
        }

        if (!await db.FormDefinitions.AnyAsync(f => f.Code == "RK-001"))
        {
            db.FormDefinitions.Add(new FormDefinition
            {
                Code = "RK-001",
                Title = "Rejestr przegladow i konserwacji",
                Description = "Biale pola wypelnia pracownik. Szare pola uzupelnia kierownik przed zatwierdzeniem.",
                Category = "Produkcja",
                IsActive = true,
                RequiresApproval = true,
                EscalationAfterDays = 1,
                AssignedRoles = ["Employee", "Supervisor", "Manager"],
                Fields =
                [
                    Section("employee_section", "Czesc pracownika", 10),
                    Field("entry_date", "Data", FieldType.Date, 20, true, "Data wykonania przegladu", "employee"),
                    Field("device_scope", "Urzadzenie / zakres prac", FieldType.TextArea, 30, true, "Opisz urzadzenie, przeglad lub konserwacje", "employee"),
                    Field("risk_assessment", "Ocena ryzyka", FieldType.Radio, 40, true, "Wybierz poziom ryzyka dla pracy", "employee", ["N - niskie", "S - srednie", "W - wysokie"]),
                    Field("work_context", "Miejsce / tryb pracy", FieldType.Checkbox, 50, false, "Zaznacz wlasciwe okolicznosci", "employee", ["w trakcie produkcji", "w przerwie produkcyjnej", "w warsztacie"]),
                    Field("risk_reduction", "Zalecenia dotyczace ograniczenia ryzyka", FieldType.TextArea, 60, true, "Jak ograniczono ryzyko podczas prac", "employee"),
                    Field("trained_people_signature", "Czytelny podpis osob przeszkolonych do wykonania prac", FieldType.TextArea, 70, false, "Wpisz imiona, nazwiska lub podpisy osob wykonujacych prace", "employee"),
                    Field("process_validation", "Uwagi / walidacja procesu", FieldType.TextArea, 80, true, "Wpisz uwagi i walidacje procesu, np. dziala poprawnie albo co wymaga poprawy", "employee"),
                    Section("manager_section", "Czesc kierownika", 100),
                    Field("manager_signature", "Podpis kierownika", FieldType.Text, 120, true, "Imie, nazwisko lub podpis kierownika", "manager"),
                    Field("hygiene_assessment", "Dopuszczenie do eksploatacji - ocena higieniczna", FieldType.Radio, 140, true, "Decyzja kierownika", "manager", ["prawidlowa", "nieprawidlowa"]),
                    Field("device_status", "Status urzadzenia", FieldType.Radio, 150, true, "Dopuszczenie urzadzenia", "manager", ["S - sprawny dopuszczony", "N - niezgodny do remontu", "SW - dopuszczony warunkowo"]),
                    Field("production_manager_signature", "Podpis kierownika produkcji", FieldType.Text, 160, true, "Podpis osoby zatwierdzajacej", "manager")
                ]
            });
        }

        var maintenanceForm = await db.FormDefinitions
            .Include(f => f.Fields)
            .FirstOrDefaultAsync(f => f.Code == "RK-001");
        if (maintenanceForm is not null)
        {
            maintenanceForm.AssignedRoles = ["Employee", "Supervisor", "Manager", "Admin", "SuperAdmin"];

            var processValidation = maintenanceForm.Fields.FirstOrDefault(f => f.FieldKey == "process_validation");
            if (processValidation is not null)
            {
                processValidation.Label = "Uwagi / walidacja procesu";
                processValidation.HelpText = "Wpisz uwagi i walidacje procesu, np. dziala poprawnie albo co wymaga poprawy";
                processValidation.Order = 80;
                processValidation.ValidationRules = new Dictionary<string, JsonElement>
                {
                    ["fillRole"] = JsonSerializer.SerializeToElement("employee")
                };
            }

            var cleaningValidation = maintenanceForm.Fields.FirstOrDefault(f => f.FieldKey == "cleaning_validation");
            if (cleaningValidation is not null)
            {
                db.FormFields.Remove(cleaningValidation);
            }

            await EnsureRejectionCategoryAsync(db, maintenanceForm.Id, "EMPLOYEE_ERROR", "Błąd pracownika", "Formularz wymaga poprawy przez pracownika.");
            await EnsureRejectionCategoryAsync(db, maintenanceForm.Id, "MACHINE_ERROR", "Błąd maszyny", "Formularz zostal odrzucony z powodu problemu z maszyna.");
        }

        if (existingSuperAdmin is not null)
        {
            await EnsureMaintenancePlanAsync(db, existingSuperAdmin.Id);
        }

        await db.SaveChangesAsync();
    }

    private static async Task EnsureDefaultPermissionsAsync(AppDbContext db, RoleManager<IdentityRole> roleManager)
    {
        await UpsertRolePermissions(db, roleManager, "SuperAdmin", Enum.GetValues<ModuleType>()
            .Select(module => Permission(module, view: true, create: true, edit: true, delete: true, approve: true, export: true)));

        await UpsertRolePermissions(db, roleManager, "Admin", Enum.GetValues<ModuleType>()
            .Select(module => Permission(module, view: true, create: true, edit: true, delete: true, approve: true, export: true)));

        await UpsertRolePermissions(db, roleManager, "Manager",
        [
            Permission(ModuleType.Dashboard, view: true),
            Permission(ModuleType.Forms, view: true, create: true, edit: true, approve: true, export: true),
            Permission(ModuleType.Archive, view: true, export: true),
            Permission(ModuleType.Reports, view: true, export: true),
            Permission(ModuleType.Notifications, view: true)
        ]);

        await UpsertRolePermissions(db, roleManager, "Supervisor",
        [
            Permission(ModuleType.Dashboard, view: true),
            Permission(ModuleType.Forms, view: true, create: true, edit: true),
            Permission(ModuleType.Archive, view: true),
            Permission(ModuleType.Notifications, view: true)
        ]);

        await UpsertRolePermissions(db, roleManager, "Employee",
        [
            Permission(ModuleType.Dashboard, view: true),
            Permission(ModuleType.Forms, view: true, create: true),
            Permission(ModuleType.Notifications, view: true)
        ]);

        await UpsertRolePermissions(db, roleManager, "Auditor",
        [
            Permission(ModuleType.Dashboard, view: true),
            Permission(ModuleType.Archive, view: true, export: true),
            Permission(ModuleType.Reports, view: true, export: true),
            Permission(ModuleType.Audit, view: true, export: true)
        ]);

        await UpsertRolePermissions(db, roleManager, "Viewer",
        [
            Permission(ModuleType.Dashboard, view: true),
            Permission(ModuleType.Reports, view: true)
        ]);
    }

    private static async Task UpsertRolePermissions(AppDbContext db, RoleManager<IdentityRole> roleManager, string roleName, IEnumerable<ModulePermission> permissions)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role is null) return;

        foreach (var template in permissions)
        {
            var permission = await db.ModulePermissions
                .FirstOrDefaultAsync(x => x.RoleId == role.Id && x.Module == template.Module);

            if (permission is null)
            {
                template.RoleId = role.Id;
                db.ModulePermissions.Add(template);
                continue;
            }

            permission.CanView = template.CanView;
            permission.CanCreate = template.CanCreate;
            permission.CanEdit = template.CanEdit;
            permission.CanDelete = template.CanDelete;
            permission.CanApprove = template.CanApprove;
            permission.CanExport = template.CanExport;
        }
    }

    private static ModulePermission Permission(ModuleType module, bool view = false, bool create = false, bool edit = false, bool delete = false, bool approve = false, bool export = false) => new()
    {
        Module = module,
        CanView = view,
        CanCreate = create,
        CanEdit = edit,
        CanDelete = delete,
        CanApprove = approve,
        CanExport = export
    };

    private static async Task EnsureRejectionCategoryAsync(AppDbContext db, int formDefinitionId, string code, string name, string description)
    {
        var category = await db.RejectionCategories
            .FirstOrDefaultAsync(c => c.FormDefinitionId == formDefinitionId && c.Code == code);

        if (category is null)
        {
            db.RejectionCategories.Add(new RejectionCategory
            {
                Code = code,
                Name = name,
                Description = description,
                FormDefinitionId = formDefinitionId,
                IsActive = true
            });
            return;
        }

        category.Name = name;
        category.Description = description;
        category.IsActive = true;
    }

    private static async Task EnsureMaintenancePlanAsync(AppDbContext db, string systemUserId)
    {
        var executionForm = await EnsureFormAsync(db, "PM-001", "Wykonanie przegladu lub konserwacji z planu", "Formularz powiazany z rocznym planem przegladow i konserwacji.", "Utrzymanie ruchu",
        [
            Section("task_section", "Dane przegladu", 10),
            Field("planned_date", "Planowany termin", FieldType.Date, 20, true, "Termin wynikajacy z planu rocznego", "employee"),
            Field("device_name", "Nazwa urzadzenia lub maszyny", FieldType.Text, 30, true, "Wpisz nazwe urzadzenia z planu", "employee"),
            Field("work_scope", "Zakres prac", FieldType.TextArea, 40, true, "Opisz wykonany przeglad lub konserwacje", "employee"),
            Field("frequency", "Czestotliwosc z planu", FieldType.Text, 50, false, "Np. 1 x 2 m-c, 1 x 3 m-ce, 1 x 6 m-cy", "employee"),
            Field("result", "Wynik prac", FieldType.Radio, 60, true, "Ocena po wykonaniu prac", "employee", ["wykonano prawidlowo", "wykonano z uwagami", "nie wykonano"]),
            Field("employee_notes", "Uwagi pracownika", FieldType.TextArea, 70, false, "Uwagi, usterki, czesci do wymiany", "employee"),
            Section("manager_section", "Czesc kierownika", 100),
            Field("manager_decision", "Decyzja kierownika", FieldType.Radio, 110, true, "Decyzja po kontroli formularza", "manager", ["zatwierdzam", "do poprawy"]),
            Field("manager_notes", "Uwagi kierownika", FieldType.TextArea, 120, false, "Uwagi do wykonanego przegladu", "manager"),
            Field("manager_signature", "Podpis kierownika", FieldType.Text, 130, true, "Imie i nazwisko kierownika", "manager")
        ]);

        var inspectionForm = await EnsureFormAsync(db, "IST-001", "Inspekcja stanu technicznego maszyn i urzadzen", "Formularz na podstawie tabeli inspekcji technicznej maszyn i urzadzen.", "Utrzymanie ruchu",
        [
            Section("inspection_header", "Dane inspekcji", 10),
            Field("week_number", "Tydzien", FieldType.Number, 20, false, "Numer tygodnia inspekcji", "employee"),
            Field("inspection_date", "Data inspekcji", FieldType.Date, 30, true, "Data wykonania inspekcji", "employee"),
            Field("device_name", "Urzadzenie / maszyna", FieldType.Select, 40, true, "Wybierz kontrolowana maszyne", "employee",
            [
                "Linia krojenia",
                "Linia transportu surowca",
                "Stacja uzdatniania wody glebinowej",
                "Zbiornik do wody glebinowej po uzdatnieniu",
                "Komory chlodnicze / mroznie / regalki / uszczelki / kurtyny",
                "Zbiornik mag. H2SO4 instalacja do dozowania kwasu",
                "Kotly procesowe bis 3-12",
                "Kotly procesowe nr 1-12",
                "Zbiorniki magaz. poziome nr 1-3 / filtry szczelinowe / wanienki nr 1-2"
            ]),
            Field("location", "Miejsce", FieldType.Text, 50, true, "Np. magazyn surowca, zalewa kwasowa, hydroliza kwasowa", "employee"),
            Field("inspection_result", "Wynik inspekcji", FieldType.TextArea, 60, true, "Kompletnosc urzadzenia, oslony, sruby, przeciwki/uszczelnienia, rdza, informacje od obslugi", "employee"),
            Section("corrective_section", "Dzialania korygujace", 100),
            Field("corrective_action", "Sposob usuniecia niezgodnosci", FieldType.TextArea, 110, false, "Opis dzialan korygujacych", "employee"),
            Field("action_due_date", "Termin zakonczenia dzialan", FieldType.Date, 120, false, "Termin usuniecia niezgodnosci", "employee"),
            Field("nonconformity_removed", "Niezgodnosc usunieta", FieldType.Radio, 130, false, "T - tak, N - nie", "employee", ["T", "N"]),
            Field("manager_signature", "Podpis kierownika utrzymania ruchu", FieldType.Text, 140, false, "Podpis osoby weryfikujacej", "manager"),
            Field("notes", "Uwagi", FieldType.TextArea, 150, false, "Dodatkowe uwagi", "employee")
        ]);

        await db.SaveChangesAsync();
        await EnsurePlanEventsAsync(db, executionForm.Id, systemUserId);
        await EnsurePlanScheduleAsync(db, executionForm.Id);
        await EnsureWeeklyInspectionScheduleAsync(db, inspectionForm.Id);
    }

    private static async Task<FormDefinition> EnsureFormAsync(AppDbContext db, string code, string title, string description, string category, List<FormField> fields)
    {
        var form = await db.FormDefinitions
            .Include(f => f.Fields)
            .Include(f => f.Schedules)
            .FirstOrDefaultAsync(f => f.Code == code);

        if (form is null)
        {
            form = new FormDefinition
            {
                Code = code,
                Title = title,
                Description = description,
                Category = category,
                IsActive = true,
                RequiresApproval = true,
                EscalationAfterDays = 3,
                AssignedRoles = ["Employee", "Supervisor", "Manager", "Admin", "SuperAdmin"],
                Fields = fields
            };
            db.FormDefinitions.Add(form);
            return form;
        }

        form.Title = title;
        form.Description = description;
        form.Category = category;
        form.IsActive = true;
        form.RequiresApproval = true;
        form.AssignedRoles = ["Employee", "Supervisor", "Manager", "Admin", "SuperAdmin"];

        foreach (var field in fields)
        {
            var existing = form.Fields.FirstOrDefault(f => f.FieldKey == field.FieldKey);
            if (existing is null)
            {
                form.Fields.Add(field);
                continue;
            }

            existing.Label = field.Label;
            existing.HelpText = field.HelpText;
            existing.Placeholder = field.Placeholder;
            existing.Type = field.Type;
            existing.IsRequired = field.IsRequired;
            existing.Order = field.Order;
            existing.Options = field.Options;
            existing.ValidationRules = field.ValidationRules;
        }

        return form;
    }

    private static async Task EnsurePlanEventsAsync(AppDbContext db, int formDefinitionId, string systemUserId)
    {
        var tasks = new List<PlanTask>
        {
            new("Ozonatory Bullonu Nr. 1,2", "Sprawdzenie poprawnosci dzialania", "1 x 3 m-ce", [DateUtc(2026, 6), DateUtc(2026, 9), DateUtc(2026, 12)]),
            new("Ozonatory Bullonu Nr. 1,2", "Sprawdzenie szczelnosci instalacji", "1 x 3 m-ce", [DateUtc(2026, 6), DateUtc(2026, 9), DateUtc(2026, 12)]),
            new("Przenosniki slimakowe surowca", "Konserwacja i regulacja", "1 x 2 m-c", [DateUtc(2026, 4), DateUtc(2026, 6), DateUtc(2026, 8), DateUtc(2026, 10), DateUtc(2026, 12)]),
            new("Przenosniki slimakowe surowca", "Sprawdzanie i regulacja elementow automatyki", "1 x 2 m-c", [DateUtc(2026, 4), DateUtc(2026, 6), DateUtc(2026, 8), DateUtc(2026, 10), DateUtc(2026, 12)]),
            new("Przenosniki slimakowe surowca", "Sprawdzanie stanu eksploatacyjnego, smarowanie lozysk", "1 x 6 m-cy", [DateUtc(2026, 5), DateUtc(2026, 11)]),
            new("Przenosniki slimakowe surowca", "Wymiana lancucha", "1 x 1 rok", [DateUtc(2026, 1)]),
            new("Przenosniki tasmowe surowca", "Sprawdzanie stanu tasm transportowych", "1 x 2 m-c", [DateUtc(2026, 4), DateUtc(2026, 6), DateUtc(2026, 8), DateUtc(2026, 10), DateUtc(2026, 12)]),
            new("Przenosniki tasmowe surowca", "Sprawdzanie stanu lozysk", "1 x 6 m-cy", [DateUtc(2026, 7), DateUtc(2027, 1)])
        };

        foreach (var task in tasks)
        {
            foreach (var date in task.Dates)
            {
                var title = $"{task.Device}: {task.Scope}";
                var exists = await db.CalendarEvents.AnyAsync(e =>
                    e.RelatedFormDefinitionId == formDefinitionId &&
                    e.EventDate == date &&
                    e.Title == title);

                if (exists) continue;

                db.CalendarEvents.Add(new CalendarEvent
                {
                    Title = title,
                    Description = $"Plan przegladow i konserwacji 2026. Czestotliwosc: {task.Frequency}. Wypelnij formularz PM-001.",
                    EventDate = date,
                    DueDate = date,
                    CreatedByUserId = systemUserId,
                    AssignedUserIds = [],
                    RelatedFormDefinitionId = formDefinitionId,
                    EventColor = date.Date < DateTime.UtcNow.Date ? "red" : "orange",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
    }

    private static async Task EnsurePlanScheduleAsync(AppDbContext db, int formDefinitionId)
    {
        var hasSchedule = await db.FormSchedules.AnyAsync(s =>
            s.FormDefinitionId == formDefinitionId &&
            s.Type == ScheduleType.Custom &&
            s.CronExpression == "PLAN-2026");

        if (hasSchedule) return;

        db.FormSchedules.Add(new FormSchedule
        {
            FormDefinitionId = formDefinitionId,
            Type = ScheduleType.Custom,
            DueTime = new TimeOnly(14, 0),
            ReminderHoursBefore = 48,
            ValidFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ValidTo = new DateTime(2027, 1, 31, 23, 59, 59, DateTimeKind.Utc),
            CronExpression = "PLAN-2026",
            IsActive = true
        });
    }

    private static async Task EnsureWeeklyInspectionScheduleAsync(AppDbContext db, int formDefinitionId)
    {
        var hasSchedule = await db.FormSchedules.AnyAsync(s =>
            s.FormDefinitionId == formDefinitionId &&
            s.Type == ScheduleType.Weekly &&
            s.DayOfWeek == DayOfWeek.Monday);

        if (hasSchedule) return;

        db.FormSchedules.Add(new FormSchedule
        {
            FormDefinitionId = formDefinitionId,
            Type = ScheduleType.Weekly,
            DayOfWeek = DayOfWeek.Monday,
            DueTime = new TimeOnly(10, 0),
            ReminderHoursBefore = 24,
            ValidFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ValidTo = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
            CronExpression = "0 0 10 ? * MON *",
            IsActive = true
        });
    }

    private static DateTime DateUtc(int year, int month) => new(year, month, 15, 8, 0, 0, DateTimeKind.Utc);

    private sealed record PlanTask(string Device, string Scope, string Frequency, List<DateTime> Dates);

    private static FormField Section(string key, string label, int order) => new()
    {
        FieldKey = key,
        Label = label,
        Type = FieldType.Section,
        Order = order
    };

    private static FormField Field(string key, string label, FieldType type, int order, bool required, string help, string fillRole, List<string>? options = null) => new()
    {
        FieldKey = key,
        Label = label,
        Type = type,
        Order = order,
        IsRequired = required,
        HelpText = help,
        Options = options,
        ValidationRules = new Dictionary<string, JsonElement>
        {
            ["fillRole"] = JsonSerializer.SerializeToElement(fillRole)
        }
    };
}
