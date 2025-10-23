using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ElectronNET.API;
using ElectronNET.API.Entities;
using NoteFlow.Pages;

namespace NoteFlow
{
    public class Startup
    {
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
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();


                endpoints.MapGet("/", async context =>
                    {
                        context.Response.Redirect("/MainPage");
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
            Electron.IpcMain.On("minimize-window", (args) =>
            {
                window.Minimize();
            });

            Electron.IpcMain.On("maximize-window", (args) =>
            {
                window.Maximize();  // Просто максимизируем каждый раз
            });

            Electron.IpcMain.On("unmaximize-window", (args) =>
            {
                window.Unmaximize();  // И отдельно для восстановления
            });
            
            Electron.IpcMain.On("close-window", (args) =>
            {
                window.Close();
            });

            window.OnClosed += () =>
            {
                Electron.App.Quit();
            };
        }
    }

}