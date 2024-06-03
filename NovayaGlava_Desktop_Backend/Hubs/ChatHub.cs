using Amazon.Runtime.Internal.Util;
using ClassLibForNovayaGlava_Desktop;
using ClassLibForNovayaGlava_Desktop.UserModel;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace NovayaGlava_Desktop_Backend.Hubs
{
    public interface IChatClient
    {
        public Task ReceiveMessage(string userName, string message);
    }

    public class ChatHub : Hub<IChatClient>
    {
        IDistributedCache _cache;

        public ChatHub(IDistributedCache cache)
        {
            _cache = cache;
        }


        public override async Task OnConnectedAsync()
        {
            await Clients.All.ReceiveMessage("System", $"{Context.ConnectionId} подключился к чату");
            await base.OnConnectedAsync();
        }

        public async Task JoinChat(UserConnection connection)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, connection.ChatId);

            string jsonUserConnection = JsonConvert.SerializeObject(connection);
            await _cache.SetStringAsync(Context.ConnectionId, jsonUserConnection);

            await Clients
                .Group(connection.ChatId)
                .ReceiveMessage("System", $"user с id [{Context.ConnectionId}] присоединился к чату ${connection.ChatId}");
        }

        public async Task SendMessage(string message)
        {
            string jsonUserConnection = _cache.GetString(Context.ConnectionId);
            UserConnection connection = JsonConvert.DeserializeObject<UserConnection>(jsonUserConnection);

            if (connection != null)
            {
                await Clients
                    .Group(connection.ChatId)
                    .ReceiveMessage(connection.UserNickName, message);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {

            string jsonUserConnection = _cache.GetString(Context.ConnectionId);
            UserConnection connection = JsonConvert.DeserializeObject<UserConnection>(jsonUserConnection);

            if (connection != null)
            {
                await _cache.RemoveAsync(Context.ConnectionId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, connection.ChatId);

                await Clients
                    .Group(connection.ChatId)
                    .ReceiveMessage("System", $"user с id [{Context.ConnectionId}] отключился от {connection.ChatId}");
            }
        }
    }
}
