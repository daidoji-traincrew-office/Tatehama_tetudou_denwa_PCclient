
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Tatehama_tetudou_denwa_PCclient.Models;

namespace Tatehama_tetudou_denwa_PCclient.Models
{
    public class ServerConnection
    {
    private string? _myConnectionId;
    public string? GetConnectionId() => _myConnectionId;
        private HubConnection? _connection;

        public event Action? NoAnswerReceived;
        public event Action<string>? CallOkReceived;
        public event Action<string>? RingingReceived;
        public event Action? EndCallReceived;

        public async Task RegisterPhoneNumber(string phoneNumber)
        {
            if (_connection is not null)
                await _connection.InvokeAsync("RegisterPhoneNumber", phoneNumber);
        }

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
                else if (message.StartsWith("ringing:"))
                {
                    RingingReceived?.Invoke(message.Substring("ringing:".Length));
                }
                else if (message.StartsWith("call_ok:"))
                {
                    CallOkReceived?.Invoke(message.Substring("call_ok:".Length));
                }
                else if (message == "end_call")
                {
                    EndCallReceived?.Invoke();
                }
                else if (message.StartsWith("connid:"))
                {
                    _myConnectionId = message.Substring("connid:".Length);
                }
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
