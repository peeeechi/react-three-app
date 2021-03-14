using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;


namespace api_server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        void OnStopping()
        {
            Console.WriteLine("Shutdown...");
        }


        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddLogging(builder =>
            {
                builder.AddConsole()
                .AddDebug()
                .AddFilter<ConsoleLoggerProvider>(category: null, LogLevel.Debug)
                .AddFilter<DebugLoggerProvider>(category: null, LogLevel.Debug);
            });

            // DI�R���e�i�֓o�^
            services.AddWebSocketChatHandler()
                .LoadAppSettings(AppConst.SETTING_FILE_PATH)
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //app.UseExceptionHandler("/Home/Error");
                //app.UseHsts();
            }


            //  Todo 
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });


            // WebSocket  �̐ݒ� ----------------------------------------------- ��

            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = AppConst.WebSocketReceiveBufferSize,
            };

            app.UseWebSockets(webSocketOptions);

            

            //var webSocketReceiver = new WebSocketReceiver();
            //app.Use(webSocketReceiver.Receive);

            //app.MapWebSocketChatMiddleware("/chat", serviceProvider.GetService<ChatMessageHandler>());
            //app.MapWebSocketChatMiddleware("/force-sensor-test/1", serviceProvider.GetService<ForceSensorTestHandlerSingle>());
            //app.MapWebSocketChatMiddleware("/force-sensor-test/2", serviceProvider.GetService<ForceSensorTestHandlerDouble>());
            //app.MapWebSocketChatMiddleware("/force-sensor/1", serviceProvider.GetService<ForceSensorHandlerSingle>());
            app.MapWebSocketChatMiddleware("/force-sensor/1", serviceProvider.GetService<SensorInfoHandler>());
            //app.MapWebSocketChatMiddleware("/force-sensor/2", serviceProvider.GetService<ForceSensorHandlerDouble>());
            //app.MapWebSocketChatMiddleware("/robot-info", serviceProvider.GetService<RobotInfoHandler>());

            // WebSocket  �̐ݒ� ----------------------------------------------- ��

            app.UseHttpsRedirection();
            //app.UseStaticFiles();         // Todo
            //app.UseCookiePolicy();        // Todo
            app.UseRouting();               // Todo

            app.UseAuthorization();         // Todo

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
