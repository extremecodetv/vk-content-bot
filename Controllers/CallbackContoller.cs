using System;
using System.Linq;
using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace ContentBot.Controllers
{
    [Route("callback")]
    public class CallbackContoller : Controller
    {
        private readonly IConfigurationRoot _configuration;
        
        public CallbackContoller(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("vk")]
        public string Vk([FromBody] dynamic json)
        {
            if (json["type"] == "confirmation")
            {
                return _configuration["Keys:Confirmation"];
            }

            if (json["type"] == "wall_post_new")
            {
                var obj = json["object"];
                var text = obj["text"];

                if (text.ToString().Contains("#задача"))
                {
                    var attachments = obj["attachments"];
                    if (attachments != null)
                    {
                        // ReSharper disable once AssignNullToNotNullAttribute
                        var lastAttachment = (attachments as IEnumerable<dynamic>).Last();
                        var photo = lastAttachment["photo"];
                        var url = photo["photo_807"];

                        if (url != null)
                        {
                            this.SendToTelegram(url.ToString(), text?.ToString());
                        }
                    }
                }
            }

            return "ok";
        }

        private void SendToTelegram(string url, string text)
        {
            var client = new TelegramBotClient(_configuration["Keys:Telegram"]);
            var chatId = new ChatId(_configuration["Keys:Username"]);
            
            var fileToSend = new FileToSend(new Uri(url));
            client.SendPhotoAsync(chatId, fileToSend, text).GetAwaiter().GetResult();
        }

        [HttpPost("telegram")]
        public OkResult Telegram([FromBody] Update updates)
        {
            return new OkResult();
        }
        
    }
}