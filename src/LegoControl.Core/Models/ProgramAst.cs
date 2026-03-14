using System.Text.Json;
using System.Text.Json.Serialization;

namespace LegoControl.Core.Models;

// ── Expression nodes (value-returning) ────────────────────────────────────

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(NumberLiteralNode), typeDiscriminator: "number")]
[JsonDerivedType(typeof(MathArithNode),     typeDiscriminator: "math")]
[JsonDerivedType(typeof(CompareNode),       typeDiscriminator: "compare")]
[JsonDerivedType(typeof(LogicOpNode),       typeDiscriminator: "logic")]
[JsonDerivedType(typeof(NegateNode),        typeDiscriminator: "negate")]
[JsonDerivedType(typeof(GetVarNode),        typeDiscriminator: "getVar")]
[JsonDerivedType(typeof(SensorValueNode),   typeDiscriminator: "sensor")]
public abstract record ExprNode;

public record NumberLiteralNode(double Value) : ExprNode;

// Op: ADD, MINUS, MULTIPLY, DIVIDE, POWER
public record MathArithNode(ExprNode Left, string Op, ExprNode Right) : ExprNode;

// Op: EQ, NEQ, LT, LTE, GT, GTE
public record CompareNode(ExprNode Left, string Op, ExprNode Right) : ExprNode;

// Op: AND, OR
public record LogicOpNode(ExprNode Left, string Op, ExprNode Right) : ExprNode;

public record NegateNode(ExprNode Operand) : ExprNode;

public record GetVarNode(string Name) : ExprNode;

// SensorType: distance, color, reflection, ambient
public record SensorValueNode(string PortId, string SensorType) : ExprNode;

// ── Statement nodes (executable) ──────────────────────────────────────────

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(DriveNode),       typeDiscriminator: "drive")]
[JsonDerivedType(typeof(SteerNode),       typeDiscriminator: "steer")]
[JsonDerivedType(typeof(StopNode),        typeDiscriminator: "stop")]
[JsonDerivedType(typeof(WaitNode),        typeDiscriminator: "wait")]
[JsonDerivedType(typeof(RepeatNode),      typeDiscriminator: "repeat")]
[JsonDerivedType(typeof(IfNode),          typeDiscriminator: "if")]
[JsonDerivedType(typeof(WhileNode),       typeDiscriminator: "while")]
[JsonDerivedType(typeof(SetVarNode),      typeDiscriminator: "setVar")]
[JsonDerivedType(typeof(WaitUntilNode),   typeDiscriminator: "waitUntil")]
public abstract record ProgramNode;

public record DriveNode(string PortId, int Speed, int DurationMs) : ProgramNode;
public record SteerNode(string PortId, int Degrees, int DurationMs) : ProgramNode;
public record StopNode(string? PortId) : ProgramNode;      // null = all drive motors
public record WaitNode(int DurationMs) : ProgramNode;

// Count is used when CountExpr is null (lego_repeat block);
// CountExpr is evaluated at runtime (controls_repeat_ext block).
public record RepeatNode(int Count, IReadOnlyList<ProgramNode> Body, ExprNode? CountExpr = null) : ProgramNode;

// Supports if / else-if chain / optional else.
public record IfBranch(ExprNode Condition, IReadOnlyList<ProgramNode> Body);
public record IfNode(IReadOnlyList<IfBranch> Branches, IReadOnlyList<ProgramNode>? ElseBody) : ProgramNode;

// Until=true → loop while condition is false (repeat-until semantics).
public record WhileNode(ExprNode Condition, bool Until, IReadOnlyList<ProgramNode> Body) : ProgramNode;

public record SetVarNode(string Name, ExprNode Value) : ProgramNode;

// Polls Condition every PollMs until truthy.
public record WaitUntilNode(ExprNode Condition, int PollMs = 200) : ProgramNode;

// ── Serialization ──────────────────────────────────────────────────────────

public static class ProgramAstSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static IReadOnlyList<ProgramNode> Deserialize(string json)
        => JsonSerializer.Deserialize<List<ProgramNode>>(json, Options) ?? [];

    public static string Serialize(IReadOnlyList<ProgramNode> nodes)
        => JsonSerializer.Serialize(nodes, Options);
}
