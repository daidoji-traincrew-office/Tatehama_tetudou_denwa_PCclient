using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;
using Tatehama_tetudou_denwa_PCclient.Models;

namespace Tatehama_tetudou_denwa_PCclient.Models
{
    public class ServerConnection
    {
        private HubConnection? _connection;
        public event Action<string, PhoneState>? StateChanged;
        public event Action<string>? IncomingCall;
        public event Action<string>? CallAnswered;
        public event Action? CallEnded;

        public async Task ConnectAsync(string url, string clientId)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(url)
                .WithAutomaticReconnect()
                .Build();

            _connection.On<string, string>("StateChanged", (id, state) =>
            {
                StateChanged?.Invoke(id, Enum.Parse<PhoneState>(state));
            });
            _connection.On<string>("IncomingCall", fromId => IncomingCall?.Invoke(fromId));
            _connection.On<string>("CallAnswered", toId => CallAnswered?.Invoke(toId));
            _connection.On("CallEnded", () => CallEnded?.Invoke());

            await _connection.StartAsync();
            await _connection.InvokeAsync("UpdateState", clientId, PhoneState.Idle.ToString());
        }

        public async Task UpdateState(string clientId, PhoneState state)
        {
            if (_connection != null)
                await _connection.InvokeAsync("UpdateState", clientId, state.ToString());
        }

        public async Task CallRequest(string fromId, string toId)
        {
            if (_connection != null)
                await _connection.InvokeAsync("CallRequest", fromId, toId);
        }

        public async Task AnswerCall(string fromId, string toId)
        {
            if (_connection != null)
                await _connection.InvokeAsync("AnswerCall", fromId, toId);
        }

        public async Task EndCall(string fromId, string toId)
        {
            if (_connection != null)
                await _connection.InvokeAsync("EndCall", fromId, toId);
        }
    }
}
