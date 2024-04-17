using Godot;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPReceiver {
	private Thread receiveThread;
	public UdpClient client;
	public int port = 5052;
	public bool startReceiving = true;
	public bool printToConsole = true;
	public string data = "";

	public void Start() {
		GD.Print("Setup port connection...");
		receiveThread = new Thread(
			new ThreadStart(ReceiveData));
		receiveThread.IsBackground = true;
		GD.Print("Starting receiving thread...");
		receiveThread.Start();
	}

	private void ReceiveData() {
		GD.Print("Receiving data...");
		client = new UdpClient(port);
		while (startReceiving) {
			try {
				IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
				byte[] dataByte = client.Receive(ref anyIP);
				data = Encoding.UTF8.GetString(dataByte);

				if (printToConsole) {
					GD.Print(data);
				}
			}
			catch (Exception err) {
				GD.Print(err.ToString());
			}
		}
	}
}
