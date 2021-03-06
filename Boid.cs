using Godot;
using System.Linq;
using System.Collections.Generic;

public class Boid : KinematicBody2D {
    [Export]
    private string groupName = "boids";

    private static int MinSpeed { get; set; } = 80;

    [Export]
    public int MaxSpeed { get; set; } = 120;

    [Export]
    public int Torque { get; set; } = 25;

    [Export]
    public bool Chosen { get; set; } = false;

    private int perceptionRadius = 200;
    public int SeparationDistance { get; } = 60;

    private static int stepDegree = 7;
    private float radStep = stepDegree * Mathf.Pi / 180;

    private static Vector2 forward = new Vector2(MinSpeed, 0);

    private Color vis_color = new Color(.867f, .91f, .247f, 0.1f);
    public List<RayCast2D> RayCasts { get; } = new List<RayCast2D>();


    public override void _EnterTree() {
        AddToGroup(groupName);
        AddRayCasts();
    }

    public void UpdateDirection() {
        HashSet<Boid> nodesInPerception = GetNodesInPerception();
        List<Boid> closest = GetClosestPoints(nodesInPerception, 1);

        // if (closest.Count > 0) {
        //     var seperateVector = Separate(closest.First());
        //     var alignmentVector = Align(closest);
        //     var movement = seperateVector + alignmentVector;
        //     // TODO: 
        // }
    }

    private void AddRayCasts() {
        float i = 0;
        while (i < 2 * Mathf.Pi) {
            if (IsInVisibleArea(i)) {
                var rayCast = new RayCast2D();
                AddChild(rayCast);
                rayCast.Enabled = true;
                rayCast.CastTo = new Vector2(0, -perceptionRadius).Rotated(i);
                RayCasts.Add(rayCast);
            }
            i += radStep;
        }
    }

    private bool IsInVisibleArea(float i) {
        // Note: Blindspot between 225° and 315°
        return (i < 5 * Mathf.Pi / 4 || i > 7 * Mathf.Pi / 4);
    }

    public override void _PhysicsProcess(float delta) {
        TeleportOnScreenExit();
        MaintainSpeed(delta);
    }

    private Vector2 Separate(Boid closest) {
        // Todo: Maybe rework separation vector calulcation to the acutal algorithm 
        //   of the paper https://www.diva-portal.org/smash/get/diva2:1154793/FULLTEXT02

        // Todo: It is not adviced to directly set Angular and LinearVelocity often (like every frame)
        //   so this should be changed
        var distanceToNode = Position.DistanceTo(closest.Position);
        if (distanceToNode < SeparationDistance) {
            return closest.Position;
            // var angle = GetAngleTo(closest.Position);
            // AngularVelocity = (-angle) * (1 / (distanceToNode / 2)) * Torque;
            // LinearVelocity = forward.Rotated(Rotation);
        } else {
            return new Vector2();
        }
    }

    private Vector2 Align(List<Boid> closest) {
        //  1/n * Sum direction of all boids
        Vector2 sum = new Vector2();
        foreach (var boid in closest) {
            // sum += boid.LinearVelocity.Normalized();
        }
        Vector2 result = (1/closest.Count) * sum;
        System.Console.Write(result);
        return result;
        // var angle = GetAngleTo(result);
        // AppliedTorque = angle;
        // AppliedForce = forward.Rotated(Rotation);
    }

    private List<Boid> GetClosestPoints(HashSet<Boid> nodesInPerception, int amount) {
        List<Boid> closest = new List<Boid>();
        if (nodesInPerception.Count > 0)
            closest = nodesInPerception.OrderBy(node => Position.DistanceTo(node.Position)).Take(amount).ToList();

        return closest;
    }

    private HashSet<Boid> GetNodesInPerception() {
        var setOfColliders = new HashSet<Boid>();
        RayCasts.ForEach(rayCast => {
            if (rayCast.IsColliding()) {
                var coll = rayCast.GetCollider();
                if (coll is Boid) {
                    setOfColliders.Add(coll as Boid);
                }
            }
        });
        return setOfColliders;
    }

    public override void _Draw() {
        base._Draw();
        if (Chosen) {
            DrawCircle(new Vector2(), perceptionRadius, vis_color);
            float i = 0;
            while (i < 2 * Mathf.Pi) {
                if (IsInVisibleArea(i)) {
                    DrawLine(new Vector2(0, 0), new Vector2(0, -perceptionRadius).Rotated(i), new Color("#ff8888"), 1);
                }
                i += radStep;
            }
        }
    }

    private void TeleportOnScreenExit() {
        var screensize = GetViewportRect().Size;
        if (Position.x < 0)
            Position = new Vector2(screensize.x, Position.y);
        if (Position.x > screensize.x)
            Position = new Vector2(0, Position.y);
        if (Position.y < 0)
            Position = new Vector2(Position.x, screensize.y);
        if (Position.y > screensize.y)
            Position = new Vector2(Position.x, 0);
    }

    private void MaintainSpeed(float delta) {
        MoveAndCollide(new Vector2(MinSpeed, 0).Rotated(Rotation) * delta);
    }
}
