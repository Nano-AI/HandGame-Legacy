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

	private MeshInstance3D meshInstance3D;
	private ArrayMesh mesh;

	[Export]
	public int handScale = 100;

	[Export]
	public int zoomScale = 20;

	[Export]
	public string[] interactables = { "Fridge" };

	[Export]
	public double interactableDistance = 0.1d;

	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		mesh = new ArrayMesh();
		meshInstance3D = new MeshInstance3D();
		AddChild(meshInstance3D);
		meshInstance3D.Mesh = mesh;
		
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

		skel = GetNode<Skeleton3D>("../Skeleton3D");
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
			float x = float.Parse(points[i * 3]) / handScale;
			float y = float.Parse(points[i * 3 + 1]) / handScale;
			float z = float.Parse(points[i * 3 + 2]) / handScale;
			Vector3 newPoint = new Vector3(-1 * x, y, z);	
			
			Node3D point = (Node3D) handPoints[i];

			float distance = handPositions[1].DistanceTo(handPositions[0]);
			Vector3 zOffset = new Vector3(0, 0, (distance / calibratedScale) * zoomScale);
			
			if (newPoint.DistanceTo(handPositions[i]) > minDistanceAway) {
				handPositions[i] = new Vector3(-1 * x, y, z);
			}

			point.Position = handPositions[i] - offset;
			
			// UpdateHand();
		}

		foreach (string name in interactables) {
			if (GetTree().Root.GetNode<Node3D>("/" + name).Position.DistanceTo(handPoints[8].Position) < interactableDistance) {
				GD.Print("close to " + name);
			}
		}
	}

	public void UpdateHand() {
		for (int i = 5; i < 8;  i++) {
			int id = skel.FindBone(i.ToString());
			Vector3 at = handPositions[i].Normalized();
			Vector3 next = handPositions[i + 1].Normalized();
			Vector3 sum = (at + next).Normalized();
			float angle = sum.Dot(next);
			Vector3 axis = sum.Cross(next).Normalized();

			if (!sum.IsNormalized() || !axis.IsNormalized()) {
				return;
			}

//			Quaternion q;
//			q = new Quaternion(axis, angle);
//			Vector3 a = at.Cross(next);
//			q.X = a.X; q.Y = a.Y; q.Z = a.Z;
//			q.W = (float)(Mathf.Sqrt(Mathf.Pow(at.Length(), 2) * Math.Pow(next.Length(), 2)) + at.Dot(next));
			// skel.SetBonePoseRotation(id, q.Normalized());
			// GD.Print(q.Normalized().GetEuler());
		}
	}

	public void Calibrate() {
		Vector3 center = handPositions[9];
		offset = center;

		calibratedScale = handPositions[9].DistanceTo(handPositions[0]);
	}
}
