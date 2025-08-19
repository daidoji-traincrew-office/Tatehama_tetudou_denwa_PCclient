using System;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;
using Tatehama_tetudou_denwa_PCclient.Models;

namespace Tatehama_tetudou_denwa_PCclient.Models
{
    public class ServerConnection
    {
        private HubConnection? _connection;
    public event Action? NoAnswerReceived;
    public event Action<string>? CallOkReceived;

        public async Task ConnectAsync(string url, string clientId)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(url)
                .WithAutomaticReconnect()
                .Build();

            _connection.On<string>("ReceiveMessage", (message) =>
            {
                if (message == "no_answer")
                {
                    NoAnswerReceived?.Invoke();
                }
                else if (message.StartsWith("call_ok:"))
                {
                    CallOkReceived?.Invoke(message.Substring("call_ok:".Length));
                }
                // ...existing code...
            });

            await _connection.StartAsync();
            await _connection.InvokeAsync("UpdateState", clientId, PhoneState.Idle.ToString());
        }

        public async Task UpdateState(string clientId, PhoneState state)
        {
            if (_connection is not null)
                await _connection.InvokeAsync("UpdateState", clientId, state.ToString());
        }

        public async Task CallRequest(string fromId, string toId)
        {
            if (_connection is not null)
                await _connection.InvokeAsync("CallRequest", fromId, toId);
        }

        public async Task AnswerCall(string fromId, string toId)
        {
            if (_connection is not null)
                await _connection.InvokeAsync("AnswerCall", fromId, toId);
        }

        public async Task EndCall(string fromId, string toId)
        {
            if (_connection is not null)
                await _connection.InvokeAsync("EndCall", fromId, toId);
        }
    }
}
