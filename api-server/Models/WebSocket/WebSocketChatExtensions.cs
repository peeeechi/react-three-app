using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Text;
using System;
using Newtonsoft.Json;

namespace api_server
{

    /// <summary>
    /// 
    /// </summary>
    public static class WebSocketChatExtensions
    {
        /// <summary>
        /// WebSocketObjectHolder �� WebSocketHandler ��DI�R���e�i�֓o�^
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddWebSocketChatHandler(this IServiceCollection services)
        {
            services.AddTransient<WebSocketObjectHolder>();     // DI����邽�тɃC���X�^���X�𐶐����܂��B
            //services.AddScoped<WebSocketObjectHolder>();        // ��xDI�����ƁA���HTTP���N�G�X�g���ł���΁A�ŏ���DI���ꂽ�l���g���܂킵�܂��B
            //services.AddSingleton<WebSocketObjectHolder>();     // ��xDI�����ƁA�A�v���P�[�V�������I������܂ŁA�ŏ���DI���ꂽ�l���g���܂킵�܂��B

            foreach (var type in Assembly.GetEntryAssembly().ExportedTypes)
            {
                if (type.GetTypeInfo().BaseType == typeof(WebSocketHandler))
                {
                    services.AddSingleton(type);
                }
            }

            return services;
        }
        
        /// <summary>
        /// ���[�g�����path�ɑ΂��� WebSocket��Handler��Map���܂�
        /// </summary>
        /// <param name="app"></param>
        /// <param name="path"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static IApplicationBuilder MapWebSocketChatMiddleware(this IApplicationBuilder app, PathString path,  WebSocketHandler handler)
        {
            return app.Map(path, _app => _app.UseMiddleware<WebSocketChatMiddleware>(handler));
        }

        public static IServiceCollection LoadAppSettings(this IServiceCollection services, string filePath)
        {
            var dir = Path.GetDirectoryName(filePath);
            var enc = new UTF8Encoding(false);
            Settings settings = null;
            if(!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                settings = new Settings();
                using (var sw = new StreamWriter(filePath, false, enc))
                {
                    var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                    sw.Write(json);
                }
            }
            else
            {
                try
                {
                    string json = null;
                    using (var sr = new StreamReader(filePath, enc))
                    {
                        json        = sr.ReadToEnd();
                        settings    = JsonConvert.DeserializeObject<Settings>(json);
                    }
                }
                catch (System.Exception e)
                {
                    Console.Error.WriteLine(e.ToString());
                    settings = new Settings();
                }
            }

            services.AddSingleton(settings);

            return services;
        }
    }
}
