using Godot;
using System;
using Godot.Collections;

public partial class HandTracking : Node {
	public UDPReceiver udpReceiver = new UDPReceiver();

	public Array<Node> handPoints;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		GD.Print(( GetNode("../Hand/Points").GetChildCount()));
		GD.Print("Setting up hand tracking...");
		handPoints = GetNode("../Hand/Points").GetChildren();
		GD.Print("Size:" + handPoints.Count);
		udpReceiver.Start();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		if (udpReceiver == null) {
			GD.PrintErr("UDP Receiver not setup.");
			return;
		}
		string data = udpReceiver.data;
		data = data.Remove(0, 1);
		data = data.Remove(data.Length - 1, 1);
		// GD.Print(data);
		string[] points = data.Split(',');
		for (int i = 0; i < 21; i++) {
			float x = float.Parse(points[i * 3]) / 100;
			float y = float.Parse(points[i * 3 + 1]) / 100;
			float z = float.Parse(points[i * 3 + 2]) / 100;
			Node3D point = (Node3D) handPoints[i];
			point.Position = point.GetParentNode3D().Position + new Vector3(-1 * x, y, z);
		}
	}
}
