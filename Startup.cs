using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

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
        private BrowserWindow? _mainWindow;
        private readonly object _windowLock = new object();
        private Task? _windowInitialization;
        private StorageService currStorage = new StorageService();

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddElectron();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            bool isElectronRuntime = IsElectronRuntime();

            if (!HybridSupport.IsElectronActive && isElectronRuntime)
            {
                Console.WriteLine("Electron runtime detected via CLI args; HybridSupport.IsElectronActive is false.");
            }
            else if (!isElectronRuntime)
            {
                Console.WriteLine("Electron runtime not detected; window creation is skipped.");
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");

                if (!isElectronRuntime)
                    app.UseHsts();
            }

            if (!isElectronRuntime)
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

            if (isElectronRuntime)
            {
                _ = EnsureElectronWindowAsync();
            }
        }

        private Task EnsureElectronWindowAsync()
        {
            lock (_windowLock)
            {
                if (_windowInitialization != null)
                    return _windowInitialization;

                if (Electron.App.IsReady)
                {
                    _windowInitialization = CreateWindowAsync();
                    _windowInitialization.ContinueWith(task =>
                    {
                        Console.WriteLine($"Electron window creation failed: {task.Exception?.GetBaseException().Message}");
                    }, TaskContinuationOptions.OnlyOnFaulted);
                    return _windowInitialization;
                }

                var tcs = new TaskCompletionSource<object?>();
                _windowInitialization = tcs.Task;

                Electron.App.Ready += async () =>
                {
                    try
                    {
                        await CreateWindowAsync();
                        tcs.TrySetResult(null);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Electron window creation failed: {e.Message}");
                        tcs.TrySetException(e);
                    }
                };

                return _windowInitialization;
            }
        }

        private async Task CreateWindowAsync()
        {
            if (_mainWindow != null)
                return;

            if (OperatingSystem.IsWindows())
                Electron.App.SetAppUserModelId("com.NoteFlow.app");

            Electron.WindowManager.IsQuitOnWindowAllClosed = true;

            string? appIconPath = ResolveAppIconPath();
            string? loadUrl = ResolveElectronLoadUrl();
            if (!string.IsNullOrWhiteSpace(loadUrl))
            {
                Console.WriteLine($"Electron load URL: {loadUrl}");
            }
            else
            {
                Console.WriteLine("Electron load URL is empty; falling back to about:blank.");
            }

            if (OperatingSystem.IsMacOS() && !string.IsNullOrWhiteSpace(appIconPath))
            {
                Electron.Dock.SetIcon(appIconPath);
            }

            _mainWindow = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
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
                Show = true,
                Title = "NoteFlow",
                AutoHideMenuBar = true,
                Frame = false,
                Icon = appIconPath
            }, string.IsNullOrWhiteSpace(loadUrl) ? "about:blank" : loadUrl);

            _mainWindow.OnReadyToShow += () =>
            {
                _mainWindow?.Show();
                _mainWindow?.Focus();
            };

            // Обработчики для кнопок
            await Electron.IpcMain.On("minimize-window", (args) =>
            {
                _mainWindow?.Minimize();
            });

            await Electron.IpcMain.On("maximize-window", (args) =>
            {
                _mainWindow?.Maximize();  // Просто максимизируем каждый раз
            });

            await Electron.IpcMain.On("unmaximize-window", (args) =>
            {
                _mainWindow?.Unmaximize();  // И отдельно для восстановления
            });
            
            await Electron.IpcMain.On("close-window", (args) =>
            {
                _mainWindow?.Close();
            });

            bool notificationsSupported = await Electron.Notification.IsSupportedAsync();
            Console.WriteLine($"Electron notifications supported: {notificationsSupported}");

            StartReminderNotificationLoop();

            _mainWindow.OnClosed += () =>
            {
                _mainWindow = null;

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
                var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

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
                catch (Exception e)
                {
                    Console.WriteLine($"Reminder loop failed: {e.Message}");
                }
                finally
                {
                    timer.Dispose();
                }
            });
        }

        private void TryShowDueReminderNotifications()
        {
            if (!IsElectronRuntime())
                return;

            var now = DateTime.Now;
            var remindersSnapshot = LoadRemindersFromDisk();

            foreach (Reminder reminder in remindersSnapshot)
            {
                if (!ShouldNotifyNow(reminder, now, out DateTime scheduledAt))
                    continue;

                string notificationKey = BuildReminderNotificationKey(reminder, scheduledAt);
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

                try
                {
                    var options = new NotificationOptions(title, body);
                    string? iconPath = ResolveAppIconPath();
                    if (!string.IsNullOrWhiteSpace(iconPath))
                        options.Icon = iconPath;

                    Electron.Notification.Show(options);
                    Console.WriteLine($"Reminder notification sent: {title} at {now:yyyy-MM-dd HH:mm:ss}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Reminder notification send failed: {e.Message}");
                }
            }
        }

        private List<Reminder> LoadRemindersFromDisk()
        {
            try
            {
                if (!Directory.Exists(currStorage._remindersPath))
                    return new List<Reminder>();

                return Directory
                    .GetFiles(currStorage._remindersPath, "*.md")
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

        private static string BuildReminderNotificationKey(Reminder reminder, DateTime scheduledAt)
        {
            string baseKey = string.IsNullOrWhiteSpace(reminder.ReminderPath)
                ? reminder.ReminderTitle
                : reminder.ReminderPath;

            if (!reminder.IsRepeating)
                return baseKey;

            return $"{baseKey}:{scheduledAt:yyyyMMdd}";
        }

        private static bool ShouldNotifyNow(Reminder reminder, DateTime now, out DateTime scheduledAt)
        {
            scheduledAt = reminder.ReminderExpires;
            TimeSpan maxDelay = TimeSpan.FromHours(18);

            if (reminder.IsRepeating)
            {
                if (now.Date < reminder.ReminderExpires.Date)
                    return false;

                scheduledAt = new DateTime(
                    now.Year,
                    now.Month,
                    now.Day,
                    reminder.ReminderExpires.Hour,
                    reminder.ReminderExpires.Minute,
                    0
                );
            }

            if (now < scheduledAt)
                return false;

            return now - scheduledAt <= maxDelay;
        }

        private static string? ResolveAppIconPath()
        {
            string[] candidates = OperatingSystem.IsWindows()
                ? new[]
                {
                    Path.Combine(AppContext.BaseDirectory, "wwwroot", "Images", "Logo.ico"),
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Logo.ico"),
                    Path.Combine(AppContext.BaseDirectory, "Images", "Logo.ico"),
                    Path.Combine(AppContext.BaseDirectory, "wwwroot", "Images", "Logo.png"),
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Logo.png"),
                    Path.Combine(AppContext.BaseDirectory, "Images", "Logo.png")
                }
                : OperatingSystem.IsMacOS()
                ? new[]
                {
                    Path.Combine(AppContext.BaseDirectory, "wwwroot", "Images", "Logo.png"),
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Logo.png"),
                    Path.Combine(AppContext.BaseDirectory, "Images", "Logo.png"),
                    Path.Combine(AppContext.BaseDirectory, "wwwroot", "Images", "Logo.icns"),
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Logo.icns"),
                    Path.Combine(AppContext.BaseDirectory, "Images", "Logo.icns")
                }
                : new[]
                {
                    Path.Combine(AppContext.BaseDirectory, "wwwroot", "Images", "Logo.png"),
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Logo.png"),
                    Path.Combine(AppContext.BaseDirectory, "Images", "Logo.png")
                };

            return candidates.FirstOrDefault(File.Exists);
        }

        private static string? ResolveElectronLoadUrl()
        {
            if (TryGetElectronWebPort(out string port))
                return $"http://localhost:{port}";

            if (!string.IsNullOrWhiteSpace(BridgeSettings.WebPort))
                return $"http://localhost:{BridgeSettings.WebPort}";

            return null;
        }

        private static bool TryGetElectronWebPort(out string port)
        {
            port = string.Empty;
            string[] args = Environment.GetCommandLineArgs();

            foreach (string arg in args)
            {
                if (!arg.StartsWith("/electronWebPort", StringComparison.OrdinalIgnoreCase) &&
                    !arg.StartsWith("--electronWebPort", StringComparison.OrdinalIgnoreCase) &&
                    !arg.StartsWith("/electron-web-port", StringComparison.OrdinalIgnoreCase) &&
                    !arg.StartsWith("--electron-web-port", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                int equalsIndex = arg.IndexOf('=');
                if (equalsIndex <= 0 || equalsIndex == arg.Length - 1)
                    continue;

                string candidate = arg.Substring(equalsIndex + 1);
                if (candidate.Length == 0)
                    continue;

                port = candidate;
                return true;
            }

            return false;
        }

        private static bool IsElectronRuntime()
        {
            if (HybridSupport.IsElectronActive)
                return true;

            string[] args = Environment.GetCommandLineArgs();
            return args.Any(arg =>
                arg.StartsWith("--electronPort", StringComparison.OrdinalIgnoreCase) ||
                arg.StartsWith("/electronPort", StringComparison.OrdinalIgnoreCase) ||
                arg.StartsWith("--electronWebPort", StringComparison.OrdinalIgnoreCase) ||
                arg.StartsWith("/electronWebPort", StringComparison.OrdinalIgnoreCase) ||
                arg.StartsWith("--electron-port", StringComparison.OrdinalIgnoreCase) ||
                arg.StartsWith("/electron-port", StringComparison.OrdinalIgnoreCase) ||
                arg.StartsWith("--electron-web-port", StringComparison.OrdinalIgnoreCase) ||
                arg.StartsWith("/electron-web-port", StringComparison.OrdinalIgnoreCase) ||
                arg.StartsWith("--aspCoreBackendPort", StringComparison.OrdinalIgnoreCase) ||
                arg.StartsWith("/aspCoreBackendPort", StringComparison.OrdinalIgnoreCase) ||
                arg.StartsWith("--aspCoreBackend-port", StringComparison.OrdinalIgnoreCase) ||
                arg.StartsWith("/aspCoreBackend-port", StringComparison.OrdinalIgnoreCase));
        }
    }

}
