using LegoControl.Core.Models;

namespace LegoControl.Core.Services;

public class ProgramInterpreter(
    IBluetoothService bluetooth,
    ILegoHubService hub,
    List<MotorConfig> motors)
{
    private readonly Dictionary<string, double> _variables = new();

    public async Task RunAsync(
        IReadOnlyList<ProgramNode> program,
        IProgress<string>? progress,
        CancellationToken ct)
    {
        foreach (var node in program)
            await ExecuteNodeAsync(node, progress, ct);
    }

    // ── Statement execution ────────────────────────────────────────────────

    private async Task ExecuteNodeAsync(ProgramNode node, IProgress<string>? progress, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        switch (node)
        {
            case DriveNode d:
            {
                var motor = motors.FirstOrDefault(m => m.PortId == d.PortId);
                if (motor is null) break;

                progress?.Report($"Drive {motor.Label} at {d.Speed}% for {d.DurationMs / 1000.0:0.#}s");

                int actual = motor.InvertDirection ? -d.Speed : d.Speed;
                await bluetooth.SendCommandAsync(LegoCommandBuilder.StartPower(motor.PortId, actual));

                if (motor.LinkedPortId is not null)
                {
                    var linked = motors.FirstOrDefault(m => m.PortId == motor.LinkedPortId);
                    int linkedRaw = motor.InvertLink ? -actual : actual;
                    int linkedActual = linked?.InvertDirection == true ? -linkedRaw : linkedRaw;
                    await bluetooth.SendCommandAsync(LegoCommandBuilder.StartPower(motor.LinkedPortId, linkedActual));
                }

                await Task.Delay(d.DurationMs, ct);

                await bluetooth.SendCommandAsync(LegoCommandBuilder.StartPower(motor.PortId, 0));
                if (motor.LinkedPortId is not null)
                    await bluetooth.SendCommandAsync(LegoCommandBuilder.StartPower(motor.LinkedPortId, 0));
                break;
            }

            case SteerNode s:
            {
                var motor = motors.FirstOrDefault(m => m.PortId == s.PortId);
                if (motor is null) break;

                progress?.Report($"Steer {motor.Label} to {s.Degrees}°");

                int actual = motor.InvertDirection ? -s.Degrees : s.Degrees;
                await bluetooth.SendCommandAsync(LegoCommandBuilder.GoToAbsolutePosition(motor.PortId, actual));

                await Task.Delay(s.DurationMs, ct);
                break;
            }

            case StopNode stop:
            {
                progress?.Report(stop.PortId is null ? "Stop all motors" : $"Stop motor {stop.PortId}");

                var targets = stop.PortId is null
                    ? motors.Where(m => m.Role != MotorRole.Steering)
                    : motors.Where(m => m.PortId == stop.PortId && m.Role != MotorRole.Steering);

                foreach (var m in targets)
                    await bluetooth.SendCommandAsync(LegoCommandBuilder.StartPower(m.PortId, 0));
                break;
            }

            case WaitNode w:
                progress?.Report($"Wait {w.DurationMs / 1000.0:0.#}s");
                await Task.Delay(w.DurationMs, ct);
                break;

            case RepeatNode r:
            {
                int count = r.CountExpr is not null
                    ? (int)Math.Round(await EvaluateAsync(r.CountExpr, ct))
                    : r.Count;

                for (int i = 0; i < count; i++)
                {
                    progress?.Report($"Repeat {i + 1}/{count}");
                    foreach (var child in r.Body)
                        await ExecuteNodeAsync(child, progress, ct);
                }
                break;
            }

            case IfNode ifNode:
            {
                foreach (var branch in ifNode.Branches)
                {
                    bool cond = IsTruthy(await EvaluateAsync(branch.Condition, ct));
                    if (cond)
                    {
                        foreach (var child in branch.Body)
                            await ExecuteNodeAsync(child, progress, ct);
                        goto doneIf;
                    }
                }
                if (ifNode.ElseBody is not null)
                    foreach (var child in ifNode.ElseBody)
                        await ExecuteNodeAsync(child, progress, ct);
                doneIf:
                break;
            }

            case WhileNode w:
            {
                while (true)
                {
                    ct.ThrowIfCancellationRequested();
                    bool cond = IsTruthy(await EvaluateAsync(w.Condition, ct));
                    bool shouldRun = w.Until ? !cond : cond;
                    if (!shouldRun) break;
                    foreach (var child in w.Body)
                        await ExecuteNodeAsync(child, progress, ct);
                    // Small yield so tight loops don't starve the runtime
                    await Task.Delay(10, ct);
                }
                break;
            }

            case SetVarNode sv:
                _variables[sv.Name] = await EvaluateAsync(sv.Value, ct);
                break;

            case WaitUntilNode wu:
            {
                progress?.Report("Waiting for condition…");
                while (true)
                {
                    ct.ThrowIfCancellationRequested();
                    if (IsTruthy(await EvaluateAsync(wu.Condition, ct))) break;
                    await Task.Delay(wu.PollMs, ct);
                }
                break;
            }
        }
    }

    // ── Expression evaluation ──────────────────────────────────────────────

    private async Task<double> EvaluateAsync(ExprNode expr, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        return expr switch
        {
            NumberLiteralNode n => n.Value,

            MathArithNode m =>
                ApplyMath(await EvaluateAsync(m.Left, ct), m.Op, await EvaluateAsync(m.Right, ct)),

            CompareNode c =>
                ApplyCompare(await EvaluateAsync(c.Left, ct), c.Op, await EvaluateAsync(c.Right, ct)) ? 1.0 : 0.0,

            LogicOpNode l =>
                ApplyLogic(await EvaluateAsync(l.Left, ct), l.Op, await EvaluateAsync(l.Right, ct)) ? 1.0 : 0.0,

            NegateNode n =>
                IsTruthy(await EvaluateAsync(n.Operand, ct)) ? 0.0 : 1.0,

            GetVarNode gv =>
                _variables.GetValueOrDefault(gv.Name, 0.0),

            SensorValueNode sv =>
                hub.SensorValues.GetValueOrDefault(sv.PortId, 0),

            _ => 0.0
        };
    }

    private static bool IsTruthy(double value) => value != 0.0;

    private static double ApplyMath(double a, string op, double b) => op switch
    {
        "ADD"      => a + b,
        "MINUS"    => a - b,
        "MULTIPLY" => a * b,
        "DIVIDE"   => b != 0 ? a / b : 0,
        "POWER"    => Math.Pow(a, b),
        _          => 0
    };

    private static bool ApplyCompare(double a, string op, double b) => op switch
    {
        "EQ"  => a == b,
        "NEQ" => a != b,
        "LT"  => a < b,
        "LTE" => a <= b,
        "GT"  => a > b,
        "GTE" => a >= b,
        _     => false
    };

    private static bool ApplyLogic(double a, string op, double b) => op switch
    {
        "AND" => IsTruthy(a) && IsTruthy(b),
        "OR"  => IsTruthy(a) || IsTruthy(b),
        _     => false
    };

    public async Task StopAllAsync()
    {
        foreach (var m in motors.Where(m => m.Role != MotorRole.Steering))
            await bluetooth.SendCommandAsync(LegoCommandBuilder.StartPower(m.PortId, 0));
    }
}
