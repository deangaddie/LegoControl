// Blockly interop module for LegoControl
// Requires window.Blockly to be loaded from CDN before this module is imported.

let _workspace = null;
let _motorDefs = [];
let _sensorDefs = [];

// ── Motor/sensor option helpers ────────────────────────────────────────────

function getDriveOptions() {
    const opts = _motorDefs
        .filter(m => m.role === 'Drive' || m.role === 'Auxiliary')
        .map(m => [m.label, m.portId]);
    return opts.length > 0 ? opts : [['Motor A', 'A']];
}

function getSteerOptions() {
    const opts = _motorDefs
        .filter(m => m.role === 'Steering')
        .map(m => [m.label, m.portId]);
    return opts.length > 0 ? opts : [['Steering', 'A']];
}

function getStopOptions() {
    const opts = _motorDefs.map(m => [m.label, m.portId]);
    opts.unshift(['All Motors', '']);
    return opts;
}

function getSensorOptions() {
    const opts = _sensorDefs.map(s => [s.label, s.portId]);
    return opts.length > 0 ? opts : [['Sensor', 'A']];
}

// ── Custom Lego block definitions ──────────────────────────────────────────

function registerLegoBlocks() {
    const B = window.Blockly;

    // Drive motor at speed for N seconds
    B.Blocks['lego_drive'] = {
        init() {
            this.appendDummyInput()
                .appendField('Drive')
                .appendField(new B.FieldDropdown(getDriveOptions()), 'PORT')
                .appendField('at')
                .appendField(new B.FieldNumber(50, -100, 100, 1), 'SPEED')
                .appendField('% for')
                .appendField(new B.FieldNumber(1, 0.1, 60, 0.1), 'DURATION')
                .appendField('s');
            this.setPreviousStatement(true, null);
            this.setNextStatement(true, null);
            this.setColour(120);
            this.setTooltip('Drive a motor at a set speed for a number of seconds. Negative speed = reverse.');
        }
    };

    // Steer to angle, hold for N seconds
    B.Blocks['lego_steer'] = {
        init() {
            this.appendDummyInput()
                .appendField('Steer')
                .appendField(new B.FieldDropdown(getSteerOptions()), 'PORT')
                .appendField('to')
                .appendField(new B.FieldNumber(0, -90, 90, 1), 'DEGREES')
                .appendField('° hold for')
                .appendField(new B.FieldNumber(1, 0, 60, 0.1), 'DURATION')
                .appendField('s');
            this.setPreviousStatement(true, null);
            this.setNextStatement(true, null);
            this.setColour(200);
            this.setTooltip('Move the steering motor to an angle (degrees from centre).');
        }
    };

    // Stop a motor (or all)
    B.Blocks['lego_stop'] = {
        init() {
            this.appendDummyInput()
                .appendField('Stop')
                .appendField(new B.FieldDropdown(getStopOptions()), 'PORT');
            this.setPreviousStatement(true, null);
            this.setNextStatement(true, null);
            this.setColour(0);
            this.setTooltip('Stop one motor or all drive motors.');
        }
    };

    // Wait N seconds
    B.Blocks['lego_wait'] = {
        init() {
            this.appendDummyInput()
                .appendField('Wait')
                .appendField(new B.FieldNumber(1, 0.1, 60, 0.1), 'DURATION')
                .appendField('s');
            this.setPreviousStatement(true, null);
            this.setNextStatement(true, null);
            this.setColour(60);
            this.setTooltip('Pause execution for a number of seconds.');
        }
    };

    // Repeat N times (fixed field)
    B.Blocks['lego_repeat'] = {
        init() {
            this.appendDummyInput()
                .appendField('Repeat')
                .appendField(new B.FieldNumber(3, 1, 1000, 1), 'COUNT')
                .appendField('times');
            this.appendStatementInput('BODY').setCheck(null);
            this.setPreviousStatement(true, null);
            this.setNextStatement(true, null);
            this.setColour(120);
            this.setTooltip('Repeat the inner blocks a set number of times.');
        }
    };

    // Wait until sensor condition (statement wrapper around expression)
    B.Blocks['lego_wait_until'] = {
        init() {
            this.appendValueInput('CONDITION')
                .setCheck('Boolean')
                .appendField('Wait until');
            this.setPreviousStatement(true, null);
            this.setNextStatement(true, null);
            this.setColour(60);
            this.setTooltip('Pause until the condition becomes true. Use sensor value blocks as input.');
        }
    };

    // Sensor value blocks (output = Number)
    B.Blocks['lego_sensor_distance'] = {
        init() {
            this.appendDummyInput()
                .appendField('distance from')
                .appendField(new B.FieldDropdown(getSensorOptions()), 'PORT')
                .appendField('(cm)');
            this.setOutput(true, 'Number');
            this.setColour(290);
            this.setTooltip('Proximity distance from the sensor (0–10 scale).');
        }
    };

    B.Blocks['lego_sensor_color'] = {
        init() {
            this.appendDummyInput()
                .appendField('color from')
                .appendField(new B.FieldDropdown(getSensorOptions()), 'PORT');
            this.setOutput(true, 'Number');
            this.setColour(290);
            this.setTooltip('Color index detected by the sensor: 0=none, 1=black, 2=blue, 3=green, 4=yellow, 5=red, 6=white, 7=brown.');
        }
    };

    B.Blocks['lego_sensor_reflection'] = {
        init() {
            this.appendDummyInput()
                .appendField('reflection from')
                .appendField(new B.FieldDropdown(getSensorOptions()), 'PORT')
                .appendField('(%)');
            this.setOutput(true, 'Number');
            this.setColour(290);
            this.setTooltip('Surface reflection percentage (0–100%).');
        }
    };

    B.Blocks['lego_sensor_ambient'] = {
        init() {
            this.appendDummyInput()
                .appendField('ambient light from')
                .appendField(new B.FieldDropdown(getSensorOptions()), 'PORT')
                .appendField('(%)');
            this.setOutput(true, 'Number');
            this.setColour(290);
            this.setTooltip('Ambient light level (0–100%).');
        }
    };

    // Named color constant for comparison
    B.Blocks['lego_color_const'] = {
        init() {
            this.appendDummyInput()
                .appendField(new B.FieldDropdown([
                    ['none',   '0'],
                    ['black',  '1'],
                    ['blue',   '3'],
                    ['green',  '5'],
                    ['yellow', '7'],
                    ['red',    '9'],
                    ['white',  '10'],
                ]), 'COLOR');
            this.setOutput(true, 'Number');
            this.setColour(290);
            this.setTooltip('A named Lego color constant for use with color comparisons.');
        }
    };
}

