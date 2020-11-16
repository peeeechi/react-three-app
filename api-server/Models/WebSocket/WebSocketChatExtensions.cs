using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

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
        /// ���[�g�����path�ɑ΂��� WebSocketWebSocket��Handler��Map���܂�
        /// </summary>
        /// <param name="app"></param>
        /// <param name="path"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static IApplicationBuilder MapWebSocketChatMiddleware(this IApplicationBuilder app, PathString path,  WebSocketHandler handler)
        {
            return app.Map(path, _app => _app.UseMiddleware<WebSocketChatMiddleware>(handler));
        }
    }
}
