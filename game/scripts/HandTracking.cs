using Godot;
using System;
using System.Data.Common;
using Godot.Collections;

public partial class HandTracking : Node {
	public UDPReceiver udpReceiver;

	public Array<Node3D> handPoints;
	public Array<Vector3> handPositions;
    
	public Vector3 offset = new Vector3(0, 0, 0);

	private Button calibrateButton;

	private string pointsPath = "../Hand/Points";
	private float minDistanceAway = 0.05f;

	private float calibratedScale = 1;

	private Skeleton3D skel;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		GD.Print("Setting up hand tracking...");
		handPoints = new Array<Node3D>();
		handPositions = new Array<Vector3>();
		foreach (Node child in GetNode(pointsPath).GetChildren()){
			handPoints.Add((Node3D) child);
			handPositions.Add(new Vector3(0, 0, 0));
		}
		GD.Print("Size:" + handPoints.Count);
		udpReceiver = new UDPReceiver();
		udpReceiver.Start();
		calibrateButton = new Button();
		calibrateButton.Text = "Calibrate";
		calibrateButton.Pressed += Calibrate;

		skel = (Skeleton3D) GetNode("../RiggedHand/Armature/Skeleton3D");
		//		skel.FindBone("")

		AddChild(calibrateButton);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		if (udpReceiver == null) {
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
			Vector3 newPoint = new Vector3(-1 * x, y, z);	
			
			Node3D point = (Node3D) handPoints[i];

			float distance = handPositions[9].DistanceTo(handPositions[0]);
			// Vector3 zOffset = new Vector3(0, 0, (distance / calibratedScale) * 10);
			Vector3 zOffset = new Vector3(0, 0, (distance / calibratedScale) * 10);
			
			if (newPoint.DistanceTo(handPositions[i]) > minDistanceAway) {
				handPositions[i] = new Vector3(-1 * x, y, z);
			}

			int id = skel.FindBone(i.ToString());
			skel.SetBonePosePosition(id, handPositions[i] - offset);
			if (id != 0 && id != 5 && id != 9 && id != 13 && id != 17) {
				skel.SetBonePoseRotation(id, new Quaternion(
					));		
			}
			// skel.SetBonePoseRotation(id, );
			
			// point.Position = handPositions[i] - offset - zOffset;
			point.Position = handPositions[i] - offset;
		}
	}

	public void Calibrate() {
		Vector3 center = handPositions[9];
		offset = center;

		calibratedScale = handPositions[9].DistanceTo(handPositions[0]);
	}

	private void DrawDebugSphere(Vector3 pos, float rad) {
		var sceneRoot = GetTree().Root.GetChildren()[0];
		var sphere = new SphereMesh();
		sphere.RadialSegments = 4;
		sphere.Rings = 4;
		sphere.Radius = rad;
		sphere.Height = rad * 2;

		var material = new StandardMaterial3D();
		material.AlbedoColor = new Color(0, 1, 0);
		
		sphere.SurfaceSetMaterial(0, material);
		var node = new MeshInstance3D();
		node.MaterialOverride = material;
		node.Position = pos;
		sceneRoot.AddChild(node);
	}
}