// ── Toolbox definition ─────────────────────────────────────────────────────

function buildToolbox() {
    const hasSteer  = _motorDefs.some(m => m.role === 'Steering');
    const hasDrive  = _motorDefs.some(m => m.role === 'Drive' || m.role === 'Auxiliary');
    const hasSensor = _sensorDefs.length > 0;

    const categories = [];

    // ── Lego: Motors ──────────────────────────────────────────────────────
    const motorContents = [];
    if (hasDrive)  motorContents.push({ kind: 'block', type: 'lego_drive' }, { kind: 'block', type: 'lego_stop' });
    if (hasSteer)  motorContents.push({ kind: 'block', type: 'lego_steer' });
    if (motorContents.length > 0)
        categories.push({ kind: 'category', name: 'Motors', colour: '120', contents: motorContents });

    // ── Lego: Sensors ─────────────────────────────────────────────────────
    if (hasSensor) {
        categories.push({
            kind: 'category', name: 'Sensors', colour: '290',
            contents: [
                { kind: 'block', type: 'lego_sensor_distance' },
                { kind: 'block', type: 'lego_sensor_color' },
                { kind: 'block', type: 'lego_sensor_reflection' },
                { kind: 'block', type: 'lego_sensor_ambient' },
                { kind: 'block', type: 'lego_color_const' },
                { kind: 'block', type: 'lego_wait_until' },
            ]
        });
    }

    // ── Lego: Timing ──────────────────────────────────────────────────────
    categories.push({
        kind: 'category', name: 'Timing', colour: '60',
        contents: [
            { kind: 'block', type: 'lego_wait' },
            { kind: 'block', type: 'lego_wait_until' },
            { kind: 'block', type: 'lego_repeat' },
        ]
    });

    // ── Standard: Logic ───────────────────────────────────────────────────
    categories.push({
        kind: 'category', name: 'Logic', colour: '210',
        contents: [
            { kind: 'block', type: 'controls_if' },
            { kind: 'sep' },
            { kind: 'block', type: 'logic_compare' },
            { kind: 'block', type: 'logic_operation' },
            { kind: 'block', type: 'logic_negate' },
            { kind: 'block', type: 'logic_boolean' },
        ]
    });

    // ── Standard: Loops ───────────────────────────────────────────────────
    categories.push({
        kind: 'category', name: 'Loops', colour: '120',
        contents: [
            {
                kind: 'block', type: 'controls_repeat_ext',
                inputs: { TIMES: { block: { type: 'math_number', fields: { NUM: 3 } } } }
            },
            { kind: 'block', type: 'controls_whileUntil' },
            { kind: 'block', type: 'controls_flow_statements' },
        ]
    });

    // ── Standard: Math ────────────────────────────────────────────────────
    categories.push({
        kind: 'category', name: 'Math', colour: '230',
        contents: [
            { kind: 'block', type: 'math_number' },
            { kind: 'block', type: 'math_arithmetic' },
            { kind: 'block', type: 'math_single' },
            { kind: 'block', type: 'math_modulo' },
            { kind: 'block', type: 'math_constrain' },
            { kind: 'block', type: 'math_random_int' },
        ]
    });

    // ── Standard: Variables ───────────────────────────────────────────────
    categories.push({ kind: 'category', name: 'Variables', colour: '330', custom: 'VARIABLE' });

    // ── Standard: Functions ───────────────────────────────────────────────
    categories.push({ kind: 'category', name: 'Functions', colour: '290', custom: 'PROCEDURE' });

    return { kind: 'categoryToolbox', contents: categories };
}

