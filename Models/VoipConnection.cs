using SocketIOClient;
using System;
using System.Threading.Tasks;

namespace Tatehama_tetudou_denwa_PCclient.Models
{
    public class VoipConnection
    {
    private SocketIOClient.SocketIO? _socket;
    public event Action<byte[]>? VoiceReceived;

        public async Task ConnectAsync(string url, string userId)
        {
            _socket = new SocketIOClient.SocketIO(url);
            _socket.OnConnected += async (sender, e) =>
            {
                if (_socket != null)
                    await _socket.EmitAsync("register", userId);
            };
            _socket.On("voice", response =>
            {
                var bytes = response.GetValue<byte[]>();
                VoiceReceived?.Invoke(bytes);
            });
            await _socket.ConnectAsync();
        }

        public async Task SendVoiceAsync(string roomId, byte[] data)
        {
            if (_socket != null)
                await _socket.EmitAsync("voice", roomId, data);
        }

        public async Task JoinRoomAsync(string roomId)
        {
            if (_socket != null)
                await _socket.EmitAsync("join", roomId);
        }

        public async Task LeaveRoomAsync(string roomId)
        {
            if (_socket != null)
                await _socket.EmitAsync("leave", roomId);
        }
    }
}
