using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.AspNetCore.Hosting;
using ElectronNET.API;
using ElectronNET.API.Entities;
using NoteFlow.Models;
using NoteFlow.Services;

namespace NoteFlow
{
    public class Startup
    {
        private readonly HashSet<string> _shownReminderNotifications = new HashSet<string>();
        private readonly object _reminderLock = new object();
        private CancellationTokenSource? _reminderNotificationCts;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");

                if (!HybridSupport.IsElectronActive)
                    app.UseHsts();
            }

            if (!HybridSupport.IsElectronActive)
                app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();


                endpoints.MapGet("/", context =>
                    {
                        context.Response.Redirect("/MainScreen");
                        return Task.CompletedTask;
                        // context.Response.Redirect("/MainPage");
                    });
            }
            );

            if (HybridSupport.IsElectronActive)
            {
                CreateWindow();
            }
        }

        private async void CreateWindow()
        {
            Electron.App.SetAppUserModelId("com.NoteFlow.app");

            var window = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
            {
                // Текущий размер окна
                Width = 1920,
                Height = 1080,

                // ОГРАНИЧЕНИЯ МАСШТАБИРОВАНИЯ
                MinWidth = 800,     // Минимальная ширина
                MinHeight = 800,    // Минимальная высота  
                MaxWidth = 2560,    // Максимальная ширина
                MaxHeight = 1449,   // Максимальная высота

                // Дополнительные настройки поведения
                Resizable = true,       // Разрешить изменение размера
                Maximizable = true,     // Разрешить максимизацию
                Minimizable = true,     // Разрешить минимизацию
                Fullscreenable = true, // Запретить полноэкранный режим

                // Центрирование окна
                Center = true,

                // Другие настройки (опционально)
                Show = true,           // Не показывать сразу
                Title = "NoteFlow",
                AutoHideMenuBar = true,
                Frame = false
            });

            // Обработчики для кнопок
            await Electron.IpcMain.On("minimize-window", (args) =>
            {
                window.Minimize();
            });

            await Electron.IpcMain.On("maximize-window", (args) =>
            {
                window.Maximize();  // Просто максимизируем каждый раз
            });

            await Electron.IpcMain.On("unmaximize-window", (args) =>
            {
                window.Unmaximize();  // И отдельно для восстановления
            });
            
            await Electron.IpcMain.On("close-window", (args) =>
            {
                window.Close();
            });

            bool notificationsSupported = await Electron.Notification.IsSupportedAsync();
            Console.WriteLine($"Electron notifications supported: {notificationsSupported}");

            StartReminderNotificationLoop();

            window.OnClosed += () =>
            {
                _reminderNotificationCts?.Cancel();
                Electron.App.Quit();
                Electron.App.Exit();
            };
        }

        private void StartReminderNotificationLoop()
        {
            _reminderNotificationCts?.Cancel();
            _reminderNotificationCts = new CancellationTokenSource();
            TryShowDueReminderNotifications();

            _ = Task.Run(async () =>
            {
                var timer = new PeriodicTimer(TimeSpan.FromSeconds(20));

                try
                {
                    while (await timer.WaitForNextTickAsync(_reminderNotificationCts.Token))
                    {
                        TryShowDueReminderNotifications();
                    }
                }
                catch (OperationCanceledException)
                {
                    // App is closing.
                }
                finally
                {
                    timer.Dispose();
                }
            });
        }

        private void TryShowDueReminderNotifications()
        {
            if (!HybridSupport.IsElectronActive)
                return;

            var now = DateTime.Now;
            var remindersSnapshot = LoadRemindersFromDisk();

            foreach (Reminder reminder in remindersSnapshot)
            {
                DateTime scheduledAt = reminder.ReminderExpires;
                if (reminder.IsRepeating)
                {
                    scheduledAt = new DateTime(
                        now.Year,
                        now.Month,
                        now.Day,
                        reminder.ReminderExpires.Hour,
                        reminder.ReminderExpires.Minute,
                        0
                    );
                }

                TimeSpan elapsed = now - scheduledAt;
                if (elapsed < TimeSpan.Zero || elapsed > TimeSpan.FromMinutes(1))
                    continue;

                string notificationKey = BuildReminderNotificationKey(reminder, now);
                bool canNotify;

                lock (_reminderLock)
                {
                    canNotify = _shownReminderNotifications.Add(notificationKey);
                }

                if (!canNotify)
                    continue;

                string title = string.IsNullOrWhiteSpace(reminder.ReminderTitle)
                    ? "Напоминание NoteFlow"
                    : reminder.ReminderTitle;

                string body = string.IsNullOrWhiteSpace(reminder.ReminderDescription)
                    ? "Пора выполнить запланированное действие."
                    : reminder.ReminderDescription;

                Electron.Notification.Show(new NotificationOptions(title, body));
                Console.WriteLine($"Reminder notification sent: {title} at {now:yyyy-MM-dd HH:mm:ss}");
            }
        }

        private static List<Reminder> LoadRemindersFromDisk()
        {
            try
            {
                if (!Directory.Exists(StorageService._remindersPath))
                    return new List<Reminder>();

                return Directory
                    .GetFiles(StorageService._remindersPath, "*.md")
                    .Select(path => new Reminder(path))
                    .OrderBy(x => x.ReminderExpires)
                    .ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Reminder loading failed: {e.Message}");
                return new List<Reminder>();
            }
        }

        private static string BuildReminderNotificationKey(Reminder reminder, DateTime now)
        {
            string baseKey = string.IsNullOrWhiteSpace(reminder.ReminderPath)
                ? reminder.ReminderTitle
                : reminder.ReminderPath;

            if (!reminder.IsRepeating)
                return baseKey;

            return $"{baseKey}:{now:yyyyMMdd}";
        }
    }

}