// ── Block → AST serialization ──────────────────────────────────────────────

// Serialize a value (expression-returning) block to an ExprNode JSON object.
function blockToExpr(block) {
    if (!block || block.isInsertionMarker()) return { type: 'number', value: 0 };

    switch (block.type) {
        case 'math_number':
            return { type: 'number', value: parseFloat(block.getFieldValue('NUM')) };

        case 'math_arithmetic':
            return {
                type: 'math',
                left: blockToExpr(block.getInputTargetBlock('A')),
                op: block.getFieldValue('OP'),  // ADD, MINUS, MULTIPLY, DIVIDE, POWER
                right: blockToExpr(block.getInputTargetBlock('B'))
            };

        case 'math_single': {
            // Treat as a unary operation — map to a number via a dummy math node
            const op = block.getFieldValue('OP'); // ROOT, ABS, NEG, etc.
            return { type: 'math', left: blockToExpr(block.getInputTargetBlock('NUM')), op, right: { type: 'number', value: 0 } };
        }

        case 'math_modulo':
            return {
                type: 'math',
                left: blockToExpr(block.getInputTargetBlock('DIVIDEND')),
                op: 'MODULO',
                right: blockToExpr(block.getInputTargetBlock('DIVISOR'))
            };

        case 'math_constrain':
            // clamp(value, low, high) — encode as nested compare nodes
            return blockToExpr(block.getInputTargetBlock('VALUE')); // simplified: just return the value

        case 'math_random_int':
            return { type: 'number', value: 0 }; // runtime: handled specially in interpreter (returns 0 as placeholder)

        case 'logic_compare':
            return {
                type: 'compare',
                left: blockToExpr(block.getInputTargetBlock('A')),
                op: block.getFieldValue('OP'),  // EQ, NEQ, LT, LTE, GT, GTE
                right: blockToExpr(block.getInputTargetBlock('B'))
            };

        case 'logic_operation':
            return {
                type: 'logic',
                left: blockToExpr(block.getInputTargetBlock('A')),
                op: block.getFieldValue('OP'),  // AND, OR
                right: blockToExpr(block.getInputTargetBlock('B'))
            };

        case 'logic_negate':
            return { type: 'negate', operand: blockToExpr(block.getInputTargetBlock('BOOL')) };

        case 'logic_boolean':
            return { type: 'number', value: block.getFieldValue('BOOL') === 'TRUE' ? 1 : 0 };

        case 'variables_get': {
            const f = block.getField('VAR');
            return { type: 'getVar', name: f ? f.getText() : '' };
        }

        case 'lego_sensor_distance':
            return { type: 'sensor', portId: block.getFieldValue('PORT'), sensorType: 'distance' };

        case 'lego_sensor_color':
            return { type: 'sensor', portId: block.getFieldValue('PORT'), sensorType: 'color' };

        case 'lego_sensor_reflection':
            return { type: 'sensor', portId: block.getFieldValue('PORT'), sensorType: 'reflection' };

        case 'lego_sensor_ambient':
            return { type: 'sensor', portId: block.getFieldValue('PORT'), sensorType: 'ambient' };

        case 'lego_color_const':
            return { type: 'number', value: parseInt(block.getFieldValue('COLOR'), 10) };

        default:
            return { type: 'number', value: 0 };
    }
}

// Serialize a statement block to a ProgramNode JSON object.
function blockToNode(block) {
    if (!block || block.isInsertionMarker()) return null;

    switch (block.type) {
        // ── Lego motor blocks ──────────────────────────────────────────────
        case 'lego_drive':
            return {
                type: 'drive',
                portId: block.getFieldValue('PORT'),
                speed: parseInt(block.getFieldValue('SPEED'), 10),
                durationMs: Math.round(parseFloat(block.getFieldValue('DURATION')) * 1000)
            };

        case 'lego_steer':
            return {
                type: 'steer',
                portId: block.getFieldValue('PORT'),
                degrees: parseInt(block.getFieldValue('DEGREES'), 10),
                durationMs: Math.round(parseFloat(block.getFieldValue('DURATION')) * 1000)
            };

        case 'lego_stop': {
            const port = block.getFieldValue('PORT');
            return { type: 'stop', portId: port === '' ? null : port };
        }

        // ── Timing ────────────────────────────────────────────────────────
        case 'lego_wait':
            return {
                type: 'wait',
                durationMs: Math.round(parseFloat(block.getFieldValue('DURATION')) * 1000)
            };

        case 'lego_repeat':
            return {
                type: 'repeat',
                count: parseInt(block.getFieldValue('COUNT'), 10),
                body: blocksToNodes(block.getInputTargetBlock('BODY'))
            };

        case 'lego_wait_until':
            return {
                type: 'waitUntil',
                condition: blockToExpr(block.getInputTargetBlock('CONDITION')),
                pollMs: 200
            };

        // ── Standard: Logic ───────────────────────────────────────────────
        case 'controls_if': {
            // Collect all if/else-if inputs by scanning for IF0, IF1, ... inputs
            const branches = [];
            let i = 0;
            while (block.getInput(`IF${i}`)) {
                branches.push({
                    condition: blockToExpr(block.getInputTargetBlock(`IF${i}`)),
                    body: blocksToNodes(block.getInputTargetBlock(`DO${i}`))
                });
                i++;
            }
            const elseInput = block.getInput('ELSE');
            return {
                type: 'if',
                branches,
                elseBody: elseInput ? blocksToNodes(block.getInputTargetBlock('ELSE')) : null
            };
        }

        // ── Standard: Loops ───────────────────────────────────────────────
        case 'controls_whileUntil':
            return {
                type: 'while',
                condition: blockToExpr(block.getInputTargetBlock('BOOL')),
                until: block.getFieldValue('MODE') === 'UNTIL',
                body: blocksToNodes(block.getInputTargetBlock('DO'))
            };

        case 'controls_repeat_ext':
            return {
                type: 'repeat',
                count: 0,
                countExpr: blockToExpr(block.getInputTargetBlock('TIMES')),
                body: blocksToNodes(block.getInputTargetBlock('DO'))
            };

        case 'controls_flow_statements':
            // break/continue — not supported in interpreter, skip silently
            return null;

        // ── Standard: Variables ───────────────────────────────────────────
        case 'variables_set': {
            const f = block.getField('VAR');
            return {
                type: 'setVar',
                name: f ? f.getText() : '',
                value: blockToExpr(block.getInputTargetBlock('VALUE'))
            };
        }

        // ── Standard: Procedures (functions) ─────────────────────────────
        // Procedure call blocks are named procedures_callnoreturn / procedures_callreturn.
        // These are complex to interpret; skip for now.
        case 'procedures_callnoreturn':
        case 'procedures_callreturn':
            return null;

        default:
            return null;
    }
}

function blocksToNodes(startBlock) {
    const nodes = [];
    let block = startBlock;
    while (block) {
        if (!block.isInsertionMarker()) {
            const node = blockToNode(block);
            if (node) nodes.push(node);
        }
        block = block.getNextBlock();
    }
    return nodes;
}

// ── Public API ─────────────────────────────────────────────────────────────

export function initWorkspace(containerId, motorDefs, sensorDefs) {
    _motorDefs = motorDefs ?? [];
    _sensorDefs = sensorDefs ?? [];

    registerLegoBlocks();

    const container = document.getElementById(containerId);
    if (!container) {
        console.error(`[blockly-interop] Container #${containerId} not found`);
        return;
    }

    _workspace = window.Blockly.inject(container, {
        toolbox: buildToolbox(),
        scrollbars: true,
        trashcan: true,
        grid: { spacing: 20, length: 3, colour: '#555', snap: true },
        zoom: { controls: true, wheel: true, startScale: 1.0 },
        theme: window.Blockly.Themes?.Dark ?? window.Blockly.Theme?.Default
    });
}

// Returns a JSON array string of ProgramNode objects for the top-level block stack.
export function getProgram() {
    if (!_workspace) return '[]';

    const nodes = [];
    for (const block of _workspace.getTopBlocks(true)) {
        // Walk the chain rooted at each top block
        let b = block;
        while (b) {
            if (!b.isInsertionMarker()) {
                const node = blockToNode(b);
                if (node) nodes.push(node);
            }
            b = b.getNextBlock();
        }
    }
    return JSON.stringify(nodes);
}

export function getWorkspaceState() {
    if (!_workspace) return '{}';
    return JSON.stringify(window.Blockly.serialization.workspaces.save(_workspace));
}

export function setWorkspaceState(json) {
    if (!_workspace) return;
    try {
        const state = JSON.parse(json);
        window.Blockly.serialization.workspaces.load(state, _workspace);
    } catch (e) {
        console.warn('[blockly-interop] Failed to restore workspace state:', e);
    }
}

export function clearWorkspace() {
    _workspace?.clear();
}

export function disposeWorkspace() {
    _workspace?.dispose();
    _workspace = null;
}
